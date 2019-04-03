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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using Windows.UI.Composition;
using System.Numerics;
using System.Diagnostics;

namespace CompositionSampleGallery
{
    public sealed partial class RadialGradients : SamplePage
    {
        public RadialGradients()
        {
            this.InitializeComponent();
        }

        public static string StaticSampleName => "Radial Gradients";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Sample application for radial gradient brush";
        public override string SampleDescription => StaticSampleDescription;


        private Compositor _compositor;

        private SpriteVisual _buttonVisual;
        private SpriteVisual _pulseVisual;

        private CompositionRadialGradientBrush _buttonBrush;
        private CompositionRadialGradientBrush _pulseBrush;

        private CompositionColorGradientStop _BBGradientStop1;
        private CompositionColorGradientStop _BBGradientStop2;

        private CompositionColorGradientStop _PBGradientStop1;
        private CompositionColorGradientStop _PBGradientStop2;
        
        private ScalarKeyFrameAnimation _stop1OffsetAnim;
        private ScalarKeyFrameAnimation _stop2offsetAnim2;
        private ColorKeyFrameAnimation _pulseColor;
        private Vector3KeyFrameAnimation _scale;

        private ColorKeyFrameAnimation _changeButtonGradientStop1;
        private ColorKeyFrameAnimation _changeButtonGradientStop2;

        private Color _blueViolet;
        private Color _lightBlue;
        private Color _indianRed;
        private Color _orangeRed;
        private Color _transparent;
        private Color _aliceBlue;

        private bool _isAnimationOn;

        private Stopwatch _stopwatch;
        private string _currentTime;
        private DispatcherTimer _dt;


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the compositor
            _compositor = Window.Current.Compositor;

            // Create the two visuals that will represent the stopwatch button and the backing pulse visual
            _buttonVisual = _compositor.CreateSpriteVisual();
            _pulseVisual = _compositor.CreateSpriteVisual();

            // create the various colors that will be used by the various radial gradient brushes
            _blueViolet = Colors.BlueViolet;
            _lightBlue = Colors.LightBlue;
            _indianRed = Colors.IndianRed;
            _orangeRed = Colors.OrangeRed;
            _transparent = Colors.Transparent;
            _aliceBlue = Colors.AliceBlue;

            // Create the brush that will paint the button visual
            _buttonBrush = _compositor.CreateRadialGradientBrush();
            _buttonBrush.EllipseCenter = new Vector2(.5f, .5f);
            _buttonBrush.EllipseRadius = new Vector2(.85f, .85f);

            // Create the different gradient stops for the radial gradient brush and set them to the brush
            _BBGradientStop1 = _compositor.CreateColorGradientStop();
            _BBGradientStop1.Offset = .3f;
            _BBGradientStop1.Color = _blueViolet;

            _BBGradientStop2 = _compositor.CreateColorGradientStop();
            _BBGradientStop2.Offset = .75f;
            _BBGradientStop2.Color = _lightBlue;

            _buttonBrush.ColorStops.Add(_BBGradientStop1);
            _buttonBrush.ColorStops.Add(_BBGradientStop2);

            // Finishing defining the properties of the button visual
            _buttonVisual.Size = new Vector2(300, 300);
            _buttonVisual.Offset = new Vector3(((float)Rectangle3.ActualWidth / 2), ((float)Rectangle3.ActualHeight / 2), 0);
            _buttonVisual.AnchorPoint = new Vector2(.5f, .5f);
            _buttonVisual.Brush = _buttonBrush;

            // Create a geometric clip that clips the rectangular sprite visual to a circle shape
            CompositionGeometricClip gClip = _compositor.CreateGeometricClip();
            CompositionEllipseGeometry circle = _compositor.CreateEllipseGeometry();
            circle.Radius = new Vector2(_buttonVisual.Size.X / 2, _buttonVisual.Size.Y / 2);
            circle.Center = new Vector2(_buttonVisual.Size.X / 2, _buttonVisual.Size.Y / 2);
            gClip.Geometry = circle;

            _buttonVisual.Clip = gClip;

            // Create the brush for the backing visual 
            _pulseBrush = _compositor.CreateRadialGradientBrush();
            _pulseBrush.EllipseCenter = new Vector2(.5f, .5f);
            _pulseBrush.EllipseRadius = new Vector2(.5f, .5f);

            _PBGradientStop1 = _compositor.CreateColorGradientStop();
            _PBGradientStop1.Offset = 0;
            _PBGradientStop1.Color = _transparent;
            _PBGradientStop2 = _compositor.CreateColorGradientStop();
            _PBGradientStop2.Offset = 1;
            _PBGradientStop2.Color = _transparent;

