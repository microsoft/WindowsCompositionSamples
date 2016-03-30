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
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using SamplesCommon;
using SamplesCommon.ImageLoader;
using Microsoft.Graphics.Canvas.Effects;

namespace CompositionSampleGallery
{
    public sealed partial class ImageLightingPlayground : SamplePage
    {
        public ImageLightingPlayground()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        public static string        StaticSampleName    { get { return "Image Lighting Playground"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Experiment with different kinds of image lighting effects"; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/?LinkId=784900"; } }

        public enum LightType
        {
            NoLight,
            DistantDiffuse,
            DistantSpecular,
            PointDiffuse,
            PointSpecular,
            SpotDiffuse,
            SpotSpecular,

            NumLightTypes
        }


        private void SamplePage_Loaded(object sender, RoutedEventArgs e)
        {
            //get interop compositor
            _compositor = ElementCompositionPreview.GetElementVisual(MainGrid).Compositor;

            //get image brush and image visual
            _image = Image; 
            _imageVisual = _image.SpriteVisual;
            _imageBrush = _image.SurfaceBrush;

            //load height map to surface brush
            _imageLoader = ImageLoaderFactory.CreateImageLoader(_compositor);
            _normalMapSurface = _imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Samples/SDK Insider/ImageLightingPlayground/rocks_NM_height.png"));
            _normalMapBrush = _compositor.CreateSurfaceBrush();
            _normalMapBrush.Surface = _normalMapSurface.Surface;

            //set up lighting effects:

            // DISTANT DIFFUSE
            // Effect description
            var distantDiffuseDesc = new ArithmeticCompositeEffect
            {
                Source1 = new CompositionEffectSourceParameter("Image"),
                Source2 = new DistantDiffuseEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Source1Amount = 0,
                Source2Amount = 0,
                MultiplyAmount = 1
            };

            //Create (Light)EffectBrush from EffectFactory and specify animatable properties
            _distantDiffuseLightBrush = _compositor.CreateEffectFactory(
                distantDiffuseDesc,
                new[]
                {
                    "light.Azimuth",
                    "light.Elevation",
                    "light.DiffuseAmount",
                    "light.LightColor"
                }
            ).CreateBrush();

            //set source parameters to image and normal map
            _distantDiffuseLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _distantDiffuseLightBrush.SetSourceParameter(
                "NormalMap",
                _normalMapBrush);

            // DISTANT SPECULAR
            // Effect description
            var distantSpecularDesc = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("Image"),
                Background = new DistantSpecularEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Mode = BlendEffectMode.Screen
            };

            //Create (Light)EffectBrush from EffectFactory and specify animatable properties
            _distantSpecularLightBrush = _compositor.CreateEffectFactory(
                distantSpecularDesc,
                new[]
                {
                    "light.Azimuth",
                    "light.Elevation",
                    "light.SpecularExponent",
                    "light.SpecularAmount",
                    "light.LightColor"
                }
            ).CreateBrush();

            //set source parameters to image and normal map
            _distantSpecularLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _distantSpecularLightBrush.SetSourceParameter(
                "NormalMap",
                _normalMapBrush);

            // POINT DIFFUSE
            // Effect description
            var pointDiffuseDesc = new ArithmeticCompositeEffect
            {
                Source1 = new CompositionEffectSourceParameter("Image"),
                Source2 = new PointDiffuseEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Source1Amount = 0,
                Source2Amount = 0,
                MultiplyAmount = 1
            };

            //Create (Light)EffectBrush from EffectFactory and specify animatable properties
            _pointDiffuseLightBrush = _compositor.CreateEffectFactory(
                pointDiffuseDesc,
                new[]
                {
                    "light.LightPosition",
                    "light.DiffuseAmount",
                    "light.HeightMapScale",
                    "light.LightColor"
                }
            ).CreateBrush();

            // set source parameters to image and normal map
            _pointDiffuseLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _pointDiffuseLightBrush.SetSourceParameter(
                "NormalMap",
               _normalMapBrush);

            // POINT SPECUALR
            // Effect description
            var pointSpecularDesc = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("Image"),
                Background = new PointSpecularEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Mode = BlendEffectMode.Screen
            };

            //Create (Light)EffectBrush from EffectFactory and specify animatable properties
            _pointSpecularLightBrush = _compositor.CreateEffectFactory(
                pointSpecularDesc,
                new[]
                {
                    "light.LightPosition",
                    "light.SpecularExponent",
                    "light.SpecularAmount",
                    "light.HeightMapScale",
                    "light.LightColor"
                }
            ).CreateBrush();

            // set source parameters to image and normal map
            _pointSpecularLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _pointSpecularLightBrush.SetSourceParameter(
                "NormalMap",
               _normalMapBrush);

            //SPOT DIFFUSE
            // Effect description
            var spotDiffuseDesc = new ArithmeticCompositeEffect
            {
                Source1 = new CompositionEffectSourceParameter("Image"),
                Source2 = new SpotDiffuseEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Source1Amount = 0,
                Source2Amount = 0,
                MultiplyAmount = 1
            };

            //Create (Light)EffectBrush from EffectFactory and specify animatable properties
            _spotDiffuseLightBrush = _compositor.CreateEffectFactory(
                spotDiffuseDesc,
                new[]
                {
                    "light.LightPosition",
                    "light.LightTarget",
                    "light.Focus",
                    "light.LimitingConeAngle",
                    "light.DiffuseAmount",
                    "light.HeightMapScale",
                    "light.LightColor"
                }
            ).CreateBrush();

