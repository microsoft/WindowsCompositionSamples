// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#include "pch.h"

#include "WICBitmapSource.h"

#include "CompositionImageOptions.h"
#include "CompositionGraphicsDevice.h"
#include "CompositionImage.h"

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

CompositionImage::CompositionImage(
    Compositor^ compositor,
    CompositionGraphicsDevice^ graphicsDevice,
    Windows::Foundation::Uri^ uri,
    StorageFile^ file,
    CompositionImageOptions^ options) :
    _compositor(compositor),
    _imageOptions(options),
    _graphicsDevice(graphicsDevice),
    _uri(uri),
    _file(file)
{
    if (options != nullptr)
    {
        _sizeDecode.cx = options->DecodeWidth;
        _sizeDecode.cy = options->DecodeHeight;
    }
}

// Creates a CompostionImage given a Uri or a StorageFile object, only one should be provided.
CompositionImage^ CompositionImage::CreateCompositionImage(
    Compositor^ compositor,
    CompositionGraphicsDevice^ graphicsDevice,
    Windows::Foundation::Uri^ uri,
    StorageFile^ file,
    CompositionImageOptions^ options)
{
    if (uri != nullptr && file != nullptr)
    {
        ERR(E_INVALIDARG, L"CompositionImage cannot be created with both a file and uri.");
        __abi_ThrowIfFailed(E_INVALIDARG);
    }

    if (uri == nullptr && file == nullptr)
    {
        ERR(E_INVALIDARG, L"CompositionImage requires either a file or uri to be created.");
        __abi_ThrowIfFailed(E_INVALIDARG);
    }

    CompositionImage^ image = ref new CompositionImage(
        compositor,
        graphicsDevice,
        uri,
        file,
        options);

    //
    // Create the underlying composition drawing surface using the given graphics device
    //
    Windows::Foundation::Size initialPixelSize;
    initialPixelSize.Width = static_cast<float>(image->_sizeDecode.cx);
    initialPixelSize.Height = static_cast<float>(image->_sizeDecode.cy);

    ICompositionSurface^ surface = graphicsDevice->CreateDrawingSurface(
        initialPixelSize,
        DirectXPixelFormat::B8G8R8A8UIntNormalized,
        DirectXAlphaMode::Premultiplied);

    image->_sizeCompositionSurface = image->_sizeDecode;
    image->_compositionSurface =
        reinterpret_cast<ABI::Windows::UI::Composition::ICompositionDrawingSurface*>(surface);

    __abi_ThrowIfFailed(image->_compositionSurface.As(&image->_compositionSurfaceInterop));

    //
    // Register for device lost since we must redraw the bitmap into the composition surface
    // in the case the graphics device is reset.
    //
    graphicsDevice->DeviceLost += ref new CompositionGraphicsDeviceLostEventHandler(
        image,
        &CompositionImage::HandleGraphicsDeviceLost);

    //
    // Kick off the image load operation so that the image will download and decode in the background.
    //
    image->_loadAsyncAction = image->LoadImageAsync();

    return image;
}

// Abstracts the actual download of the image and drawing into the underlying surface.
// An async action is scheduled as a result of calling LoadImageAsync and
// its result should be obtained vby registering for the ImageLoaded event.
IAsyncAction^ CompositionImage::LoadImageAsync()
{
    //
    // Downloading, decoding, and then drawing the bitmap could take some time so create an async operation
    // to perform this action synchronously off the caller's thread.
    //
    return create_async([this]() -> void
    {
        //
        // Load the Buffer from the Uri or StorageFile that backs this composition image.
        //
        IBuffer^ imageRawBuffer = nullptr;
        if (_uri != nullptr)
        {
            imageRawBuffer = LoadImageFromUri(_uri);
        }
        else if (_file != nullptr)
        {
            imageRawBuffer = LoadImageFromFile(_file);
        }

        if (imageRawBuffer == nullptr)
        {
            ReportImageLoaded(CompositionImageLoadStatus::FileAccessError);
            return;
        }

        //
        // Decode the buffer into an IWICBitmapSource
        //
        ComPtr<IWICBitmapSource> bitmapSource = DecodeBufferIntoBitmap(imageRawBuffer, _sizeDecode);
        if (bitmapSource == nullptr)
        {
            // If for some reason the DecodeBufferIntoBitmap failed report a decode error to ImageLoaded.
            ReportImageLoaded(CompositionImageLoadStatus::DecodeError);
            return;
        }

        //
        // Update the size of the loaded bitmap now that it's decoded
        //
        UINT cxSizeBitmap;
        UINT cySizeBitmap;
        HRESULT hr = bitmapSource->GetSize(&cxSizeBitmap, &cySizeBitmap);
        if (FAILED(hr))
        {
            ERR(hr, L"Failed to get the size of the decoded bitmap");
            ReportImageLoaded(CompositionImageLoadStatus::Other);
            return;
        }

        _sizeBitmapSource.cx = cxSizeBitmap;
        _sizeBitmapSource.cy = cySizeBitmap;

        //
        // Draw the bitmap into the composition surface that backs this CompositionImage
        //
        hr = DrawBitmapOnSurface(bitmapSource);
        if (FAILED(hr))
        {
            ERR(hr, L"Failed to draw bitmap on drawing surface.");

            // Only report an image load error in the case we get a return value other than
            // DXGI_ERROR_DEVICE_REMOVED since those error cases are handled by the DeviceLost
            // callback which is recover the image load on the new device.
            if (hr != DXGI_ERROR_DEVICE_REMOVED)
            {
                ReportImageLoaded(CompositionImageLoadStatus::NotEnoughResources);
            }

            return;
        }

        ReportImageLoaded(CompositionImageLoadStatus::Success);
    });
};

