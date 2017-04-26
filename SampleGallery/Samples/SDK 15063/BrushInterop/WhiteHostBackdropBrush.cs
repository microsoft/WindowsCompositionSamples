using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    class WhiteHostBackdropBrush : XamlCompositionBrushBase 
    {
        override protected void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are HostBackdrop Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsFast();
            FallbackColor = Colors.White;

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
                    Color = Color.FromArgb(100, 255, 255, 255),
                },                   
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create HostBackdropBrush, a kind of BackdropBrush that provides blurred backdrop content
            CompositionBackdropBrush hostBrush = compositor.CreateHostBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", hostBrush);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
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