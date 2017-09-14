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
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    class BackdropTintBlurBrush : XamlCompositionBrushBase
    {
        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Define Effect graph
            var graphicsEffect = new BlendEffect
            {
                Mode = BlendEffectMode.LinearBurn,
                Background = new ColorSourceEffect()
                {
                    Name = "Tint",
                    Color = Windows.UI.Colors.Silver,
                },
                Foreground = new GaussianBlurEffect()
                {
                    Name = "Blur",
                    Source = new CompositionEffectSourceParameter("Backdrop"),
                    BlurAmount = 0,
                    BorderMode = EffectBorderMode.Hard,
                }
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[] { "Blur.BlurAmount" });
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create BackdropBrush
            CompositionBackdropBrush backdrop = compositor.CreateBackdropBrush();
            effectBrush.SetSourceParameter("backdrop", backdrop);

            // Trivial looping animation to demonstrate animated effects
            TimeSpan _duration = TimeSpan.FromSeconds(5);
            ScalarKeyFrameAnimation blurAnimation = compositor.CreateScalarKeyFrameAnimation();
            blurAnimation.InsertKeyFrame(0, 0);
            blurAnimation.InsertKeyFrame(0.5f, 10f);
            blurAnimation.InsertKeyFrame(1, 0);
            blurAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            blurAnimation.Duration = _duration;
            effectBrush.Properties.StartAnimation("Blur.BlurAmount", blurAnimation);

            // Set EffectBrush to paint Xaml UIElement
            CompositionBrush = effectBrush;      
        }

        protected override void OnDisconnected()
        {
            // Dispose CompositionBrushes if XamlCompBrushBase is removed from tree
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }
        }
    }
}