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
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class Photo
    //
    //  This class represents a single image, loaded from a Uri.  It may be
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


        public Photo(Uri uri, CompositionGraphicsDevice device)
        {
            _uri = uri;
            _device = device;
        }


        public CompositionImage ThumbnailImage
        {
            get
            {
                if (_thumbnailImage == null)
                {
                    _thumbnailImage = _device.CreateImageFromUri(_uri, s_thumbnailImageOptions);
                }

                return _thumbnailImage;
            }
        }


        public Uri Uri
        {
            get { return _uri; }
        }

        private static CompositionImageOptions s_thumbnailImageOptions;

        private Uri _uri;
        private CompositionGraphicsDevice _device;
        private CompositionImage _thumbnailImage;
    }
}
