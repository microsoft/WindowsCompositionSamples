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

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.Numerics;

namespace CompositionSampleGallery
{

    public sealed partial class OffsetStompingFix : SamplePage
    {

        public OffsetStompingFix()
        {
            InitializeComponent();
            StartAnimationTranslation();
            StartAnimationOffset();
            SetupImplicitAnimations();
            
        }
        public static string    StaticSampleName => "Using Translation Instead of Offset"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "By enabling the new Translation property via ElementCompositionPreview you animate a Visual's position without using the Offset property shared with the Xaml Framework."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868950";

        //Green rectangle animation using the new Translation property inserted into the Visual's PropertySet
        void StartAnimationTranslation()
        {

            //Get the compositor from the current Window. This is new in the Creator's Update.
            var compositor = Window.Current.Compositor;

            var animation = compositor.CreateScalarKeyFrameAnimation();

            //Enable The new Translation property
            ElementCompositionPreview.SetIsTranslationEnabled(Rectangle1, true);

            //now intialize the Value of Translation in the Popertyset to Zero for first use to avoid timing issues. This ensures that
            //the property is ready for use immediately.

            var rect1VisaulPropertySet = ElementCompositionPreview.GetElementVisual(Rectangle1).Properties;
            rect1VisaulPropertySet.InsertVector3("Translation", Vector3.Zero);

            //Animation   
            animation.InsertExpressionKeyFrame(0, "this.StartingValue");
            animation.InsertExpressionKeyFrame(0.5f, "this.StartingValue - 120");
            animation.InsertExpressionKeyFrame(1, "this.StartingValue");
            animation.StopBehavior = AnimationStopBehavior.SetToInitialValue;
            animation.IterationBehavior = AnimationIterationBehavior.Forever;
            animation.Duration = TimeSpan.FromSeconds(1.5);

            StartAnimation1(Rectangle1, animation);

        }

        //Yellow Rectangle and Red Rectangle Animations using Offset as in Aniversary Update
        void StartAnimationOffset()
        {
            var compositor = Window.Current.Compositor;

            //Animation
            var animation2 = compositor.CreateScalarKeyFrameAnimation();


            animation2.InsertExpressionKeyFrame(0, "this.StartingValue");
            animation2.InsertExpressionKeyFrame(0.5f, "this.StartingValue - 120");
            animation2.InsertExpressionKeyFrame(1, "this.StartingValue");

            animation2.StopBehavior = AnimationStopBehavior.SetToInitialValue;
            animation2.IterationBehavior = AnimationIterationBehavior.Forever;
            animation2.Duration = TimeSpan.FromSeconds(1.5);


            StartAnimation2(Rectangle2, animation2);
            StartAnimation2(Rectangle3, animation2);

        }


        private void StartAnimation1(UIElement element, KeyFrameAnimation animation)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);

            visual.StopAnimation("Translation.Y");
            visual.StartAnimation("Translation.Y", animation);
        }

        private void StartAnimation2(UIElement element, KeyFrameAnimation animation2)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.StopAnimation("Offset.Y");
            visual.StartAnimation("Offset.Y", animation2);
        }

     //Implicit animation driven by offset used to tilt all the rects when grid resized.
        private void SetupImplicitAnimations()
        {
            //set up each rect for a tilt animation
            var visual1 = ElementCompositionPreview.GetElementVisual(Rectangle1);
            //Note - we need to get the backing Visual for Border1 instead of the rectangle because of the workaround to hide from Xaml Layout
            var visual2 = ElementCompositionPreview.GetElementVisual(Border1);
            var visual3 = ElementCompositionPreview.GetElementVisual(Rectangle2);
            var compositor = Window.Current.Compositor;

            //Define the implicit collection
            var tiltAnimations = compositor.CreateImplicitAnimationCollection();

            //Define the tilt rotation
            var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(.4f, .2f);
            rotationAnimation.InsertKeyFrame(1f, 0f);
            rotationAnimation.Duration = TimeSpan.FromSeconds(.4);
            rotationAnimation.Target = "RotationAngle";

            //Use Offset as the trigger. By sharing the Visual Offset to allow for triggers we now see
            //why stomping occurs, Xaml layout uses the property when doing a window resize, making Offset a great implicit animation trigger,
            //but invalidates it as an explicit animation target.

            tiltAnimations.Add("Offset", rotationAnimation);

            //Tilt animations
            visual1.ImplicitAnimations = tiltAnimations;
            visual2.ImplicitAnimations = tiltAnimations;
            visual3.ImplicitAnimations = tiltAnimations;
        }
        //Button click to restart the explicit animations all rectangles
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartAnimationOffset();
            StartAnimationTranslation();
        }
        
        

    }
}
