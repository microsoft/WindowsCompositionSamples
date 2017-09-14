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
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.ViewManagement;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery
{
    public sealed partial class PullToAnimate : SamplePage
    {
        public PullToAnimate()
        {
            this.InitializeComponent();
            SizeChanged += PullToAnimate_SizeChanged;
        }

        private void PullToAnimate_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(_blurredBackgroundImageVisual != null && _backgroundVisualDimmer != null)
            {
                _blurredBackgroundImageVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);
                _backgroundVisualDimmer.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);
            }
        }

        public static string        StaticSampleName => "Pull To Animate"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to use InteractionTracker to create custom resting points with Animations. Pan with Touch or Precision Touchpad, or hit the toggle button to animate."; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/?LinkId=761166"; 

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(1792, 1008));
            _backgroundImageVisual = ElementCompositionPreview.GetElementVisual(BackgroundImage);
            _weatherLayerVisual = ElementCompositionPreview.GetElementVisual(WeatherLayer);
            _todoLayerVisual = ElementCompositionPreview.GetElementVisual(ToDoLayer);
            _calendarIconVisual = ElementCompositionPreview.GetElementVisual(CalendarIcon);
            _backgroundVisual = ElementCompositionPreview.GetElementVisual(Root);

            _compositor = _backgroundImageVisual.Compositor;
            _backgroundVisual.Clip = _compositor.CreateInsetClip(0, 0, 0, 0);


            _backgroundImageVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);
           

            _backgroundImageVisual.CenterPoint = new Vector3(
                                                    (float)Root.ActualWidth / 2.0f,
                                                    (float)Root.ActualHeight / 2.0f,
                                                    0);

            _calendarIconVisual.CenterPoint = new Vector3(
                                                (float)CalendarIcon.ActualWidth / 2.0f,
                                                (float)CalendarIcon.ActualHeight / 2.0f,
                                                0);

            _weatherLayerVisual.CenterPoint = new Vector3(
                                                (float)WeatherLayer.ActualWidth / 2.0f,
                                                -(float)WeatherLayer.ActualHeight,
                                                0);

            _todoLayerVisual.CenterPoint = new Vector3(
                                                (float)ToDoLayer.ActualWidth / 2.0f,
                                                (float)ToDoLayer.ActualHeight / 2.0f,
                                                0);

            _backgroundVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            _backgroundVisualDimmer = _compositor.CreateSpriteVisual();
            _backgroundVisualDimmer.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Black);
            _backgroundVisualDimmer.Size = _backgroundVisual.Size;

            ElementCompositionPreview.SetElementChildVisual(BackgroundImageOverlay, _backgroundVisualDimmer);


            //
            // Store the state 1 and 2 values.  We'll blend between them later.
            //

            _visualState1 = new VisualState
            {
                BackgroundBlurAmount = 1.0f,
                BackgroundScale = new Vector3(1.0f, 1.0f, 1.0f),
                CalendarIconOffset = new Vector3(0, -50, 0),
                CalendarIconScale = new Vector3(0.7f, 0.7f, 1.0f),
                ToDoLayerBlurAmount = 0.3f,
                ToDoLayerOpacity = 0.0f,
                ToDoLayerScale = new Vector3(0.8f, 0.8f, 1.0f),
                WeatherLayerBlurAmount = 0.0f,
                WeatherLayerOpacity = 3.0f,
                WeatherLayerScale = new Vector3(1.0f),
            };

            _visualState2 = new VisualState
            {
                BackgroundBlurAmount = 9.0f,
                BackgroundScale = new Vector3(1.11f, 1.11f, 1.0f),
                CalendarIconOffset = new Vector3(),
                CalendarIconScale = new Vector3(1.0f),
                ToDoLayerBlurAmount = 0.0f,
                ToDoLayerOpacity = 1.0f,
                ToDoLayerScale = new Vector3(1.0f),
                WeatherLayerBlurAmount = 0.3f,
                WeatherLayerOpacity = 0.0f,
                WeatherLayerScale = new Vector3(0.8f, 0.8f, 1.0f),
            };

            //
            // Store the interaction "progress" into a property set.  
            // We'll reference it later in all of the expressions.
            //

            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertScalar("progress", 0.0f);


            //
            // Create a "canned" key frame animation that we'll use to 
            // play when the toggle button is pressed.
            //

            _cannedProgressAnimation = _compositor.CreateVector3KeyFrameAnimation();
            _cannedProgressAnimation.Duration = TimeSpan.FromMilliseconds(1500);


            InitializeBlurVisuals();

            ConfigureInteractionTracker();

            ConfigureRestingPoints();

            ConfigureTransitionAnimations();

            TransitionToState1(false);
        }

        private void InitializeBlurVisuals()
        {
            var blurEffect = new GaussianBlurEffect()
            {
                Name = "blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };

            var blurBrush = _compositor.CreateEffectFactory(blurEffect,
                new[] { "blur.BlurAmount" })
                .CreateBrush();

            blurBrush.SetSourceParameter("source", _compositor.CreateBackdropBrush());

            _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();
            _blurredBackgroundImageVisual.Brush = blurBrush;
            _blurredBackgroundImageVisual.Size = new Vector2((float)Root.ActualWidth, (float)Root.ActualHeight);

            ElementCompositionPreview.SetElementChildVisual(BackgroundImageBlurPanel, _blurredBackgroundImageVisual);
        }

        private void ToggleState_Click(object sender, RoutedEventArgs e)
        {
            bool animate = true;

            if (_isState1)
            {
                TransitionToState2(animate);
            }
            else
            {
                TransitionToState1(animate);
            }
        }

        private void TransitionToState1(bool animate)
        {
            var minPosition = new Vector3();

            if (animate)
            {
                _cannedProgressAnimation.InsertKeyFrame(1.0f, minPosition);

                _tracker.TryUpdatePositionWithAnimation(_cannedProgressAnimation);
            }
            else
            {
                _tracker.TryUpdatePosition(minPosition);
            }

            _isState1 = true;
        }


        private void TransitionToState2(bool animate)
        {
            Vector3 maxPosition = new Vector3(0, _tracker.MaxPosition.Y, 0);

            if (animate)
            {
                _cannedProgressAnimation.InsertKeyFrame(1.0f, maxPosition);

                _tracker.TryUpdatePositionWithAnimation(_cannedProgressAnimation);
            }
            else
            {
                _tracker.TryUpdatePosition(maxPosition);
            }

            _isState1 = false;

        }

        private void ConfigureInteractionTracker()
        {
            _tracker = InteractionTracker.Create(_compositor);
            _tracker.MaxPosition = new Vector3(0, _backgroundImageVisual.Size.Y * 0.5f, 0);
            _tracker.MinPosition = new Vector3();

            _interactionSource = VisualInteractionSource.Create(_backgroundVisual);
            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _tracker.InteractionSources.Add(_interactionSource);

            var trackerNode = _tracker.GetReference();
            var progressExp = EF.Clamp(trackerNode.Position.Y / trackerNode.MaxPosition.Y, 0, 1);
            _propertySet.StartAnimation("progress", progressExp);
        }

        private void ConfigureRestingPoints()
        {
            var trackerTarget = ExpressionValues.Target.CreateInteractionTrackerTarget();

            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(_compositor);       
            endpoint1.SetCondition(trackerTarget.NaturalRestingPosition.Y < (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint1.SetRestingValue(trackerTarget.MinPosition.Y);

            var endpoint2 = InteractionTrackerInertiaRestingValue.Create(_compositor);
            endpoint2.SetCondition(trackerTarget.NaturalRestingPosition.Y >= (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint2.SetRestingValue(trackerTarget.MaxPosition.Y);

            _tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { endpoint1, endpoint2 });
        }

        private void ConfigureTransitionAnimations()
        {
            //
            // Create a common expression that blends between the start and end value, 
            // based on the tracker's progres (0 to 1).
            //

            var startNode = ExpressionValues.Constant.CreateConstantScalar("start");
            var endNode = ExpressionValues.Constant.CreateConstantScalar("end");
            var progress = _propertySet.GetReference().GetScalarProperty("progress");
            var blendExpression = EF.Lerp(startNode, endNode, progress);

            //
            // Apply the expression to the background image's blur amount.
            //

            blendExpression.SetScalarParameter("start", _visualState1.BackgroundBlurAmount);
            blendExpression.SetScalarParameter("end", _visualState2.BackgroundBlurAmount);

            _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blendExpression);

            //
            // Apply the expression to the background image's blur amount. Since the expression 
            // structure isn't changing, we can simply update the parameter values (i.e. placeholders) 
            // before starting the animation.  
            //

            blendExpression.SetVector3Parameter("start", _visualState1.BackgroundScale);
            blendExpression.SetVector3Parameter("end", _visualState2.BackgroundScale);

            _backgroundImageVisual.StartAnimation("scale", blendExpression);


            //
            // Apply the expression to the background image overlay's opacity, to dim the image.
            //

            blendExpression.SetScalarParameter("start", 0.0f);
            blendExpression.SetScalarParameter("end", 0.4f);

            _backgroundVisualDimmer.StartAnimation("opacity", blendExpression);


            //
            // Set up the Calendar icon to move up/down, using the common expression.
            //

            blendExpression.SetVector3Parameter("start", _visualState1.CalendarIconOffset);
            blendExpression.SetVector3Parameter("end", _visualState2.CalendarIconOffset);
            
            _calendarIconVisual.StartAnimation("offset", blendExpression);


            //
            // Set up the Calendar icon to also scale up/down a bit, using the common expression.
            //

            blendExpression.SetVector3Parameter("start", _visualState1.CalendarIconScale);
            blendExpression.SetVector3Parameter("end", _visualState2.CalendarIconScale);

            _calendarIconVisual.StartAnimation("scale", blendExpression);


            //
            // Blend the ToDo layer's starting -> ending opacity.  Instead of linear, we'll add a slightly more dramatic effect.
            //

            var weightNode = ExpressionValues.Constant.CreateConstantScalar("weight");
            blendExpression = EF.Lerp(startNode, endNode, (EF.Pow(weightNode, progress) - 1.0f) / (weightNode - 1.0f));
            blendExpression.SetScalarParameter("start", _visualState1.ToDoLayerOpacity);
            blendExpression.SetScalarParameter("end", _visualState2.ToDoLayerOpacity);
            blendExpression.SetScalarParameter("weight", 30);

            _todoLayerVisual.StartAnimation("opacity", blendExpression);


            //
            // Set up ToDo layer to scale up/down with the same warped blend.
            //

            blendExpression.SetVector3Parameter("start", _visualState1.ToDoLayerScale);
            blendExpression.SetVector3Parameter("end", _visualState2.ToDoLayerScale);

            _todoLayerVisual.StartAnimation("scale", blendExpression);


            //
            // Set up weather layer's opacity to increase/decrease.  We'll use the same warped blend, but change the weight.
            //

            blendExpression.SetScalarParameter("start", _visualState1.WeatherLayerOpacity);
            blendExpression.SetScalarParameter("end", _visualState2.WeatherLayerOpacity);
            blendExpression.SetScalarParameter("weight", 1.0f / 30.0f);

            _weatherLayerVisual.StartAnimation("opacity", blendExpression);


            //
            // Set up weather layer to scale up/down the same way.
            //

            blendExpression.SetVector3Parameter("start", _visualState1.WeatherLayerScale);
            blendExpression.SetVector3Parameter("end", _visualState2.WeatherLayerScale);

            _weatherLayerVisual.StartAnimation("scale", blendExpression);
        }

        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                //Capture pointer to system for gestures
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
            }

        }

        private Compositor _compositor;
        private InteractionTracker _tracker;
        private VisualInteractionSource _interactionSource;
        private Visual _backgroundImageVisual;
        private Visual _weatherLayerVisual;
        private Visual _todoLayerVisual;
        private Visual _calendarIconVisual;
        private Visual _backgroundVisual;
        private SpriteVisual _blurredBackgroundImageVisual;
        private SpriteVisual _backgroundVisualDimmer;
        private CompositionPropertySet _propertySet;
        private Vector3KeyFrameAnimation _cannedProgressAnimation;

        private VisualState _visualState1;
        private VisualState _visualState2;
        private bool _isState1 = false;

        internal struct VisualState
        {
            public float BackgroundBlurAmount;
            public Vector3 BackgroundScale;

            public Vector3 CalendarIconOffset;
            public Vector3 CalendarIconScale;

            public float ToDoLayerBlurAmount;
            public float ToDoLayerOpacity;
            public Vector3 ToDoLayerScale;

            public float WeatherLayerBlurAmount;
            public float WeatherLayerOpacity;
            public Vector3 WeatherLayerScale;
        }
    }
}
