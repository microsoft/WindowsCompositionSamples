//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

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
