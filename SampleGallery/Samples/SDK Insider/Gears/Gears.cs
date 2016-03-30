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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI;
using Windows.UI.ViewManagement;
using System.Diagnostics;

namespace CompositionSampleGallery
{
    public sealed partial class Gears : SamplePage
    {
        public Gears()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName    { get { return "Gears"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Demonstrates how to use ExpressionAnimations to update many Visual properites based off of one driving property. Press any button to see the gears spin."; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761162"; } }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _gearVisuals = new Visual[Container.Children.Count()];

            for (int i = 0; i < _gearVisuals.Length; i++)
            {
                AddGear(i, Container.Children.ElementAt(i) as Image);

                if (i == 0)
                {
                    _compositor = _gearVisuals[0].Compositor;
                }
                else
                {
                    ConfigureGearAnimation(_gearVisuals[i], _gearVisuals[i - 1]);
                }
            }
        }

        private void AddGear(int index, Image gear)
        {
            _gearVisuals[index] = ElementCompositionPreview.GetElementVisual(gear);
            _gearVisuals[index].Size = new Vector2((float)gear.ActualWidth, (float)gear.ActualHeight);
            _gearVisuals[index].AnchorPoint = new Vector2(0.5f, 0.5f);
        }

        private void ConfigureGearAnimation(Visual currentGear, Visual previousGear)
        {
            if (_rotationExpression == null)
            {
                _rotationExpression = _compositor.CreateExpressionAnimation("-previousGear.RotationAngleInDegrees");
            }

            _rotationExpression.SetReferenceParameter("previousGear", previousGear);

            currentGear.StartAnimation("RotationAngleInDegrees", _rotationExpression);
        }

        private void EnsureGearMotor()
        {
            if (_gearMotor == null)
            {
                _gearMotor = _compositor.CreateScalarKeyFrameAnimation();
                var linear = _compositor.CreateLinearEasingFunction();

                _gearMotor.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
                _gearMotor.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 360", linear);

                _gearMotor.IterationBehavior = AnimationIterationBehavior.Forever;
            }
        }

        private void AnimateSlow_Click(object sender, RoutedEventArgs e)
        {
            EnsureGearMotor();
            _gearMotor.Duration = TimeSpan.FromSeconds(5);
            _gearVisuals[0].StartAnimation("RotationAngleInDegrees", _gearMotor);
        }

        private void AnimateFast_Click(object sender, RoutedEventArgs e)
        {
            EnsureGearMotor();
            _gearMotor.Duration = TimeSpan.FromSeconds(3);
            _gearVisuals[0].StartAnimation("RotationAngleInDegrees", _gearMotor);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _gearVisuals[0].StopAnimation("RotationAngleInDegrees");
        }

        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            if (_gearMotor.Direction == AnimationDirection.Normal)
            {
                _gearMotor.Direction = AnimationDirection.Reverse;
            }
            else
            {
                _gearMotor.Direction = AnimationDirection.Normal;
            }
            _gearVisuals[0].StartAnimation("RotationAngleInDegrees", _gearMotor);
        }

        private Compositor _compositor;
        private Visual[] _gearVisuals;
        private ExpressionAnimation _rotationExpression;
        private ScalarKeyFrameAnimation _gearMotor;
    }
}
