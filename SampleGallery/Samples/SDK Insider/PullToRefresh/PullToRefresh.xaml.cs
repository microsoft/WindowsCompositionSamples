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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.System;


namespace CompositionSampleGallery
{
    public sealed partial class PullToRefresh : SamplePage
    {
        private Visual _contentPanelVisual;
        private Visual _root;
        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;
        private Windows.UI.Core.CoreWindow _Window;
        private static Size ControlSize = new Size(500, 500);
        private ExpressionAnimation m_positionExpression;
        private ScalarKeyFrameAnimation _gearMotionScalarAnimation;
        
        public PullToRefresh()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
            _Window = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow;
        }


        public static string StaticSampleName { get { return "PullToRefresh ListView Items"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription { get { return "Demonstrates how to create a custom Pull-to-Refresh control using Interaction Tracker Source Modifiers"; } }
        public override string SampleCodeUri { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761169"; } }

        public LocalDataSource Model { set; get; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThumbnailList.ItemsSource = Model.Cities;
            _contentPanelVisual = ElementCompositionPreview.GetElementVisual(ContentPanel);
            _root = ElementCompositionPreview.GetElementVisual(Root);
            _compositor = _root.Compositor;
            ConfigureInteractionTracker();
            SetupAnimatingRefreshPanel();
        }


        private void SetupAnimatingRefreshPanel()
        {
            Visual loadingVisual = ElementCompositionPreview.GetElementVisual(FirstGear);
            loadingVisual.Size = new Vector2((float)FirstGear.ActualWidth, (float)FirstGear.ActualHeight);
            loadingVisual.AnchorPoint = new Vector2(0.5f, 0.5f);

            // Animate the refresh panel icon using a simple rotating key frame animation
            ScalarKeyFrameAnimation _loadingMotionScalarAnimation = _compositor.CreateScalarKeyFrameAnimation();
            var linear = _compositor.CreateLinearEasingFunction();

            _loadingMotionScalarAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
            _loadingMotionScalarAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 360", linear);
            _loadingMotionScalarAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            _loadingMotionScalarAnimation.Duration = TimeSpan.FromSeconds(2);

            loadingVisual.StartAnimation("RotationAngleInDegrees", _loadingMotionScalarAnimation);

        }


        private void ConfigureInteractionTracker()
        {
            _tracker = InteractionTracker.Create(_compositor);

            _interactionSource = VisualInteractionSource.Create(_root);

            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionYChainingMode = InteractionChainingMode.Always;

            _tracker.InteractionSources.Add(_interactionSource);
            float refreshPanelHeight = (float)RefreshPanel.ActualHeight;

            _tracker.MaxPosition = new Vector3((float)Root.ActualWidth, 0, 0);
            _tracker.MinPosition = new Vector3(-(float)Root.ActualWidth, -refreshPanelHeight, 0);

            //The PointerPressed handler needs to be added using AddHandler method with the handledEventsToo boolean set to "true"
            //instead of the XAML element's "PointerPressed=Window_PointerPressed",
            //because the list view needs to chain PointerPressed handled events as well. 
            ContentPanel.AddHandler(PointerPressedEvent, new PointerEventHandler(Window_PointerPressed), true);

            SetupPullToRefreshBehavior(refreshPanelHeight);

            //Apply spring force to pull the content back to Zero
            ConfigureRestingPoint(refreshPanelHeight);

            //
            // Use the Tacker's Position (negated) to apply to the Offset of the Image. The -{refreshPanelHeight} is to hide the refresh panel
            //
            m_positionExpression = _compositor.CreateExpressionAnimation($"-tracker.Position.Y - {refreshPanelHeight} ");
            m_positionExpression.SetReferenceParameter("tracker", _tracker);
            _contentPanelVisual.StartAnimation("Offset.Y", m_positionExpression);

        }

        private void Window_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                // Tell the system to use the gestures from this pointer point (if it can).
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(null));
            }
        }

        // Apply two sourcemodifiers to the input source: One to provide resistance, one to stop motion
        public void SetupPullToRefreshBehavior(
            float pullToRefreshDistance)
        {
            //
            // Modifier 1: Cut DeltaY to a third as long as the InteractionTracker is not yet at the 
            // pullRefreshDistance.
            //

            CompositionConditionalValue resistanceModifier = CompositionConditionalValue.Create(_compositor);

            ExpressionAnimation resistanceCondition = _compositor.CreateExpressionAnimation(
                $"-tracker.Position.Y < {pullToRefreshDistance}");

            resistanceCondition.SetReferenceParameter("tracker", _tracker);

            ExpressionAnimation resistanceAlternateValue = _compositor.CreateExpressionAnimation(
            "source.DeltaPosition.Y / 3");

            resistanceAlternateValue.SetReferenceParameter("source", _interactionSource);

            resistanceModifier.Condition = resistanceCondition;
            resistanceModifier.Value = resistanceAlternateValue;

            //
            // Modifier 2: Zero the delta if we are past the pullRefreshDistance. (So we can't pan 
            // past the pullRefreshDistance)
            //

            CompositionConditionalValue stoppingModifier = CompositionConditionalValue.Create(_compositor);
            ExpressionAnimation stoppingCondition = _compositor.CreateExpressionAnimation(
                            $"-tracker.Position.Y >= {pullToRefreshDistance}");
            stoppingCondition.SetReferenceParameter("tracker", _tracker);

            ExpressionAnimation stoppingAlternateValue = _compositor.CreateExpressionAnimation("0");

            stoppingModifier.Condition = stoppingCondition;
            stoppingModifier.Value = stoppingAlternateValue;


            //
            // Apply the modifiers to the source as a list
            //

            List<CompositionConditionalValue> modifierList =
            new List<CompositionConditionalValue>() { resistanceModifier, stoppingModifier
            };

            _interactionSource.ConfigureDeltaPositionYModifiers(modifierList);


        }

        private void ActivateSpringForce(float pullToRefreshDistance)
        {
            var dampingConstant = 5;
            var springConstant = 5;

            var modifier = InteractionTrackerInertiaMotion.Create(_compositor);

            // Set the condition to true (always)
            modifier.Condition = _compositor.CreateExpressionAnimation("true");

            // Define a spring-like force, anchored at position 0. This brings the listView back to position 0.
            modifier.Motion = _compositor.CreateExpressionAnimation(@"(-(this.target.Position.Y) * springConstant) - (dampingConstant * this.target.PositionVelocityInPixelsPerSecond.Y)");

            modifier.Motion.SetScalarParameter("dampingConstant", dampingConstant);
            modifier.Motion.SetScalarParameter("springConstant", springConstant);
            modifier.Motion.SetScalarParameter("pullToRefreshDistance", pullToRefreshDistance);

            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { modifier });
        }


        private void ConfigureRestingPoint(float pullToRefreshDistance)
        {
            // Setup a possible inertia endpoint (snap point) for the InteractionTracker's minimum position
            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(_compositor);

            // Use this endpoint when the natural resting position of the interaction is less than the halfway point 
            endpoint1.Condition = _compositor.CreateExpressionAnimation(
                    $"this.target.NaturalRestingPosition.y < {pullToRefreshDistance}");

            // Set the result for this condition to make the InteractionTracker's y position the minimum y position
            endpoint1.RestingValue = _compositor.CreateExpressionAnimation($"-this.target.MinPosition.y - {pullToRefreshDistance}");
            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { endpoint1 });
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
            System.Diagnostics.Debug.WriteLine("GridClip.Rect" + GridClip.Rect);
        }

    }
}
