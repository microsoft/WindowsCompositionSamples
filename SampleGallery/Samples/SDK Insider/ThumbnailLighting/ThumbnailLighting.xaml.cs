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

using CompositionSampleGallery.Shared;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class ThumbnailLighting : SamplePage
    {
        private Compositor                  _compositor;
        private CompositionEffectFactory    _effectFactory;
        private CompositionSurfaceBrush     _flatNormalsBrush;
        private CompositionSurfaceBrush     _circleNormalsBrush;
        private Vector3KeyFrameAnimation    _lightPositionAnimation;
        private ColorKeyFrameAnimation      _lightColorAnimation;
        private ScalarKeyFrameAnimation     _lightAzimuthAnimation;
        private ScalarKeyFrameAnimation     _lightElevationAnimation;

        public enum LightingTypes
        {
            PointDiffuse,
            PointSpecular,
            SpotLightDiffuse,
            SpotLightSpecular,
            DistantDiffuse,
            DistantSpecular,
        }

        public ThumbnailLighting()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
        }

        public static string    StaticSampleName    { get { return "Thumbnail Lighting"; } }
        public override string  SampleName          { get { return StaticSampleName; } }
        public override string  SampleDescription   { get { return "Demonstrates how to apply Image Lighting to ListView Items.  Switch between different combinations of light types(point, spot, distant) and lighting properties such as diffuse and specular."; } }
        public override string  SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761165"; } }

        public LocalDataSource Model { get; set; }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate the light type combobox
            IList<ComboBoxItem> lightList = new List<ComboBoxItem>();
            foreach (LightingTypes type in Enum.GetValues(typeof(LightingTypes)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = type;
                item.Content = type.ToString();
                lightList.Add(item);
            }

            LightingSelection.ItemsSource = lightList;
            LightingSelection.SelectedIndex = 0;
            
            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            ThumbnailList.ItemsSource = Model.Items;

            //
            // Create the sperical normal map.  The normals will give the appearance of a sphere, and the alpha channel is used
            // for masking off the rectangular edges.
            //

            CompositionDrawingSurface normalMap = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///Samples/SDK Insider/ThumbnailLighting/SphericalWithMask.png"));
            _circleNormalsBrush = _compositor.CreateSurfaceBrush(normalMap);
            _circleNormalsBrush.Stretch = CompositionStretch.Fill;


            // 
            // Create the flat normal map with beveled edges.  This should give the appearance of slanting of the surface along
            // the edges, flat in the middle.
            //

            normalMap = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///Samples/SDK Insider/ThumbnailLighting/BeveledEdges.jpg"));
            _flatNormalsBrush = _compositor.CreateSurfaceBrush(normalMap);
            _flatNormalsBrush.Stretch = CompositionStretch.Fill;

            // Update the effect brushes now that the normal maps are available.
            UpdateEffectBrush();
        }

        private void UpdateEffectBrush()
        {
            if (ThumbnailList.ItemsPanelRoot != null)
            {
                foreach (ListViewItem item in ThumbnailList.ItemsPanelRoot.Children)
                {
                    CompositionImage image = item.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
                    SetImageEffect(image);
                }
            }
        }
        
        private void UpdateAnimations()
        {
            Vector2 sizeLightBounds = new Vector2((float)RootPanel.ActualWidth, (float)RootPanel.ActualHeight + 200f);

            ComboBoxItem item = LightingSelection.SelectedValue as ComboBoxItem;
            switch ((LightingTypes)item.Tag)
            {
                case LightingTypes.PointDiffuse:
                case LightingTypes.PointSpecular:
                    {
                        // Create the light position animation
                        _lightPositionAnimation = _compositor.CreateVector3KeyFrameAnimation();
                        _lightPositionAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 100f));
                        _lightPositionAnimation.InsertKeyFrame(.25f, new Vector3(sizeLightBounds.X * .7f, sizeLightBounds.Y * .5f, 100f));
                        _lightPositionAnimation.InsertKeyFrame(.50f, new Vector3(sizeLightBounds.X, sizeLightBounds.Y, 100f));
                        _lightPositionAnimation.InsertKeyFrame(.75f, new Vector3(sizeLightBounds.X * .2f, sizeLightBounds.Y, 100f));
                        _lightPositionAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 100f));
                        _lightPositionAnimation.Duration = TimeSpan.FromMilliseconds(7500);
                        _lightPositionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        _lightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
                        _lightColorAnimation.InsertKeyFrame(0f, Colors.White);
                        _lightColorAnimation.InsertKeyFrame(.33f, Colors.White);
                        _lightColorAnimation.InsertKeyFrame(.66f, Colors.Yellow);
                        _lightColorAnimation.InsertKeyFrame(1f, Colors.White);
                        _lightColorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
                        _lightColorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                    }
                    break;

                case LightingTypes.SpotLightDiffuse:
                case LightingTypes.SpotLightSpecular:
                    {
                        // Create the light position animation
                        _lightPositionAnimation = _compositor.CreateVector3KeyFrameAnimation();
                        _lightPositionAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 100f));
                        _lightPositionAnimation.InsertKeyFrame(.33f, new Vector3(sizeLightBounds.X * .5f, sizeLightBounds.Y * .5f, 400f));
                        _lightPositionAnimation.InsertKeyFrame(.66f, new Vector3(sizeLightBounds.X, sizeLightBounds.Y * .5f, 1400f));
                        _lightPositionAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 100f));
                        _lightPositionAnimation.Duration = TimeSpan.FromMilliseconds(7500);
                        _lightPositionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        _lightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
                        _lightColorAnimation.InsertKeyFrame(0f, Colors.White);
                        _lightColorAnimation.InsertKeyFrame(.33f, Colors.White);
                        _lightColorAnimation.InsertKeyFrame(.66f, Colors.Yellow);
                        _lightColorAnimation.InsertKeyFrame(1f, Colors.White);
                        _lightColorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
                        _lightColorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                    }
                    break;

                case LightingTypes.DistantDiffuse:
                case LightingTypes.DistantSpecular:
                    {
                        _lightAzimuthAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        _lightAzimuthAnimation.InsertKeyFrame(0f, 0f);
                        _lightAzimuthAnimation.InsertKeyFrame(1f, (float)Math.PI * 2f);
                        _lightAzimuthAnimation.Duration = TimeSpan.FromMilliseconds(10000);
                        _lightAzimuthAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        _lightElevationAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        _lightElevationAnimation.InsertKeyFrame(0f, (float)Math.PI * .5f);
                        _lightElevationAnimation.InsertKeyFrame(.33f, (float)Math.PI * .25f);
                        _lightElevationAnimation.InsertKeyFrame(.66f, (float)Math.PI * .75f);
                        _lightElevationAnimation.InsertKeyFrame(1f, (float)Math.PI * .5f);
                        _lightElevationAnimation.Duration = TimeSpan.FromMilliseconds(5000);
                        _lightElevationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                    }
                    break;

                default:
                    break;
            }
        }

        private void SetImageEffect(CompositionImage image)
        {
            // Create the effect brush and bind the normal map
            CompositionEffectBrush brush = _effectFactory.CreateBrush();
            
            ComboBoxItem item = LightingSelection.SelectedValue as ComboBoxItem;
            switch ((LightingTypes)item.Tag)
            {
                case LightingTypes.SpotLightSpecular:
                case LightingTypes.PointSpecular:
                    brush.StartAnimation("Light1.LightPosition", _lightPositionAnimation);
                    brush.StartAnimation("Light1.LightColor", _lightColorAnimation);
                    brush.StartAnimation("Light2.LightPosition", _lightPositionAnimation);
                    brush.StartAnimation("Light2.LightColor", _lightColorAnimation);
                    brush.SetSourceParameter("NormalMap", _circleNormalsBrush);
                    break;
                case LightingTypes.DistantDiffuse:
                    brush.SetSourceParameter("NormalMap", _circleNormalsBrush);
                    brush.StartAnimation("Light1.Azimuth", _lightAzimuthAnimation);
                    brush.StartAnimation("Light1.Elevation", _lightElevationAnimation);
                    break;
                case LightingTypes.DistantSpecular:
                    brush.SetSourceParameter("NormalMap", _circleNormalsBrush);
                    brush.StartAnimation("Light1.Azimuth", _lightAzimuthAnimation);
                    brush.StartAnimation("Light1.Elevation", _lightElevationAnimation);
                    brush.StartAnimation("Light2.Azimuth", _lightAzimuthAnimation);
                    brush.StartAnimation("Light2.Elevation", _lightElevationAnimation);
                    break;
                default:
                    brush.StartAnimation("Light1.LightPosition", _lightPositionAnimation);
                    brush.StartAnimation("Light1.LightColor", _lightColorAnimation);
                    brush.SetSourceParameter("NormalMap", _flatNormalsBrush);
                    break;
            }

            // Update the CompositionImage to use the custom effect brush
            image.Brush = brush;
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            CompositionImage image = args.ItemContainer.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
            Thumbnail thumbnail = args.Item as Thumbnail;
            Uri uri = new Uri(thumbnail.ImageUrl);

            // Setup the brush for this image
            SetImageEffect(image);

            // Update the image URI
            image.Source = uri;
        }

        private void ThumbnailList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAnimations();
        }

        private void LightingSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLightingEffect();
        }

        private void UpdateLightingEffect()
        {
            if (_compositor == null)
            {
                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            }

            ComboBoxItem item = LightingSelection.SelectedValue as ComboBoxItem;
            switch ((LightingTypes)item.Tag)
            {
                case LightingTypes.PointDiffuse:
                    {
                        //
                        // Result = Ambient +       Diffuse
                        // Result = (Image) + (.75 * Diffuse color)
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.Add,
                            Sources =
                            {
                                new CompositionEffectSourceParameter("ImageSource"),
                                new PointDiffuseEffect()
                                {
                                    Name = "Light1",
                                    DiffuseAmount = .75f,
                                    LightPosition = new Vector3(0f, 0f, 100),
                                    LightColor = Colors.White,
                                    Source = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.LightPosition", "Light1.LightColor" });

                    }
                    break;

                case LightingTypes.PointSpecular:
                    {
                        //
                        // Result =    Ambient   +       Diffuse           +        Specular
                        // Result = (Image * .6) + (Image * Diffuse color) + (.75 * Specular color)
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.Add,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    Source1Amount = .6f,
                                    Source2Amount = 0,
                                    MultiplyAmount = 1,

                                    Source1 = new CompositeEffect()
                                    {
                                        Mode = CanvasComposite.DestinationIn,
                                        Sources =
                                        {
                                            new CompositionEffectSourceParameter("ImageSource"),
                                            new CompositionEffectSourceParameter("NormalMap"),
                                        }
                                    },
                                    Source2 = new PointDiffuseEffect()
                                    {
                                        Name = "Light1",
                                        DiffuseAmount = .75f,
                                        LightPosition = new Vector3(0f, 0f, 100),
                                        LightColor = Colors.White,
                                        Source = new CompositionEffectSourceParameter("NormalMap"),
                                    },
                                },
                                new PointSpecularEffect()
                                {
                                    Name = "Light2",
                                    SpecularAmount = 1f,
                                    SpecularExponent = 50f,
                                    LightPosition = new Vector3(0f, 0f, 100),
                                    LightColor = Colors.White,
                                    Source = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.LightPosition", "Light1.LightColor",
                                                    "Light2.LightPosition", "Light2.LightColor"});

                    }
                    break;

                case LightingTypes.SpotLightDiffuse:
                    {
                        //
                        // Result = Ambient +      Diffuse
                        // Result =  Image  + (Diffuse color *.6)
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.Add,
                            Sources =
                            {
                                new CompositionEffectSourceParameter("ImageSource"),
                                new SpotDiffuseEffect()
                                {
                                    Name = "Light1",
                                    DiffuseAmount = .6f,
                                    LimitingConeAngle = (float)Math.PI / 8f,
                                    LightTarget = new Vector3(1000, 1000, 100),
                                    LightPosition = new Vector3(1000f, 1000f, 1400),
                                    LightColor = Colors.White,
                                    Source = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.LightPosition", "Light1.LightColor" });
                    };
                    break;
                case LightingTypes.SpotLightSpecular:
                    {
                        //p0
                        // Result =    Ambient   +       Diffuse           +     Specular
                        // Result = (Image * .6) + (Image * Diffuse color) + (Specular color)
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.Add,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    Source1Amount  = .6f,
                                    Source2Amount  = 0,
                                    MultiplyAmount = 1,

                                    Source1 = new CompositeEffect()
                                    {
                                        Mode = CanvasComposite.DestinationIn,
                                        Sources =
                                        {
                                            new CompositionEffectSourceParameter("ImageSource"),
                                            new CompositionEffectSourceParameter("NormalMap"),
                                        }
                                    },

                                    Source2 = new SpotDiffuseEffect()
                                    {
                                        Name = "Light1",
                                        DiffuseAmount = 1f,
                                        LimitingConeAngle = (float)Math.PI / 8f,
                                        LightTarget = new Vector3(1000, 1000, 100),
                                        LightPosition = new Vector3(0f, 0f, 100),
                                        LightColor = Colors.White,
                                        Source = new CompositionEffectSourceParameter("NormalMap"),
                                    },
                                },
                                new SpotSpecularEffect()
                                {
                                    Name = "Light2",
                                    SpecularAmount = 1f,
                                    SpecularExponent = 50f,
                                    LimitingConeAngle = (float)Math.PI / 8f,
                                    LightTarget = new Vector3(1000, 1000, 100),
                                    LightPosition = new Vector3(0f, 0f, 100),
                                    LightColor = Colors.White,
                                    Source = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };


                        // Create the effect factory, we're going to animate the light positions and colors
                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.LightPosition", "Light1.LightColor",
                                                    "Light2.LightPosition", "Light2.LightColor" });

                    };
                    break;

                case LightingTypes.DistantDiffuse:
                    {
                        //
                        // Result =       Diffuse
                        // Result =  Image * Diffuse color
                        //

                        IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect()
                        {
                            Source1Amount  = 0,
                            Source2Amount  = 0,
                            MultiplyAmount = 1,

                            Source1 = new CompositeEffect()
                            {
                                Mode = CanvasComposite.DestinationIn,
                                Sources =
                                {
                                    new CompositionEffectSourceParameter("ImageSource"),
                                    new CompositionEffectSourceParameter("NormalMap"),
                                }
                            },

                            Source2 = new DistantDiffuseEffect()
                            {
                                Name = "Light1",
                                DiffuseAmount = 1f,
                                Elevation = 0f,
                                Azimuth = 0f,
                                LightColor = Colors.White,
                                Source = new CompositionEffectSourceParameter("NormalMap"),
                            },
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.Azimuth", "Light1.Elevation" });

                    };
                    break;
                case LightingTypes.DistantSpecular:
                    {
                        //
                        // Result =          Diffuse        +    Specular
                        // Result = (Image * Diffuse color) + (Specular color)
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.Add,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    Source1Amount  = 0,
                                    Source2Amount  = 0,
                                    MultiplyAmount = 1,
                                    Source1 = new CompositeEffect()
                                    {
                                        Mode = CanvasComposite.DestinationIn,
                                        Sources =
                                        {
                                            new CompositionEffectSourceParameter("ImageSource"),
                                            new CompositionEffectSourceParameter("NormalMap"),
                                        }
                                    },
                                    Source2 = new DistantDiffuseEffect()
                                    {
                                        Name = "Light1",
                                        DiffuseAmount = 1f,
                                        Elevation = 0f,
                                        Azimuth = 0f,
                                        LightColor = Colors.White,
                                        Source = new CompositionEffectSourceParameter("NormalMap"),
                                    }
                                },
                                new DistantSpecularEffect()
                                {
                                    Name = "Light2",
                                    SpecularAmount = 1f,
                                    SpecularExponent = 50f,
                                    Elevation = 0f,
                                    Azimuth = 0f,
                                    LightColor = Colors.White,
                                    Source = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                            new[] { "Light1.Azimuth", "Light1.Elevation",
                                                    "Light2.Azimuth", "Light2.Elevation" });

                    };
                    break;

                default:
                    break;
            }

            // Update the animations
            UpdateAnimations();

            // Update all the image to have the new effect
            UpdateEffectBrush();
        }
    }
}
