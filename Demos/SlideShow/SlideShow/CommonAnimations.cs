//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class CommonAnimations
    //
    //  This class defines some common animation templates and timings used
    //  throughout the application.
    //
    //------------------------------------------------------------------------------

    class CommonAnimations
    {
        public static void Initialize(Compositor compositor)
        {
            // Create keyframes to select and unselect:
            // - Since both Saturation and Opacity use float [0.0 -> 1.0], we can actually use the
            //   same keyframe instances and just bind to different properties.

            NormalOnAnimation = compositor.CreateScalarKeyFrameAnimation();
            NormalOnAnimation.InsertKeyFrame(1.0f, 1.0f /* opaque */);
            NormalOnAnimation.Duration = NormalTime;

            SlowOffAnimation = compositor.CreateScalarKeyFrameAnimation();
            SlowOffAnimation.InsertKeyFrame(1.0f, 0.0f /* transparent */);
            SlowOffAnimation.Duration = SlowTime;
        }

        public static void Uninitialize()
        {
            if (SlowOffAnimation != null)
            {
                SlowOffAnimation.Dispose();
                SlowOffAnimation = null;
            }

            if (NormalOnAnimation != null)
            {
                NormalOnAnimation.Dispose();
                NormalOnAnimation = null;
            }
        }

        public static ScalarKeyFrameAnimation NormalOnAnimation;
        public static ScalarKeyFrameAnimation SlowOffAnimation;

        public static readonly TimeSpan NormalTime = TimeSpan.FromMilliseconds(800);
        public static readonly TimeSpan SlowTime = TimeSpan.FromMilliseconds(1400);
    }
}
