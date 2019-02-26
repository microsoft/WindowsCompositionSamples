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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
        public static string StaticSampleDescription => "Simple visual effect showing linear gradient animations.The first and last words will flip the colors from left to right.Clicking on any of the words will change the color scheme from warm to cool and back when clicked again." + Environment.NewLine + "This sample is to show how linear gradients can highlight content in colorful ways.";
        public override string SampleDescription => StaticSampleDescription;

        private Compositor _compositor;

        private SpriteVisual _vis1;
        private SpriteVisual _vis2;
        private SpriteVisual _vis3;
        private SpriteVisual _vis4;

        private CompositionLinearGradientBrush _brush1;
        private CompositionColorGradientStop b1GradientStop1;
        private CompositionColorGradientStop b1GradientStop2;


        private Vector3KeyFrameAnimation _scaleAnim;
        private ScalarKeyFrameAnimation _offsetAnim;

        private ColorKeyFrameAnimation _colorAnimBrush1Stop1;
        private ColorKeyFrameAnimation _colorAnimBrush1Stop2;
        private ColorKeyFrameAnimation _changeb1stop1;
        private ColorKeyFrameAnimation _changeb1stop2;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Update Rectangle widths to match text width
            Rectangle1.Width = TextBlock1.ActualWidth;
            Rectangle2.Width = TextBlock2.ActualWidth;
            Rectangle3.Width = TextBlock3.ActualWidth;
            Rectangle4.Width = TextBlock4.ActualWidth;

            // Create the four visuals that will be used to hold the LG brush
            _vis1 = _compositor.CreateSpriteVisual();
            _vis2 = _compositor.CreateSpriteVisual();
            _vis3 = _compositor.CreateSpriteVisual();
            _vis4 = _compositor.CreateSpriteVisual();

            // Create the purple to orange brush that will be used on all visuals
            _brush1 = _compositor.CreateLinearGradientBrush();
            b1GradientStop1 = _compositor.CreateColorGradientStop();
            b1GradientStop1.Offset = 0;
            b1GradientStop1.Color = Colors.DeepPink;
            b1GradientStop2 = _compositor.CreateColorGradientStop();
            b1GradientStop2.Offset = 1;
            b1GradientStop2.Color = Colors.Honeydew;
            _brush1.ColorStops.Add(b1GradientStop1);
            _brush1.ColorStops.Add(b1GradientStop2);
            

            // Paint visuals with brushes and set their locations
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


            //create the scale & offset animation
            Vector3KeyFrameAnimation offsetAnim_r1 = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnim_r1.InsertKeyFrame(1, new Vector3((float)Rectangle1.ActualWidth, (float)Rectangle1.ActualHeight, 0));
            offsetAnim_r1.Duration = TimeSpan.FromSeconds(1);

            _scaleAnim = _compositor.CreateVector3KeyFrameAnimation();
            _scaleAnim.InsertKeyFrame(0, new Vector3(0, 1, 0));
            _scaleAnim.InsertKeyFrame(.5f, new Vector3(1, 1, 0));
            _scaleAnim.InsertKeyFrame(1, new Vector3(0, 1, 0));
            _scaleAnim.Duration = TimeSpan.FromSeconds(2);


            // animation of color stops
            _colorAnimBrush1Stop1 = _compositor.CreateColorKeyFrameAnimation();
            _colorAnimBrush1Stop1.InsertKeyFrame(.5f, Colors.Honeydew);
            _colorAnimBrush1Stop1.Duration = TimeSpan.FromSeconds(4);

            _colorAnimBrush1Stop2 = _compositor.CreateColorKeyFrameAnimation();
            _colorAnimBrush1Stop2.InsertKeyFrame(.5f, Colors.DeepPink);
            _colorAnimBrush1Stop2.Duration = TimeSpan.FromSeconds(4);

            _offsetAnim = _compositor.CreateScalarKeyFrameAnimation();
            _offsetAnim.Duration = TimeSpan.FromSeconds(1);

            // when the buttons of text are pressed, the brush will change colors. Below is the set up for animation
            _changeb1stop1 = _compositor.CreateColorKeyFrameAnimation();
            _changeb1stop1.InsertKeyFrame(.5f, Colors.LightSkyBlue);
            _changeb1stop1.Duration = TimeSpan.FromSeconds(2);

            _changeb1stop2 = _compositor.CreateColorKeyFrameAnimation();
            _changeb1stop2.InsertKeyFrame(.5f, Colors.Teal);
            _changeb1stop2.Duration = TimeSpan.FromSeconds(2);
        }

        /*
         * Run animation on target visual brush
         */

        private void Pointer_Pressed(object sender, RoutedEventArgs e)
        {
            pointerPressedChangeColors(b1GradientStop1, b1GradientStop2);
        }

        private void pointerPressedChangeColors(CompositionColorGradientStop target, CompositionColorGradientStop target1)
        {

            if (target.Color == Colors.DeepPink || target.Color == Colors.Honeydew)
            {
                target.StartAnimation(nameof(target.Color), _changeb1stop1);
                target1.StartAnimation(nameof(target1.Color), _changeb1stop2);
            }
            else if (target.Color == Colors.LightSkyBlue || target.Color == Colors.Teal)
            {
                target1.StartAnimation(nameof(target1.Color), _colorAnimBrush1Stop1);
                target.StartAnimation(nameof(target.Color), _colorAnimBrush1Stop2);
            }
        }

        private void AnimateGradient(SpriteVisual target)
        {
            target.StartAnimation("Scale", _scaleAnim);
        }

        private void AnimateBrushStop1(CompositionColorGradientStop target, CompositionColorGradientStop target1)
        {
            if (target.Color == Colors.DeepPink)
            {
                target.StartAnimation(nameof(target.Color), _colorAnimBrush1Stop1);
                target1.StartAnimation(nameof(target1.Color), _colorAnimBrush1Stop2);
            } else if (target.Color == Colors.Teal)
            {
                target.StartAnimation(nameof(target.Color), _changeb1stop1);
                target1.StartAnimation(nameof(target1.Color), _changeb1stop2);
            }
        }

        private void ChangeBrushBack(CompositionColorGradientStop target, CompositionColorGradientStop target1)
        {
            if (target.Color == Colors.Honeydew)
            {
                target1.StartAnimation(nameof(target1.Color), _colorAnimBrush1Stop1);
                target.StartAnimation(nameof(target.Color), _colorAnimBrush1Stop2);
            } else if (target.Color == Colors.LightSkyBlue)
            {
                target1.StartAnimation(nameof(target1.Color), _changeb1stop1);
                target.StartAnimation(nameof(target.Color), _changeb1stop2);
            }
        }

        private void AnimateOffset(SpriteVisual target)
        {
            float endPoint = target.Size.X;
            float startPoint = target.Size.X - target.Size.X;

            if (target.Offset.X == startPoint)
            {
                target.AnchorPoint = new Vector2(1f, 0f);
                _offsetAnim.InsertKeyFrame(1, endPoint);

            }
            else if (target.Offset.X == endPoint)
            {
                target.AnchorPoint = new Vector2(0f, 0f);
                _offsetAnim.InsertKeyFrame(1, startPoint);
            }
            target.StartAnimation("Offset.X", _offsetAnim);

        }

        private void c1_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateGradient(_vis1);
            AnimateBrushStop1(b1GradientStop1, b1GradientStop2);
            AnimateOffset(_vis1);
        }

        private void c2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateGradient(_vis2);
            AnimateOffset(_vis2);
        }

        private void c3_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateGradient(_vis3);
            AnimateOffset(_vis3);
        }

        private void c4_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateGradient(_vis4);
            ChangeBrushBack(b1GradientStop1, b1GradientStop2);
            AnimateOffset(_vis4);
        }
    }
}