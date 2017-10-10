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
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class FlipToReveal : SamplePage
    {
        public FlipToReveal()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Flip to reveal"; 
        public override string      SampleName => StaticSampleName;
        public static string        StaticSampleDescription => "Demonstrates how to use Animations and Transforms to create a flip effect. Tap on the tile to reveal the image underneath. Tap again to see it flip back.";
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761175"; 


        private Boolean IsFlipped = false;

        private void MainPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(CaptionTile);
            Compositor compositor = visual.Compositor;

            visual.Size = new Vector2((float)CaptionTile.Width / 2, (float)CaptionTile.Height / 2);

            // Rotate around the X-axis
            visual.RotationAxis = new Vector3(1, 0, 0);

            // Start the rotation animation
            LinearEasingFunction linear = compositor.CreateLinearEasingFunction();
            ScalarKeyFrameAnimation rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
            if (!IsFlipped) // default
            {
                rotationAnimation.InsertKeyFrame(0, 0, linear);
                rotationAnimation.InsertKeyFrame(1, 250f, linear); // flip over
            }
            else
            {
                rotationAnimation.InsertKeyFrame(0, 250f, linear);
                rotationAnimation.InsertKeyFrame(1, 0f, linear);   // flip back

            }
            rotationAnimation.Duration = TimeSpan.FromMilliseconds(800);

            var transaction = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            transaction.Completed += Animation_Completed;

            // we want the CaptionTile visible as it flips back
            if(IsFlipped)
                CaptionTile.Visibility = Windows.UI.Xaml.Visibility.Visible;

            visual.StartAnimation("RotationAngleInDegrees", rotationAnimation);

            transaction.End();
        }

        private void Animation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            IsFlipped = !IsFlipped;

            // we want the CaptionTile invisible once flipped over
            if(IsFlipped)
                CaptionTile.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            
        }

        private void UpdatePerspective()
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(MainPanel);

            // Get the size of the area we are enabling perspective for
            Vector2 sizeList = new Vector2((float)MainPanel.ActualWidth, (float)MainPanel.ActualHeight);

            // Setup the perspective transform.
            Matrix4x4 perspective = new Matrix4x4(
                        1.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 1.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 1.0f, -1.0f / sizeList.X,
                        0.0f, 0.0f, 0.0f, 1.0f);

            // Set the parent transform to apply perspective to all children
            visual.TransformMatrix =
                               Matrix4x4.CreateTranslation(-sizeList.X / 2, -sizeList.Y / 2, 0f) *      // Translate to origin
                               perspective *                                                            // Apply perspective at origin
                               Matrix4x4.CreateTranslation(sizeList.X / 2, sizeList.Y / 2, 0f);         // Translate back to original position

        }

        private void MainPanel_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            UpdatePerspective();
        }
    }
}