// Loads the raw image contents from the Uri synchronously and returns the resulting IBuffer
// which contains the raw image contents.
IBuffer^ CompositionImage::LoadImageFromUri(Uri^ uri)
{
    task_completion_event<IBuffer^> fileLoaded;
    task<StorageFile^> fileLoadTask;

    if (uri != nullptr)
    {
        if ((uri->SchemeName == L"http") ||
            (uri->SchemeName == L"https"))
        {
            fileLoadTask =
                create_task(StorageFile::CreateStreamedFileFromUriAsync(uri->Extension, uri, nullptr));
        }
        else if (uri->SchemeName == L"file")
        {
            ERR(E_INVALIDARG, L"Uri cannot have the file scheme, use CreateCompositionImageFromFile instead.");
        }
        else
        {
            fileLoadTask = create_task(StorageFile::GetFileFromApplicationUriAsync(uri));
        }

        fileLoadTask.then([fileLoaded](StorageFile^ file)
        {
            fileLoaded.set(LoadImageFromFile(file));
        });
    }

    return create_task(fileLoaded).get();
}

// Opens a StorageFile and reads its contents into the returned IBuffer synchronously.
IBuffer^ CompositionImage::LoadImageFromFile(StorageFile^ file)
{
    task_completion_event<IBuffer^> fileLoaded;

    auto fileLoadTask = create_task(file->OpenAsync(FileAccessMode::Read)).then(
            [file, fileLoaded](task<IRandomAccessStream^> task)
    {
        IRandomAccessStream^ readStream = task.get();
        unsigned long long size = readStream->Size;

        DataReader^ dataReader = ref new DataReader(readStream);
        return create_task(dataReader->LoadAsync(static_cast<UINT32>(size))).then(
            [dataReader, fileLoaded](unsigned int numBytesLoaded)
        {
            IBuffer^ imageRawBuffer = dataReader->ReadBuffer(numBytesLoaded);

            // As a best practice, explicitly close the dataReader resource as soon as it is no longer needed.
            delete dataReader;

            fileLoaded.set(imageRawBuffer);
        });
    });

    return create_task(fileLoaded).get();
}

