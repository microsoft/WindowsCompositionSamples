using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Diagnostics;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery.Samples.BrushInterop
{
    public sealed class ImageEffectBrush : XamlCompositionBrushBase
    {
        private LoadedImageSurface _surface;
        private CompositionSurfaceBrush _surfaceBrush;

        public static readonly DependencyProperty ImageUriStringProperty = DependencyProperty.Register(
           "ImageUri",
           typeof(string),
           typeof(ImageEffectBrush),
           new PropertyMetadata(String.Empty, new PropertyChangedCallback(OnImageUriStringChanged))
           );

        public string ImageUriString
        {
            get { return (String)GetValue(ImageUriStringProperty); }
            set { SetValue(ImageUriStringProperty, value); }
        }

        private static void OnImageUriStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = (ImageEffectBrush)d;
            // Unbox and update surface if CompositionBrush exists     
            if (brush._surfaceBrush != null)
            {
                var newSurface = LoadedImageSurface.StartLoadFromUri(new Uri((String)e.NewValue));
                brush._surface = newSurface;
                brush._surfaceBrush.Surface = newSurface;
            }       
        }

        protected override void OnConnected()
        {
            // return if Uri String is null or empty
            if (String.IsNullOrEmpty(ImageUriString))
            {
                return;
            }

            Compositor compositor = Window.Current.Compositor;

            // Use LoadedImageSurface API to get ICompositionSurface from image uri provided
            _surface = LoadedImageSurface.StartLoadFromUri(new Uri(ImageUriString));

            // Load Surface onto SurfaceBrush
            _surfaceBrush = compositor.CreateSurfaceBrush(_surface);
            _surfaceBrush.Stretch = CompositionStretch.UniformToFill;

            // CompositionCapabilities: Are Tint+Temperature and Saturation supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            if (usingFallback)
            {
                // If Effects are not supported, Fallback to image without effects
                CompositionBrush = _surfaceBrush;
                return;
            }

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
            effectBrush.SetSourceParameter("Surface", _surfaceBrush);

            // Set EffectBrush to paint Xaml UIElement
            CompositionBrush = effectBrush;

            // Trivial looping animation to demonstrate animated effect
            ScalarKeyFrameAnimation tempAnim = compositor.CreateScalarKeyFrameAnimation();
            tempAnim.InsertKeyFrame(0, 0);
            tempAnim.InsertKeyFrame(0.5f, 1f);
            tempAnim.InsertKeyFrame(1, 0);
            tempAnim.Duration = TimeSpan.FromSeconds(5);
            tempAnim.IterationBehavior = Windows.UI.Composition.AnimationIterationBehavior.Forever;
            effectBrush.Properties.StartAnimation("TempAndTint.Temperature", tempAnim);
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
