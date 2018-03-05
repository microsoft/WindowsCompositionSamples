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

using SamplesCommon;
using System;
using Windows.UI;
using Windows.UI.Composition;

namespace CompositionSampleGallery
{
    public sealed partial class BackDropSample : SamplePage
    {
        public BackDropSample()
        {
            this.InitializeComponent();

            var compositor = backDrop.VisualProperties.Compositor;
            var blurAnim = compositor.CreateScalarKeyFrameAnimation();
            blurAnim.Duration = TimeSpan.FromSeconds(10);
            blurAnim.InsertKeyFrame(0.0f, 0);
            blurAnim.InsertKeyFrame(0.5f, (float)backDrop.BlurAmount); // animate around the specified value
            blurAnim.InsertKeyFrame(1.0f, 0);
            blurAnim.IterationBehavior = AnimationIterationBehavior.Forever;

            backDrop.VisualProperties.StartAnimation(BackDrop.BlurAmountProperty, blurAnim);

            var colorAnim = compositor.CreateColorKeyFrameAnimation();
            var linearEasing = compositor.CreateLinearEasingFunction();
            colorAnim.Duration = TimeSpan.FromSeconds(5);
            colorAnim.InsertKeyFrame(0.0f, Colors.Transparent, linearEasing);
            colorAnim.InsertKeyFrame(0.05f, backDrop.TintColor, linearEasing);
            colorAnim.InsertKeyFrame(1.0f, Colors.Transparent, linearEasing);
            colorAnim.IterationBehavior = AnimationIterationBehavior.Forever;

            backDrop.VisualProperties.StartAnimation(BackDrop.TintColorProperty, colorAnim);
        }

        public static string        StaticSampleName => "BackDrop Control Sample"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to create your own custom BackDrop UserControl that provides Blur and Tint properties that can be animated with Composition Animations."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868994"; 
    }
}