// Decodes the image respresented by the raw data contained in imageRawBuffer into a bitmap synchronously.
ComPtr<IWICBitmapSource> CompositionImage::DecodeBufferIntoBitmap(IBuffer^ imageRawBuffer, SIZE sizeDecode)
{
    HRESULT hr = S_OK;
    bool succeeded = false;

    ComPtr<IBufferByteAccess> byteBuffer;
    ComPtr<IWICStream> wicStream;
    ComPtr<IWICImagingFactory> imagingFactory;
    ComPtr<IWICBitmapDecoder> bitmapDecoder;
    ComPtr<IWICBitmapFrameDecode> bitmapFrame;
    ComPtr<IWICBitmapSource> bitmapSource;
    ComPtr<IWICBitmapSourceTransform> sourceTransform;

    GUID pixelFormat = GUID_NULL;

    UINT cxImage = 0;
    UINT cyImage = 0;
    UINT cxScaled = 0;
    UINT cyScaled = 0;

    FAILHARD(CoCreateInstance(
        CLSID_WICImagingFactory,
        NULL,
        CLSCTX_INPROC_SERVER,
        IID_IWICImagingFactory,
        (LPVOID*)&imagingFactory
        ));

    unsigned int bufferLength = imageRawBuffer->Length;
    ComPtr<IInspectable> inspectableSavedBits(reinterpret_cast<IInspectable*>(imageRawBuffer));
    ComPtr<IBufferByteAccess> byteAccess;
    IFC(inspectableSavedBits.As(&byteAccess));

    BYTE* bytes = nullptr;
    IFC(byteAccess->Buffer(&bytes));

    IFC(imagingFactory->CreateStream(wicStream.GetAddressOf()));
    IFC(wicStream->InitializeFromMemory(bytes, bufferLength));

    hr = imagingFactory->CreateDecoderFromStream(
        wicStream.Get(),
        NULL,
        WICDecodeMetadataCacheOnDemand,
        &bitmapDecoder);
    IFC(hr);

    //
    // Initialize bitmapFrame with the original image frame. bitmapSource
    // is updated throughout this function as additional transforms
    // (scaling, conversion, cropping, etc) are added to the image prior
    // to the actual decoding.
    //
    IFC(bitmapDecoder->GetFrame(0, &bitmapFrame));

    bitmapSource = bitmapFrame;

    //
    // Determine if source image may have an alpha channel
    //
    IFC(bitmapSource->GetPixelFormat(&pixelFormat));

    //
    // If there's no alpha channel, all pixels can be assumed to be opaque, allowing
    // converting to 32bppPBGRA instead of 32bppBGRA. This allows WIC scalers
    // to skip premultiplying and unpremultiplying.
    //
    REFWICPixelFormatGUID desiredPixelFormat = GUID_WICPixelFormat32bppPBGRA;

    //
    // Compute the size that the image should be resized to, and the
    // subrect that should be cropped out.
    //
    IFC(bitmapSource->GetSize(&cxImage, &cyImage));

    //
    // It's safe to use these without a lock, since this is the only thread
    // that can access them.
    //
    if (sizeDecode.cx == 0)
    {
        sizeDecode.cx = cxImage;
    }

    if (sizeDecode.cy == 0)
    {
        sizeDecode.cy = cyImage;
    }

    // Do nothing if decode size is greater than the actual size.
    if ((cxImage > (UINT)sizeDecode.cx) ||
        (cyImage > (UINT)sizeDecode.cy))
    {
        //
        // Need to reduce both dimensions by the same %; use the one that
        // will maximize shrinkage to ensure the image <= max specified.
        //
        if ((sizeDecode.cx * cyImage) < (sizeDecode.cy * cxImage))
        {
            // Maximize the width and then scale the height.
            cxScaled = sizeDecode.cx;
            cyScaled = cxScaled * cyImage / cxImage;
        }
        else
        {
            // Maximize the height and then scale the width.
            cyScaled = sizeDecode.cy;
            cxScaled = cyScaled * cxImage / cyImage;
        }
    }
    else
    {
        cxScaled = cxImage;
        cyScaled = cyImage;
    }

    // We cannot decode to a dimension of zero.
    cxScaled = max(1u, cxScaled);
    cyScaled = max(1u, cyScaled);

    if (cxImage >= INT_MAX || cyImage >= INT_MAX)
    {
        ERR(E_INVALIDARG, L"Image size is too large to decode.");
        IFC(E_INVALIDARG);
    }

    //
    // Convert pixel format if needed. These converters are chained, so if the bitmap
    // is getting a source transform, this wraps the transformer, otherwise the default
    // image.
    //
    if ((pixelFormat != GUID_NULL) && (pixelFormat != desiredPixelFormat))
    {
        ComPtr<IWICFormatConverter> formatConverter;

        IFC(imagingFactory->CreateFormatConverter(&formatConverter));
        hr = formatConverter->Initialize(
            bitmapSource.Get(),               // Input bitmap to convert
            desiredPixelFormat,               // Destination pixel format
            WICBitmapDitherTypeNone,          // Specified dither pattern
            NULL,                             // Specify a particular palette
            0.0f,                             // Alpha threshold
            WICBitmapPaletteTypeCustom        // Palette translation type
            );
        IFC(hr);

        bitmapSource = formatConverter;
    }

    //
    // Scale the image if necessary.
    //
    if ((cxScaled != cxImage) || (cyScaled != cyImage))
    {
        ComPtr<IWICBitmapScaler> scaler;

        IFC(imagingFactory->CreateBitmapScaler(&scaler));

        WICBitmapInterpolationMode interpolationMode = WICBitmapInterpolationModeFant;

        if ((cxScaled * 2 >= cxImage) && (cyScaled * 2 >= cyImage))
        {
            //
            // If upscaling, or if downscaling less than 50%, use Linear interpolation
            // which is faster than Fant and will provide similar quality for these cases.
            //

            interpolationMode = WICBitmapInterpolationModeLinear;
        }

        hr = scaler->Initialize(
            bitmapSource.Get(),
            cxScaled,
            cyScaled,
            interpolationMode);
        IFC(hr);

        bitmapSource = scaler;
    }

    succeeded = true;

Cleanup:
    if (succeeded)
    {
        return bitmapSource;
    }

    return nullptr;
}

