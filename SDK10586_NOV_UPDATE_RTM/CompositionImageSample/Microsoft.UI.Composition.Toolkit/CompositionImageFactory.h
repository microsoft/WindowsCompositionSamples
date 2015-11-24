// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

#pragma once

namespace Microsoft {
namespace UI {
namespace Composition {
namespace Toolkit {

    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class CompositionImageFactory sealed
    {
    public:
        static CompositionImageFactory^ CreateCompositionImageFactory(
            Compositor^ compositor);

        CompositionImage^ CreateImageFromUri(Uri^ uri);

        CompositionImage^ CreateImageFromUri(
            Uri^ uri,
            CompositionImageOptions^ options);

        CompositionImage^ CreateImageFromFile(StorageFile^ file);

        CompositionImage^ CreateImageFromFile(
            StorageFile^ file,
            CompositionImageOptions^ options);

    private:
        CompositionImageFactory(
            Compositor^ compositor,
            CompositionGraphicsDevice^ graphicsDevice);

    private:
        CompositionGraphicsDevice^ _graphicsDevice;
        Compositor^ _compositor;
    };

} // Toolkit
} // Composition
} // UI
} // Microsoft
