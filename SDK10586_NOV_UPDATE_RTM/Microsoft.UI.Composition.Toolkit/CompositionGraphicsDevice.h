// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#pragma once

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

    // Forward declare for CompositionGraphicsDeviceLostEventHandler delegate.
    ref class CompositionGraphicsDevice;

    public delegate void CompositionGraphicsDeviceLostEventHandler(CompositionGraphicsDevice^ sender);

    [Windows::Foundation::Metadata::Internal]
    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class CompositionGraphicsDevice sealed
    {
    public:
        static CompositionGraphicsDevice^ CreateCompositionGraphicsDevice(Compositor^ compositor);

        // Called anytime the underlying D3D11Device is lost allowing the event handler to redraw
        // any device specific resources.
        event CompositionGraphicsDeviceLostEventHandler^ DeviceLost;

        ICompositionSurface^ CreateDrawingSurface(
            Size sizePixels,
            DirectXPixelFormat pixelFormat,
            DirectXAlphaMode alphaMode);

        void AcquireDrawingLock();

        void ReleaseDrawingLock();

        virtual ~CompositionGraphicsDevice();

    private:
        CompositionGraphicsDevice(Compositor^ compositor);

        HRESULT InitializeGraphicsDevice();

        HRESULT InitializeDX();

        void UninitializeDX();

        HRESULT AttachDeviceLostHandler();

        static void CALLBACK OnDeviceLostCallback(PTP_CALLBACK_INSTANCE, PVOID context, PTP_WAIT, TP_WAIT_RESULT);

    private:
        Compositor^ _compositor;

        // Any changes to member variables should be made while holding _stateLock.
        ComPtr<IUnknown> _graphicsFactoryBackingDXDevice;
        ComPtr<ABI::Windows::UI::Composition::ICompositionGraphicsDevice> _igraphicsDevice;
        ComPtr<ID3D11Device4> _d3dDevice4;
        HANDLE _deviceLostEvent;
        DWORD  _deviceLostRegistrationCookie;
        PTP_WAIT _threadPoolWait;

        std::mutex _stateLock;
        std::mutex _drawingLock;
};

} // Toolkit
} // Composition
} // UI
} // Microsoft
