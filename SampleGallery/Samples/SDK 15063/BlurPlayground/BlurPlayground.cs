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

using CompositionSampleGallery.Shared;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;

namespace CompositionSampleGallery
{
    public sealed partial class BlurPlayground : SamplePage
    {
        private CompositionEffectBrush  _brush;
        private Compositor              _compositor;
       
        public BlurPlayground()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            this.InitializeComponent();
        }

        public static string   StaticSampleName     { get { return "Blur Playground"; } }
        public override string SampleName           { get { return StaticSampleName; } }
        public override string SampleDescription    { get { return "Windows UI Composition lets you provide rich effects to help users to focus on the right place and be more productive. Blur is a great way to get distractions out of the way and let user focus on the piece of content that is the most important to them. You can also easily animate blur properties using implicit animations. "; } }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate the BlendEffectMode combobox
            IList<ComboBoxItem> blendList = new List<ComboBoxItem>();

            foreach (BlendEffectMode type in Enum.GetValues(typeof(BlendEffectMode)))
            {
                // Exclude unsupported types
                if (type != BlendEffectMode.Dissolve &&
                    type != BlendEffectMode.Saturation &&
                    type != BlendEffectMode.Color &&
                    type != BlendEffectMode.Hue &&
                    type != BlendEffectMode.Luminosity
                    )
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Tag = type;
                    item.Content = type.ToString();
                    blendList.Add(item);
                }
            }

            BlendSelection.ItemsSource = blendList;
            BlendSelection.SelectedIndex = 0;

            BitmapImage image = new BitmapImage(new Uri("ms-appx:///Assets/Landscapes/Landscape-7.jpg"));
            BackgroundImage.Source = image;
            
                         
        }

        private void BlendSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = BlendSelection.SelectedValue as ComboBoxItem;
            BlendEffectMode blendmode = (BlendEffectMode)item.Tag;

            // Create a chained effect graph using a BlendEffect, blending color and blur
            var graphicsEffect = new BlendEffect
            {
                Mode = blendmode,
                Background = new ColorSourceEffect()
                {
                    Name = "Tint",
                    Color = Tint.Color,
                },

                Foreground = new GaussianBlurEffect()
                {
                    Name = "Blur",
                    Source = new CompositionEffectSourceParameter("Backdrop"),
                    BlurAmount = (float)BlurAmount.Value,
                    BorderMode = EffectBorderMode.Hard,
                }
            };

            var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect, 
                new[] { "Blur.BlurAmount", "Tint.Color"});

            // Create EffectBrush, BackdropBrush and SpriteVisual
            _brush = blurEffectFactory.CreateBrush();

            SetUpAnimationBehavior();

            // If the animation is running, restart it on the new brush
            if (AnimateToggle.IsOn)
            {
                StartBlurAnimation();
            }

            var destinationBrush = _compositor.CreateBackdropBrush();
            _brush.SetSourceParameter("Backdrop", destinationBrush);

            var blurSprite = _compositor.CreateSpriteVisual();
            blurSprite.Size = new Vector2((float)BackgroundImage.ActualWidth, (float)BackgroundImage.ActualHeight);
            blurSprite.Brush = _brush;

            ElementCompositionPreview.SetElementChildVisual(BackgroundImage, blurSprite);
        }

        private void SetUpAnimationBehavior()
        {
            //setup Implicit Animation for BlurAmount change and Color Change. 

            var implicitAnimations = _compositor.CreateImplicitAnimationCollection();

            //Define animations to animate blur and color change. 
            ScalarKeyFrameAnimation blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
            blurAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            blurAnimation.Duration = TimeSpan.FromSeconds(1);
            blurAnimation.Target = "Blur.BlurAmount";

            ColorKeyFrameAnimation tintAnimation = _compositor.CreateColorKeyFrameAnimation();
            tintAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            tintAnimation.Duration = TimeSpan.FromSeconds(1);
            tintAnimation.Target = "Tint.Color";

            implicitAnimations["Blur.BlurAmount"] = blurAnimation;
            implicitAnimations["Tint.Color"] = tintAnimation;

            //Associate implicit animations to property sets. 
            _brush.Properties.ImplicitAnimations = implicitAnimations;

        }


        private void BackgroundImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SpriteVisual blurVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(BackgroundImage);

            if (blurVisual != null)
            {
                blurVisual.Size = e.NewSize.ToVector2();
            }

        }

        private void BlurAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Get slider value
            var blur_amount = (float)e.NewValue;

            // Set new BlurAmount
            _brush.Properties.InsertScalar("Blur.BlurAmount", blur_amount);
        }
            
        private void ColorChanged(object sender, ColorEventArgs e)
        {
            if (_brush != null)
            {
                // Get color value
                _brush.Properties.InsertColor("Tint.Color", e.NewColor);
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (AnimateToggle.IsOn)
            {
                StartBlurAnimation();
            }
            else
            {
                _brush.StopAnimation("Blur.BlurAmount");
            }
        }

        private void StartBlurAnimation()
        {
            ScalarKeyFrameAnimation blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
            blurAnimation.InsertKeyFrame(0.0f, 0.0f);
            blurAnimation.InsertKeyFrame(0.5f, 100.0f);
            blurAnimation.InsertKeyFrame(1.0f, 0.0f);
            blurAnimation.Duration = TimeSpan.FromSeconds(4);
            blurAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            _brush.StartAnimation("Blur.BlurAmount", blurAnimation);
        }
    }
}