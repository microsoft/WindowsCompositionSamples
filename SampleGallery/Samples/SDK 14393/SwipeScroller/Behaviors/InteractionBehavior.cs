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

using ExpressionBuilder;
using Microsoft.Xaml.Interactivity;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery.Samples.SDK_14393.SwipeScroller.Behaviors
{
    class InteractionBehavior : DependencyObject, IInteractionTrackerOwner, IBehavior
    {
        #region setup

        public FrameworkElement InfoContent
        {
            get { return (FrameworkElement)GetValue(InfoContentProperty); }
            set { SetValue(InfoContentProperty, value); }
        }

        public static readonly DependencyProperty InfoContentProperty =
            DependencyProperty.Register("InfoContent", typeof(FrameworkElement), typeof(InteractionBehavior), new PropertyMetadata(null));

        public FrameworkElement InfoContainer
        {
            get { return (FrameworkElement)GetValue(InfoContainerProperty); }
            set { SetValue(InfoContainerProperty, value); }
        }

        public static readonly DependencyProperty InfoContainerProperty =
            DependencyProperty.Register("InfoContainer", typeof(FrameworkElement), typeof(InteractionBehavior), new PropertyMetadata(null));

        public FrameworkElement PhotoContent
        {
            get { return (FrameworkElement)GetValue(PhotoContentProperty); }
            set { SetValue(PhotoContentProperty, value); }
        }

        public static readonly DependencyProperty PhotoContentProperty =
            DependencyProperty.Register("PhotoContent", typeof(FrameworkElement), typeof(InteractionBehavior), new PropertyMetadata(null));

        public FrameworkElement HittestContent
        {
            get { return (FrameworkElement)GetValue(HittestContentProperty); }
            set { SetValue(HittestContentProperty, value); }
        }

        public static readonly DependencyProperty HittestContentProperty =
            DependencyProperty.Register("HittestContent", typeof(FrameworkElement), typeof(InteractionBehavior), new PropertyMetadata(null, OnHittestContentChanged));

        private void ImageContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConfigureInteractionTracker();
        }
        #endregion
        private void ConfigureInteractionTracker()
        {
            if (InfoContent == null) return;
            if (HittestContent == null) return;

            // Configure hittestVisual size for the interaction (size needs to be explicitly set in order for the hittesting to work correctly due to XAML-COMP interop policy)
            _hittestVisual.Size = new Vector2((float)HittestContent.ActualWidth, (float)HittestContent.ActualHeight);

            _props = _compositor.CreatePropertySet();

            Visual infoVisual = ElementCompositionPreview.GetElementVisual(InfoContent);
            Visual photoVisual = ElementCompositionPreview.GetElementVisual(PhotoContent);
            photoVisual.Size = new Vector2((float)PhotoContent.ActualWidth, (float)PhotoContent.ActualHeight);

            photoVisual.CenterPoint = new Vector3((float)PhotoContent.ActualWidth * .5f, (float)PhotoContent.ActualHeight * .5f, 0f);
            infoVisual.CenterPoint = new Vector3((float)InfoContent.ActualWidth * .5f, (float)InfoContent.ActualHeight * .5f, 0f);

            VisualInteractionSource interactionSource = VisualInteractionSource.Create(_hittestVisual);

            //Configure for y-direction panning
            interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;

            //Create tracker and associate interaction source
            _tracker.InteractionSources.Add(interactionSource);

            //Configure tracker boundaries
            _tracker.MaxPosition = new Vector3((float)HittestContent.ActualHeight);

            SpriteVisual shadowVisual = _compositor.CreateSpriteVisual();
            shadowVisual.Size = photoVisual.Size;
            ElementCompositionPreview.SetElementChildVisual(InfoContainer, shadowVisual);

            ConfigureAnimations(photoVisual, infoVisual, shadowVisual);

            ConfigureRestingPoints();

            HittestContent.PointerPressed += (s, a) =>
            {
                // Capture the touch manipulation to the InteractionTracker for automatic handling
                if (a.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    try
                    {
                        interactionSource.TryRedirectForManipulation(a.GetCurrentPoint(s as UIElement));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignoring the failed redirect to prevent app crashing
                    }
                }
                // Update the InteractionTracker's position to change between min and max for other input types
                else
                {
                    float direction = 1;
                    if (_isExpanded)
                    {
                        direction = -1;
                    }
                    _tracker.TryUpdatePositionWithAdditionalVelocity(new Vector3(0f, direction*1000f, 0f));
                }
            };
        }

        private void ConfigureRestingPoints()
        {
            // Setup a possible inertia endpoint (snap point) for the InteractionTracker's minimum position
            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(_compositor);

            // Use this endpoint when the natural resting position of the interaction is less than the halfway point 
            var trackerTarget = ExpressionValues.Target.CreateInteractionTrackerTarget();
            endpoint1.SetCondition(trackerTarget.NaturalRestingPosition.Y < (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);

            // Set the result for this condition to make the InteractionTracker's y position the minimum y position
            endpoint1.SetRestingValue(trackerTarget.MinPosition.Y);

            // Setup a possible inertia endpoint (snap point) for the InteractionTracker's maximum position
            var endpoint2 = InteractionTrackerInertiaRestingValue.Create(_compositor);

            //Use this endpoint when the natural resting position of the interaction is more than the halfway point 
            endpoint2.SetCondition(trackerTarget.NaturalRestingPosition.Y >= (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);

            //Set the result for this condition to make the InteractionTracker's y position the maximum y position
            endpoint2.SetRestingValue(trackerTarget.MaxPosition.Y);

            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { endpoint1, endpoint2 });
        }

        private void ConfigureAnimations(Visual photoVisual, Visual infoVisual, SpriteVisual shadowVisual)
        {
            // Create a drop shadow to be animated by the manipulation
            var shadow = _compositor.CreateDropShadow();
            shadowVisual.Shadow = shadow;
            _props.InsertScalar("progress", 0);

            // Create an animation that tracks the progress of the manipulation and stores it in a the PropertySet _props
            var trackerNode = _tracker.GetReference();
            _props.StartAnimation("progress", trackerNode.Position.Y / trackerNode.MaxPosition.Y);

            // Create an animation that scales up the infoVisual based on the manipulation progress
            var propSetProgress = _props.GetReference().GetScalarProperty("progress");
            infoVisual.StartAnimation("Scale", EF.Vector3(1, 1, 1) * EF.Lerp(1, 1.2f, propSetProgress));

            // Create an animation that changes the offset of the photoVisual and shadowVisual based on the manipulation progress
            var photoOffsetExp = -120f * _props.GetReference().GetScalarProperty("Progress");
            photoVisual.StartAnimation("offset.y", photoOffsetExp);
            shadowVisual.StartAnimation("offset.y", photoOffsetExp);

            // Create an animation that fades in the info visual based on the manipulation progress
            infoVisual.StartAnimation("opacity", EF.Lerp(0, 1, _props.GetReference().GetScalarProperty("Progress")));
            shadow.StartAnimation("blurradius", EF.Lerp(1, 50, _props.GetReference().GetScalarProperty("Progress")));
        }

        private static void OnHittestContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                var thisBehavior = d as InteractionBehavior;

                if (thisBehavior != null)
                {
                    var oldElement = e.OldValue as FrameworkElement;
                    var newElement = e.NewValue as FrameworkElement;

                    if (oldElement != null)
                    {
                        oldElement.SizeChanged -= thisBehavior.ImageContainerSizeChanged;
                    }

                    if (newElement != null)
                    {
                        newElement.SizeChanged += thisBehavior.ImageContainerSizeChanged;
                        thisBehavior.ConfigureInteractionTracker();
                    }
                }
            }
        }

        #region Callbacks
        public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
        {
            
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
            
        }

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            
        }

        public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
        {
            
        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {
            // Store whether the item is expanded in order to know whether a mouse click should expand or collapse 
            _isExpanded = (args.Position.Y > 0); 
        }
        #endregion //Callbacks

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            _hittestVisual = ElementCompositionPreview.GetElementVisual(HittestContent);

            if (_compositor == null)
            {
                _compositor = _hittestVisual.Compositor;
            }
            _tracker = InteractionTracker.CreateWithOwner(_compositor, this);
            _isExpanded = false;
        }

        public void Detach()
        {
            _tracker.Dispose();
            _tracker = null;
            _hittestVisual = null;
            _compositor = null;
            _props = null;
        }

        protected DependencyObject AssociatedObject { get; set; }

        DependencyObject IBehavior.AssociatedObject
        {
            get { return this.AssociatedObject; }
        }

        InteractionTracker _tracker;
        CompositionPropertySet _props;
        Compositor _compositor;
        Visual _hittestVisual;
        bool _isExpanded;
    }
}
