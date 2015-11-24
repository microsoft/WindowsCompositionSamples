// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#pragma once

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

    class WICBitmapSourceWrapper sealed :
        public IWICBitmapSource
    {
    public:
        STDMETHOD_(ULONG, AddRef)(void);
        STDMETHOD_(ULONG, Release)(void);
        STDMETHOD(QueryInterface)(REFIID riid, void** object);

    private:
        WICBitmapSourceWrapper(
            __in IWICBitmapSource* bitmapSource,
            __in IWICBitmapSourceTransform* bitmapSourceTransform,
            UINT width,
            UINT height,
            REFWICPixelFormatGUID pixelFormat);

    public:

        // IWICBitmapSource
        HRESULT STDMETHODCALLTYPE CopyPalette(__in IWICPalette* pPalette);
        HRESULT STDMETHODCALLTYPE CopyPixels(__in_opt const WICRect* prc, UINT stride, UINT bufferSize, __out_ecount(bufferSize) BYTE* buffer);
        HRESULT STDMETHODCALLTYPE GetPixelFormat(__out WICPixelFormatGUID* pixelFormat);
        HRESULT STDMETHODCALLTYPE GetResolution(__out double* dpiX, __out double* dpiY);
        HRESULT STDMETHODCALLTYPE GetSize(__out UINT* width, __out UINT* height);

        static bool CreateSourceTransform(
            __in IWICBitmapSource* bitmapSource,
            UINT desiredWidth,
            UINT desiredHeight,
            REFWICPixelFormatGUID desiredFormat,
            __out IWICBitmapSource** newBitmapSource);

    private:
        UINT                _width;
        UINT                _height;
        WICPixelFormatGUID  _pixelFormat;

        WRL::ComPtr<IWICBitmapSource> _bitmapSource;
        WRL::ComPtr<IWICBitmapSourceTransform> _bitmapSourceTransform;

        volatile LONG _cRef;
    };
}
}
}
}