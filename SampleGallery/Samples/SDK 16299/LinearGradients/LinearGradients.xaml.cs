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

        public static string StaticSampleName => "Linear Gradients";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Simple visual effect showing linear gradient animations. Scroll mouse over the textblocks or click on them to see the animations.";
        public override string SampleDescription => StaticSampleDescription;

        // Create the Compositor.
        private static readonly Compositor _compositor = Window.Current.Compositor;
        
        // Create the four colors that will be alternate on the two stops of the linear gradient brush.
        private readonly static Color s_warmColor1 = Colors.DeepPink;
        private readonly static Color s_warmColor2 = Colors.Honeydew;
        private readonly static Color s_coolColor1 = Colors.LightSkyBlue;
        private readonly static Color s_coolColor2 = Colors.Teal;

        private static SpriteVisual s_vis1;
        private static SpriteVisual s_vis2;
        private static SpriteVisual s_vis3;
        private static SpriteVisual s_vis4;

        private static CompositionLinearGradientBrush s_brush;
        private static CompositionColorGradientStop s_gradientStop1;
        private static CompositionColorGradientStop s_gradientStop2;

        private static Vector3KeyFrameAnimation s_scaleAnim;

        private static ColorKeyFrameAnimation s_changeStopToWarmColor1;
        private static ColorKeyFrameAnimation s_changeStopToHWarmColor2;
        private static ColorKeyFrameAnimation s_changeStopToCoolColor1;
        private static ColorKeyFrameAnimation s_changeStopToCoolColor2;
        
        public LinearGradients()
        {
            this.InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Update Rectangle widths to match text width.
            Rectangle1.Width = TextBlock1.ActualWidth;
            Rectangle2.Width = TextBlock2.ActualWidth;
            Rectangle3.Width = TextBlock3.ActualWidth;
            Rectangle4.Width = TextBlock4.ActualWidth;

            // Create the four visuals that will be used to hold the linear gradient brush.
            s_vis1 = _compositor.CreateSpriteVisual();
            s_vis2 = _compositor.CreateSpriteVisual();
            s_vis3 = _compositor.CreateSpriteVisual();
            s_vis4 = _compositor.CreateSpriteVisual();
            
            // Create the linear gradient brush and set up the first colors for it, DeepPink and HoneyDew. This brush will paint all of the visuals.
            s_brush = _compositor.CreateLinearGradientBrush();
            s_gradientStop1 = _compositor.CreateColorGradientStop();
            s_gradientStop1.Offset = 0;
            s_gradientStop1.Color = s_warmColor1;
            s_gradientStop2 = _compositor.CreateColorGradientStop();
            s_gradientStop2.Offset = 1;
            s_gradientStop2.Color = s_warmColor2;
            s_brush.ColorStops.Add(s_gradientStop1);
            s_brush.ColorStops.Add(s_gradientStop2);
            
            // Paint visuals with brushes and set their locations to match the locations of their corresponding XAML UI Element.
            s_vis1.Brush = s_brush;
            s_vis1.Scale = new Vector3(0, 1, 0);
            s_vis1.Size = new Vector2((float)Rectangle1.ActualWidth, (float)Rectangle1.ActualHeight);
            
            s_vis2.Brush = s_brush;
            s_vis2.Scale = new Vector3(0, 1, 0);
            s_vis2.Size = new Vector2((float)Rectangle2.ActualWidth, (float)Rectangle2.ActualHeight);
            
            s_vis3.Brush = s_brush;
            s_vis3.Scale = new Vector3(0, 1, 0);
            s_vis3.Size = new Vector2((float)Rectangle3.ActualWidth, (float)Rectangle3.ActualHeight);

            s_vis4.Brush = s_brush;
            s_vis4.Scale = new Vector3(0, 1, 0);
            s_vis4.Size = new Vector2((float)Rectangle4.ActualWidth, (float)Rectangle4.ActualHeight);

            // Parent visuals to XAML rectangles.
            ElementCompositionPreview.SetElementChildVisual(Rectangle1, s_vis1);
            ElementCompositionPreview.SetElementChildVisual(Rectangle2, s_vis2);
            ElementCompositionPreview.SetElementChildVisual(Rectangle3, s_vis3);
            ElementCompositionPreview.SetElementChildVisual(Rectangle4, s_vis4);

            // Create the scale & offset animation that animate the visuals' scale and offset.
            s_scaleAnim = _compositor.CreateVector3KeyFrameAnimation();
            s_scaleAnim.InsertKeyFrame(0, new Vector3(0, 1, 0));
            s_scaleAnim.InsertKeyFrame(.5f, Vector3.One);
            s_scaleAnim.InsertKeyFrame(1, new Vector3(0, 1, 0));
            s_scaleAnim.Duration = TimeSpan.FromSeconds(2);

            // Color animations that change the color of a stop when called to that stop. These animations are triggered from different intereactions with the XAML UI elements.
            s_changeStopToHWarmColor2 = _compositor.CreateColorKeyFrameAnimation();
            s_changeStopToHWarmColor2.InsertKeyFrame(1, s_warmColor2);
            s_changeStopToHWarmColor2.Duration = TimeSpan.FromSeconds(2);

            s_changeStopToWarmColor1 = _compositor.CreateColorKeyFrameAnimation();
            s_changeStopToWarmColor1.InsertKeyFrame(1, s_warmColor1);
            s_changeStopToWarmColor1.Duration = TimeSpan.FromSeconds(2);

            s_changeStopToCoolColor1 = _compositor.CreateColorKeyFrameAnimation();
            s_changeStopToCoolColor1.InsertKeyFrame(1, s_coolColor1);
            s_changeStopToCoolColor1.Duration = TimeSpan.FromSeconds(2);

            s_changeStopToCoolColor2 = _compositor.CreateColorKeyFrameAnimation();
            s_changeStopToCoolColor2.InsertKeyFrame(1, s_coolColor2);
            s_changeStopToCoolColor2.Duration = TimeSpan.FromSeconds(2);
        }

        // When pointer is pressed whatever is the current set of colors (DeepPink/Honeydew and LightSkyBlue/Teal) is swtiched to the set not displayed.
        private void Pointer_Pressed(object sender, RoutedEventArgs e)
        {
            SwitchColorSetOnPressed(s_gradientStop1, s_gradientStop2);
        }

        // That switches the sets of colors (DeepPink/Honeydew and LightSkyBlue/Teal).
        private void SwitchColorSetOnPressed(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            // If stop 1 is currently one of the warmer colors that are set together, it will call animations to switch both stops to the cooler color sets when the mouse clicks on the visuals.
            if (stop1.Color == s_warmColor1 || stop1.Color == s_warmColor2)
            {
                stop1.StartAnimation("Color", s_changeStopToCoolColor1);
                stop2.StartAnimation("Color", s_changeStopToCoolColor2);
            }
            // If stop 1 is currently one of the cooler colors that are set together, it will call animations to switch both stops to the warmer color sets when the mouse clicks on the visuals.
            else if (stop1.Color == s_coolColor1 || stop1.Color == s_coolColor2)
            {
                stop1.StartAnimation("Color", s_changeStopToWarmColor1);
                stop2.StartAnimation("Color", s_changeStopToHWarmColor2);
            }
        }

        // Animates scale of visual that method is called on.
        private void AnimateScale(SpriteVisual target)
        {
            target.StartAnimation("Scale", s_scaleAnim);
        }

        // Switch the current colors of the two color stops in the linear gradient brush. Called when first XAML UI Element is entered by the mouse pointer.
        private void ChangeStopColorsOnEntered(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            if (stop1.Color == s_warmColor1)
            {
                stop1.StartAnimation("Color", s_changeStopToHWarmColor2);
                stop2.StartAnimation("Color", s_changeStopToWarmColor1);
            }
            else if (stop1.Color == s_coolColor2)
            {
                stop1.StartAnimation("Color", s_changeStopToCoolColor1);
                stop2.StartAnimation("Color", s_changeStopToCoolColor2);
            }
        }

        // Switch the current colors of the two color stops in the linear gradient brush. Called when last XAML UI Element is entered by the mouse pointer.
        private void ReverseStopColorsOnEntered(CompositionColorGradientStop stop1, CompositionColorGradientStop stop2)
        {
            if (stop1.Color == s_warmColor2)
            {            
                stop1.StartAnimation("Color", s_changeStopToWarmColor1);
                stop2.StartAnimation("Color", s_changeStopToHWarmColor2);
            }
            else if (stop1.Color == s_coolColor1)
            {
                stop1.StartAnimation("Color", s_changeStopToCoolColor2);
                stop2.StartAnimation("Color", s_changeStopToCoolColor1);
            }
        }

        // Animate the offset of the visuals to move back and forth the length of the given visual that this method is called on.
        private void AnimateOffset(SpriteVisual target)
        {
            // The endpoint is the width of the inidividual sprite visual being targetted. The offset of the individual visuals will travel back and forth between these points.
            float endPoint = target.Size.X;
            
            // Create offset animation
            ScalarKeyFrameAnimation offsetAnim = _compositor.CreateScalarKeyFrameAnimation();
            offsetAnim.Duration = TimeSpan.FromSeconds(1);

            // When the visual is on the left, we set the anchor point to the right edge of the visual and the offset animation to end on the right side.
            if (target.Offset.X == 0)
            {
                target.AnchorPoint = new Vector2(1, 0);
                offsetAnim.InsertKeyFrame(1, endPoint);
            }

            // When the visual is on the right, we set the anchor point to the left edge of the visual and the offset animation to end on the left side.
            else if (target.Offset.X == endPoint)
            {
                target.AnchorPoint = Vector2.Zero;
                offsetAnim.InsertKeyFrame(1, 0);
            }
            target.StartAnimation("Offset.X", offsetAnim);
        }

        // Method for setting animations for the first canvas. This method is unique in that it contains a call to switch the color stops when this canvas is entered by mouse pointer.
        private void Canvas1_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(s_vis1);
            AnimateOffset(s_vis1);
            ChangeStopColorsOnEntered(s_gradientStop1, s_gradientStop2);
        }

        // Method for setting animations for the second canvas.
        private void Canvas2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(s_vis2);
            AnimateOffset(s_vis2);
        }

        // Method for setting animations for the third canvas.
        private void Canvas3_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(s_vis3);
            AnimateOffset(s_vis3);
        }

        // Method for setting animations for the last canvas. This method is unique in that it contains a call to switch the color stops when this canvas is entered by mouse pointer.
        private void Canvas4_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(s_vis4);
            AnimateOffset(s_vis4);
            ReverseStopColorsOnEntered(s_gradientStop1, s_gradientStop2);
        }
    }
}