// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#include "pch.h"
#include "CompositionGraphicsDevice.h"

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

CompositionGraphicsDevice^ CompositionGraphicsDevice::CreateCompositionGraphicsDevice(Compositor^ compositor)
{
    CompositionGraphicsDevice^ device = ref new CompositionGraphicsDevice(compositor);

    __abi_ThrowIfFailed(device->InitializeGraphicsDevice());

    return device;
}

// Creates an ICompositionSurface that can be provided used to create a CompositionSurfaceBrush
// to be associated with a CompositionSpriteVisual.
ICompositionSurface^ CompositionGraphicsDevice::CreateDrawingSurface(Windows::Foundation::Size sizePixels, DirectXPixelFormat pixelFormat, DirectXAlphaMode alphaMode)
{
    ComPtr<ABI::Windows::UI::Composition::ICompositionDrawingSurface> drawingSurface;
    ABI::Windows::Foundation::Size size;
    size.Height = sizePixels.Height;
    size.Width = sizePixels.Width;

    // Make sure to call CreateDrawingSurface under this CompositionGraphicsDevice's state lock
    // to avoid any races with D3D initialization and (re)creation of the underlying device.
    std::lock_guard<std::mutex> lock(_stateLock);

    __abi_ThrowIfFailed(_igraphicsDevice->CreateDrawingSurface(
        size,
        (ABI::Windows::Graphics::DirectX::DirectXPixelFormat)pixelFormat,
        (ABI::Windows::Graphics::DirectX::DirectXAlphaMode)alphaMode,
        &drawingSurface));

    return reinterpret_cast<ICompositionSurface^>(drawingSurface.Get());
}

// Acquires exclusive access to the underlying D3D11Device for the purpose of drawing to surfaces
// created by this CompositionGraphicsDevice. Once finished, the caller should call ReleaseDrawingLock.
void CompositionGraphicsDevice::AcquireDrawingLock()
{
    _drawingLock.lock();
}

// Releases ownership of the drawing lock associated with the drawing surfaces created
// by this CompositionGraphicsDevice. It is not safe to call this if AcquireDrawingLock
// was not called previously.
void CompositionGraphicsDevice::ReleaseDrawingLock()
{
    _drawingLock.unlock();
}

CompositionGraphicsDevice::CompositionGraphicsDevice(Compositor^ compositor) :
    _compositor(compositor),
    _threadPoolWait(NULL)
{
}

CompositionGraphicsDevice::~CompositionGraphicsDevice()
{
    UninitializeDX();
}

// Calls InitializeDX and then creates an ICompositionGraphicsDevice which is
// used to create drawing surfaces to be associated with composition visuals.
HRESULT CompositionGraphicsDevice::InitializeGraphicsDevice()
{
    HRESULT hr = S_OK;
    ComPtr<ABI::Windows::UI::Composition::ICompositorInterop> compositorInterop;
    ComPtr<ABI::Windows::UI::Composition::ICompositionGraphicsDevice> graphicsDevice;
    ComPtr<ABI::Windows::UI::Composition::ICompositor> iCompositor;

    std::lock_guard<std::mutex> lock(_stateLock);

    IFC(InitializeDX());

    iCompositor = reinterpret_cast<ABI::Windows::UI::Composition::ICompositor*>(_compositor);
    IFC(iCompositor.As(&compositorInterop));
    IFC(compositorInterop->CreateGraphicsDevice(_graphicsFactoryBackingDXDevice.Get(), &graphicsDevice));

    _igraphicsDevice = graphicsDevice;

Cleanup:
    return hr;
}

