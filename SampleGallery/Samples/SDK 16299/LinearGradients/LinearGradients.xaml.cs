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
using Windows.UI.Xaml.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using System.Numerics;

namespace CompositionSampleGallery
{
    public sealed partial class LinearGradients : SamplePage
    {
        public LinearGradients()
        {
            this.InitializeComponent();
        }

        public static string StaticSampleName => "Linear Gradients";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Simple visual effect showing linear gradient animations. The first and last words will flip the colors from left to right.Clicking on any of the words will change the color scheme from warm to cool and back when clicked again." + Environment.NewLine + "This sample is to show how linear gradients can highlight content in colorful ways.";
        public override string SampleDescription => StaticSampleDescription;

        private Compositor _compositor;

        private SpriteVisual _vis1;
        private SpriteVisual _vis2;
        private SpriteVisual _vis3;
        private SpriteVisual _vis4;

        private CompositionLinearGradientBrush _brush1;
        private CompositionColorGradientStop _b1GradientStop1;
        private CompositionColorGradientStop _b1GradientStop2;


        private Vector3KeyFrameAnimation _scaleAnim;

        private ColorKeyFrameAnimation _changeStopToHoneydew;
        private ColorKeyFrameAnimation _changeStopToDeepPink;
        private ColorKeyFrameAnimation _changeStopToLightSkyBlue;
        private ColorKeyFrameAnimation _changeStopToTeal;

        private Color _deepPink;
        private Color _honeydew;
        private Color _lightSkyBlue;
        private Color _teal;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the Compositor 
            _compositor = Window.Current.Compositor;

            // Update Rectangle widths to match text width
            Rectangle1.Width = TextBlock1.ActualWidth;
            Rectangle2.Width = TextBlock2.ActualWidth;
            Rectangle3.Width = TextBlock3.ActualWidth;
            Rectangle4.Width = TextBlock4.ActualWidth;

            // Create the four visuals that will be used to hold the linear gradient brush
            _vis1 = _compositor.CreateSpriteVisual();
            _vis2 = _compositor.CreateSpriteVisual();
            _vis3 = _compositor.CreateSpriteVisual();
            _vis4 = _compositor.CreateSpriteVisual();

            // Create the four colors that will be used on the two stops of the linear gradient brush
            _deepPink = Colors.DeepPink;
            _honeydew = Colors.Honeydew;
            _lightSkyBlue = Colors.LightSkyBlue;
            _teal = Colors.Teal;

            // Create the linear gradient brush and set up the first colors for it, DeepPink and HoneyDew. This brush will paint all of the visuals.
            _brush1 = _compositor.CreateLinearGradientBrush();
            _b1GradientStop1 = _compositor.CreateColorGradientStop();
            _b1GradientStop1.Offset = 0;
            _b1GradientStop1.Color = _deepPink;
            _b1GradientStop2 = _compositor.CreateColorGradientStop();
            _b1GradientStop2.Offset = 1;
            _b1GradientStop2.Color = _honeydew;
            _brush1.ColorStops.Add(_b1GradientStop1);
            _brush1.ColorStops.Add(_b1GradientStop2);
            

            // Paint visuals with brushes and set their locations to match the locations of their corresponding XAML UI Element
            _vis1.Brush = _brush1;
            _vis1.Scale = new Vector3(0, 1, 0);
            _vis1.Size = new Vector2((float)Rectangle1.ActualWidth, (float)Rectangle1.ActualHeight);


            _vis2.Brush = _brush1;
            _vis2.Scale = new Vector3(0, 1, 0);
            _vis2.Size = new Vector2((float)Rectangle2.ActualWidth, (float)Rectangle2.ActualHeight);

            
            _vis3.Brush = _brush1;
            _vis3.Scale = new Vector3(0, 1, 0);
            _vis3.Size = new Vector2((float)Rectangle3.ActualWidth, (float)Rectangle3.ActualHeight);

            _vis4.Brush = _brush1;
            _vis4.Scale = new Vector3(0, 1, 0);
            _vis4.Size = new Vector2((float)Rectangle4.ActualWidth, (float)Rectangle4.ActualHeight);

            // Parent visuals to XAML rectangles
            ElementCompositionPreview.SetElementChildVisual(Rectangle1, _vis1);
            ElementCompositionPreview.SetElementChildVisual(Rectangle2, _vis2);
            ElementCompositionPreview.SetElementChildVisual(Rectangle3, _vis3);
            ElementCompositionPreview.SetElementChildVisual(Rectangle4, _vis4);


            // Create the scale & offset animation that animate the visuals' scale and offset
            _scaleAnim = _compositor.CreateVector3KeyFrameAnimation();
            _scaleAnim.InsertKeyFrame(0, new Vector3(0, 1, 0));
            _scaleAnim.InsertKeyFrame(.5f, new Vector3(1, 1, 0));
            _scaleAnim.InsertKeyFrame(1, new Vector3(0, 1, 0));
            _scaleAnim.Duration = TimeSpan.FromSeconds(2);


