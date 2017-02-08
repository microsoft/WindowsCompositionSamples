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

using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using SamplesCommon.ImageLoader;
using System;
using System.ComponentModel;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class CompCapabilities : SamplePage, INotifyPropertyChanged
    {
        private Compositor _compositor;
        private SpriteVisual _circleImageVisual;
        private SpriteVisual _backgroundImageVisual;
        private CompositionCapabilities _capabilities;
        private CompositionSurfaceBrush _imageSurfaceBrush;
        private ContainerVisual _imageContainer;
        private string _capabilityText = "";
        private IImageLoader _imageLoader;
        private bool _containsCircleImage = false;

        public static string StaticSampleName { get { return "Composition Capabilities"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription
        {
            get
            {
                return "Demonstrates how to use the capabilities API to detect hardware capabilities, " +
                       "listen to capability changes, and adjust effect usage and UI based on hardware.";
            }
        }

        public CompCapabilities()
        {
            this.InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            _imageLoader = ImageLoaderFactory.CreateImageLoader(_compositor);

            // Get hardware capabilities and register changed event listener
            _capabilities = CompositionCapabilities.GetForCurrentView();
        }

        /// <summary>
        /// Handles property changes for data binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Binding for TextBlock description of effects applied
        /// </summary>
        public string CapabilityText
        {
            get
            {
                return _capabilityText;
            } 
            set
            {
                _capabilityText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CapabilityText)));
            }
        }

        /// <summary>
        /// Handles hardware capabilities changed updates
        /// </summary>
        private void HandleCapabilitiesChanged(CompositionCapabilities sender, object args)
        {
            UpdateAlbumArt();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundImageVisual = _compositor.CreateSpriteVisual();
            _imageContainer = _compositor.CreateContainerVisual();
            _imageSurfaceBrush = _compositor.CreateSurfaceBrush();
            _capabilities.Changed += HandleCapabilitiesChanged;

            ElementCompositionPreview.SetElementChildVisual(ImageCanvas, _imageContainer);

            // Load in image
            var uri = new Uri("ms-appx:///Assets/Landscapes/Landscape-7.jpg");
            var surface = await SurfaceLoader.LoadFromUri(uri);
            var imageSurface = _compositor.CreateSurfaceBrush(surface);

            _imageSurfaceBrush.Surface = imageSurface.Surface;
            _imageSurfaceBrush.Stretch = CompositionStretch.Fill;

            _imageContainer.Size = new Vector2((float)ImageCanvas.ActualWidth, (float)ImageCanvas.ActualHeight);

            _imageContainer.Children.InsertAtTop(_backgroundImageVisual);

            UpdateVisualSizeAndPosition();

            UpdateAlbumArt();
        }

        /// <summary>
        /// Updates the effects applied and visuals shown based on information retrieved from 
        /// calling the capabilities API
        /// </summary>
        private void UpdateAlbumArt()
        {
            if (_capabilities.AreEffectsSupported())
            {
                // 
                // If effects are supported, add effects to the background image and 
                // add a masked circle image in the center for better visual appearance.
                //

                if (!_containsCircleImage)
                {
                    // Create circle mask
                    var circleMaskSurface = _imageLoader.CreateCircleSurface(200, Colors.White);

                    // Create image visual to use as the circle-masked center image
                    _circleImageVisual = _compositor.CreateSpriteVisual();
                    _circleImageVisual.Size = new Vector2((float)ImageCanvas.ActualWidth / 2, (float)ImageCanvas.ActualHeight / 2);
                    var xOffset = (float)(ImageCanvas.ActualWidth / 2 - _circleImageVisual.Size.X / 2);
                    var yOffset = (float)(ImageCanvas.ActualHeight / 2 - _circleImageVisual.Size.Y / 2);
                    _circleImageVisual.Offset = new Vector3(xOffset, yOffset, 0);

                    // Create circle image surface
                    CompositionSurfaceBrush circleSurfaceBrush = _compositor.CreateSurfaceBrush();
                    circleSurfaceBrush.Surface = circleMaskSurface.Surface;
                    circleSurfaceBrush.Stretch = CompositionStretch.Uniform;

                    // Apply mask to visual
                    CompositionMaskBrush maskBrush = _compositor.CreateMaskBrush();
                    maskBrush.Source = _imageSurfaceBrush;
                    maskBrush.Mask = circleSurfaceBrush;

                    _circleImageVisual.Brush = maskBrush;

                    _imageContainer.Children.InsertAtTop(_circleImageVisual);
                    _containsCircleImage = true;
                }

                //
                // Create saturation effect, which will be either used alone if effects are slow, or chained 
                // with blur if effects are fast
                //
                var saturationEffect = new SaturationEffect
                {
                    Saturation = 0.3f,
                    Source = new CompositionEffectSourceParameter("SaturationSource")
                };

                if (_capabilities.AreEffectsFast())
                {
                    // Create blur effect and chain with saturation effect
                    GaussianBlurEffect chainedEffect = new GaussianBlurEffect()
                    {
                        Name = "Blur",
                        Source = saturationEffect, //takes saturation effect as input
                        BlurAmount = 6.0f,
                        BorderMode = EffectBorderMode.Hard,
                        Optimization = EffectOptimization.Balanced
                    };

                    CompositionEffectFactory chainedEffectFactory = _compositor.CreateEffectFactory(chainedEffect);
                    CompositionEffectBrush effectBrush = chainedEffectFactory.CreateBrush();

                    effectBrush.SetSourceParameter("SaturationSource", _imageSurfaceBrush);

                    _backgroundImageVisual.Brush = effectBrush;

                    CapabilityText = "Effects are supported and fast. Background image is blurred and desaturated.";
                }
                else
                {
                    // If effects are slow but supported use desaturation effect since it is less expensive than blur
                    CompositionEffectFactory saturationEffectFactory = _compositor.CreateEffectFactory(saturationEffect);
                    CompositionEffectBrush saturationBrush = saturationEffectFactory.CreateBrush();

                    saturationBrush.SetSourceParameter("SaturationSource", _imageSurfaceBrush);

                    _backgroundImageVisual.Brush = saturationBrush;

                    CapabilityText = "Effects are supported but not fast. Background image is desaturated.";
                }
            }
            else
            {
                //
                // If effects are not supported, just use the image as the background with no effects
                // and remove the center circle image to declutter the UI.
                //

                if(_containsCircleImage)
                {
                    _imageContainer.Children.Remove(_circleImageVisual);
                    _containsCircleImage = false;
                }
                
                _backgroundImageVisual.Brush = _imageSurfaceBrush;

                CapabilityText = "Effects not supported. No effects are applied.";
            }
        }

        /// <summary>
        /// Updates size and position of the two image visuals on grid size change
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualSizeAndPosition();
        }

        /// <summary>
        /// Resizes and centers the two image visuals based on their parent canvas
        /// </summary>
        private void UpdateVisualSizeAndPosition()
        {
            if(_backgroundImageVisual != null) { 
                _backgroundImageVisual.Size = new Vector2((float)ImageCanvas.ActualWidth, (float)ImageCanvas.ActualHeight);
            }

            if (_circleImageVisual != null)
            {
                _circleImageVisual.Size = new Vector2((float)ImageCanvas.ActualWidth / 2, (float)ImageCanvas.ActualHeight / 2);
                var xOffset = (float)(ImageCanvas.ActualWidth / 2 - _circleImageVisual.Size.X / 2);
                var yOffset = (float)(ImageCanvas.ActualHeight / 2 - _circleImageVisual.Size.Y / 2);
                _circleImageVisual.Offset = new Vector3(xOffset, yOffset, 0);
            }
        }

        /// <summary>
        /// Clean up resources on unload
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _capabilities.Changed -= HandleCapabilitiesChanged;

            if (_imageSurfaceBrush != null)
            {
                _imageLoader.Dispose();
                _imageSurfaceBrush.Dispose();
            }
        }
    }
}