// Draws the given bitmap on the ICompositionSurface represented by this CompositionImage synchronously.
// This is thread-safe since it only accesses members under the CompositionGraphicsDevice drawing lock.
HRESULT CompositionImage::DrawBitmapOnSurface(ComPtr<IWICBitmapSource>& bitmapSource)
{
    HRESULT hr = S_OK;
    ComPtr<ID2D1DeviceContext> d2d1DeviceContext;
    ComPtr<ID2D1Bitmap> d2d1BitmapSource;
    ComPtr<ID2D1BitmapRenderTarget> compatibleRenderTarget;

    // Keep track of whether or not BeginDraw was called successfully on our backing surface so
    // that we know in the Cleanup whether or not to call EndDraw.
    bool drawingBegun = false;

    // CompositionGraphicsDevice only allows one surface to be active (i.e. BeginDraw has been called but not EndDraw) so
    // we acquire the drawing lock so that all drawing onto composition surfaces created by the CompositionGraphicsDevice
    // happen synchronously with no overlap.
    _graphicsDevice->AcquireDrawingLock();

    // Resize the composition drawing surface to match the size of the bitmap if needed.
    if ((_sizeCompositionSurface.cx != _sizeBitmapSource.cx) ||
        (_sizeCompositionSurface.cx != _sizeBitmapSource.cy))
    {
        hr = _compositionSurfaceInterop->Resize(_sizeBitmapSource);
        if (FAILED(hr))
        {
            ERR(hr, L"Failed to resize composition image surface.");
            IFC(hr);
        }

        _sizeCompositionSurface = _sizeBitmapSource;
    }

    RECT rect;
    rect.left = 0;
    rect.top = 0;
    rect.right = _sizeBitmapSource.cx;
    rect.bottom = _sizeBitmapSource.cy;
    POINT offset;

    IFC(_compositionSurfaceInterop->BeginDraw(
        &rect,
        IID_PPV_ARGS(&d2d1DeviceContext),
        &offset));
    drawingBegun = true;

    IFC(d2d1DeviceContext->CreateCompatibleRenderTarget(&compatibleRenderTarget));

    IFC(compatibleRenderTarget->CreateBitmapFromWicBitmap(
        bitmapSource.Get(),
        nullptr,
        d2d1BitmapSource.GetAddressOf()));

    D2D1_RECT_F destRect;
    destRect.left = static_cast<float>(offset.x);
    destRect.top = static_cast<float>(offset.y);
    destRect.right = static_cast<float>(destRect.left + _sizeBitmapSource.cx);
    destRect.bottom = static_cast<float>(destRect.top + _sizeBitmapSource.cy);

    D2D1_RECT_F sourceRect;
    sourceRect.left = 0;
    sourceRect.top = 0;
    sourceRect.right = static_cast<float>(_sizeBitmapSource.cx);
    sourceRect.bottom = static_cast<float>(_sizeBitmapSource.cy);

    d2d1DeviceContext->SetPrimitiveBlend(D2D1_PRIMITIVE_BLEND_COPY);
    d2d1DeviceContext->DrawBitmap(
        d2d1BitmapSource.Get(),
        &destRect,
        1.0f,
        D2D1_BITMAP_INTERPOLATION_MODE_LINEAR,
        &sourceRect);

Cleanup:
    if (drawingBegun)
    {
        HRESULT endDrawHr = _compositionSurfaceInterop->EndDraw();
        if (FAILED(endDrawHr))
        {
            ERR(endDrawHr, L"Failed to EndDraw");
        }
    }

    _graphicsDevice->ReleaseDrawingLock();

    return hr;
}

// In the case that the CompositionGraphicsDevice that was used by this CompositionImage
// to create our ICompositionSurface is lost, we need to redraw the contents of the bitmap
// to the ICompositionSurface.
void CompositionImage::HandleGraphicsDeviceLost(CompositionGraphicsDevice^ sender)
{
    if (_loadAsyncAction != nullptr)
    {
        _loadAsyncAction->Cancel();
        _loadAsyncAction = nullptr;
    }

    _loadAsyncAction = LoadImageAsync();
}

// Fires the ImageLoaded event give the CompositionImageLoadStatus.
void CompositionImage::ReportImageLoaded(CompositionImageLoadStatus status)
{
    ImageLoaded(this, status);
}

}  // namespace Toolkit
}  // namespace Composition
}  // namespace UI
}  // namespace Microsoft