            // Color animations that change the color of a stop when called to that stop. These animations are triggered from different intereactions with the XAML UI elements
            _changeStopToHoneydew = _compositor.CreateColorKeyFrameAnimation();
            _changeStopToHoneydew.InsertKeyFrame(1f, _honeydew);
            _changeStopToHoneydew.Duration = TimeSpan.FromSeconds(2);

            _changeStopToDeepPink = _compositor.CreateColorKeyFrameAnimation();
            _changeStopToDeepPink.InsertKeyFrame(1f, _deepPink);
            _changeStopToDeepPink.Duration = TimeSpan.FromSeconds(2);

            _changeStopToLightSkyBlue = _compositor.CreateColorKeyFrameAnimation();
            _changeStopToLightSkyBlue.InsertKeyFrame(1f, _lightSkyBlue);
            _changeStopToLightSkyBlue.Duration = TimeSpan.FromSeconds(2);

            _changeStopToTeal = _compositor.CreateColorKeyFrameAnimation();
            _changeStopToTeal.InsertKeyFrame(1f, _teal);
            _changeStopToTeal.Duration = TimeSpan.FromSeconds(2);
        }

        // When pointer is pressed whatever is the current set of colors (DeepPink/Honeydew and LightSkyBlue/Teal) is swtiched to the set not displayed
        private void Pointer_Pressed(object sender, RoutedEventArgs e)
        {
            SwitchColorSetOnPressed(_b1GradientStop1, _b1GradientStop2);
        }

        // That switches the sets of colors (DeepPink/Honeydew and LightSkyBlue/Teal)
        private void SwitchColorSetOnPressed(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            // If stop 1 is currently one of the warmer colors that are set together, it will call animations to switch both stops to the cooler color sets when the mouse clicks on the visuals
            if (stop1.Color == _deepPink || stop1.Color == _honeydew)
            {
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToLightSkyBlue);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToTeal);
            }
            // If stop 1 is currently one of the cooler colors that are set together, it will call animations to switch both stops to the warmer color sets when the mouse clicks on the visuals
            else if (stop1.Color == _lightSkyBlue || stop1.Color == _teal)
            {
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToDeepPink);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToHoneydew);
            }
        }

        // Animates scale of visual that method is called on
        private void AnimateScale(SpriteVisual target)
        {
            target.StartAnimation("Scale", _scaleAnim);
        }

        // Switch the current colors of the two color stops in the linear gradient brush. Called when first XAML UI Element is entered by the mouse pointer
        private void ChangeStopColorsOnEntered(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            if (stop1.Color == Colors.DeepPink)
            {
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToHoneydew);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToDeepPink);
            } else if (stop1.Color == Colors.Teal)
            {
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToLightSkyBlue);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToTeal);
            }
        }

        // Switch the current colors of the two color stops in the linear gradient brush. Called when last XAML UI Element is entered by the mouse pointer
        private void ReverseStopColorsOnEntered(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            if (stop1.Color == Colors.Honeydew)
            {            
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToDeepPink);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToHoneydew);
            } else if (stop1.Color == Colors.LightSkyBlue)
            {
                stop1.StartAnimation(nameof(stop1.Color), _changeStopToTeal);
                stop2.StartAnimation(nameof(stop2.Color), _changeStopToLightSkyBlue);
            }
        }

        // Animate the offset of the visuals to move back and forth the length of the given visual that this method is called on
        private void AnimateOffset(SpriteVisual target)
        {
            // The endpoint is the width of the inidividual sprite visual being targetted. The offset of the individual visuals will travel back and forth between these points
            float endPoint = target.Size.X;
            
            // Create offset animation
            ScalarKeyFrameAnimation offsetAnim = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnim.Duration = TimeSpan.FromSeconds(1);

            // When the visual is on the left, we set the anchor point to the right edge of the visual and the offset animation to end on the right side
            if (target.Offset.X == 0)
            {
                target.AnchorPoint = new Vector2(1f, 0f);
                offsetAnim.InsertKeyFrame(1, endPoint);

            }

            // When the visual is on the right, we set the anchor point to the left edge of the visual and the offset animation to end on the left side
            else if (target.Offset.X == endPoint)
            {
                target.AnchorPoint = new Vector2(0f, 0f);
                offsetAnim.InsertKeyFrame(1, 0);
            }
            target.StartAnimation("Offset.X", offsetAnim);

        }

        // Method for setting animations for the first canvas. This method is unique in that it contains a call to switch the color stops when this canvas is entered by mouse pointer
        private void Canvas1_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(_vis1);
            ChangeStopColorsOnEntered(_b1GradientStop1, _b1GradientStop2);
            AnimateOffset(_vis1);
        }

        // Method for setting animations for the second canvas
        private void Canvas2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(_vis2);
            AnimateOffset(_vis2);
        }

        // Method for setting animations for the third canvas
        private void Canvas3_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(_vis3);
            AnimateOffset(_vis3);
        }

        // Method for setting animations for the last canvas. This method is unique in that it contains a call to switch the color stops when this canvas is entered by mouse pointer
        private void Canvas4_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(_vis4);
            ReverseStopColorsOnEntered(_b1GradientStop1, _b1GradientStop2);
            AnimateOffset(_vis4);
        }
    }
}