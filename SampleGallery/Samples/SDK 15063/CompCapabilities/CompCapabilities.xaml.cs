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
        private ManagedSurface _surface;
        private ManagedSurface _circleMaskSurface;
        private ContainerVisual _imageContainer;
        private string _capabilityText = "";
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
            _capabilities.Changed += HandleCapabilitiesChanged;

            ElementCompositionPreview.SetElementChildVisual(ImageCanvas, _imageContainer);

            // Load the image
            _surface = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/Landscapes/Landscape-7.jpg"));
            _surface.Brush.Stretch = CompositionStretch.Fill;
            _circleMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
            _circleMaskSurface.Brush.Stretch = CompositionStretch.Uniform;

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
                    _circleMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);

                    // Create image visual to use as the circle-masked center image
                    _circleImageVisual = _compositor.CreateSpriteVisual();
                    _circleImageVisual.Size = new Vector2((float)ImageCanvas.ActualWidth / 2, (float)ImageCanvas.ActualHeight / 2);
                    var xOffset = (float)(ImageCanvas.ActualWidth / 2 - _circleImageVisual.Size.X / 2);
                    var yOffset = (float)(ImageCanvas.ActualHeight / 2 - _circleImageVisual.Size.Y / 2);
                    _circleImageVisual.Offset = new Vector3(xOffset, yOffset, 0);
                    
                    // Apply mask to visual
                    CompositionMaskBrush maskBrush = _compositor.CreateMaskBrush();
                    maskBrush.Source = _surface.Brush;
                    maskBrush.Mask = _circleMaskSurface.Brush;

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

                    effectBrush.SetSourceParameter("SaturationSource", _surface.Brush);

                    _backgroundImageVisual.Brush = effectBrush;

                    CapabilityText = "Effects are supported and fast. Background image is blurred and desaturated.";
                }
                else
                {
                    // If effects are slow but supported use desaturation effect since it is less expensive than blur
                    CompositionEffectFactory saturationEffectFactory = _compositor.CreateEffectFactory(saturationEffect);
                    CompositionEffectBrush saturationBrush = saturationEffectFactory.CreateBrush();

                    saturationBrush.SetSourceParameter("SaturationSource", _surface.Brush);

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
                
                _backgroundImageVisual.Brush = _surface.Brush;

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

            if (_surface != null)
            {
                _surface.Dispose();
                _surface = null;
            }

            if (_circleMaskSurface != null)
            {
                _circleMaskSurface.Dispose();
                _circleMaskSurface = null;
            }
        }
    }
}
