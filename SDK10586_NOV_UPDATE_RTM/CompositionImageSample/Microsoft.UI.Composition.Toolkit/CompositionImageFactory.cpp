// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#include "pch.h"
#include "CompositionGraphicsDevice.h"
#include "CompositionImageOptions.h"
#include "CompositionImage.h"
#include "CompositionImageFactory.h"

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

// Returns a CompositionImageFactory that can be used to create CompositionImages
// associated with a particular Compositor instance. A reference should be kept to
// the returned factory for the lifetime of the composition images created by the
// factory.
CompositionImageFactory^ CompositionImageFactory::CreateCompositionImageFactory(Compositor^ compositor)
{
    if (compositor == nullptr)
    {
        __abi_ThrowIfFailed(E_INVALIDARG);
    }

    CompositionGraphicsDevice^ device = CompositionGraphicsDevice::CreateCompositionGraphicsDevice(compositor);
    return ref new CompositionImageFactory(compositor, device);
}

CompositionImageFactory::CompositionImageFactory(Compositor^ compositor, CompositionGraphicsDevice^ graphicsDevice) :
    _compositor(compositor),
    _graphicsDevice(graphicsDevice)
{
}

// Creates a CompositionImage given a Uri that represents a packaged resource (ms-appx:///)
// a application data resource (ms-appdata:///) or an http(s) resource (http:// or https://)
// Does not support creating a CompositionImage from a Uri with the file scheme, instead
// a StorageFile object should be passed as an argument to CreateImageFromFile.
CompositionImage^ CompositionImageFactory::CreateImageFromUri(Windows::Foundation::Uri^ uri)
{
    CompositionImageOptions^ options = ref new CompositionImageOptions();
    options->DecodeHeight = 0;
    options->DecodeWidth = 0;

    return CreateImageFromUri(uri, options);
}

// Functions similarly to CreateImageFromUri with the exception that any options provided
// by the CompositionImageOptions are used to initialize the CompositionImage.
CompositionImage^ CompositionImageFactory::CreateImageFromUri(Windows::Foundation::Uri^ uri, CompositionImageOptions^ options)
{
    SIZE szDecode = { 0, 0 };
    if (options)
    {
        szDecode.cx = (ULONG)options->DecodeWidth;
        szDecode.cy = (ULONG)options->DecodeHeight;
    }

    return CompositionImage::CreateCompositionImage(
        _compositor,
        _graphicsDevice,
        uri,
        nullptr,
        options);
}

// Creates a CompositionImage given a StorageFile.
CompositionImage^ CompositionImageFactory::CreateImageFromFile(StorageFile^ file)
{
    CompositionImageOptions^ options = ref new CompositionImageOptions();
    options->DecodeHeight = 0;
    options->DecodeWidth = 0;

    return CreateImageFromFile(file, options);
}

// Functions similarly to CreateImageFromFile with the exception that any options provided
// by the CompositionImageOptions are used to initialize the CompositionImage.
CompositionImage^ CompositionImageFactory::CreateImageFromFile(StorageFile^ file, CompositionImageOptions^ options)
{
    SIZE szDecode = { 0, 0 };
    if (options)
    {
        szDecode.cx = (ULONG)options->DecodeWidth;
        szDecode.cy = (ULONG)options->DecodeHeight;
    }

    return CompositionImage::CreateCompositionImage(
        _compositor,
        _graphicsDevice,
        nullptr,
        file,
        options);
}

}  // namespace Toolkit
}  // namespace Composition
}  // namespace UI
}  // namespace Microsoft