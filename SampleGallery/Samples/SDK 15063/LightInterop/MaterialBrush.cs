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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition.Effects;
using Windows.UI;
using System.Numerics;
using Windows.Foundation;

namespace CompositionSampleGallery.Samples.LightInterop

{
    class MaterialBrush : XamlCompositionBrushBase
    {
        private LoadedImageSurface _surface;

        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // CompositionCapabilities: Are Effects supported?
            bool usingFallback = !CompositionCapabilities.GetForCurrentView().AreEffectsSupported();
            FallbackColor = Color.FromArgb(100, 60, 60, 60);

            if (usingFallback)
            {
                // If Effects are not supported, use Fallback Solid Color
                CompositionBrush = compositor.CreateColorBrush(FallbackColor);
                return;
            }

            // Load NormalMap onto an ICompositionSurface using LoadedImageSurface
            _surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/NormalMapsAndMasks/bubblenm.jpg"), new Size(400,400));

            // Load Surface onto SurfaceBrush
            CompositionSurfaceBrush normalMap = compositor.CreateSurfaceBrush(_surface);
            normalMap.Stretch = CompositionStretch.Uniform;

            // Define Effect graph
            const float glassLightAmount = 0.5f;
            const float glassBlurAmount = 0.95f;
            Color tintColor = Color.FromArgb(255, 128, 128, 128);

            var graphicsEffect = new ArithmeticCompositeEffect()
            {
                Name = "LightComposite",
                Source1Amount = 1,
                Source2Amount = glassLightAmount,
                MultiplyAmount = 0,
                Source1 = new ArithmeticCompositeEffect()
                {
                    Name = "BlurComposite",
                    Source1Amount = 1 - glassBlurAmount,
                    Source2Amount = glassBlurAmount,
                    MultiplyAmount = 0,
                    Source1 = new ColorSourceEffect()
                    {
                        Name = "Tint",
                        Color = tintColor,
                    },
                    Source2 = new GaussianBlurEffect()
                    {
                        BlurAmount = 20,
                        Source = new CompositionEffectSourceParameter("Backdrop"),
                        Optimization = EffectOptimization.Balanced,
                        BorderMode = EffectBorderMode.Hard,
                    },
                },
                Source2 = new SceneLightingEffect()
                {
                    AmbientAmount = 0.15f,
                    DiffuseAmount = 1,
                    SpecularAmount = 0.1f,
                    NormalMapSource = new CompositionEffectSourceParameter("NormalMap")
                },
            };

            // Create EffectFactory and EffectBrush
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(graphicsEffect);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            
            // Create BackdropBrush
            CompositionBackdropBrush backdrop = compositor.CreateBackdropBrush();

            // Set Sources to Effect
            effectBrush.SetSourceParameter("NormalMap", normalMap);
            effectBrush.SetSourceParameter("Backdrop", backdrop);

            // Set EffectBrush as the brush that XamlCompBrushBase paints onto Xaml UIElement
            CompositionBrush = effectBrush;
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
