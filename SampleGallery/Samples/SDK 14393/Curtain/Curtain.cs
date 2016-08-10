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

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.ViewManagement;
using Windows.UI.Composition.Interactions;

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

        public static string        StaticSampleName    { get { return "Curtain"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Demonstrates how to provide custom interia expressions in response to touch input. Select a motion and swipe up with touch to see how the UI reacts. \n\n Known Issue: The ListBox can only be changed with mouse input."; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=784885"; } }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _image = ElementCompositionPreview.GetElementVisual(Clock);
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

            var positionExpression = _compositor.CreateExpressionAnimation("-tracker.Position");
            positionExpression.SetReferenceParameter("tracker", _tracker);

            _image.StartAnimation("Offset", positionExpression);
        }

        private void ActivateSpringForce()
        {
            var dampingConstant = 5;
            var springConstant = 20;

            var modifier = InteractionTrackerInertiaMotion.Create(_compositor);

            // Set the condition to true (always)
            modifier.Condition = _compositor.CreateExpressionAnimation("true");

            // Define a spring-like force, anchored at position 0.
            modifier.Motion = _compositor.CreateExpressionAnimation(@"(-(this.target.Position.Y) * springConstant) - (dampingConstant * this.target.PositionVelocityInPixelsPerSecond.Y)");

            modifier.Motion.SetScalarParameter("dampingConstant", dampingConstant);
            modifier.Motion.SetScalarParameter("springConstant", springConstant);

            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }

        private void ActivateGravityForce()
        {
            var gravity = -700;
            var bounce = -150f;

            var modifier = InteractionTrackerInertiaMotion.Create(_compositor);

            modifier.Condition = _compositor.CreateExpressionAnimation("true");

            modifier.Motion = _compositor.CreateExpressionAnimation(
                "(this.target.Position.Y > 0) ? (gravity) :" +
                    "((this.target.PositionVelocityInPixelsPerSecond.Y <= 0) ?" +
                        "(bounce * this.target.PositionVelocityInPixelsPerSecond.Y) : (0))");

            modifier.Motion.SetScalarParameter("gravity", gravity);
            modifier.Motion.SetScalarParameter("bounce", bounce);

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
