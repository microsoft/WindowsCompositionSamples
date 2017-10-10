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
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    class HostBackdropEffectBrush : XamlCompositionBrushBase
    {
        ColorKeyFrameAnimation _stateChangeAnimation;

        public static readonly DependencyProperty OverlayColorProperty = DependencyProperty.Register(
            "OverlayColor",
            typeof(Color),
            typeof(HostBackdropEffectBrush),
            new PropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnOverlayColorChanged))
            );

        public Color OverlayColor
        {
            get { return (Color)GetValue(OverlayColorProperty); }
            set { SetValue(OverlayColorProperty, value); }
        }

        private static void OnOverlayColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = (HostBackdropEffectBrush)d;
            // Unbox and set a new OverlayColor if the CompositionBrush exists
            brush.CompositionBrush?.Properties.InsertColor("OverlayColor.Color", (Color)e.NewValue);
        }

        override protected void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are HostBackdrop Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsFast();
            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Define Effect graph
            var graphicsEffect = new BlendEffect
            {
                Mode = BlendEffectMode.Overlay,
                Background = new CompositionEffectSourceParameter("Backdrop"),
                Foreground = new ColorSourceEffect()
                {
                    Name = "OverlayColor",
                    Color = OverlayColor,
                },                   
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[]{ "OverlayColor.Color"});
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create HostBackdropBrush, a kind of BackdropBrush that provides blurred backdrop content
            CompositionBackdropBrush hostBrush = compositor.CreateHostBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", hostBrush);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;

            // When the Window loses focus, animate HostBackdrop to FallbackColor
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;

            // Configure color animation to for state change
            _stateChangeAnimation = compositor.CreateColorKeyFrameAnimation();
            _stateChangeAnimation.InsertKeyFrame(0, OverlayColor);
            _stateChangeAnimation.InsertKeyFrame(1, FallbackColor);
            _stateChangeAnimation.Duration = TimeSpan.FromSeconds(1);

        }

        private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
        {
            // Change animation direction depending on Window state and animate OverlayColor
            if (args.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _stateChangeAnimation.Direction = AnimationDirection.Normal;
            }
            else
            {
                _stateChangeAnimation.Direction = AnimationDirection.Reverse;
            }
            CompositionBrush?.Properties.StartAnimation("OverlayColor.Color", _stateChangeAnimation);
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