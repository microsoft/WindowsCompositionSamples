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
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using CompositionSampleGallery.Shared;

namespace CompositionSampleGallery
{
    public sealed partial class Photos : SamplePage
    {
        public static string    StaticSampleName => "Layout Animations";
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Animate Layout updates using Implicit animations. Resize the window to see pictures animate to their new location."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868999";

        Compositor _compositor;
        ImplicitAnimationCollection _elementImplicitAnimation;
        public LocalDataSource Model { set; get; }

        public Photos()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            Model = new LocalDataSource();

            // Create ImplicitAnimations Collection. 
            _elementImplicitAnimation = _compositor.CreateImplicitAnimationCollection();

            //Define trigger and animation that should play when the trigger is triggered. 
            _elementImplicitAnimation["Offset"] = createOffsetAnimation();

            gridView.ItemsSource = Model.AggregateDataSources(new ObservableCollection<Thumbnail>[] { Model.Landscapes, Model.Nature });

        }

        private void gridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
            if (args.InRecycleQueue)
            {
                elementVisual.ImplicitAnimations = null;
            }
            else
            {
                //Add implicit animation to each visual 
                elementVisual.ImplicitAnimations = _elementImplicitAnimation;
            }
        }

        #region Animation

        private CompositionAnimationGroup createOffsetAnimation()
        {

            //Define Offset Animation for the ANimation group
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(.4);
            
            //Define Animation Target for this animation to animate using definition. 
            offsetAnimation.Target = "Offset";

            //Define Rotation Animation for Animation Group. 
            ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.InsertKeyFrame(.5f, 0.160f);
            rotationAnimation.InsertKeyFrame(1f, 0f);
            rotationAnimation.Duration = TimeSpan.FromSeconds(.4);

            //Define Animation Target for this animation to animate using definition. 
            rotationAnimation.Target = "RotationAngle";

            //Add Animations to Animation group. 
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);
            animationGroup.Add(rotationAnimation);

            return animationGroup;
        }
        #endregion


    }
}
