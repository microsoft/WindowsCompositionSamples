// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#pragma once

#include <d2d1_1.h>
#include <d3d11.h>
#include <d3d11_4.h>
#include <windows.h>
#include <winnt.h>
#include <collection.h>
#include <ppltasks.h>
#include <robuffer.h>
#include <roerrorapi.h>
#include <Wincodec.h>
#include <windows.ui.composition.h>
#include <windows.ui.composition.interop.h>

using namespace concurrency;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;
using namespace Windows::Graphics::DirectX;
using namespace Windows::UI::Composition;
using namespace Windows::Foundation;

// Helper to goto a Cleanup label if the provided HRESULT value fails.
#define IFC(expr) {hr = expr; if (FAILED(hr)) goto Cleanup;}

// Logs an error message and HRESULT using RoOriginateError.
inline void ERR(HRESULT hr, LPCWSTR msg)
{
    RoOriginateError(hr, StringReference(msg).GetHSTRING());
}

// Raises a fail fast exception if the provided HRESULT value fails.
inline void FAILHARD(HRESULT hr)
{
    if (FAILED(hr))
    {
        __debugbreak();
        ::RaiseFailFastException(nullptr, nullptr, 0);
    }
}
