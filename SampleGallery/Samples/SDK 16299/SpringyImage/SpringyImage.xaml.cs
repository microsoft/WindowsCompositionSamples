//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using ExpressionBuilder;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

using EF = ExpressionBuilder.ExpressionFunctions;
using EV = ExpressionBuilder.ExpressionValues;

namespace CompositionSampleGallery
{

    public sealed partial class SpringyImage : SamplePage
    {
        public SpringyImage()
        {
            this.InitializeComponent();
            Setup();
        }

        public static string    StaticSampleName => "Springy Image"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Animate the Scale property of a Visual with a Spring-based Animation."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868957";

        private void Setup()
        {
            // Grab the existing Compositor for this page
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Define the Spring Animation that will be used to animate images
            // Create a SpringVector3Animation since we will be animating Scale
            _springAnimation = _compositor.CreateSpringVector3Animation();
            _springAnimation.DampingRatio = 0.65f;
            _springAnimation.Period = TimeSpan.FromSeconds(.05);

            // Track if an image has been clicked, initially set to false
            _imageClickScaleMode = false;

            // Setup the Expressions to manage list layout
            SetupImageList();
        }

        private void SetupImageList()
        {                        
            // Note: Translation is an additive value to the Offset property when defining the final position of a backed visual of a UIElement            
            
            // This Expression defines an equation that changes the Y Translation of an image based on the scale of its vertical neighbor image.
            // As the scale of an image grows, the images below it will dynamically move down.
            // The equation is built by summing up two different deltas caused by a scale animation:
            //    1) The delta caused by the image directly above the target scaling to a value > 1 (should have no effect if scale == 1)
            //    2) Any additional delta caused by if the image above has translated down because the image above it (or higher up) has scaled up
            //        [Will either be a large scale from Click, or medium scale if on hover]
            // Note: 120 represents the height of the images + the gap between them
            
            var visualPlaceHolder = EV.Reference.CreateVisualReference("visual");
            var factor = EV.Constant.CreateConstantScalar("factor");            
            _translationDeltaExp = ((visualPlaceHolder.Scale.Y - 1) * (float)image.Height) +
                                 EF.Conditional(
                                     ((visualPlaceHolder.Translation.Y / (120 * factor)) > 1), 
                                     (EV.Constant.CreateConstantScalar("largeScaleDiff")), 
                                     visualPlaceHolder.Translation.Y % (120 * factor)
                                     );
                                        
            // Activate Translation property for Visuals backing a UIElement
            TranslationSetup(image);
            TranslationSetup(image2);
            TranslationSetup(image3);
            TranslationSetup(image4);

            // Setup the Expression on the Y Translation property for images 2 - 4. 
            // Pass in the target image that the Expression gets applied to, the image above it for reference and images indexed position
            // Note: Since the first image does not have an image above it, do not need to apply the Expression to it
            StartAnimationHelper(image2, image, 1);
            StartAnimationHelper(image3, image2, 2);
            StartAnimationHelper(image4, image3, 3);
        }

        private void TranslationSetup(UIElement element)
        {
            // Activate Translation property on the UIElement images
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Properties.InsertVector3("Translation", new Vector3(0));
        }

        private void StartAnimationHelper(UIElement targetImage, UIElement imageAbove, int indexFactor)
        {
            // Retrieve references to the target Visual of the Expression along with the vertical neighbor we need to reference
            var referenceVisual = ElementCompositionPreview.GetElementVisual(imageAbove);
            var targetVisual = ElementCompositionPreview.GetElementVisual(targetImage);

            // Set the references to the Expression and Start it on the target Visual
            _translationDeltaExp.SetReferenceParameter("visual", referenceVisual);
            _translationDeltaExp.SetScalarParameter("factor", indexFactor);
            _translationDeltaExp.SetScalarParameter("largeScaleDiff", 2 * (float)image.Height);
            targetVisual.StartAnimation("Translation.Y", _translationDeltaExp);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            float dampInputResult;
            TimeSpan periodInputResult;

            // Check input and assign if valid
            if (float.TryParse(DampingInput.Text, out dampInputResult))
            {
                _springAnimation.DampingRatio = dampInputResult;
            }
            if (TimeSpan.TryParse(PeriodInput.Text, out periodInputResult))
            {
                _springAnimation.Period = periodInputResult;
            }
        }

        private void image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Animating the image only if not been previously clicked
            if (!_imageClickScaleMode)
            {
                _springAnimation.FinalValue = new Vector3(1.5f);
                var visual = ElementCompositionPreview.GetElementVisual((UIElement)sender);
                visual.StartAnimation("Scale", _springAnimation);
            }
        }

        private void image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Animating the image only if not been previously clicked
            if (!_imageClickScaleMode)
            {
                _springAnimation.FinalValue = new Vector3(1f);
                var visual = ElementCompositionPreview.GetElementVisual((UIElement)sender);
                visual.StartAnimation("Scale", _springAnimation);
            }
        }

        private void image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_imageClickScaleMode)
            {
                // If not previously clicked, then scale to large size
                _imageClickScaleMode = true;
                _springAnimation.FinalValue = new Vector3(3.0f);
            }
            else
            {
                // Else scale to original size
                _imageClickScaleMode = false;
                _springAnimation.FinalValue = new Vector3(1f);
            }
            var visual = ElementCompositionPreview.GetElementVisual((UIElement)sender);
            visual.StartAnimation("Scale", _springAnimation);
        }

        private Compositor _compositor;
        private SpringVector3NaturalMotionAnimation _springAnimation;
        private ExpressionNode _translationDeltaExp;
        private bool _imageClickScaleMode;
    }
}
