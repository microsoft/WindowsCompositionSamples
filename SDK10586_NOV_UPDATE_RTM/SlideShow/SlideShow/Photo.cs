//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition.Toolkit;
using Windows.Storage;
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class Photo
    //
    //  This class represents a single image, loaded from a StorageFile.  It may be
    //  displayed by multiple Tiles.
    //
    //------------------------------------------------------------------------------

    class Photo
    {
        static Photo()
        {
            s_thumbnailImageOptions = new CompositionImageOptions()
            {
                DecodeWidth = 400,
                DecodeHeight = 400,
            };
        }


        public Photo(StorageFile file, CompositionImageFactory imageFactory)
        {
            _file = file;
            _imageFactory = imageFactory;
        }


        public CompositionImage ThumbnailImage
        {
            get
            {
                if (_thumbnailImage == null)
                {
                    _thumbnailImage = _imageFactory.CreateImageFromFile(_file, s_thumbnailImageOptions);
                }

                return _thumbnailImage;
            }
        }


        public StorageFile File
        {
            get { return _file; }
        }

        private static CompositionImageOptions s_thumbnailImageOptions;

        private StorageFile _file;
        private CompositionImageFactory _imageFactory;
        private CompositionImage _thumbnailImage;
    }
}
