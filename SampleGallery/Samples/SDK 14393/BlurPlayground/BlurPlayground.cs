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

using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class BlurPlayground : SamplePage
    {
        private CompositionEffectBrush _brush;
        private Compositor _compositor;

        public BlurPlayground()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            this.InitializeComponent();
        }

        public static string    StaticSampleName => "Blur Playground"; 
        public override string  SampleName => StaticSampleName;
        public static string    StaticSampleDescription => "This is a place to play around with different blur and blend recipes";
        public override string  SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868995";

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
                    Source = new CompositionEffectSourceParameter("ImageSource"),
                    BlurAmount = (float)BlurAmount.Value,
                    BorderMode = EffectBorderMode.Hard,
                }
                };

       
            var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                new[] { "Blur.BlurAmount", "Tint.Color" });

            // Create EffectBrush to be painted on CompositionImage Control’s SpriteVisual
            _brush = blurEffectFactory.CreateBrush();
            _brush.SetSourceParameter("ImageSource", CatImage.SurfaceBrush);
            CatImage.Brush = _brush;

            //If the animation is running, restart it on the new brush
            if (AnimateToggle.IsOn)
            {
                StartBlurAnimation();
            }
      
        }
       private void BlurAmount_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Get slider value
            var blur_amount = (float)e.NewValue;

            // Set new BlurAmount
            _brush.Properties.InsertScalar("Blur.BlurAmount", blur_amount);
        }


        private void isEnabled (object sender, RangeBaseValueChangedEventArgs e)
        {
            _brush.StopAnimation("Blur.BlurAmount");
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
                BlurAmount.IsEnabled = false;
            }
            else
            {
                _brush.StopAnimation("Blur.BlurAmount");
                BlurAmount.IsEnabled = true;
                BlurAmount.Value = 0;
                _brush.Properties.InsertScalar("Blur.BlurAmount", 0);


               
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
