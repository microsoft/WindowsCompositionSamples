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
using Windows.UI.Composition.Effects;
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
        private AmbientLight                _ambientLight;
        private PointLight                  _pointLight;
        private DistantLight                _distantLight;
        private SpotLight                   _spotLight;

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

            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;


            //
            // Create the lights
            //

            _ambientLight = _compositor.CreateAmbientLight();
            _pointLight = _compositor.CreatePointLight();
            _distantLight = _compositor.CreateDistantLight();
            _spotLight = _compositor.CreateSpotLight();
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
            Vector2 sizeLightBounds = new Vector2((float)RootPanel.ActualWidth, (float)RootPanel.ActualHeight);
            Vector3KeyFrameAnimation lightPositionAnimation;
            ColorKeyFrameAnimation lightColorAnimation;

            ComboBoxItem item = LightingSelection.SelectedValue as ComboBoxItem;
            switch ((LightingTypes)item.Tag)
            {
                case LightingTypes.PointDiffuse:
                case LightingTypes.PointSpecular:
                    {
                        float flZDistance = 50f;

                        // Create the light position animation
                        lightPositionAnimation = _compositor.CreateVector3KeyFrameAnimation();
                        lightPositionAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, flZDistance));
                        lightPositionAnimation.InsertKeyFrame(.25f, new Vector3(sizeLightBounds.X * .2f, sizeLightBounds.Y * .5f, flZDistance));
                        lightPositionAnimation.InsertKeyFrame(.50f, new Vector3(sizeLightBounds.X * .75f, sizeLightBounds.Y * .5f, flZDistance));
                        lightPositionAnimation.InsertKeyFrame(.75f, new Vector3(sizeLightBounds.X * .2f, sizeLightBounds.Y * .2f, flZDistance));
                        lightPositionAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, flZDistance));
                        lightPositionAnimation.Duration = TimeSpan.FromMilliseconds(7500);
                        lightPositionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        lightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
                        lightColorAnimation.InsertKeyFrame(0f, Colors.White);
                        lightColorAnimation.InsertKeyFrame(.33f, Colors.White);
                        lightColorAnimation.InsertKeyFrame(.66f, Colors.Yellow);
                        lightColorAnimation.InsertKeyFrame(1f, Colors.White);
                        lightColorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
                        lightColorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        _pointLight.StartAnimation("Offset", lightPositionAnimation);
                        _pointLight.StartAnimation("Color", lightColorAnimation);
                    }
                    break;

                case LightingTypes.SpotLightDiffuse:
                case LightingTypes.SpotLightSpecular:
                    {
                        // Create the light position animation
                        lightPositionAnimation = _compositor.CreateVector3KeyFrameAnimation();
                        lightPositionAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 100f));
                        lightPositionAnimation.InsertKeyFrame(.33f, new Vector3(sizeLightBounds.X * .5f, sizeLightBounds.Y * .5f, 200f));
                        lightPositionAnimation.InsertKeyFrame(.66f, new Vector3(sizeLightBounds.X, sizeLightBounds.Y * .5f, 400f));
                        lightPositionAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 100f));
                        lightPositionAnimation.Duration = TimeSpan.FromMilliseconds(7500);
                        lightPositionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        lightColorAnimation = _compositor.CreateColorKeyFrameAnimation();
                        lightColorAnimation.InsertKeyFrame(0f, Colors.White);
                        lightColorAnimation.InsertKeyFrame(.33f, Colors.White);
                        lightColorAnimation.InsertKeyFrame(.66f, Colors.Yellow);
                        lightColorAnimation.InsertKeyFrame(1f, Colors.White);
                        lightColorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
                        lightColorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        _spotLight.StartAnimation("Offset", lightPositionAnimation);
                        _spotLight.StartAnimation("InnerConeColor", lightColorAnimation);
                    }
                    break;

                case LightingTypes.DistantDiffuse:
                case LightingTypes.DistantSpecular:
                    {
                        // Animate the light direction from left to right
                        double flLeftSweepRadians = Math.PI / 8;
                        double flRightSweepRadians = Math.PI - flLeftSweepRadians;
                        double flCenterRadians = Math.PI / 2;

                        Vector3KeyFrameAnimation lightDirectionAnimation = _compositor.CreateVector3KeyFrameAnimation();
                        lightDirectionAnimation.InsertKeyFrame(0f,   new Vector3((float)Math.Cos(flCenterRadians),     0, (float)-Math.Sin(flCenterRadians)));
                        lightDirectionAnimation.InsertKeyFrame(.25f, new Vector3((float)Math.Cos(flLeftSweepRadians),  0, (float)-Math.Sin(flLeftSweepRadians)));
                        lightDirectionAnimation.InsertKeyFrame(.50f, new Vector3((float)Math.Cos(flRightSweepRadians), 0, (float)-Math.Sin(flRightSweepRadians)));
                        lightDirectionAnimation.InsertKeyFrame(.75f, new Vector3((float)Math.Cos(flLeftSweepRadians),  0, (float)-Math.Sin(flLeftSweepRadians)));
                        lightDirectionAnimation.InsertKeyFrame(1f,   new Vector3((float)Math.Cos(flCenterRadians),     0, (float)-Math.Sin(flCenterRadians)));
                        lightDirectionAnimation.Duration = TimeSpan.FromMilliseconds(7500);
                        lightDirectionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                        
                        _distantLight.StartAnimation("Direction", lightDirectionAnimation);
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
                case LightingTypes.DistantDiffuse:
                case LightingTypes.DistantSpecular:
                    brush.SetSourceParameter("NormalMap", _circleNormalsBrush);
                    break;
                default:
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
            _ambientLight.Targets.RemoveAll();
            _pointLight.Targets.RemoveAll();
            _distantLight.Targets.RemoveAll();
            _spotLight.Targets.RemoveAll();

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
                                new SceneLightingEffect()
                                {
                                    AmbientAmount = 0,
                                    DiffuseAmount = .75f,
                                    SpecularAmount = 0,
                                    NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        // Set the light coordinate space and add the target
                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _pointLight.CoordinateSpace = lightRoot;
                        _pointLight.Targets.Add(lightRoot);
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
                            Mode = CanvasComposite.DestinationIn,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    MultiplyAmount = 1,
                                    Source1Amount = 0,
                                    Source2Amount = 0,
                                    Source1 = new CompositionEffectSourceParameter("ImageSource"),
                                    Source2 = new SceneLightingEffect()
                                    {
                                        AmbientAmount = .6f,
                                        DiffuseAmount = .05f,
                                        SpecularAmount = .5f,
                                        SpecularShine = 10f,
                                        NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                    }
                                },
                                new CompositionEffectSourceParameter("NormalMap"),
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        // Set the light coordinate space and add the target
                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _ambientLight.Targets.Add(lightRoot);
                        _pointLight.CoordinateSpace = lightRoot;
                        _pointLight.Targets.Add(lightRoot);
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
                                new SceneLightingEffect()
                                {
                                    AmbientAmount = 0,
                                    DiffuseAmount = .75f,
                                    SpecularAmount = 0,
                                    NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                }
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        // Set the light coordinate space and add the target
                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _spotLight.CoordinateSpace = lightRoot;
                        _spotLight.Targets.Add(lightRoot);
                        _spotLight.InnerConeAngle = (float)(Math.PI / 15);
                        _spotLight.OuterConeAngle = (float)(Math.PI / 10);
                        _spotLight.Direction = new Vector3(0, 0, -1);
                    };
                    break;

                case LightingTypes.SpotLightSpecular:
                    {
                        //
                        // Result =    Ambient   +       Diffuse           +     Specular
                        // Result = (Image * .6) + (Image * Diffuse color) + (Specular color)
                        //
                        
                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.DestinationIn,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    MultiplyAmount = 1,
                                    Source1Amount = 0,
                                    Source2Amount = 0,
                                    Source1 = new CompositionEffectSourceParameter("ImageSource"),
                                    Source2 = new SceneLightingEffect()
                                    {
                                        AmbientAmount = .6f,
                                        DiffuseAmount = .95f,
                                        SpecularAmount = .5f,
                                        SpecularShine = 30f,
                                        NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                    }
                                },
                                new CompositionEffectSourceParameter("NormalMap"),
                            }
                        };

                        // Create the effect factory
                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        // Set the light coordinate space and add the target
                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _ambientLight.Targets.Add(lightRoot);
                        _spotLight.CoordinateSpace = lightRoot;
                        _spotLight.Targets.Add(lightRoot);
                        _spotLight.InnerConeAngle = (float)(Math.PI / 15);
                        _spotLight.OuterConeAngle = (float)(Math.PI / 10);
                        _spotLight.Direction = new Vector3(0, 0, -1);
                    };
                    break;

                case LightingTypes.DistantDiffuse:
                    {
                        //
                        // Result =       Diffuse
                        // Result =  Image * Diffuse color
                        //

                        IGraphicsEffect graphicsEffect = new CompositeEffect()
                        {
                            Mode = CanvasComposite.DestinationIn,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    MultiplyAmount = 1,
                                    Source1Amount = 0,
                                    Source2Amount = 0,
                                    Source1 = new CompositionEffectSourceParameter("ImageSource"),
                                    Source2 = new SceneLightingEffect()
                                    {
                                        AmbientAmount = 0,
                                        DiffuseAmount = .75f,
                                        SpecularAmount = 0,
                                        NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                    }
                                },
                                new CompositionEffectSourceParameter("NormalMap"),
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _distantLight.CoordinateSpace = lightRoot;
                        _distantLight.Targets.Add(lightRoot);
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
                            Mode = CanvasComposite.DestinationIn,
                            Sources =
                            {
                                new ArithmeticCompositeEffect()
                                {
                                    MultiplyAmount = 1,
                                    Source1Amount = 0,
                                    Source2Amount = 0,
                                    Source1 = new CompositionEffectSourceParameter("ImageSource"),
                                    Source2 = new SceneLightingEffect()
                                    {
                                        AmbientAmount = 0,
                                        DiffuseAmount = .75f,
                                        SpecularAmount = .75f,
                                        NormalMapSource = new CompositionEffectSourceParameter("NormalMap"),
                                    }
                                },
                                new CompositionEffectSourceParameter("NormalMap"),
                            }
                        };

                        _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

                        Visual lightRoot = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                        _distantLight.CoordinateSpace = lightRoot;
                        _distantLight.Targets.Add(lightRoot);
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

        private void ThumbnailList_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Vector2 offset = e.GetCurrentPoint(ThumbnailList).Position.ToVector2();
            ComboBoxItem item = LightingSelection.SelectedValue as ComboBoxItem;
            switch ((LightingTypes)item.Tag)
            {
                case LightingTypes.PointDiffuse:
                case LightingTypes.PointSpecular:
                    _pointLight.Offset = new Vector3(offset.X, offset.Y, 75);
                    break;

                case LightingTypes.SpotLightDiffuse:
                case LightingTypes.SpotLightSpecular:
                    _spotLight.Offset = new Vector3(offset.X, offset.Y, 100);
                    break;

                case LightingTypes.DistantDiffuse:
                case LightingTypes.DistantSpecular:
                    Vector3 position = new Vector3((float)ThumbnailList.ActualWidth / 2, (float)ThumbnailList.ActualHeight / 2, 200);
                    Vector3 lookAt = new Vector3((float)ThumbnailList.ActualWidth - offset.X, (float)ThumbnailList.ActualHeight - offset.Y, 0);
                    _distantLight.Direction = Vector3.Normalize(lookAt - position);
                    break;

                default:
                    break;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(MouseHover.IsChecked == true)
            {
                ThumbnailList.PointerMoved += ThumbnailList_PointerMoved;
            }
            else
            {
                ThumbnailList.PointerMoved -= ThumbnailList_PointerMoved;
                UpdateAnimations();
            }
        }
    }
}
