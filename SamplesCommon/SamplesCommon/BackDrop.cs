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
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

using Microsoft.Graphics.Canvas.Effects;

namespace SamplesCommon
{
    public class BackDrop : Control
    {
        Compositor m_compositor;
        SpriteVisual m_blurVisual;
        CompositionEffectBrush m_blurBrush;
        Visual m_rootVisual;

#if SDKVERSION_14393
        bool m_setUpExpressions;
        ManagedSurface m_noiseSurface;
#endif

        public BackDrop()
        {
            m_rootVisual = ElementCompositionPreview.GetElementVisual(this as UIElement);
            Compositor = m_rootVisual.Compositor;

            m_blurVisual = Compositor.CreateSpriteVisual();

#if SDKVERSION_14393
            CompositionEffectBrush brush = BuildBlurBrush();
            brush.SetSourceParameter("source", m_compositor.CreateBackdropBrush());
            m_blurBrush = brush;
            m_blurVisual.Brush = m_blurBrush;

            BlurAmount = 9;
            TintColor = Colors.Transparent;
#else
            m_blurBrush = Compositor.CreateColorBrush(Colors.White);
            m_blurVisual.Brush = m_blurBrush;
#endif
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
#if SDKVERSION_14393
                m_rootVisual.Properties.TryGetScalar(BlurAmountProperty, out value);
#endif
                return value;
            }
            set
            {
#if SDKVERSION_14393
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertScalar("Blur.BlurAmount", (float)value);
                }
                m_rootVisual.Properties.InsertScalar(BlurAmountProperty, (float)value);
#endif
            }
        }

        public Color TintColor
        {
            get
            {
                Color value;
#if SDKVERSION_14393
                m_rootVisual.Properties.TryGetColor("TintColor", out value);
#else
                value = ((CompositionColorBrush)m_blurBrush).Color;
#endif
                return value;
            }
            set
            {
#if SDKVERSION_14393
                if (!m_setUpExpressions)
                {
                    m_blurBrush.Properties.InsertColor("Color.Color", value);
                }
                m_rootVisual.Properties.InsertColor(TintColorProperty, value);
#else
                ((CompositionColorBrush)m_blurBrush).Color = value;
#endif
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

#if SDKVERSION_14393
            m_noiseSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/Noise.jpg"));
            m_noiseSurface.Brush.Stretch = CompositionStretch.UniformToFill;
            m_blurBrush.SetSourceParameter("NoiseImage", m_noiseSurface.Brush);
#endif
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= OnSizeChanged;
        }


        private void OnSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (m_blurVisual != null)
            {
                m_blurVisual.Size = new System.Numerics.Vector2((float)this.ActualWidth, (float)this.ActualHeight);
            }
        }

#if SDKVERSION_14393
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
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source"),
            };

            BlendEffect blendEffect = new BlendEffect
            {
                Background = blurEffect,
                Foreground = new ColorSourceEffect { Name = "Color", Color = Color.FromArgb(64, 255, 255, 255) },
                Mode = BlendEffectMode.SoftLight
            };

            SaturationEffect saturationEffect = new SaturationEffect
            {
                Source = blendEffect,
                Saturation = 1.75f,
            };

            BlendEffect finalEffect = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("NoiseImage"),
                Background = saturationEffect,
                Mode = BlendEffectMode.Screen,
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

#endif

    }
}
