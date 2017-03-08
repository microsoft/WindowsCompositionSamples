using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using System.Diagnostics;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    class GreyHostBackdropBrush : XamlCompositionBrushBase
    {
        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;
            
            // CompositionCapabilities: Are HostBackdrop Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsFast();
            FallbackColor = Colors.DimGray;

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
                Foreground = new ColorSourceEffect
                {
                    Name = "OverlayColor",
                    Color = Color.FromArgb(200, 20, 20, 20),
                }
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            // Create HostBackdropBrush, a kind of BackdropBrush that provides blurred backdrop content
            CompositionBackdropBrush hostbackdropbrush = compositor.CreateHostBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", hostbackdropbrush);

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