            // set source parameters to image and normal map
            _spotDiffuseLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _spotDiffuseLightBrush.SetSourceParameter(
                "NormalMap",
                _normalMapBrush);

            // SPOT SPECULAR
            // Effect description
            var spotSpecularDesc = new BlendEffect
            {
                Foreground = new CompositionEffectSourceParameter("Image"),
                Background = new SpotSpecularEffect
                {
                    Name = "light",
                    Source = new CompositionEffectSourceParameter("NormalMap")
                },
                Mode = BlendEffectMode.Screen
            };

            //Create (Light)EffectBrush from EffectFactory
            _spotSpecularLightBrush = _compositor.CreateEffectFactory(
                spotSpecularDesc,
                new[]
                {
                    "light.LightPosition",
                    "light.LightTarget",
                    "light.Focus",
                    "light.LimitingConeAngle",
                    "light.SpecularExponent",
                    "light.SpecularAmount",
                    "light.HeightMapScale",
                    "light.LightColor"
                }
            ).CreateBrush();

            // set source parameters to image and normal map
            _spotSpecularLightBrush.SetSourceParameter(
                "Image",
                _imageBrush);
            _spotSpecularLightBrush.SetSourceParameter(
                "NormalMap",
                _normalMapBrush);

            // For simplying UI states switch, put light parameter grids in an array
            _lightParamsGrids = new Grid[(int)LightType.NumLightTypes];
            _lightParamsGrids[(int)LightType.NoLight] = null;
            _lightParamsGrids[(int)LightType.DistantDiffuse] = DistantDiffuseParams;
            _lightParamsGrids[(int)LightType.DistantSpecular] = DistantSpecularParams;
            _lightParamsGrids[(int)LightType.PointDiffuse] = PointDiffuseParams;
            _lightParamsGrids[(int)LightType.PointSpecular] = PointSpecularParams;
            _lightParamsGrids[(int)LightType.SpotDiffuse] = SpotDiffuseParams;
            _lightParamsGrids[(int)LightType.SpotSpecular] = SpotSpecularParams;

            // Same as grids
            _lightBrushes = new CompositionBrush[(int)LightType.NumLightTypes];
            _lightBrushes[(int)LightType.NoLight] = _imageBrush;
            _lightBrushes[(int)LightType.DistantDiffuse] = _distantDiffuseLightBrush;
            _lightBrushes[(int)LightType.DistantSpecular] = _distantSpecularLightBrush;
            _lightBrushes[(int)LightType.PointDiffuse] = _pointDiffuseLightBrush;
            _lightBrushes[(int)LightType.PointSpecular] = _pointSpecularLightBrush;
            _lightBrushes[(int)LightType.SpotDiffuse] = _spotDiffuseLightBrush;
            _lightBrushes[(int)LightType.SpotSpecular] = _spotSpecularLightBrush;