            _pulseBrush.ColorStops.Add(_PBGradientStop1);
            _pulseBrush.ColorStops.Add(_PBGradientStop2);

            // Finish defining the properties of the backing visual creates a pulsing effect when animated
            _pulseVisual.Size = new Vector2(500, 500);
            _pulseVisual.Offset = new Vector3(((float)Rectangle1.ActualWidth / 2), ((float)Rectangle1.ActualHeight / 2), 0);
            _pulseVisual.AnchorPoint = new Vector2(.5f, .5f);
            _pulseVisual.Brush = _pulseBrush;

            // Create a geometric clip that clips the rectangular sprite visual to a circle shape
            CompositionGeometricClip gClip2 = _compositor.CreateGeometricClip();
            CompositionEllipseGeometry circle2 = _compositor.CreateEllipseGeometry();
            circle2.Radius = new Vector2(_pulseVisual.Size.X / 2, _pulseVisual.Size.Y / 2);
            circle2.Center = new Vector2(_pulseVisual.Size.X / 2, _pulseVisual.Size.Y / 2);
            gClip2.Geometry = circle2;

            _pulseVisual.Clip = gClip2;

            // Tie sprite visuals to corresponding XAML UI Elements
            ElementCompositionPreview.SetElementChildVisual(Rectangle1, _pulseVisual);
            ElementCompositionPreview.SetElementChildVisual(Rectangle2, _buttonVisual);

            // WHen the page first loads we want the animations to be off so we set this boolean flag to false
            _isAnimationOn = false;
        }

