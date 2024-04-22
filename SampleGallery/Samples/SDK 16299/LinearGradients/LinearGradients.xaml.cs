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
using System.Numerics;
using System.Linq;

using Windows.UI;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    public sealed partial class LinearGradients : SamplePage
    {
        public static string StaticSampleName => "Linear Gradients";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Simple visual effect showing linear gradient animations. Scroll mouse over the textblocks or click on them to see the animations.";
        public override string SampleDescription => StaticSampleDescription;

        private static readonly (Color, Color) s_coolColors = (Colors.LightSkyBlue, Colors.Teal);
        private static readonly (Color, Color) s_warmColors = (Colors.DeepPink, Colors.Honeydew);

        private readonly Compositor _compositor = CompositionTarget.GetCompositorForCurrentThread();

        private readonly CompositionColorGradientStop _gradientStop1;
        private readonly CompositionColorGradientStop _gradientStop2;

        private ColorScheme _currentColorScheme = ColorScheme.Warm;

        public LinearGradients()
        {
            this.InitializeComponent();

            // Create the linear gradient brush with the initial color scheme.
            var (stop1Color, stop2Color) = GetCurrentColors();
            var linearGradientBrush = _compositor.CreateLinearGradientBrush();
            _gradientStop1 = _compositor.CreateColorGradientStop();
            _gradientStop1.Color = stop1Color;
            _gradientStop2 = _compositor.CreateColorGradientStop();
            _gradientStop2.Offset = 1;
            _gradientStop2.Color = stop2Color;
            linearGradientBrush.ColorStops.Add(_gradientStop1);
            linearGradientBrush.ColorStops.Add(_gradientStop2);

            // Create the Visuals that will be used to paint with the linear gradient brush
            // behind each line of text.
            foreach (Grid lineElement in TextLines.Children)
            {
                var visual = _compositor.CreateSpriteVisual();
                visual.Brush = linearGradientBrush;

                // Initially 0% scale on the X axis, so the Visual will be invisible.
                visual.Scale = new Vector3(0, 1, 1);

                // The Visual will be sized relative to its parent.
                visual.RelativeSizeAdjustment = Vector2.One;

                // Save the Visual in the Tag on the element for easy access in the event handlers.
                lineElement.Tag = visual;

                // Parent the Visual to the first child. The second child
                // is the TextBlock that will draw on top of this Visual.
                ElementCompositionPreview.SetElementChildVisual(lineElement.Children[0], visual);
            }
        }
        private ColorKeyFrameAnimation CreateAnimationToColor(Color color)
        {
            var animation = _compositor.CreateColorKeyFrameAnimation();
            animation.InsertKeyFrame(1, color);
            animation.Duration = TimeSpan.FromSeconds(2);
            return animation;
        }

        private void AnimateToNewColorScheme(ColorScheme colorScheme)
        {
            // Save the color scheme as the current one.
            _currentColorScheme = colorScheme;

            // Animate to the colors in the color scheme.
            var (stop1Color, stop2Color) = GetCurrentColors();
            _gradientStop1.StartAnimation("Color", CreateAnimationToColor(stop1Color));
            _gradientStop2.StartAnimation("Color", CreateAnimationToColor(stop2Color));
        }

        // Animate the given Visual to sweep across the text.
        private void AnimateToNewPosition(Visual visual)
        {
            // Scale the width from 0 to 1 to 0.
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0, new Vector3(0, 1, 1));
            scaleAnimation.InsertKeyFrame(.5f, Vector3.One);
            scaleAnimation.InsertKeyFrame(1, new Vector3(0, 1, 1));
            scaleAnimation.Duration = TimeSpan.FromSeconds(2);
            visual.StartAnimation("Scale", scaleAnimation);

            // Sweep the offset along the x axis.
            var targetX =
                visual.RelativeOffsetAdjustment.X == 0
                ? 1
                : 0;

            visual.AnchorPoint = new Vector2(targetX, 0);

            var offsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.Duration = TimeSpan.FromSeconds(1);
            offsetAnimation.InsertKeyFrame(1, targetX);
            visual.StartAnimation("RelativeOffsetAdjustment.X", offsetAnimation);
        }

        private void Line_PointerPressed(object sender, RoutedEventArgs e)
        {
            // Toggle the color scheme.
            AnimateToNewColorScheme(GetComplementaryColorScheme(_currentColorScheme));
        }

        private void Line_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Get the Visual associated with the sender and animate its position and width.
            AnimateToNewPosition((SpriteVisual)((FrameworkElement)sender).Tag);

            if (sender == (object) TextLines.Children.First())
            {
                // Pointer is over the top line.
                switch (_currentColorScheme)
                {
                    case ColorScheme.CoolReversed:
                    case ColorScheme.Warm:
                        AnimateToNewColorScheme(GetReversedColorScheme(_currentColorScheme));
                        break;
                }
            }
            else if (sender == (object) TextLines.Children.Last())
            {
                // Pointer is over the bottom line.
                switch (_currentColorScheme)
                {
                    case ColorScheme.Cool:
                    case ColorScheme.WarmReversed:
                        AnimateToNewColorScheme(GetReversedColorScheme(_currentColorScheme));
                        break;
                }
            }
        }

        private (Color, Color) GetCurrentColors()
        {
            switch (_currentColorScheme)
            {
                case ColorScheme.Warm: return s_warmColors;
                case ColorScheme.WarmReversed: return Reverse(s_warmColors);
                case ColorScheme.Cool: return s_coolColors;
                case ColorScheme.CoolReversed: return Reverse(s_coolColors);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static ColorScheme GetComplementaryColorScheme(ColorScheme colorScheme)
            => ApplyColorSchemeFunction(
                colorScheme,
                ifWarm: ColorScheme.Cool,
                ifWarmReversed: ColorScheme.CoolReversed,
                ifCool: ColorScheme.Warm,
                ifCoolReversed: ColorScheme.WarmReversed);

        private static ColorScheme GetReversedColorScheme(ColorScheme colorScheme)
            => ApplyColorSchemeFunction(
                colorScheme,
                ifWarm: ColorScheme.WarmReversed,
                ifWarmReversed: ColorScheme.Warm,
                ifCool: ColorScheme.CoolReversed,
                ifCoolReversed: ColorScheme.Cool);

        private static ColorScheme ApplyColorSchemeFunction(
            ColorScheme input,
            ColorScheme ifWarm,
            ColorScheme ifWarmReversed,
            ColorScheme ifCool,
            ColorScheme ifCoolReversed)
        {
            switch (input)
            {
                case ColorScheme.Warm: return ifWarm;
                case ColorScheme.WarmReversed: return ifWarmReversed;
                case ColorScheme.Cool: return ifCool;
                case ColorScheme.CoolReversed: return ifCoolReversed;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static (T, T) Reverse<T>((T, T) pair) => (pair.Item2, pair.Item1);

        private enum ColorScheme
        {
            Warm,
            WarmReversed,
            Cool,
            CoolReversed,
        }
    }
}