            //Initialize values for all light types
            InitializeValues();

            
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_normalMapSurface != null)
            {
                _normalMapSurface.Dispose();
            }

            if (_imageLoader != null)
            {
                _imageLoader.Dispose();
            }
        }

        private void InitializeValues()
        {
            //update light type
            RefreshActiveLightType();
            //update distant diffuse defaults
            RefreshDistantDiffuseAzimuth();
            RefreshDistantDiffuseElevation();
            RefreshDistantDiffuseAmount();
            RefreshDistantDiffuseLightColor();
            //update distant specular defaults
            RefreshDistantSpecularAzimuth();
            RefreshDistantSpecularElevation();
            RefreshDistantSpecularExponent();
            RefreshDistantSpecularAmount();
            RefreshDistantSpecularLightColor();
            //update point diffuse defaults
            RefreshPointDiffuseLightPosition();
            RefreshPointDiffuseAmount();
            RefreshPointDiffuseHeightMapScale();
            RefreshPointDiffuseLightColor();
            //update point specular defaults
            RefreshPointSpecularLightPosition();
            RefreshPointSpecularExponent();
            RefreshPointSpecularAmount();
            RefreshPointSpecularHeightMapScale();
            RefreshPointSpecularLightColor();
            //update spot diffuse defaults
            RefreshSpotDiffuseLightPosition();
            RefreshSpotDiffuseLightTarget();
            RefreshSpotDiffuseFocus();
            RefreshSpotDiffuseLimitingConeAngle();
            RefreshSpotDiffuseAmount();
            RefreshSpotDiffuseHeightMapScale();
            RefreshSpotDiffuseLightColor();
            //update spot specular defaults
            RefreshSpotSpecularLightPosition();
            RefreshSpotSpecularLightTarget();
            RefreshSpotSpecularFocus();
            RefreshSpotSpecularLimitingConeAngle();
            RefreshSpotSpecularExponent();
            RefreshSpotSpecularAmount();
            RefreshSpotSpecularHeightMapScale();
            RefreshSpotSpecularLightColor();
        }

        //light type:
        private void RefreshActiveLightType()
        {
            foreach (Grid paramsGrid in _lightParamsGrids)
            {
                if (paramsGrid != null)
                {
                    paramsGrid.Visibility = Visibility.Collapsed;
                }
            }

            Grid selectedParamsGrid = _lightParamsGrids[(int)_activeLightType];
            if (selectedParamsGrid != null)
            {
                selectedParamsGrid.Visibility = Visibility.Visible;
            }

            _imageVisual.Brush = _lightBrushes[(int)_activeLightType];

            LightControls.UpdateLayout();
        }

        public int ActiveLightType
        {
            get
            {
                return (int)_activeLightType;
            }
            set
            {
                _activeLightType = (LightType)value;
                RefreshActiveLightType();
            }
        }

        //Distant diffuse:
        private void RefreshDistantDiffuseAzimuth()
        {
            _distantDiffuseLightBrush.Properties.InsertScalar(
                "light.Azimuth",
                (float)_distantDiffuseAzimuth);
        }

        private void RefreshDistantDiffuseElevation()
        {
            _distantDiffuseLightBrush.Properties.InsertScalar(
                "light.Elevation",
                (float)_distantDiffuseElevation);
        }

        private void RefreshDistantDiffuseAmount()
        {
            _distantDiffuseLightBrush.Properties.InsertScalar(
                "light.DiffuseAmount",
                (float)_distantDiffuseAmount);
        }

        private void RefreshDistantDiffuseLightColor()
        {
            _distantDiffuseLightBrush.Properties.InsertColor(
                "light.LightColor",
                _distantDiffuseLightColor);
        }

        public double DistantDiffuseAzimuthValue
        {
            get
            {
                return _distantDiffuseAzimuth / (Math.PI * 2);
            }
            set
            {
                _distantDiffuseAzimuth = value * Math.PI * 2;
                RefreshDistantDiffuseAzimuth();
            }
        }

        public double DistantDiffuseElevationValue
        {
            get
            {
                return _distantDiffuseElevation / Math.PI;
            }
            set
            {
                _distantDiffuseElevation = value * Math.PI;
                RefreshDistantDiffuseElevation();
            }
        }

        public double DistantDiffuseAmountValue
        {
            get
            {
                return _distantDiffuseAmount;
            }
            set
            {
                _distantDiffuseAmount = value;
                RefreshDistantDiffuseAmount();
            }
        }

        public double DistantDiffuseLightColorRValue
        {
            get
            {
                return _distantDiffuseLightColor.R / 255.0;
            }
            set
            {
                _distantDiffuseLightColor.R = (byte)(value * 255);
                RefreshDistantDiffuseLightColor();
            }
        }

        public double DistantDiffuseLightColorGValue
        {
            get
            {
                return _distantDiffuseLightColor.G / 255.0;
            }
            set
            {
                _distantDiffuseLightColor.G = (byte)(value * 255);
                RefreshDistantDiffuseLightColor();
            }
        }

        public double DistantDiffuseLightColorBValue
        {
            get
            {
                return _distantDiffuseLightColor.B / 255.0;
            }
            set
            {
                _distantDiffuseLightColor.B = (byte)(value * 255);
                RefreshDistantDiffuseLightColor();
            }
        }

        //Distant Specular:
        private void RefreshDistantSpecularAzimuth()
        {
            _distantSpecularLightBrush.Properties.InsertScalar(
                "light.Azimuth",
                (float)_distantSpecularAzimuth);
        }

        private void RefreshDistantSpecularElevation()
        {
            _distantSpecularLightBrush.Properties.InsertScalar(
                "light.Elevation",
                (float)_distantSpecularElevation);
        }

        private void RefreshDistantSpecularExponent()
        {
            _distantSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularExponent",
                (float)_distantSpecularExponent);
        }

        private void RefreshDistantSpecularAmount()
        {
            _distantSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularAmount",
                (float)_distantSpecularAmount);
        }

        private void RefreshDistantSpecularLightColor()
        {
            _distantSpecularLightBrush.Properties.InsertColor(
                "light.LightColor",
                _distantSpecularLightColor);
        }

        public double DistantSpecularAzimuthValue
        {
            get
            {
                return _distantSpecularAzimuth / (Math.PI * 2);
            }
            set
            {
                _distantSpecularAzimuth = value * Math.PI * 2;
                RefreshDistantSpecularAzimuth();
            }
        }

        public double DistantSpecularElevationValue
        {
            get
            {
                return _distantSpecularElevation / Math.PI;
            }
            set
            {
                _distantSpecularElevation = value * Math.PI;
                RefreshDistantSpecularElevation();
            }
        }

        public double DistantSpecularExponentValue
        {
            get
            {
                return _distantSpecularExponent;
            }
            set
            {
                _distantSpecularExponent = value;
                RefreshDistantSpecularExponent();
            }
        }

        public double DistantSpecularAmountValue
        {
            get
            {
                return _distantSpecularAmount;
            }
            set
            {
                _distantSpecularAmount = value;
                RefreshDistantSpecularAmount();
            }
        }

        public double DistantSpecularLightColorRValue
        {
            get
            {
                return _distantSpecularLightColor.R / 255.0;
            }
            set
            {
                _distantSpecularLightColor.R = (byte)(value * 255);
                RefreshDistantSpecularLightColor();
            }
        }

        public double DistantSpecularLightColorGValue
        {
            get
            {
                return _distantSpecularLightColor.G / 255.0;
            }
            set
            {
                _distantSpecularLightColor.G = (byte)(value * 255);
                RefreshDistantSpecularLightColor();
            }
        }

        public double DistantSpecularLightColorBValue
        {
            get
            {
                return _distantSpecularLightColor.B / 255.0;
            }
            set
            {
                _distantSpecularLightColor.B = (byte)(value * 255);
                RefreshDistantSpecularLightColor();
            }
        }

        //Point Diffuse:
        private void RefreshPointDiffuseLightPosition()
        {
            _pointDiffuseLightBrush.Properties.InsertVector3(
                "light.LightPosition",
                _pointDiffuseLightPosition);
        }

        private void RefreshPointDiffuseAmount()
        {
            _pointDiffuseLightBrush.Properties.InsertScalar(
                "light.DiffuseAmount",
                (float)_pointDiffuseAmount);
        }

        private void RefreshPointDiffuseHeightMapScale()
        {
            _pointDiffuseLightBrush.Properties.InsertScalar(
                "light.HeightMapScale",
                (float)_pointDiffuseHeightMapScale);
        }

        private void RefreshPointDiffuseLightColor()
        {
            _pointDiffuseLightBrush.Properties.InsertColor(
                "light.LightColor",
                _pointDiffuseLightColor);
        }

        public double PointDiffuseXValue
        {
            get
            {
                return _pointDiffuseLightPosition.X;
            }
            set
            {
                _pointDiffuseLightPosition.X = (float)value;
                RefreshPointDiffuseLightPosition();
            }
        }

        public double PointDiffuseYValue
        {
            get
            {
                return _pointDiffuseLightPosition.Y;
            }
            set
            {
                _pointDiffuseLightPosition.Y = (float)value;
                RefreshPointDiffuseLightPosition();
            }
        }

        public double PointDiffuseZValue
        {
            get
            {
                return _pointDiffuseLightPosition.Z;
            }
            set
            {
                _pointDiffuseLightPosition.Z = (float)value;
                RefreshPointDiffuseLightPosition();
            }
        }

        public double PointDiffuseAmountValue
        {
            get
            {
                return _pointDiffuseAmount;
            }
            set
            {
                _pointDiffuseAmount = value;
                RefreshPointDiffuseAmount();
            }
        }

        public double PointDiffuseHeightMapScaleValue
        {
            get
            {
                return _pointDiffuseHeightMapScale;
            }
            set
            {
                _pointDiffuseHeightMapScale = value;
                RefreshPointDiffuseHeightMapScale();
            }
        }

        public double PointDiffuseLightColorRValue
        {
            get
            {
                return _pointDiffuseLightColor.R / 255.0;
            }
            set
            {
                _pointDiffuseLightColor.R = (byte)(value * 255);
                RefreshPointDiffuseLightColor();
            }
        }

        public double PointDiffuseLightColorGValue
        {
            get
            {
                return _pointDiffuseLightColor.G / 255.0;
            }
            set
            {
                _pointDiffuseLightColor.G = (byte)(value * 255);
                RefreshPointDiffuseLightColor();
            }
        }

        public double PointDiffuseLightColorBValue
        {
            get
            {
                return _pointDiffuseLightColor.B / 255.0;
            }
            set
            {
                _pointDiffuseLightColor.B = (byte)(value * 255);
                RefreshPointDiffuseLightColor();
            }
        }

        //Point Specular:
        private void RefreshPointSpecularLightPosition()
        {
            _pointSpecularLightBrush.Properties.InsertVector3(
                "light.LightPosition",
                _pointSpecularLightPosition);
        }

        private void RefreshPointSpecularExponent()
        {
            _pointSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularExponent",
                (float)_pointSpecularExponent);
        }

        private void RefreshPointSpecularAmount()
        {
            _pointSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularAmount",
                (float)_pointSpecularAmount);
        }

        private void RefreshPointSpecularHeightMapScale()
        {
            _pointSpecularLightBrush.Properties.InsertScalar(
                "light.HeightMapScale",
                (float)_pointSpecularHeightMapScale);
        }

        private void RefreshPointSpecularLightColor()
        {
            _pointSpecularLightBrush.Properties.InsertColor(
                "light.LightColor",
                _pointSpecularLightColor);
        }

        public double PointSpecularXValue
        {
            get
            {
                return _pointSpecularLightPosition.X;
            }
            set
            {
                _pointSpecularLightPosition.X = (float)value;
                RefreshPointSpecularLightPosition();
            }
        }

        public double PointSpecularYValue
        {
            get
            {
                return _pointSpecularLightPosition.Y;
            }
            set
            {
                _pointSpecularLightPosition.Y = (float)value;
                RefreshPointSpecularLightPosition();
            }
        }

        public double PointSpecularZValue
        {
            get
            {
                return _pointSpecularLightPosition.Z;
            }
            set
            {
                _pointSpecularLightPosition.Z = (float)value;
                RefreshPointSpecularLightPosition();
            }
        }

        public double PointSpecularExponentValue
        {
            get
            {
                return _pointSpecularExponent;
            }
            set
            {
                _pointSpecularExponent = value;
                RefreshPointSpecularExponent();
            }
        }

        public double PointSpecularAmountValue
        {
            get
            {
                return _pointSpecularAmount;
            }
            set
            {
                _pointSpecularAmount = value;
                RefreshPointSpecularAmount();
            }
        }

        public double PointSpecularHeightMapScaleValue
        {
            get
            {
                return _pointSpecularHeightMapScale;
            }
            set
            {
                _pointSpecularHeightMapScale = value;
                RefreshPointSpecularHeightMapScale();
            }
        }

        public double PointSpecularLightColorRValue
        {
            get
            {
                return _pointSpecularLightColor.R / 255.0;
            }
            set
            {
                _pointSpecularLightColor.R = (byte)(value * 255);
                RefreshPointSpecularLightColor();
            }
        }

        public double PointSpecularLightColorGValue
        {
            get
            {
                return _pointSpecularLightColor.G / 255.0;
            }
            set
            {
                _pointSpecularLightColor.G = (byte)(value * 255);
                RefreshPointSpecularLightColor();
            }
        }

        public double PointSpecularLightColorBValue
        {
            get
            {
                return _pointSpecularLightColor.B / 255.0;
            }
            set
            {
                _pointSpecularLightColor.B = (byte)(value * 255);
                RefreshPointSpecularLightColor();
            }
        }

        //Spot Diffuse:
        private void RefreshSpotDiffuseLightPosition()
        {
            _spotDiffuseLightBrush.Properties.InsertVector3(
                "light.LightPosition",
                _spotDiffuseLightPosition);
        }

        private void RefreshSpotDiffuseLightTarget()
        {
            _spotDiffuseLightBrush.Properties.InsertVector3(
                "light.LightTarget",
                _spotDiffuseLightTarget);
        }

        private void RefreshSpotDiffuseFocus()
        {
            _spotDiffuseLightBrush.Properties.InsertScalar(
                "light.Focus",
                (float)_spotDiffuseFocus);
        }

        private void RefreshSpotDiffuseLimitingConeAngle()
        {
            _spotDiffuseLightBrush.Properties.InsertScalar(
                "light.LimitingConeAngle",
                (float)_spotDiffuseLimitingConeAngle);
        }

        private void RefreshSpotDiffuseAmount()
        {
            _spotDiffuseLightBrush.Properties.InsertScalar(
                "light.DiffuseAmount",
                (float)_spotDiffuseAmount);
        }

        private void RefreshSpotDiffuseHeightMapScale()
        {
            _spotDiffuseLightBrush.Properties.InsertScalar(
                "light.HeightMapScale",
                (float)_spotDiffuseHeightMapScale);
        }

        private void RefreshSpotDiffuseLightColor()
        {
            _spotDiffuseLightBrush.Properties.InsertColor(
                "light.LightColor",
                _spotDiffuseLightColor);
        }

        public double SpotDiffuseXValue
        {
            get
            {
                return _spotDiffuseLightPosition.X;
            }
            set
            {
                _spotDiffuseLightPosition.X = (float)value;
                RefreshSpotDiffuseLightPosition();
            }
        }

        public double SpotDiffuseYValue
        {
            get
            {
                return _spotDiffuseLightPosition.Y;
            }
            set
            {
                _spotDiffuseLightPosition.Y = (float)value;
                RefreshSpotDiffuseLightPosition();
            }
        }

        public double SpotDiffuseZValue
        {
            get
            {
                return _spotDiffuseLightPosition.Z;
            }
            set
            {
                _spotDiffuseLightPosition.Z = (float)value;
                RefreshSpotDiffuseLightPosition();
            }
        }

        public double SpotDiffuseLightTargetXValue
        {
            get
            {
                return _spotDiffuseLightTarget.X;
            }
            set
            {
                _spotDiffuseLightTarget.X = (float)value;
                RefreshSpotDiffuseLightTarget();
            }
        }

        public double SpotDiffuseLightTargetYValue
        {
            get
            {
                return _spotDiffuseLightTarget.Y;
            }
            set
            {
                _spotDiffuseLightTarget.Y = (float)value;
                RefreshSpotDiffuseLightTarget();
            }
        }

        public double SpotDiffuseLightTargetZValue
        {
            get
            {
                return _spotDiffuseLightTarget.Z;
            }
            set
            {
                _spotDiffuseLightTarget.Z = (float)value;
                RefreshSpotDiffuseLightTarget();
            }
        }

        public double SpotDiffuseFocusValue
        {
            get
            {
                return _spotDiffuseFocus;
            }
            set
            {
                _spotDiffuseFocus = value;
                RefreshSpotDiffuseFocus();
            }
        }

        public double SpotDiffuseLimitingConeAngleValue
        {
            get
            {
                return _spotDiffuseLimitingConeAngle * (180 / Math.PI);
            }
            set
            {
                _spotDiffuseLimitingConeAngle = value * (Math.PI / 180);
                RefreshSpotDiffuseLimitingConeAngle();
            }
        }

        public double SpotDiffuseAmountValue
        {
            get
            {
                return _spotDiffuseAmount;
            }
            set
            {
                _spotDiffuseAmount = value;
                RefreshSpotDiffuseAmount();
            }
        }

        public double SpotDiffuseHeightMapScaleValue
        {
            get
            {
                return _spotDiffuseHeightMapScale;
            }
            set
            {
                _spotDiffuseHeightMapScale = value;
                RefreshSpotDiffuseHeightMapScale();
            }
        }

        public double SpotDiffuseLightColorRValue
        {
            get
            {
                return _spotDiffuseLightColor.R / 255.0;
            }
            set
            {
                _spotDiffuseLightColor.R = (byte)(value * 255);
                RefreshSpotDiffuseLightColor();
            }
        }

        public double SpotDiffuseLightColorGValue
        {
            get
            {
                return _spotDiffuseLightColor.G / 255.0;
            }
            set
            {
                _spotDiffuseLightColor.G = (byte)(value * 255);
                RefreshSpotDiffuseLightColor();
            }
        }

        public double SpotDiffuseLightColorBValue
        {
            get
            {
                return _spotDiffuseLightColor.B / 255.0;
            }
            set
            {
                _spotDiffuseLightColor.B = (byte)(value * 255);
                RefreshSpotDiffuseLightColor();
            }
        }


        //Spot Specular:
        private void RefreshSpotSpecularLightPosition()
        {
            _spotSpecularLightBrush.Properties.InsertVector3(
                "light.LightPosition",
                _spotSpecularLightPosition);
        }

        private void RefreshSpotSpecularLightTarget()
        {
            _spotSpecularLightBrush.Properties.InsertVector3(
                "light.LightTarget",
                _spotSpecularLightTarget);
        }

        private void RefreshSpotSpecularFocus()
        {
            _spotSpecularLightBrush.Properties.InsertScalar(
                "light.Focus",
                (float)_spotSpecularFocus);
        }

        private void RefreshSpotSpecularLimitingConeAngle()
        {
            _spotSpecularLightBrush.Properties.InsertScalar(
                "light.LimitingConeAngle",
                (float)_spotSpecularLimitingConeAngle);
        }

        private void RefreshSpotSpecularExponent()
        {
            _spotSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularExponent",
                (float)_spotSpecularExponent);
        }

        private void RefreshSpotSpecularAmount()
        {
            _spotSpecularLightBrush.Properties.InsertScalar(
                "light.SpecularAmount",
                (float)_spotSpecularAmount);
        }

        private void RefreshSpotSpecularHeightMapScale()
        {
            _spotSpecularLightBrush.Properties.InsertScalar(
                "light.HeightMapScale",
                (float)_spotSpecularHeightMapScale);
        }

        private void RefreshSpotSpecularLightColor()
        {
            _spotSpecularLightBrush.Properties.InsertColor(
                "light.LightColor",
                _spotSpecularLightColor);
        }
        
        public double SpotSpecularXValue
        {
            get
            {
                return _spotSpecularLightPosition.X;
            }
            set
            {
                _spotSpecularLightPosition.X = (float)value;
                RefreshSpotSpecularLightPosition();
            }
        }

        public double SpotSpecularYValue
        {
            get
            {
                return _pointSpecularLightPosition.Y;
            }
            set
            {
                _spotSpecularLightPosition.Y = (float)value;
                RefreshSpotSpecularLightPosition();
            }
        }

        public double SpotSpecularZValue
        {
            get
            {
                return _spotSpecularLightPosition.Z;
            }
            set
            {
                _spotSpecularLightPosition.Z = (float)value;
                RefreshSpotSpecularLightPosition();
            }
        }

        public double SpotSpecularLightTargetXValue
        {
            get
            {
                return _spotSpecularLightTarget.X;
            }
            set
            {
                _spotSpecularLightTarget.X = (float)value;
                RefreshSpotSpecularLightTarget();
            }
        }

        public double SpotSpecularLightTargetYValue
        {
            get
            {
                return _spotSpecularLightTarget.Y;
            }
            set
            {
                _spotSpecularLightTarget.Y = (float)value;
                RefreshSpotSpecularLightTarget();
            }
        }

        public double SpotSpecularLightTargetZValue
        {
            get
            {
                return _spotSpecularLightTarget.Z;
            }
            set
            {
                _spotSpecularLightTarget.Z = (float)value;
                RefreshSpotSpecularLightTarget();
            }
        }

        public double SpotSpecularFocusValue
        {
            get
            {
                return _spotSpecularFocus;
            }
            set
            {
                _spotSpecularFocus = value;
                RefreshSpotSpecularFocus();
            }
        }

        public double SpotSpecularLimitingConeAngleValue
        {
            get
            {
                return _spotSpecularLimitingConeAngle * (180 / Math.PI);
            }
            set
            {
                _spotSpecularLimitingConeAngle = value * (Math.PI / 180);
                RefreshSpotSpecularLimitingConeAngle();
            }
        }

        public double SpotSpecularExponentValue
        {
            get
            {
                return _spotSpecularExponent;
            }
            set
            {
                _spotSpecularExponent = value;
                RefreshSpotSpecularExponent();
            }
        }

        public double SpotSpecularAmountValue
        {
            get
            {
                return _spotSpecularAmount;
            }
            set
            {
                _spotSpecularAmount = value;
                RefreshSpotSpecularAmount();
            }
        }

        public double SpotSpecularHeightMapScaleValue
        {
            get
            {
                return _spotSpecularHeightMapScale;
            }
            set
            {
                _spotSpecularHeightMapScale = value;
                RefreshSpotSpecularHeightMapScale();
            }
        }

        public double SpotSpecularLightColorRValue
        {
            get
            {
                return _spotSpecularLightColor.R / 255.0;
            }
            set
            {
                _spotSpecularLightColor.R = (byte)(value * 255);
                RefreshSpotSpecularLightColor();
            }
        }

        public double SpotSpecularLightColorGValue
        {
            get
            {
                return _spotSpecularLightColor.G / 255.0;
            }
            set
            {
                _spotSpecularLightColor.G = (byte)(value * 255);
                RefreshSpotSpecularLightColor();
            }
        }

        public double SpotSpecularLightColorBValue
        {
            get
            {
                return _spotSpecularLightColor.B / 255.0;
            }
            set
            {
                _spotSpecularLightColor.B = (byte)(value * 255);
                RefreshSpotSpecularLightColor();
            }
        }

        //Animations:
        private ScalarKeyFrameAnimation CreateAzimuthAnimation()
        {
            var azimuthAnimation = _compositor.CreateScalarKeyFrameAnimation();
            azimuthAnimation.InsertExpressionKeyFrame(0.0f, "0.0f");
            azimuthAnimation.InsertExpressionKeyFrame(1.0f, "PI * 2");
            azimuthAnimation.Duration = TimeSpan.FromSeconds(5);
            azimuthAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            return azimuthAnimation;
        }

        private ScalarKeyFrameAnimation CreateElevationAnimation()
        {
            var elevationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            elevationAnimation.InsertExpressionKeyFrame(0.0f, "PI * 5 / 8");
            elevationAnimation.InsertExpressionKeyFrame(0.5f, "PI * 7 / 8");
            elevationAnimation.InsertExpressionKeyFrame(1.0f, "PI * 5 / 8");
            elevationAnimation.Duration = TimeSpan.FromSeconds(5);
            return elevationAnimation;
        }

        //Distant Diffuse:
        public bool DistantDiffuseAnimationOn
        {
            get
            {
                return _distantDiffuseAnimationOn;
            }
            set
            {
                _distantDiffuseAnimationOn = value;

                if (_distantDiffuseAnimationOn)
                {
                    DistantDiffuseAzimuth.IsEnabled = false;
                    DistantDiffuseElevation.IsEnabled = false;

                    // An animation for rotating the distant light

                    _distantDiffuseLightBrush.StartAnimation(
                        "light.Azimuth",
                        CreateAzimuthAnimation());

                    _distantDiffuseLightBrush.StartAnimation(
                         "light.Elevation",
                         CreateElevationAnimation());
                }
                else
                {
                    _distantDiffuseLightBrush.StopAnimation("light.Azimuth");
                    _distantDiffuseLightBrush.StopAnimation("light.Elevation");

                    DistantDiffuseAzimuth.IsEnabled = true;
                    DistantDiffuseElevation.IsEnabled = true;
                }
            }
        }

        //Distant Specular
        public bool DistantSpecularAnimationOn
        {
            get
            {
                return _distantSpecularAnimationOn;
            }
            set
            {
                _distantSpecularAnimationOn = value;

                if (_distantSpecularAnimationOn)
                {
                    DistantSpecularAzimuth.IsEnabled = false;
                    DistantSpecularElevation.IsEnabled = false;

                    // An animation for rotating the distant light

                    _distantSpecularLightBrush.StartAnimation(
                        "light.Azimuth",
                        CreateAzimuthAnimation());

                    _distantSpecularLightBrush.StartAnimation(
                        "light.Elevation",
                        CreateElevationAnimation());
                }
                else
                {
                    _distantSpecularLightBrush.StopAnimation("light.Azimuth");
                    _distantSpecularLightBrush.StopAnimation("light.Elevation");

                    DistantSpecularAzimuth.IsEnabled = true;
                    DistantSpecularElevation.IsEnabled = true;
                }
            }
        }

        private Vector3KeyFrameAnimation CreateXYAnimation(float z)
        {
            var xyAnimation = _compositor.CreateVector3KeyFrameAnimation();
            xyAnimation.InsertKeyFrame(0.0f, new Vector3(200, 200, z));
            xyAnimation.InsertKeyFrame(0.25f, new Vector3(700, 200, z));
            xyAnimation.InsertKeyFrame(0.50f, new Vector3(700, 700, z));
            xyAnimation.InsertKeyFrame(0.75f, new Vector3(200, 700, z));
            xyAnimation.InsertKeyFrame(1.0f, new Vector3(200, 200, z));
            xyAnimation.Duration = TimeSpan.FromSeconds(5);
            xyAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            return xyAnimation;
        }

        //Point Diffuse
        public bool PointDiffuseAnimationOn
        {
            get
            {
                return _pointDiffuseAnimationOn;
            }
            set
            {
                _pointDiffuseAnimationOn = value;

                if (_pointDiffuseAnimationOn)
                {
                    PointDiffuseX.IsEnabled = false;
                    PointDiffuseY.IsEnabled = false;
                    PointDiffuseZ.IsEnabled = false;

                    // An animation for moving the point light

                    _pointDiffuseLightBrush.StartAnimation(
                        "light.LightPosition",
                        CreateXYAnimation(500));
                }
                else
                {
                    _pointDiffuseLightBrush.StopAnimation("light.LightPosition");

                    PointDiffuseX.IsEnabled = true;
                    PointDiffuseY.IsEnabled = true;
                    PointDiffuseZ.IsEnabled = true;
                }
            }
        }

        //Point Specular
        public bool PointSpecularAnimationOn
        {
            get
            {
                return _pointSpecularAnimationOn;
            }
            set
            {
                if (PointSpecularAnimation.IsOn)
                {
                    PointSpecularX.IsEnabled = false;
                    PointSpecularY.IsEnabled = false;
                    PointSpecularZ.IsEnabled = false;

                    // An animation for moving the point light

                    _pointSpecularLightBrush.StartAnimation(
                        "light.LightPosition",
                        CreateXYAnimation(500));
                }
                else
                {
                    _pointSpecularLightBrush.StopAnimation("light.LightPosition");

                    PointSpecularX.IsEnabled = true;
                    PointSpecularY.IsEnabled = true;
                    PointSpecularZ.IsEnabled = true;
                }
            }
        }

        //Spot Diffuse
        public bool SpotDiffuseAnimationOn
        {
            get
            {
                return _spotDiffuseAnimationOn;
            }
            set
            {
                _spotDiffuseAnimationOn = value;

                if (_spotDiffuseAnimationOn)
                {
                    SpotDiffuseX.IsEnabled = false;
                    SpotDiffuseY.IsEnabled = false;
                    SpotDiffuseZ.IsEnabled = false;

                    // An animation for moving the spot light

                    _spotDiffuseLightBrush.StartAnimation(
                        "light.LightTarget",
                        CreateXYAnimation(0));
                }
                else
                {
                    _spotDiffuseLightBrush.StopAnimation("light.LightTarget");

                    SpotDiffuseX.IsEnabled = true;
                    SpotDiffuseY.IsEnabled = true;
                    SpotDiffuseZ.IsEnabled = true;
                }
            }
        }

        //Spot Specular
        public bool SpotSpecularAnimationOn
        {
            get
            {
                return _spotSpecularAnimationOn;
            }
            set
            {
                if (SpotSpecularAnimation.IsOn)
                {
                    SpotSpecularX.IsEnabled = false;
                    SpotSpecularY.IsEnabled = false;
                    SpotSpecularZ.IsEnabled = false;

                    // An animation for moving the spot light

                    _spotSpecularLightBrush.StartAnimation(
                        "light.LightTarget",
                        CreateXYAnimation(0));
                }
                else
                {
                    _spotSpecularLightBrush.StopAnimation("light.LightTarget");

                    SpotSpecularX.IsEnabled = true;
                    SpotSpecularY.IsEnabled = true;
                    SpotSpecularZ.IsEnabled = true;
                }
            }
        }


        private Compositor _compositor;
        private CompositionImage _image;
        private SpriteVisual _imageVisual;
        private IImageLoader _imageLoader;
        private IManagedSurface _normalMapSurface;
        private CompositionSurfaceBrush _normalMapBrush;

        private CompositionSurfaceBrush _imageBrush;
        private CompositionEffectBrush _distantDiffuseLightBrush;
        private CompositionEffectBrush _distantSpecularLightBrush;
        private CompositionEffectBrush _pointDiffuseLightBrush;
        private CompositionEffectBrush _pointSpecularLightBrush;
        private CompositionEffectBrush _spotDiffuseLightBrush;
        private CompositionEffectBrush _spotSpecularLightBrush;

        // distant diffuse initial values
        private double _distantDiffuseAzimuth = 0;
        private double _distantDiffuseElevation = Math.PI * 0.6;
        private double _distantDiffuseAmount = 1;
        private Color _distantDiffuseLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _distantDiffuseAnimationOn = false;

        // distant specular initial values
        private double _distantSpecularAzimuth = 0;
        private double _distantSpecularElevation = Math.PI * 0.6;
        private double _distantSpecularExponent = 16;
        private double _distantSpecularAmount = 1;
        private Color _distantSpecularLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _distantSpecularAnimationOn = false;

        // point diffuse initial values
        private Vector3 _pointDiffuseLightPosition = new Vector3(500, 500, 500);
        private double _pointDiffuseAmount = 1;
        private double _pointDiffuseHeightMapScale = 1;
        private Color _pointDiffuseLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _pointDiffuseAnimationOn = false;

        //point specular initial values
        private Vector3 _pointSpecularLightPosition = new Vector3(500, 500, 500);
        private double _pointSpecularExponent = 16;
        private double _pointSpecularAmount = 1;
        private double _pointSpecularHeightMapScale = 1;
        private Color _pointSpecularLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _pointSpecularAnimationOn = false;

        // spot diffuse initial values
        private Vector3 _spotDiffuseLightPosition = new Vector3(500, 500, 500);
        private Vector3 _spotDiffuseLightTarget = new Vector3(500, 500, 0);
        private double _spotDiffuseFocus = 1;
        private double _spotDiffuseLimitingConeAngle = Math.PI / 6;
        private double _spotDiffuseAmount = 1;
        private double _spotDiffuseHeightMapScale = 1;
        private Color _spotDiffuseLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _spotDiffuseAnimationOn = false;

        // spot specular initial values
        private Vector3 _spotSpecularLightPosition = new Vector3(500, 500, 500);
        private Vector3 _spotSpecularLightTarget = new Vector3(500, 500, 0);
        private double _spotSpecularFocus = 1;
        private double _spotSpecularLimitingConeAngle = Math.PI / 6;
        private double _spotSpecularExponent = 16;
        private double _spotSpecularAmount = 1;
        private double _spotSpecularHeightMapScale = 1;
        private Color _spotSpecularLightColor = Color.FromArgb(255, 255, 255, 255);
        private bool _spotSpecularAnimationOn = false;

        private Grid[] _lightParamsGrids;
        private CompositionBrush[] _lightBrushes;

        // initialize light type
        private LightType _activeLightType = LightType.NoLight;

    }
}
