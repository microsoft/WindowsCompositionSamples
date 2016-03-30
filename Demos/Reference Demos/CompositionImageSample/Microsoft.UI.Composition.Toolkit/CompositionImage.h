// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#pragma once

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

    [Windows::Foundation::Metadata::WebHostHidden]
    public enum class CompositionImageLoadStatus
    {
        Success,
        FileAccessError,
        DecodeError,
        NotEnoughResources,
        Other
    };

    // Forward declare for CompositionImageLoadedEventHandler delegate.
    ref class CompositionImage;

    public delegate void CompositionImageLoadedEventHandler(
        CompositionImage^ sender,
        CompositionImageLoadStatus status);

    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class CompositionImage sealed
    {
        // Friend CompositionImageFactory so that CreateCompositionImage does not have to
        // be exposed.
        friend ref class CompositionImageFactory;

    public:
        // Called anytime the bitmap associated with this CompositionImage's Surface is drawn,
        // or an error occurs during loading the image. Check the CompositionImageLoadStatus
        // for details regarding the success or failure to load and draw the image.
        //
        // Note that this event makes no guarantees as to what thread the associated CompositionImageLoadedEventHandler
        // delegate will be invoked on. If there is a specific thread that the contents of the delegate should be
        // executed on, the delegate should implementation should handle that.
        event CompositionImageLoadedEventHandler^ ImageLoaded;

        // Gets the actual size of the decoded bitmap.
        property Windows::Foundation::Size Size
        {
            Windows::Foundation::Size get()
            {
                return Windows::Foundation::Size(
                    static_cast<float>(_sizeBitmapSource.cx),
                    static_cast<float>(_sizeBitmapSource.cy));
            }
        };

        // Gets the ICompositionSurface that backs this CompositionImage.
        // When the CompositionImage is first creates, this will be a 0 x 0 Surface
        // (or the size specified by any CompositionImageOptions provided at the time of creation)
        // and the Surface will be resized and drawn to once the bitmap has been loaded
        // and drawn. To be notified when the image has been completely loaded and drawn,
        // call CompleteLoadAsync which will return the CompositionImageLoadStatus indicating
        // whether the image was successfully loaded or not.
        property ICompositionSurface^ Surface
        {
            ICompositionSurface^ get() {
                return reinterpret_cast<ICompositionSurface^>(_compositionSurface.Get());
            }
        }

    private:
        static CompositionImage^ CreateCompositionImage(
            Compositor^ compositor,
            CompositionGraphicsDevice^ graphicsDevice,
            Uri^ uri,
            StorageFile^ file,
            CompositionImageOptions^ options);

        CompositionImage(
            Compositor^ compositor,
            CompositionGraphicsDevice^ graphicsDevice,
            Uri^ uri,
            StorageFile^ file,
            CompositionImageOptions^ options);

        IAsyncAction^ LoadImageAsync();

        static IBuffer^ LoadImageFromUri(Uri^ uri);

        static IBuffer^ LoadImageFromFile(StorageFile^ file);

        static ComPtr<IWICBitmapSource> DecodeBufferIntoBitmap(
            IBuffer^ imageRawBuffer,
            SIZE sizeDecode);

        HRESULT CompositionImage::DrawBitmapOnSurface(ComPtr<IWICBitmapSource>& bitmapSource);

        void HandleGraphicsDeviceLost(CompositionGraphicsDevice^ sender);

        void ReportImageLoaded(CompositionImageLoadStatus status);

    private:
        Compositor^ _compositor;
        CompositionImageOptions^ _imageOptions;
        CompositionGraphicsDevice^ _graphicsDevice;
        Uri^ _uri;
        StorageFile^ _file;

        ComPtr<ABI::Windows::UI::Composition::ICompositionDrawingSurface> _compositionSurface;
        ComPtr<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop> _compositionSurfaceInterop;

        IAsyncAction^ _loadAsyncAction;

        SIZE _sizeBitmapSource{};
        SIZE _sizeCompositionSurface{};
        SIZE _sizeDecode{};
    };

} // Toolkit
} // Composition
} // UI
} // Microsoft
