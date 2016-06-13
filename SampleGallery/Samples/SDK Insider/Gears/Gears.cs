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
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Composition
{
    public sealed partial class Gears : Page, INotifyPropertyChanged
    {
        #region Variables
        private Compositor compositor;
        private List<Visual> gearVisuals;
        private ExpressionAnimation rotationExpression;
        private ScalarKeyFrameAnimation gearMotionScalarAnimation;
        private double x = 87, y = 0d;
        private double width = 100, height = 100;

        private double gearDimension = 87;

        public event PropertyChangedEventHandler PropertyChanged;

        public Gears()
        {
            this.InitializeComponent();
        }

        private int count;

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                this.RaisePropertyChanged();
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;

            this.Setup();
        }

        private void Setup()
        {
            var firstGearVisual = ElementCompositionPreview.GetElementVisual(this.FirstGear);
            firstGearVisual.Size = new Vector2((float)this.FirstGear.ActualWidth, (float)this.FirstGear.ActualHeight);
            firstGearVisual.AnchorPoint = new Vector2(0.5f, 0.5f);

            for (int i = this.Container.Children.Count - 1; i > 0; i--)
            {
                this.Container.Children.RemoveAt(i);
            }

            x = 87;
            y = 0d;
            width = 100;
            height = 100;
            gearDimension = 87;

            this.Count = 1;
            this.gearVisuals = new List<Visual>() { firstGearVisual };
        }

        #endregion

        private async void AddGear_Click(object sender, RoutedEventArgs e)
        {
            // Create an image
            var bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/Gear.png"));
            var image = new Image
            {
                Source = bitmapImage,
                Width = this.width,
                Height = this.height,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Set the coordinates of where is should be
            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);

            PerformLayoutCalculation();

            // Add the gear to the container
            this.Container.Children.Add(image);

            // Add a gear visual to the screen
            var gearVisual = this.AddGear(image);

            ConfigureGearAnimation(gearVisuals[this.gearVisuals.Count - 1], gearVisuals[this.gearVisuals.Count - 2]);
        }

        private Visual AddGear(Image gear)
        {
            // Create a visual based on the XAML object
            var visual = ElementCompositionPreview.GetElementVisual(gear);
            visual.Size = new Vector2((float)gear.ActualWidth, (float)gear.ActualHeight);
            visual.AnchorPoint = new Vector2(0.5f, 0.5f);
            this.gearVisuals.Add(visual);

            this.Count++;

            return visual;
        }

        private void ConfigureGearAnimation(Visual currentGear, Visual previousGear)
        {
            // If rotation expression is null then create an expression of a gear rotating the opposite direction
            rotationExpression = rotationExpression ?? compositor.CreateExpressionAnimation("-previousGear.RotationAngleInDegrees");

            // put in placeholder parameters
            rotationExpression.SetReferenceParameter("previousGear", previousGear);

            // Start the animation based on the Rotation Angle in Degrees.
            currentGear.StartAnimation("RotationAngleInDegrees", rotationExpression);
        }

        private void EnsureGearMotor()
        {
            // Start the first gear (the red one)
            if (gearMotionScalarAnimation == null)
            {
                gearMotionScalarAnimation = compositor.CreateScalarKeyFrameAnimation();
                var linear = compositor.CreateLinearEasingFunction();

                gearMotionScalarAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
                gearMotionScalarAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 360", linear);

                gearMotionScalarAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            }
        }

        private void AnimateFast_Click(object sender, RoutedEventArgs e)
        {
            // Start red gear
            EnsureGearMotor();

            // Fast....
            gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(1);

            // I'm only animating the first gear! :-O
            gearVisuals.First().StartAnimation("RotationAngleInDegrees", gearMotionScalarAnimation);
        }

        private void AnimateSlow_Click(object sender, RoutedEventArgs e)
        {
            EnsureGearMotor();

            // Slow...
            gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(5);

            // I'm only animating the first gear! :-O
            gearVisuals.First().StartAnimation("RotationAngleInDegrees", gearMotionScalarAnimation);
        }

        #region Other Methods

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            gearVisuals.First().StopAnimation("RotationAngleInDegrees");
        }

        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            if (gearMotionScalarAnimation.Direction == AnimationDirection.Normal)
            {
                gearMotionScalarAnimation.Direction = AnimationDirection.Reverse;
            }
            else
            {
                gearMotionScalarAnimation.Direction = AnimationDirection.Normal;
            }

            gearVisuals.First().StartAnimation("RotationAngleInDegrees", gearMotionScalarAnimation);
        }

        private void AddXGearsButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = int.Parse(this.NumberOfGears.Text) + this.gearVisuals.Count - 1;

            this.Setup();
            var maxAreaPerTile = Math.Sqrt((this.Container.ActualWidth * this.Container.ActualHeight) / (amount + this.Container.Children.Count));

            if (maxAreaPerTile < this.width)
            {
                var wholeTilesHeight = Math.Floor(this.Container.ActualHeight / maxAreaPerTile);
                var wholeTileWidth = Math.Floor(this.Container.ActualWidth / maxAreaPerTile);

                this.FirstGear.Width = this.FirstGear.Height = maxAreaPerTile;
                this.width = this.height = maxAreaPerTile;

                this.x = this.gearDimension = this.width * 0.87;
            }

            for (int i = 0; i < amount; i++)
            {
                AddGear_Click(sender, e);
            }
        }

        private void RotateGears_Click(object sender, RoutedEventArgs e)
        {
            // Slow...
            gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(5);

            var container = ElementCompositionPreview.GetElementVisual(this.Root);
            container.AnchorPoint = new Vector2(1f, 1f);
            //container.CenterPoint = new Vector3(0.5f, 0.5f, 0.5f);
            // I'm only animating the first gear! :-O
            container.StartAnimation("RotationAngleInDegrees", gearMotionScalarAnimation);
        }

        private void PerformLayoutCalculation()
        {
            if (((x + this.Container.Margin.Left + this.width > this.Container.ActualWidth) && gearDimension > 0) || (x < this.Container.Margin.Left && gearDimension < 0))
            {
                if (gearDimension < 0)
                {
                    y -= gearDimension;
                }
                else
                {
                    y += gearDimension;
                }
                gearDimension = -gearDimension;
            }
            else
            {
                x += gearDimension;
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
