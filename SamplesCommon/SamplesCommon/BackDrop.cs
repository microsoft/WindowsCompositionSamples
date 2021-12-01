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

using Windows.UI;

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

using Microsoft.Graphics.Canvas.Effects;

namespace SamplesCommon
{
    public class BackDrop : Control
    {
        Compositor m_compositor;
        SpriteVisual m_blurVisual;
        CompositionEffectBrush m_blurBrush;
        Visual m_rootVisual;

        bool m_setUpExpressions;
        ManagedSurface m_noiseSurface;

        public BackDrop()
        {
            m_rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            Compositor = m_rootVisual.Compositor;

            m_blurVisual = Compositor.CreateSpriteVisual();

            CompositionEffectBrush brush = BuildBlurBrush();
            brush.SetSourceParameter("source", m_compositor.CreateBackdropBrush());
            m_blurBrush = brush;
            m_blurVisual.Brush = m_blurBrush;

            BlurAmount = 9;
            TintColor = Microsoft.UI.Colors.Transparent;

            ElementCompositionPreview.SetElementChildVisual(this as UIElement, m_blurVisual);

            this.Loading += OnLoading;
            this.Unloaded += OnUnloaded;
        }

        public const string BlurAmountProperty = nameof(BlurAmount);
        public const string TintColorProperty = nameof(TintColor);

        public double BlurAmount
        {
            get
            {
                float value = 0;

                m_rootVisual.Properties.TryGetScalar(BlurAmountProperty, out value);

                return value;
            }
            set
            {
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
                m_rootVisual.Properties.InsertScalar(BlurAmountProperty, (float)value);
            }
        }

        public Color TintColor
        {
            get
            {
                Color value = Microsoft.UI.Colors.Purple;

                m_rootVisual.Properties.TryGetColor("TintColor", out value);

                return value;
            }
            set
            {
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertColor("Color.Color", value);
                }
                m_rootVisual.Properties.InsertColor(TintColorProperty, value);
            }
        }

        public Compositor Compositor
        {
            get
            {
                return m_compositor;
            }

            private set
            {
                m_compositor = value;
            }
        }

#pragma warning disable 1998
        private async void OnLoading(FrameworkElement sender, object args)
        {
            this.SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null);

            m_noiseSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/Noise.jpg"));
            m_noiseSurface.Brush.Stretch = CompositionStretch.UniformToFill;
            m_blurBrush.SetSourceParameter("NoiseImage", m_noiseSurface.Brush);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;
        }


        private void OnSizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            if (m_blurVisual != null)
            {
                m_blurVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
            }
        }


        private void SetUpPropertySetExpressions()
        {
            m_setUpExpressions = true;

            var exprAnimation = Compositor.CreateExpressionAnimation();
            exprAnimation.Expression = $"sourceProperties.{BlurAmountProperty}";
            exprAnimation.SetReferenceParameter("sourceProperties", m_rootVisual.Properties);

            m_blurBrush.Properties.StartAnimation("Blur.BlurAmount", exprAnimation);

            exprAnimation.Expression = $"sourceProperties.{TintColorProperty}";

            m_blurBrush.Properties.StartAnimation("Color.Color", exprAnimation);
        }


        private CompositionEffectBrush BuildBlurBrush()
        {
            Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect blurEffect = new Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = Microsoft.Graphics.Canvas.Effects.EffectBorderMode.Hard,
                Optimization = Microsoft.Graphics.Canvas.Effects.EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source"),
            };

            Microsoft.Graphics.Canvas.Effects.BlendEffect blendEffect = new Microsoft.Graphics.Canvas.Effects.BlendEffect
            {
                Background = blurEffect,
                Foreground = new Microsoft.Graphics.Canvas.Effects.ColorSourceEffect { Name = "Color", Color = Color.FromArgb(64, 255, 255, 255) },
                Mode = Microsoft.Graphics.Canvas.Effects.BlendEffectMode.SoftLight
            };

            Microsoft.Graphics.Canvas.Effects.SaturationEffect saturationEffect = new Microsoft.Graphics.Canvas.Effects.SaturationEffect
            {
                Source = blendEffect,
                Saturation = 1.75f,
            };

            Microsoft.Graphics.Canvas.Effects.BlendEffect finalEffect = new Microsoft.Graphics.Canvas.Effects.BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("NoiseImage"),
                Background = saturationEffect,
                Mode = Microsoft.Graphics.Canvas.Effects.BlendEffectMode.Screen,
            };

            var factory = Compositor.CreateEffectFactory(
                finalEffect,
                new[] { "Blur.BlurAmount", "Color.Color" }
                );

            return factory.CreateBrush();
        }

        public CompositionPropertySet VisualProperties
        {
            get
            {
                if (!m_setUpExpressions)
                {
                    SetUpPropertySetExpressions();
                }
                return m_rootVisual.Properties;
            }
        }

    }
}
