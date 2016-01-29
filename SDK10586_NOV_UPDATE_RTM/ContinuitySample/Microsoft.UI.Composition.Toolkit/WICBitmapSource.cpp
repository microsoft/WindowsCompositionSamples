// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#include "pch.h"

#include <Wincodec.h>
#include "WICBitmapSource.h"

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::WICBitmapSourceWrapper
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    WICBitmapSourceWrapper::WICBitmapSourceWrapper(
        __in IWICBitmapSource* bitmapSource,
        __in IWICBitmapSourceTransform* bitmapSourceTransform,
        UINT width,
        UINT height,
        REFWICPixelFormatGUID pixelFormat)
    {
        _bitmapSourceTransform = bitmapSourceTransform;
        _bitmapSource = bitmapSource;
        _width = width;
        _height = height;
        _pixelFormat = pixelFormat;
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::CopyPalette
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    HRESULT
    WICBitmapSourceWrapper::CopyPalette(__in IWICPalette* palette)
    {
        WICPixelFormatGUID sourceFormat;

        if (SUCCEEDED(_bitmapSource->GetPixelFormat(&sourceFormat)) &&
            (sourceFormat == _pixelFormat))
        {
            return _bitmapSource->CopyPalette(palette);
        }

        return WINCODEC_ERR_PALETTEUNAVAILABLE;
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::CopyPixels
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    HRESULT
    WICBitmapSourceWrapper::CopyPixels(
            __in_opt const WICRect* prc,
            UINT stride,
            UINT bufferSize,
            __out_ecount(bufferSize) BYTE* buffer)
    {
        return _bitmapSourceTransform->CopyPixels(
            prc,
            _width,
            _height,
            &_pixelFormat,
            WICBitmapTransformRotate0,
            stride,
            bufferSize,
            buffer
            );
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::GetPixelFormat
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    HRESULT
    WICBitmapSourceWrapper::GetPixelFormat(__out WICPixelFormatGUID* pixelFormat)
    {
        *pixelFormat = _pixelFormat;
        return S_OK;
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::GetResolution
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    HRESULT
    WICBitmapSourceWrapper::GetResolution(__out double* dpiX, __out double* dpiY)
    {
        return _bitmapSource->GetResolution(dpiX, dpiY);
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::GetSize
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    HRESULT
    WICBitmapSourceWrapper::GetSize(__out UINT* width, __out UINT* height)
    {
        *width = _width;
        *height = _height;
        return S_OK;
    }


    //------------------------------------------------------------------------------
    //
    // WICBitmapSourceWrapper::CreateSourceTransform
    //
    //------------------------------------------------------------------------------

    _Use_decl_annotations_
    bool
    WICBitmapSourceWrapper::CreateSourceTransform(
        __in IWICBitmapSource* bitmapSource,
        UINT desiredWidth,
        UINT desiredHeight,
        REFWICPixelFormatGUID desiredFormat,
        __out IWICBitmapSource** newBitmapSource)
    {
        HRESULT hr;

        WRL::ComPtr<IWICBitmapSourceTransform> sourceTransform;
        bool transformingScale = false;
        bool transformingFormat = false;
        UINT cxClosest = desiredWidth;
        UINT cyClosest = desiredHeight;
        WICPixelFormatGUID closestPixelFormat = desiredFormat;
        WICPixelFormatGUID currentPixelFormat;
        UINT cxImage = 0;
        UINT cyImage = 0;


        *newBitmapSource = NULL;

        hr = bitmapSource->QueryInterface(__uuidof(sourceTransform), (void**)&sourceTransform);

        if (SUCCEEDED(hr))
        {
            hr = bitmapSource->GetPixelFormat(&currentPixelFormat);
        }

        if (SUCCEEDED(hr))
        {
            hr = bitmapSource->GetSize(&cxImage, &cyImage);
        }

        if (SUCCEEDED(hr))
        {
            hr = sourceTransform->GetClosestPixelFormat(&closestPixelFormat);
        }

        if (SUCCEEDED(hr))
        {
            hr = sourceTransform->GetClosestSize(&cxClosest, &cyClosest);
        }

        if (SUCCEEDED(hr))
        {
            if ((closestPixelFormat == desiredFormat) && (closestPixelFormat != currentPixelFormat))
            {
                //
                // The codec will convert the pixel format for us.
                //

                transformingFormat = true;
            }

            if (((cxClosest < cxImage) || (cyClosest < cyImage)) &&
                ((cxImage >= desiredWidth) && (cyClosest >= desiredHeight)))
            {
                //
                // The codec is partially or fully scaling the image for us.
                //

                transformingScale = true;
            }
        }

        if (transformingFormat || transformingScale)
        {
            //
            // The codec is transforming pixel format and/or source size for us
            // Create a wrapper around its IWICBitmapTransform to provide a new
            // IWICBitmapSource for the next stage.
            //

            *newBitmapSource = new WICBitmapSourceWrapper(
                bitmapSource,
                sourceTransform.Get(),
                transformingScale ? cxClosest : cxImage,
                transformingScale ? cyClosest : cyImage,
                transformingFormat ? closestPixelFormat : currentPixelFormat);

            if (!*newBitmapSource)
            {
                ::RaiseFailFastException(nullptr, nullptr, 0);
            }
        }

        return *newBitmapSource != NULL;
    }

    STDMETHODIMP_(ULONG) WICBitmapSourceWrapper::AddRef()
    {
        return InterlockedIncrement(&_cRef);
    }

    STDMETHODIMP_(ULONG) WICBitmapSourceWrapper::Release()
    {
        ULONG cRef = InterlockedDecrement(&_cRef);

        if (0 == cRef)
        {
            delete this;
        }

        return cRef;
    }

    STDMETHODIMP WICBitmapSourceWrapper::QueryInterface(REFIID riid, void** object)
    {
        if (object == nullptr)
        {
            return E_POINTER;
        }

        if (riid == __uuidof(IUnknown))
        {
            *object = static_cast<IUnknown*>(this);
        }
        else if (riid == IID_IWICBitmapSource)
        {
            *object = static_cast<IWICBitmapSource*>(this);
        }
        else
        {
            return E_NOINTERFACE;
        }

        AddRef();
        return S_OK;
    }
}
}
}
}