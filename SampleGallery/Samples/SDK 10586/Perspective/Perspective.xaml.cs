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
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class Perspective : SamplePage
    {
        public Perspective()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Perspective"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to apply and animate perspective to Composition Visuals."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868943"; 

        private void SamplePage_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // Example 1 - Animate a visual's Z position towards the vanishing point, away from the
            //             view position.
            //

            Visual visual = ElementCompositionPreview.GetElementVisual(TextBlock1);
            Compositor compositor = visual.Compositor;

            // Animate the Z offset towards the vanishing point and back again
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0.0f, new Vector3(0, 0, 0));
            offsetAnimation.InsertKeyFrame(0.5f, new Vector3(0, 0, -2000));
            offsetAnimation.InsertKeyFrame(1.0f, new Vector3(0, 0, 0));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(8000);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            visual.StartAnimation("Offset", offsetAnimation);


            //
            // Example 2 - Rotate a rectangular image with perspective applied.
            //

            visual = ElementCompositionPreview.GetElementVisual(RotatedImage);
            
            // Rotate around the center
            visual.Size = new Vector2((float)RotatedImage.Width / 2, (float)RotatedImage.Height / 2);
            visual.CenterPoint = new Vector3(visual.Size.X, visual.Size.Y, 0f);

            // Rotate around the Y-axis
            visual.RotationAxis = new Vector3(0, 1, 0);

            // Start the rotation animation
            LinearEasingFunction linear = compositor.CreateLinearEasingFunction();
            ScalarKeyFrameAnimation rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(0, 0, linear);
            rotationAnimation.InsertKeyFrame(1, 360f, linear);
            rotationAnimation.Duration = TimeSpan.FromMilliseconds(8000);
            rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            visual.StartAnimation("RotationAngleInDegrees", rotationAnimation);

            // Animate up and down to show differences in perspective relative to the vanishing point.
            offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0.0f, new Vector3(0,    0, -200f));
            offsetAnimation.InsertKeyFrame(0.5f, new Vector3(0, 300f, -200f));
            offsetAnimation.InsertKeyFrame(1.0f, new Vector3(0,    0, -200f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(8000);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            visual.StartAnimation("Offset", offsetAnimation);
        }

    }
}
