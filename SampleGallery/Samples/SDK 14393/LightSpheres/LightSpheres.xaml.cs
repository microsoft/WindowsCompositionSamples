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

using ExpressionBuilder;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class LightSphere : SamplePage
    {
        private Compositor _compositor;
        private AmbientLight _ambientLight;
        private ContainerVisual _rootContainer;
        private ContainerVisual _sceneContainer;
        private ContainerVisual _worldSpaceContainer;
        private List<SpriteVisual> _sphereList;
        private List<CompositionEffectBrush> _brushList;
        private ManagedSurface _normalMap;

        private Vector3 _defaultSphereOffset = new Vector3(-1500, 700, -1000);
        private int  _defaultSphereSpace = 350;


        public LightSphere()
        {
            this.InitializeComponent();

            _sphereList = new List<SpriteVisual>(16);
            _brushList = new List<CompositionEffectBrush>(16);

            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Setup the basic scene
            _rootContainer = _compositor.CreateContainerVisual();
            _sceneContainer = _compositor.CreateContainerVisual();
            _worldSpaceContainer = _compositor.CreateContainerVisual();

            _rootContainer.Children.InsertAtTop(_worldSpaceContainer);
            _worldSpaceContainer.Children.InsertAtTop(_sceneContainer);

            // Create the ambient light for the scene
            _ambientLight = _compositor.CreateAmbientLight();
            _ambientLight.Color = Color.FromArgb(255, 255, 255, 255);
            _ambientLight.Targets.Add(_sceneContainer);

            ElementCompositionPreview.SetElementChildVisual(EnvironmentPanel, _rootContainer);

        }

        public static string   StaticSampleName => "Light Spheres"; 
        public override string SampleName => StaticSampleName; 
        public static string   StaticSampleDescription => "Demonstrates a simulated 3D scene with multiple lights."; 
        public override string SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=869000";

        private async void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ConstructWalls();

            _normalMap = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/NormalMapsAndMasks/SphericalWithMask.png"));

            LightControl1.Offset = new Vector3(-77, 21, -768);
            LightControl1.LightColor.Color = Color.FromArgb(255, 255, 255, 255);
            LightControl1.LinearAttenuation.Value = 29;
            LightControl1.ConstantAttenuation.Value = 50;
            LightControl2.QuadraticAttenuation.Value = 0;

            LightControl2.Offset = new Vector3(2047, 500, -676);
            LightControl2.LightColor.Color = Color.FromArgb(255, 255, 0, 255);
            LightControl2.LinearAttenuation.Value = 0;
            LightControl2.ConstantAttenuation.Value = 9;
            LightControl2.QuadraticAttenuation.Value = 100;

            // Create the colors for the spheres
            Color[] colors = new Color[]
            {
                Color.FromArgb(255, 60, 60, 60),        // Black
                Color.FromArgb(255, 200, 200, 200),     // White
                Color.FromArgb(255, 139, 255, 211),     // Green
                Color.FromArgb(255, 160, 244, 255),     // Blue
                Color.FromArgb(255, 253, 99, 105),      // Red
                Color.FromArgb(255, 206, 181, 251),     // Purple
                Color.FromArgb(255, 67, 228, 124),      // Bright Green
                Color.FromArgb(255, 248, 216, 148),     // Yellow
                Color.FromArgb(255, 228, 190, 198),     // Mauve
            };

            // Create the lighting effect for the spheres
            IGraphicsEffect graphicsEffect = new CompositeEffect()
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new ArithmeticCompositeEffect()
                    {
                        Source1Amount = .9f,
                        Source2Amount = 1f,
                        MultiplyAmount = 0,
                        Source1 = new ArithmeticCompositeEffect()
                        {
                            MultiplyAmount = .5f,
                            Source1Amount = .5f,
                            Source2Amount = 0f,
                            Source1 = new ArithmeticCompositeEffect()
                            {
                                MultiplyAmount = 1f,
                                Source1Amount = 0f,
                                Source2Amount = 0f,
                                Source1 = new ColorSourceEffect()
                                {
                                    Name = "Color",
                                },
                                Source2 = new SceneLightingEffect()
                                {
                                    Name = "Ambient",
                                    AmbientAmount = 1f,
                                    DiffuseAmount = 0f,
                                    SpecularAmount = 0f,
                                    NormalMapSource = new CompositionEffectSourceParameter("ImageSource"),
                                }
                            },
                            Source2 = new SceneLightingEffect()
                            {
                                Name="Diffuse",
                                AmbientAmount = 0f,
                                DiffuseAmount = 1f,
                                SpecularAmount = 0f,
                                NormalMapSource = new CompositionEffectSourceParameter("ImageSource"),
                            }
                        },
                        Source2 = new SceneLightingEffect()
                        {
                            Name = "Specular",
                            AmbientAmount = 0,
                            DiffuseAmount = 0f,
                            SpecularAmount = .1f,
                            SpecularShine = 5,
                            NormalMapSource = new CompositionEffectSourceParameter("ImageSource"),
                        }
                    },
                    new CompositionEffectSourceParameter("ImageSource"),
                }
            };

            CompositionEffectFactory effectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                                             new string[] { "Color.Color","Diffuse.DiffuseAmount",
                                                            "Specular.SpecularAmount", "Specular.SpecularShine" });

            // Create the spheres
            int xOffset = 0;
            foreach (Color color in colors)
            {
                CompositionEffectBrush brush = effectFactory.CreateBrush();
                brush.Properties.InsertColor("Color.Color", color);
                brush.SetSourceParameter("ImageSource", _normalMap.Brush);
                _brushList.Add(brush);

                SpriteVisual sprite = _compositor.CreateSpriteVisual();
                sprite.BorderMode = CompositionBorderMode.Hard;
                sprite.Brush = brush;
                sprite.Size = new Vector2(200, 200);
                sprite.Offset = new Vector3(xOffset, 0, 0) + _defaultSphereOffset;
                _sceneContainer.Children.InsertAtTop(sprite);
                _sphereList.Add(sprite);
                xOffset += _defaultSphereSpace;

                LightControl1.AddTargetVisual(sprite);
                LightControl2.AddTargetVisual(sprite);
            }

            UpdateAnimationState();
        }

        private void SamplePage_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (CompositionEffectBrush brush in _brushList)
            {
                brush.Dispose();
            }
            _brushList.Clear();

            if (_normalMap != null)
            {
                _normalMap.Dispose();
                _normalMap = null;
            }
    }

    private void ConstructWalls()
        {
            // Connect the light controls to the walls
            LightControl1.AddTargetVisual(_sceneContainer);
            LightControl2.AddTargetVisual(_sceneContainer);

            LightControl1.CoordinateSpace = _worldSpaceContainer;
            LightControl2.CoordinateSpace = _worldSpaceContainer;

            // Setup the perspective transform
            UpdatePerspective();

            float boxWidth = 6000.0f;
            float boxHeight = 2000.0f;
            float boxDepth = 4000.0f;
            CompositionColorBrush colorBrush = _compositor.CreateColorBrush(Color.FromArgb(255, 101, 104, 112));

            // Create the walls
            SpriteVisual bottomWall = _compositor.CreateSpriteVisual();
            bottomWall.Size = new Vector2(boxWidth, boxDepth);
            bottomWall.Brush = colorBrush;
            bottomWall.BackfaceVisibility = CompositionBackfaceVisibility.Hidden;
            bottomWall.TransformMatrix = Matrix4x4.CreateRotationX((float)Math.PI / 2, new Vector3(boxWidth / 2, boxDepth / 2, 0));
            bottomWall.Offset = new Vector3(-boxWidth / 2, -boxDepth / 2 + boxHeight / 2, 0);
            _sceneContainer.Children.InsertAtTop(bottomWall);

            SpriteVisual backWall = _compositor.CreateSpriteVisual();
            backWall.Size = new Vector2(boxWidth, boxHeight);
            backWall.Brush = colorBrush;
            backWall.BackfaceVisibility = CompositionBackfaceVisibility.Hidden;
            backWall.Offset = new Vector3(-boxWidth / 2, -boxHeight / 2, -boxDepth / 2);
            _sceneContainer.Children.InsertAtBottom(backWall);
        }

        private void UpdatePerspective()
        {
            // Setup the perspective transform
            float flPerspectiveEntry = 0.0f;
            float _perspectiveAmount = (float)Math.Sqrt(RootGrid.ActualWidth * RootGrid.ActualWidth + RootGrid.ActualHeight * RootGrid.ActualHeight);

            if (_perspectiveAmount >= 1)
            {
                flPerspectiveEntry = -1.0f / (float)_perspectiveAmount;
            }

            Matrix4x4 perspectiveTransform = new Matrix4x4(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, flPerspectiveEntry,
                0.0f, 0.0f, 0.0f, 1.0f
                );

            float BoxOffsetX = 100.0f;
            float BoxOffsetY = -700.0f;
            float BoxOffsetZ = -1000.0f;

            _rootContainer.TransformMatrix =
                Matrix4x4.CreateTranslation((float)-RootGrid.ActualWidth / 2, (float)-RootGrid.ActualHeight / 2, 0.0f) *
                perspectiveTransform *
                Matrix4x4.CreateTranslation((float)RootGrid.ActualWidth / 2, (float)RootGrid.ActualHeight / 2, 0.0f);

            _worldSpaceContainer.Offset = new Vector3((float)BoxOffsetX + (float)RootGrid.ActualWidth / 2, (float)BoxOffsetY + (float)RootGrid.ActualHeight / 2, (float)BoxOffsetZ);
        }

        private void ToggleLightMenu(object sender, RoutedEventArgs e)
        {
            LightMenu.Visibility = LightMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);

            UpdatePerspective();
        }

        void UpdateAnimationState()
        {
            bool enabled = AnimateCheckBox.IsChecked == true ? true : false;
 
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            var vec3StartingValue = ExpressionValues.StartingValue.CreateVector3StartingValue();
            offsetAnimation.InsertExpressionKeyFrame(0, vec3StartingValue);
            offsetAnimation.InsertExpressionKeyFrame(.33f, vec3StartingValue + ExpressionFunctions.Vector3(0, 0, -800));
            offsetAnimation.InsertExpressionKeyFrame(.66f, vec3StartingValue + ExpressionFunctions.Vector3(0, 0, 800));
            offsetAnimation.InsertExpressionKeyFrame(1, vec3StartingValue);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            offsetAnimation.Duration = TimeSpan.FromSeconds(10);

            int index = 0;
            int xOffset = 0;
            foreach (SpriteVisual sprite in _sphereList)
            {
                if (enabled)
                {
                    offsetAnimation.DelayTime = TimeSpan.FromSeconds(.2 * index++);
                    sprite.StartAnimation("Offset", offsetAnimation);
                }
                else
                {
                    sprite.StopAnimation("Offset");
                    sprite.Offset = new Vector3(xOffset, 0, 0) + _defaultSphereOffset;
                    xOffset += _defaultSphereSpace;
                }

            }
        }

        private void AnimateCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            UpdateAnimationState();
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_brushList != null)
            {
                if (_ambientLight != null)
                {
                    float channelAmount = 255f * (float)(AmbientAmount.Value / 100);
                    byte channel = (byte)Math.Round(channelAmount);
                    _ambientLight.Color = Color.FromArgb(255, channel, channel, channel);
                }

                foreach (CompositionEffectBrush brush in _brushList)
                {
                    brush.Properties.InsertScalar("Diffuse.DiffuseAmount", (float)DiffuseAmount.Value / 100);
                    brush.Properties.InsertScalar("Specular.SpecularAmount", (float)SpecularAmount.Value / 100);
                    brush.Properties.InsertScalar("Specular.SpecularShine", (float)SpecularShine.Value);
                }
            }
        }
    }
}
