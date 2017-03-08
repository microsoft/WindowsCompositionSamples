using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Diagnostics;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    public sealed class ImageEffectBrush : XamlCompositionBrushBase
    {
        private LoadedImageSurface _surface;

        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Tint+Temperature and Saturation supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            FallbackColor = Colors.DimGray;

            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Load Image onto an ICompositionSurface using LoadedImageSurface
            _surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/Background.png"));
            
            // Load Surface onto SurfaceBrush
            var surfacebrush = compositor.CreateSurfaceBrush(_surface);
            surfacebrush.Stretch = CompositionStretch.UniformToFill;

            // Define Effect graph
            IGraphicsEffect graphicsEffect = new SaturationEffect
            {
                Name = "Saturation",
                Saturation = 0.3f,
                Source = new TemperatureAndTintEffect
                {
                    Name = "TempAndTint",
                    Temperature = 0,
                    Source = new CompositionEffectSourceParameter("Surface"),
                }
            };

            // Create EffectFactory and EffectBrush 
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect, new[] { "TempAndTint.Temperature" });
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("Surface", surfacebrush);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;

            // Trivial looping animation to target Temeperature property
            TimeSpan _duration = TimeSpan.FromSeconds(5);
            ScalarKeyFrameAnimation temperatureAnimation = compositor.CreateScalarKeyFrameAnimation();
            temperatureAnimation.InsertKeyFrame(0, 0);
            temperatureAnimation.InsertKeyFrame(0.5f, 1f);
            temperatureAnimation.InsertKeyFrame(1, 0);
            temperatureAnimation.Duration = _duration;
            temperatureAnimation.IterationBehavior = Windows.UI.Composition.AnimationIterationBehavior.Forever;
            effectBrush.Properties.StartAnimation("TempAndTint.Temperature", temperatureAnimation);
        }

        protected override void OnDisconnected()
        {
            // Dispose Surface and CompositionBrushes if XamlCompBrushBase is removed from tree
            if (_surface != null)
            {
                _surface.Dispose();
                _surface = null;
            }
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
                CompositionBrush = null;
            }
        }
    }
}