        // When the XAML element is clicked, this method with kick off the animation of the stop watch or turn it back to it's original state depending on if it's on or off
        private void OnClick1(object sender, RoutedEventArgs e)
        {

            if (_isAnimationOn == false) {
                // Animation for changing the colors of the button from cool colors to warm colors to signal the timer is now on
                _changeButtonGradientStop1 = _compositor.CreateColorKeyFrameAnimation();
                _changeButtonGradientStop1.InsertKeyFrame(0, _blueViolet);
                _changeButtonGradientStop1.InsertKeyFrame(1, _indianRed);
                _changeButtonGradientStop1.Duration = TimeSpan.FromSeconds(2);

                _BBGradientStop1.StartAnimation("Color", _changeButtonGradientStop1);

                _changeButtonGradientStop2 = _compositor.CreateColorKeyFrameAnimation();
                _changeButtonGradientStop2.InsertKeyFrame(0, _lightBlue);
                _changeButtonGradientStop2.InsertKeyFrame(1, _orangeRed);
                _changeButtonGradientStop2.Duration = TimeSpan.FromSeconds(2);

                _BBGradientStop2.StartAnimation("Color", _changeButtonGradientStop2);

                // Creating the animation for outer visual to create the pulsing effect that radiates from the center of the button out
                // Animation for the first stop of the radial gradient brush applied on the pulsing sprite visual
                _stop1OffsetAnim = _compositor.CreateScalarKeyFrameAnimation();
                _stop1OffsetAnim.InsertKeyFrame(0, 0);
                _stop1OffsetAnim.InsertKeyFrame(1f, 1f);
                _stop1OffsetAnim.Duration = TimeSpan.FromSeconds(1);
                _stop1OffsetAnim.IterationCount = 50;

                _PBGradientStop1.StartAnimation("Offset", _stop1OffsetAnim);

                // Animation for the second stop of the radial gradient brush applied on the pulsing sprite visual
                _stop2offsetAnim2 = _compositor.CreateScalarKeyFrameAnimation();
                _stop2offsetAnim2.InsertKeyFrame(0, 0);
                _stop2offsetAnim2.InsertKeyFrame(1f, 1f);
                _stop2offsetAnim2.Duration = TimeSpan.FromSeconds(1);
                _stop2offsetAnim2.IterationCount = 50;
                _stop2offsetAnim2.DelayTime = TimeSpan.FromSeconds(.25f);

                _PBGradientStop2.StartAnimation("Offset", _stop2offsetAnim2);

                // When the stopwatch starts, the color for the gradient stops are changed to Alice Blue so that the pulse is visible
                _pulseColor = _compositor.CreateColorKeyFrameAnimation();
                _pulseColor.InsertKeyFrame(0, _transparent);
                _pulseColor.InsertKeyFrame(.99f, _aliceBlue);
                _pulseColor.InsertKeyFrame(1, _transparent);
                _pulseColor.Duration = TimeSpan.FromSeconds(1);
                _pulseColor.IterationBehavior = AnimationIterationBehavior.Forever;

                _PBGradientStop1.StartAnimation("Color", _pulseColor);

                // Create the animation that animates the scale of the pulsing sprite visual
                _scale = _compositor.CreateVector3KeyFrameAnimation();
                _scale.InsertKeyFrame(0, new Vector3(0, 0, 0));
                _scale.InsertKeyFrame(1, new Vector3(1, 1, 0));
                _scale.Duration = TimeSpan.FromSeconds(1);
                _scale.IterationCount = 50;

                _pulseVisual.StartAnimation("Scale", _scale);

                // Set up the stopwatch to reset each it is started again
                _dt = new DispatcherTimer();
                _stopwatch = Stopwatch.StartNew();
                _dt.Tick += Dt_Tick;
                _dt.Interval = new TimeSpan(0, 0, 0, 0, 1);
                _dt.Start();

                _isAnimationOn = true;
            } else {
                // Button turns the colors of it's gradient stops back to cooler colors when it is shut off
                _changeButtonGradientStop1 = _compositor.CreateColorKeyFrameAnimation();
                _changeButtonGradientStop1.InsertKeyFrame(0, _indianRed);
                _changeButtonGradientStop1.InsertKeyFrame(1, _blueViolet);
                _changeButtonGradientStop1.Duration = TimeSpan.FromSeconds(2);

                _BBGradientStop1.StartAnimation("Color", _changeButtonGradientStop1);

                _changeButtonGradientStop2 = _compositor.CreateColorKeyFrameAnimation();
                _changeButtonGradientStop2.InsertKeyFrame(0, _orangeRed);
                _changeButtonGradientStop2.InsertKeyFrame(1, _lightBlue);
                _changeButtonGradientStop2.Duration = TimeSpan.FromSeconds(2);

                _BBGradientStop2.StartAnimation("Color", _changeButtonGradientStop2);

                // Animations for the pulsing effect are turned off
                _stop1OffsetAnim = _compositor.CreateScalarKeyFrameAnimation();
                _stop1OffsetAnim.InsertKeyFrame(0, 0);
                _stop1OffsetAnim.InsertKeyFrame(0f, 0f);
                _stop1OffsetAnim.Duration = TimeSpan.FromSeconds(1.5f);
                _stop1OffsetAnim.IterationCount = 50;
                
                _PBGradientStop1.StartAnimation("Offset", _stop1OffsetAnim);

                _stop2offsetAnim2 = _compositor.CreateScalarKeyFrameAnimation();
                _stop2offsetAnim2.InsertKeyFrame(0, 0);
                _stop2offsetAnim2.InsertKeyFrame(0f, 0f);
                _stop2offsetAnim2.Duration = TimeSpan.FromSeconds(1.5f);
                _stop2offsetAnim2.IterationCount = 50;
                _stop2offsetAnim2.DelayTime = TimeSpan.FromSeconds(.25f);

                _PBGradientStop2.StartAnimation("Offset", _stop2offsetAnim2);

                _pulseColor = _compositor.CreateColorKeyFrameAnimation();
                _pulseColor.InsertKeyFrame(0, _transparent);
                _pulseColor.InsertKeyFrame(1, _transparent);
                _pulseColor.Duration = TimeSpan.FromSeconds(1.5);

                _PBGradientStop1.StartAnimation("Color", _pulseColor);

                // Create scale animation the helps with the final visual end result of create a pulsing effect
                _scale = _compositor.CreateVector3KeyFrameAnimation();
                _scale.InsertKeyFrame(0, new Vector3(0, 0, 0));
                _scale.InsertKeyFrame(1, new Vector3(1, 1, 0));
                _scale.Duration = TimeSpan.FromSeconds(1.5);
                _scale.IterationCount = 50;

                _pulseVisual.StartAnimation("Scale", _scale);

                // Turn off the stopwatch and change text back to Start
                _dt.Stop();
                _stopwatch.Start();
                Timer.Content = ("Start");

                // Set our boolean flag for the stopwatch animation to false to indicate it's off
                _isAnimationOn = false;
            }
        }

        // Method that sets up the stopwatch to display the time or "Start" when clicked on and off
        private void Dt_Tick(object sender, object e)
        {
            if (_isAnimationOn == true) {
                TimeSpan ts = _stopwatch.Elapsed;
                _currentTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Timer.Content = _currentTime;
            } else {
                Timer.Content = ("Start");
            }
        }
    }
}