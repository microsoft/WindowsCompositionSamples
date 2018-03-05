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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class Gears : SamplePage, INotifyPropertyChanged
    {
        private Compositor _compositor;
        private List<Visual> _gearVisuals;
        private ScalarKeyFrameAnimation _gearMotionScalarAnimation;
        private double _x = 87, _y = 0d, _width = 100, _height = 100;
        private double _gearDimension = 87;
        private int _count;

        public event PropertyChangedEventHandler PropertyChanged;

        public Gears()
        {
            InitializeComponent();
        }

        public static string StaticSampleName => "Gears";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Demonstrates how to use ExpressionAnimations to update many Visual properites based off of one driving property. Press the slow or fast buttons to see the gears spin.";
        public override string SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761162";

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                RaisePropertyChanged();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            Setup();
        }

        private void Setup()
        {
            var firstGearVisual = ElementCompositionPreview.GetElementVisual(FirstGear);
            firstGearVisual.Size = new Vector2((float)FirstGear.ActualWidth, (float)FirstGear.ActualHeight);
            firstGearVisual.AnchorPoint = new Vector2(0.5f, 0.5f);

            for (int i = Container.Children.Count - 1; i > 0; i--)
            {
                Container.Children.RemoveAt(i);
            }

            _x = 87;
            _y = 0d;
            _width = 100;
            _height = 100;
            _gearDimension = 87;

            Count = 1;
            _gearVisuals = new List<Visual>() { firstGearVisual };
        }

        private void AddGear_Click(object sender, RoutedEventArgs e)
        {
            // Create an image
            var bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/Other/Gear.png"));
            var image = new Image
            {
                Source = bitmapImage,
                Width = _width,
                Height = _height,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Set the coordinates of where the image should be
            Canvas.SetLeft(image, _x);
            Canvas.SetTop(image, _y);

            PerformLayoutCalculation();

            // Add the gear to the container
            Container.Children.Add(image);

            // Add a gear visual to the screen
            var gearVisual = AddGear(image);

            ConfigureGearAnimation(_gearVisuals[_gearVisuals.Count - 1], _gearVisuals[_gearVisuals.Count - 2]);
        }

        private Visual AddGear(Image gear)
        {
            // Create a visual based on the XAML object
            var visual = ElementCompositionPreview.GetElementVisual(gear);
            visual.Size = new Vector2((float)gear.ActualWidth, (float)gear.ActualHeight);
            visual.AnchorPoint = new Vector2(0.5f, 0.5f);
            _gearVisuals.Add(visual);

            Count++;

            return visual;
        }

        private void ConfigureGearAnimation(Visual currentGear, Visual previousGear)
        {
            // If rotation expression is null then create an expression of a gear rotating the opposite direction

            var rotateExpression = -previousGear.GetReference().RotationAngleInDegrees;

            // Start the animation based on the Rotation Angle in Degrees.
            currentGear.StartAnimation("RotationAngleInDegrees", rotateExpression);
        }

        private void StartGearMotor(double secondsPerRotation)
        {
            // Start the first gear (the red one)
            if (_gearMotionScalarAnimation == null)
            {
                _gearMotionScalarAnimation = _compositor.CreateScalarKeyFrameAnimation();
                var linear = _compositor.CreateLinearEasingFunction();

                var startingValue = ExpressionValues.StartingValue.CreateScalarStartingValue();
                _gearMotionScalarAnimation.InsertExpressionKeyFrame(0.0f, startingValue);
                _gearMotionScalarAnimation.InsertExpressionKeyFrame(1.0f, startingValue + 360f, linear);

                _gearMotionScalarAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            }

            _gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(secondsPerRotation);
            _gearVisuals.First().StartAnimation("RotationAngleInDegrees", _gearMotionScalarAnimation);
        }

        private void AnimateFast_Click(object sender, RoutedEventArgs e)
        {
            // Setup and start the animation on the red gear.
            StartGearMotor(1);
        }

        private void AnimateSlow_Click(object sender, RoutedEventArgs e)
        {
            // Setup and start the animation on the red gear.
            StartGearMotor(5);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _gearVisuals.First().StopAnimation("RotationAngleInDegrees");
        }

        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            if (_gearMotionScalarAnimation.Direction == AnimationDirection.Normal)
            {
                _gearMotionScalarAnimation.Direction = AnimationDirection.Reverse;
            }
            else
            {
                _gearMotionScalarAnimation.Direction = AnimationDirection.Normal;
            }

            _gearVisuals.First().StartAnimation("RotationAngleInDegrees", _gearMotionScalarAnimation);
        }

        private void AddXGearsButton_Click(object sender, RoutedEventArgs e)
        {
            int gearsToAdd;

            if (int.TryParse(NumberOfGears.Text, out gearsToAdd))
            {
                int amount = gearsToAdd + _gearVisuals.Count - 1;
                Setup();

                var maxAreaPerTile = Math.Sqrt((Container.ActualWidth * Container.ActualHeight) / (amount + Container.Children.Count));

                if (maxAreaPerTile < _width)
                {
                    var wholeTilesHeight = Math.Floor(Container.ActualHeight / maxAreaPerTile);
                    var wholeTileWidth = Math.Floor(Container.ActualWidth / maxAreaPerTile);

                    FirstGear.Width = FirstGear.Height = maxAreaPerTile;
                    _width = _height = maxAreaPerTile;

                    _x = _gearDimension = _width * 0.87;
                }

                for (int i = 0; i < amount; i++)
                {
                    AddGear_Click(sender, e);
                }
            }
        }

        private void PerformLayoutCalculation()
        {
            if (
                ((_x + Container.Margin.Left + _width > Container.ActualWidth) && _gearDimension > 0) ||
                (_x < Container.Margin.Left && _gearDimension < 0))
            {
                if (_gearDimension < 0)
                {
                    _y -= _gearDimension;
                }
                else
                {
                    _y += _gearDimension;
                }
                _gearDimension = -_gearDimension;
            }
            else
            {
                _x += _gearDimension;
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}