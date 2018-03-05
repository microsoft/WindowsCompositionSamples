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
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery
{
    public sealed partial class Curtain : SamplePage
    {
        private Visual _image;
        private Visual _root;
        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;
        public Curtain()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Curtain"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to provide custom inertia expressions in response to touch input. Select a motion and swipe up with touch to see how the UI reacts."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868996"; 

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _image = ElementCompositionPreview.GetElementVisual(Cover);
            _root = ElementCompositionPreview.GetElementVisual(Root);
            _compositor = _image.Compositor;

            ConfigureInteractionTracker();
        }

        private void ConfigureInteractionTracker()
        {
            _tracker = InteractionTracker.Create(_compositor);

            _interactionSource = VisualInteractionSource.Create(_root);
            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly; 
            _tracker.InteractionSources.Add(_interactionSource);        
            _tracker.MaxPosition = new Vector3(0, (float)Root.ActualHeight, 0);

            //
            // Use the Tacker's Position (negated) to apply to the Offset of the Image.
            //

            _image.StartAnimation("Offset", -_tracker.GetReference().Position);
        }

        private void ActivateSpringForce()
        {
            var dampingConstant = 5;
            var springConstant = 20;

            var modifier = InteractionTrackerInertiaMotion.Create(_compositor);

            // Set the condition to true (always)
            modifier.SetCondition((BooleanNode)true);

            // Define a spring-like force, anchored at position 0.
            var target = ExpressionValues.Target.CreateInteractionTrackerTarget();
            modifier.SetMotion((-target.Position.Y * springConstant) - (dampingConstant * target.PositionVelocityInPixelsPerSecond.Y));

            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }

        private void ActivateGravityForce()
        {
            //
            // Setup the gravity+bounce inertia modifier.
            //

            var target = ExpressionValues.Target.CreateInteractionTrackerTarget();
            var posY = target.Position.Y;
            var velY = target.PositionVelocityInPixelsPerSecond.Y;

            //
            // Gravity Force
            // 

            // Adding a factor of 100 since -9.8 pixels / second ^2 is not very fast.
            var gravity = -9.8f * 100;

            //
            // Floor Force
            //
            // This is the force that resists gravity and causes the bouncing. It's defined as:
            // 1. Zero if above the floor,
            // 2. Equal and opposite to gravity if "on" the floor (1 pixel above floor or below),
            // 3. The effects of 2., plus a reflective force if the direction of motion is still downward and the tracker is below the floor.
            //        This force is at its strongest (-1.8 * 100 * V) if the tracker is 5 or more
            //        pixels below floor, and weakest (-1.0 * 100 * V) if the tracker is "at" the
            //        floor.
            //

            // The amount the tracker is below the floor, capped to at most 5 below.

            var belowFloor = EF.Clamp(0, 0 - posY, 5);

            // The time slice our force engine uses.
            float dt = .01f;

            //
            // Defining bounce constants.
            // -2 would cause perfectly inellastic reflection, bouncing as high as it fell from.
            // -1 would cause perfectly ellastic reflection, freezing motion.
            // We want some bounce, but we also want the bouncing to decay, so choose
            // bounce factors between -1 and -2.
            //
            // Also, divide by the time slice width to make this reflective force apply entirely
            // all at once.
            //
            var weakestBounce = -1.1f / dt;
            var strongestBounce = -1.8f / dt;

            var floorForceExpression = EF.Conditional(posY < 1,
                                                      -gravity, 
                                                      0) + 
                                       EF.Conditional(EF.And(velY < 0f, posY < 0f), 
                                                      EF.Lerp(weakestBounce, strongestBounce, belowFloor/5) * velY, 
                                                      0);

            //
            // Apply the forces to the modifier
            //
            var modifier = InteractionTrackerInertiaMotion.Create(_compositor);
            modifier.SetCondition((BooleanNode)true);
            modifier.SetMotion(gravity + floorForceExpression);
            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });
        }

        private void ClearInertiaModifiers()
        {
            _tracker.ConfigurePositionYInertiaModifiers(null);
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                // Tell the system to use the gestures from this pointer point (if it can).
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
            }
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tracker != null)
            {
                switch (((ListBox)sender).SelectedIndex)
                {
                    case 0:
                        ClearInertiaModifiers();
                        break;

                    case 1:
                        ActivateSpringForce();
                        break;

                    case 2:
                        ActivateGravityForce();
                        break;

                }
            }
        }
    }
}
