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
using System.Diagnostics;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class RadialGradients : SamplePage
    {
        public static string StaticSampleName => "Radial Gradients";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Sample application for radial gradient brush";
        public override string SampleDescription => StaticSampleDescription;


        // Create the compositor
        private readonly Compositor _compositor = Window.Current.Compositor;

        // create the various colors that will be used by the various radial gradient brushes
        private static readonly Color s_innerRingCoolColor = Colors.BlueViolet;
        private static readonly Color s_outerRingCoolColor = Colors.LightBlue;
        private static readonly Color s_innerRingWarmColor = Colors.IndianRed;
        private static readonly Color s_outerRingWarmColor = Colors.OrangeRed;
        private static readonly Color s_innerPulseColor = Colors.Transparent;
        private static readonly Color s_outerPulseColor = Colors.AliceBlue;

        private static readonly DispatcherTimer s_dt = new DispatcherTimer();

        private static SpriteVisual s_buttonVisual;
        private static SpriteVisual s_pulseVisual;

        private static CompositionRadialGradientBrush s_buttonBrush;
        private static CompositionRadialGradientBrush s_pulseBrush;

        private static CompositionColorGradientStop s_BBGradientStop1;
        private static CompositionColorGradientStop s_BBGradientStop2;

        private static CompositionColorGradientStop s_PBGradientStop1;
        private static CompositionColorGradientStop s_PBGradientStop2;

        private static ScalarKeyFrameAnimation s_stop1OffsetAnim;
        private static ScalarKeyFrameAnimation s_stop2offsetAnim2;
        private static ColorKeyFrameAnimation s_pulseColor;
        private static Vector3KeyFrameAnimation s_scale;

        private static ColorKeyFrameAnimation s_changeButtonGradientStop1;
        private static ColorKeyFrameAnimation s_changeButtonGradientStop2;

        private static bool s_isAnimationOn = false;

        private static Stopwatch s_stopwatch;
        private string s_currentTime;

        public RadialGradients()
        {
            this.InitializeComponent();
        }
 
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the two visuals that will represent the stopwatch button and the backing pulse visual
            s_buttonVisual = _compositor.CreateSpriteVisual();
            s_pulseVisual = _compositor.CreateSpriteVisual();

            // Create the brush that will paint the button visual
            s_buttonBrush = _compositor.CreateRadialGradientBrush();
            s_buttonBrush.EllipseCenter = new Vector2(.5f, .5f);
            s_buttonBrush.EllipseRadius = new Vector2(.85f, .85f);

            // Create the different gradient stops for the radial gradient brush and set them to the brush
            s_BBGradientStop1 = _compositor.CreateColorGradientStop();
            s_BBGradientStop1.Offset = .3f;
            s_BBGradientStop1.Color = s_innerRingCoolColor;

            s_BBGradientStop2 = _compositor.CreateColorGradientStop();
            s_BBGradientStop2.Offset = .75f;
            s_BBGradientStop2.Color = s_outerRingCoolColor;

            s_buttonBrush.ColorStops.Add(s_BBGradientStop1);
            s_buttonBrush.ColorStops.Add(s_BBGradientStop2);

            // Finishing defining the properties of the button visual
            s_buttonVisual.Size = new Vector2(300, 300);
            s_buttonVisual.Offset = new Vector3(((float)Rectangle3.ActualWidth / 2), ((float)Rectangle3.ActualHeight / 2), 0);
            s_buttonVisual.AnchorPoint = new Vector2(.5f, .5f);
            s_buttonVisual.Brush = s_buttonBrush;

            // Create a geometric clip that clips the rectangular sprite visual to a circle shape
            CompositionGeometricClip gClip = _compositor.CreateGeometricClip();
            CompositionEllipseGeometry circle = _compositor.CreateEllipseGeometry();
            circle.Radius = new Vector2(s_buttonVisual.Size.X / 2, s_buttonVisual.Size.Y / 2);
            circle.Center = new Vector2(s_buttonVisual.Size.X / 2, s_buttonVisual.Size.Y / 2);
            gClip.Geometry = circle;

            s_buttonVisual.Clip = gClip;

            // Create the brush for the backing visual 
            s_pulseBrush = _compositor.CreateRadialGradientBrush();
            s_pulseBrush.EllipseCenter = new Vector2(.5f, .5f);
            s_pulseBrush.EllipseRadius = new Vector2(.5f, .5f);

            s_PBGradientStop1 = _compositor.CreateColorGradientStop();
            s_PBGradientStop1.Offset = 0;
            s_PBGradientStop1.Color = s_innerPulseColor;
            s_PBGradientStop2 = _compositor.CreateColorGradientStop();
            s_PBGradientStop2.Offset = 1;
            s_PBGradientStop2.Color = s_innerPulseColor;

            s_pulseBrush.ColorStops.Add(s_PBGradientStop1);
            s_pulseBrush.ColorStops.Add(s_PBGradientStop2);

            // Finish defining the properties of the backing visual creates a pulsing effect when animated
            s_pulseVisual.Size = new Vector2(500, 500);
            s_pulseVisual.Offset = new Vector3(((float)Rectangle1.ActualWidth / 2), ((float)Rectangle1.ActualHeight / 2), 0);
            s_pulseVisual.AnchorPoint = new Vector2(.5f, .5f);
            s_pulseVisual.Brush = s_pulseBrush;

            // Create a geometric clip that clips the rectangular sprite visual to a circle shape
            CompositionGeometricClip gClip2 = _compositor.CreateGeometricClip();
            CompositionEllipseGeometry circle2 = _compositor.CreateEllipseGeometry();
            circle2.Radius = new Vector2(s_pulseVisual.Size.X / 2, s_pulseVisual.Size.Y / 2);
            circle2.Center = new Vector2(s_pulseVisual.Size.X / 2, s_pulseVisual.Size.Y / 2);
            gClip2.Geometry = circle2;

            s_pulseVisual.Clip = gClip2;

            // Tie sprite visuals to corresponding XAML UI Elements
            ElementCompositionPreview.SetElementChildVisual(Rectangle1, s_pulseVisual);
            ElementCompositionPreview.SetElementChildVisual(Rectangle2, s_buttonVisual);
        }

        // When the XAML element is clicked, this method with kick off the animation of the stop watch or turn it back to it's original state depending on if it's on or off
        private void OnClick1(object sender, RoutedEventArgs e)
        {
            if (s_isAnimationOn == false)
            {
                // Animation for changing the colors of the button from cool colors to warm colors to signal the timer is now on
                s_changeButtonGradientStop1 = _compositor.CreateColorKeyFrameAnimation();
                s_changeButtonGradientStop1.InsertKeyFrame(0, s_innerRingCoolColor);
                s_changeButtonGradientStop1.InsertKeyFrame(1, s_innerRingWarmColor);
                s_changeButtonGradientStop1.Duration = TimeSpan.FromSeconds(2);

                s_BBGradientStop1.StartAnimation("Color", s_changeButtonGradientStop1);

                s_changeButtonGradientStop2 = _compositor.CreateColorKeyFrameAnimation();
                s_changeButtonGradientStop2.InsertKeyFrame(0, s_outerRingCoolColor);
                s_changeButtonGradientStop2.InsertKeyFrame(1, s_outerRingWarmColor);
                s_changeButtonGradientStop2.Duration = TimeSpan.FromSeconds(2);

                s_BBGradientStop2.StartAnimation("Color", s_changeButtonGradientStop2);

                // Creating the animation for outer visual to create the pulsing effect that radiates from the center of the button out
                // Animation for the first stop of the radial gradient brush applied on the pulsing sprite visual
                s_stop1OffsetAnim = _compositor.CreateScalarKeyFrameAnimation();
                s_stop1OffsetAnim.InsertKeyFrame(0, 0);
                s_stop1OffsetAnim.InsertKeyFrame(1f, 1f);
                s_stop1OffsetAnim.Duration = TimeSpan.FromSeconds(1);
                s_stop1OffsetAnim.IterationCount = 50;

                s_PBGradientStop1.StartAnimation("Offset", s_stop1OffsetAnim);

                // Animation for the second stop of the radial gradient brush applied on the pulsing sprite visual
                s_stop2offsetAnim2 = _compositor.CreateScalarKeyFrameAnimation();
                s_stop2offsetAnim2.InsertKeyFrame(0, 0);
                s_stop2offsetAnim2.InsertKeyFrame(1f, 1f);
                s_stop2offsetAnim2.Duration = TimeSpan.FromSeconds(1);
                s_stop2offsetAnim2.IterationCount = 50;
                s_stop2offsetAnim2.DelayTime = TimeSpan.FromSeconds(.25f);

                s_PBGradientStop2.StartAnimation("Offset", s_stop2offsetAnim2);

                // When the stopwatch starts, the color for the gradient stops are changed to Alice Blue so that the pulse is visible
                s_pulseColor = _compositor.CreateColorKeyFrameAnimation();
                s_pulseColor.InsertKeyFrame(0, s_innerPulseColor);
                s_pulseColor.InsertKeyFrame(.99f, s_outerPulseColor);
                s_pulseColor.InsertKeyFrame(1, s_innerPulseColor);
                s_pulseColor.Duration = TimeSpan.FromSeconds(1);
                s_pulseColor.IterationBehavior = AnimationIterationBehavior.Forever;

                s_PBGradientStop1.StartAnimation("Color", s_pulseColor);

                // Create the animation that animates the scale of the pulsing sprite visual
                s_scale = _compositor.CreateVector3KeyFrameAnimation();
                s_scale.InsertKeyFrame(0, Vector3.Zero);
                s_scale.InsertKeyFrame(1, Vector3.One);
                s_scale.Duration = TimeSpan.FromSeconds(1);
                s_scale.IterationBehavior = AnimationIterationBehavior.Forever;

                s_pulseVisual.StartAnimation("Scale", s_scale);

                // Set up the stopwatch to reset each it is started again
                s_stopwatch = Stopwatch.StartNew();
                s_dt.Tick += Dt_Tick;
                s_dt.Interval = TimeSpan.FromMilliseconds(32);
                s_dt.Start();

                s_isAnimationOn = true;
            }
            else
            {
                // Button turns the colors of it's gradient stops back to cooler colors when it is shut off
                s_changeButtonGradientStop1 = _compositor.CreateColorKeyFrameAnimation();
                s_changeButtonGradientStop1.InsertKeyFrame(0, s_innerRingWarmColor);
                s_changeButtonGradientStop1.InsertKeyFrame(1, s_innerRingCoolColor);
                s_changeButtonGradientStop1.Duration = TimeSpan.FromSeconds(2);

                s_BBGradientStop1.StartAnimation("Color", s_changeButtonGradientStop1);

                s_changeButtonGradientStop2 = _compositor.CreateColorKeyFrameAnimation();
                s_changeButtonGradientStop2.InsertKeyFrame(0, s_outerRingWarmColor);
                s_changeButtonGradientStop2.InsertKeyFrame(1, s_outerRingCoolColor);
                s_changeButtonGradientStop2.Duration = TimeSpan.FromSeconds(2);

                s_BBGradientStop2.StartAnimation("Color", s_changeButtonGradientStop2);

                // Animations for the pulsing effect are turned off
                s_stop1OffsetAnim = _compositor.CreateScalarKeyFrameAnimation();
                s_stop1OffsetAnim.InsertKeyFrame(0, 0);
                s_stop1OffsetAnim.Duration = TimeSpan.FromSeconds(1.5);
                s_stop1OffsetAnim.IterationBehavior = AnimationIterationBehavior.Forever;
                
                s_PBGradientStop1.StartAnimation("Offset", s_stop1OffsetAnim);

                s_stop2offsetAnim2 = _compositor.CreateScalarKeyFrameAnimation();
                s_stop2offsetAnim2.InsertKeyFrame(0, 0);
                s_stop2offsetAnim2.Duration = TimeSpan.FromSeconds(1.5);
                s_stop2offsetAnim2.IterationBehavior = AnimationIterationBehavior.Forever;
                s_stop2offsetAnim2.DelayTime = TimeSpan.FromSeconds(.25);

                s_PBGradientStop2.StartAnimation("Offset", s_stop2offsetAnim2);

                s_pulseColor = _compositor.CreateColorKeyFrameAnimation();
                s_pulseColor.InsertKeyFrame(0, s_innerPulseColor);
                s_pulseColor.InsertKeyFrame(1, s_innerPulseColor);
                s_pulseColor.Duration = TimeSpan.FromSeconds(1.5);

                s_PBGradientStop1.StartAnimation("Color", s_pulseColor);

                // Create scale animation the helps with the final visual end result of create a pulsing effect
                s_scale = _compositor.CreateVector3KeyFrameAnimation();
                s_scale.InsertKeyFrame(0, Vector3.Zero);
                s_scale.InsertKeyFrame(1, Vector3.One);
                s_scale.Duration = TimeSpan.FromSeconds(1.5);
                s_scale.IterationBehavior = AnimationIterationBehavior.Forever;

                s_pulseVisual.StartAnimation("Scale", s_scale);

                // Turn off the stopwatch and change text back to Start
                s_dt.Stop();
                s_stopwatch.Start();
                Timer.Content = "Start";

                // Set our boolean flag for the stopwatch animation to false to indicate it's off
                s_isAnimationOn = false;
            }
        }

        // Method that sets up the stopwatch to display the time or "Start" when clicked on and off
        private void Dt_Tick(object sender, object e)
        {
            if (s_isAnimationOn)
            {
                TimeSpan ts = s_stopwatch.Elapsed;
                s_currentTime = $"{Math.Floor(ts.TotalMinutes)}:{ts.TotalSeconds % 60:00.00}";
                Timer.Content = s_currentTime;
            }
            else
            {
                Timer.Content = "Start";
            }
        }
    }
}