// Initializes DirectX and registers for device lost notifications. Should be called with _stateLock held.
HRESULT CompositionGraphicsDevice::InitializeDX()
{
    HRESULT hr = S_OK;
    ComPtr<ID3D11Device> d3dDevice;
    ComPtr<ID3D11DeviceContext> d3dContext;
    ComPtr<IDXGIDevice> dxgiDevice;
    ComPtr<ID2D1Factory1> d2dFactory;
    ComPtr<ID2D1Device> d2d1Device;
    ComPtr<ID3D11Device1> d3dDevice1;
    ComPtr<ID3D11Device4> d3dDevice4;
    ComPtr<IDXGIFactory> dxgiFactory;

    UninitializeDX();

    // D3D11_CREATE_DEVICE_BGRA_SUPPORT is required for Direct2D interoperability with Direct3D resources.
    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
    D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
        D3D_FEATURE_LEVEL_9_3,
        D3D_FEATURE_LEVEL_9_2,
        D3D_FEATURE_LEVEL_9_1
    };
    D3D_FEATURE_LEVEL usedFeatureLevel;

    IFC(D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        creationFlags,
        featureLevels,
        ARRAYSIZE(featureLevels),
        D3D11_SDK_VERSION,
        &d3dDevice,
        &usedFeatureLevel,
        &d3dContext));

    if (FAILED(hr))
    {
        FAILHARD(hr);
    }

    IFC(d3dDevice.As(&d3dDevice1));

    // We're using our own synchronization so we can create the D2D factory in single threaded mode.
    IFC(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, __uuidof(ID2D1Factory1), &d2dFactory));
    IFC(d3dDevice.As(&dxgiDevice));
    IFC(d2dFactory->CreateDevice(dxgiDevice.Get(), &d2d1Device));

    _graphicsFactoryBackingDXDevice = d2d1Device;

    IFC(d3dDevice.As(&_d3dDevice4));
    IFC(AttachDeviceLostHandler());

Cleanup:
    return hr;
}

// Registers a device removed event with D3D so that we can be called back
// anytime our underlying D3D11Device is lost or reset. Should be called with _stateLock held.
HRESULT CompositionGraphicsDevice::AttachDeviceLostHandler()
{
    HRESULT hr = S_OK;

    _deviceLostEvent = CreateEvent(
        NULL,  // Attributes
        TRUE,  // Manual reset
        FALSE, // Initial state
        NULL); // Name
    if (_deviceLostEvent == NULL)
    {
        FAILHARD(HRESULT_FROM_WIN32(GetLastError()));
    }

    // It's okay for us to use a weak reference to ourself here since we unregister on destruction.
    _threadPoolWait = CreateThreadpoolWait(CompositionGraphicsDevice::OnDeviceLostCallback, (PVOID)this, NULL);
    if (_threadPoolWait == NULL)
    {
        FAILHARD(HRESULT_FROM_WIN32(GetLastError()));
    }

    SetThreadpoolWait(_threadPoolWait, _deviceLostEvent, NULL);
    hr = _d3dDevice4->RegisterDeviceRemovedEvent(_deviceLostEvent, &_deviceLostRegistrationCookie);
    if (FAILED(hr))
    {
        FAILHARD(hr);
    }

    return hr;
}

// Called in response to the underlying D3D11Device being lost, triggers a DeviceLost
// event to any subscribers.
void CALLBACK CompositionGraphicsDevice::OnDeviceLostCallback(PTP_CALLBACK_INSTANCE, PVOID context, PTP_WAIT, TP_WAIT_RESULT)
{
    CompositionGraphicsDevice^ pThis = reinterpret_cast<CompositionGraphicsDevice^>(context);
    ComPtr<ABI::Windows::UI::Composition::ICompositionGraphicsDeviceInterop> graphicsDeviceInterop;

    if (pThis == nullptr)
    {
        FAILHARD(E_POINTER);
    }

    HRESULT hr = pThis->InitializeDX();
    if (FAILED(hr))
    {
        FAILHARD(hr);
    }

    // Set the new DX context as the rendering device on the CompositionGraphicsDevice
    hr = pThis->_igraphicsDevice.As(&graphicsDeviceInterop);
    if (FAILED(hr))
    {
        FAILHARD(hr);
    }

    ComPtr<IUnknown> unknownDXDevice;
    hr = pThis->_graphicsFactoryBackingDXDevice.As(&unknownDXDevice);
    if (FAILED(hr))
    {
        FAILHARD(hr);
    }

    graphicsDeviceInterop->SetRenderingDevice(unknownDXDevice.Get());

    // fire the event that this graphics device was lost and reinitialized
    pThis->DeviceLost(pThis);
}

// Should be called with _stateLock held.
void CompositionGraphicsDevice::UninitializeDX()
{
    if ((_deviceLostRegistrationCookie != 0) && (_d3dDevice4 != nullptr))
    {
        _d3dDevice4->UnregisterDeviceRemoved(_deviceLostRegistrationCookie);
        _deviceLostRegistrationCookie = 0;
    }

    if (_deviceLostEvent != NULL)
    {
        CloseHandle(_deviceLostEvent);
        _deviceLostEvent = NULL;
    }

    if (_threadPoolWait != NULL)
    {
        CloseThreadpoolWait(_threadPoolWait);
        _threadPoolWait = NULL;
    }

    _d3dDevice4 = nullptr;
}

}  // namespace Toolkit
}  // namespace Composition
}  // namespace UI
}  // namespace Microsoft