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
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas.Effects;

namespace CompositionSampleGallery
{
    public sealed partial class TreeEffects : SamplePage
    {
       
        public static string        StaticSampleName    { get { return "Tree Effects"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Use LayerVisual to apply an animated effect to a tree of SpriteVisuals"; } }

        public TreeEffects()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            _foregroundSwitch = false;
            _backgroundSwitch = false;
        }

        #region Background Toggle

        public bool BackgroundSwitch
        {
            get
            {
                return _backgroundSwitch;
            }
            set
            {
                // Animate in normal or reverse based on switch toggle
                var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                _blurAnimation.Direction = value ? Windows.UI.Composition.AnimationDirection.Normal : Windows.UI.Composition.AnimationDirection.Reverse;
                _blurBrush.Properties.StartAnimation("blurEffect.BlurAmount", _blurAnimation);
                BackgroundToggle.IsEnabled = false; // disable button while animation is in progress
                batch.End();
                batch.Completed += Background_BatchCompleted;
                _backgroundSwitch = value;
            }
        }

        private void Background_BatchCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            BackgroundToggle.IsEnabled = true;
        }
        #endregion

        #region Foreground Toggle
        public bool ForegroundSwitch
        {
            get
            {
                return _foregroundSwitch;
            }
            set
            {
                // Animate in normal or reverse based on switch toggle
                var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                _saturationAnimation.Direction = value ? Windows.UI.Composition.AnimationDirection.Normal : Windows.UI.Composition.AnimationDirection.Reverse;
                _saturationBrush.Properties.StartAnimation("saturationEffect.Saturation", _saturationAnimation);
                ForegroundToggle.IsEnabled = false; // disable button while animation is in progress
                batch.End();
                batch.Completed += Foreground_BatchCompleted;
                _foregroundSwitch = value;
            }
        }

        private void Foreground_BatchCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            ForegroundToggle.IsEnabled = true;
        }

        #endregion

        private static int CompareZOffset(SpriteVisual one, SpriteVisual two)
        {
            return (one.Offset.Z).CompareTo(two.Offset.Z);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            // Set up scene: get interop compositor, add comp root with clip, insert sceneContainer
            _compositor = ElementCompositionPreview.GetElementVisual(CompGrid).Compositor;
            var root = _compositor.CreateContainerVisual();
            root.Size = CompGrid.RenderSize.ToVector2();
            root.Clip = _compositor.CreateInsetClip();
            ElementCompositionPreview.SetElementChildVisual(CompGrid, root);

            // Populate screen with visuals based on container size
            var smallerSize = Math.Min(root.Size.X, root.Size.Y);
            _spriteSize = new Vector2(smallerSize / 10);
            _numVisuals = (root.Size.X * root.Size.Y) * 100 / (smallerSize * smallerSize);

            _sceneContainer = _compositor.CreateContainerVisual();
            _sceneContainer.Size = root.Size - _spriteSize;
            _sceneContainer.Offset = new Vector3(_spriteSize / 2, 0);
            root.Children.InsertAtTop(_sceneContainer);     

            // Apply perspective transform to sceneContainer
            float perspectiveOriginPercent = 0.5f;
            Vector3 perspectiveOrigin = new Vector3(perspectiveOriginPercent * _sceneContainer.Size, 0);
            float perspectiveDepth = -1000;
            _sceneContainer.TransformMatrix = Matrix4x4.CreateTranslation(-perspectiveOrigin) *
                    new Matrix4x4(1, 0, 0, 0, 
                                  0, 1, 0, 0, 
                                  0, 0, 1, 1 / perspectiveDepth, 
                                  0, 0, 0, 1) *
                    Matrix4x4.CreateTranslation(perspectiveOrigin);

            // Create SpriteVisuals and add to lists
            PopulateSpriteVisuals();

            // Initialize layervisuals; specify size 
            _foregroundLayerVisual = _compositor.CreateLayerVisual();
            _foregroundLayerVisual.Size = root.Size;
            _backgroundLayerVisual = _compositor.CreateLayerVisual();
            _backgroundLayerVisual.Size = root.Size;
            
            // Sort the lists by z offset so drawing order matches z order
            _listOfForegroundVisuals.Sort(CompareZOffset);
            _listOfBackgroundVisuals.Sort(CompareZOffset);

            // Insert spritevisuals into layervisuals
            foreach (var visual in _listOfForegroundVisuals)
            {
                _foregroundLayerVisual.Children.InsertAtTop(visual);
            }
            foreach (var visual in _listOfBackgroundVisuals)
            {
                _backgroundLayerVisual.Children.InsertAtTop(visual);
            }

            // Insert layervisuals in visual tree
            _sceneContainer.Children.InsertAtTop(_foregroundLayerVisual);
            _sceneContainer.Children.InsertBelow(_backgroundLayerVisual, _foregroundLayerVisual);

            // Initialize effects and their animations 
            CreateEffectAnimations();
        }

        private void PopulateSpriteVisuals()
        {
            // Initialize variables for visual creation loop
            int count = 0;
            var rand = new Random();
            float z_depth = -1000.0f;

            while (count < _numVisuals)
            {
                var r_x = (float)rand.NextDouble();
                var r_y = (float)rand.NextDouble();
                var r_z = (float)rand.NextDouble();

                // Create a new sprite with random offset and color
                var sprite = _compositor.CreateSpriteVisual();
                sprite.Size = _spriteSize;
                sprite.AnchorPoint = new Vector2(0.5f, 0.5f);
                sprite.Offset = new Vector3(r_x * _sceneContainer.Size.X, r_y * _sceneContainer.Size.Y, r_z * z_depth);
                sprite.Brush = _compositor.CreateColorBrush(Color.FromArgb(190, (byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255)));

                // Partition into two lists
                if (sprite.Offset.Z > z_depth / 2)
                {
                    _listOfForegroundVisuals.Add(sprite);
                }
                else
                {
                    _listOfBackgroundVisuals.Add(sprite);
                }
                count++;
            }
        }

        private void CreateEffectAnimations()
        {
            // Create saturation effect
            var saturationEffect = new SaturationEffect
            {
                Name = "saturationEffect",
                Saturation = 1f,
                Source = new CompositionEffectSourceParameter("foregroundLayerVisual"),
            };

            var saturationFactory = _compositor.CreateEffectFactory(saturationEffect, new[] { "saturationEffect.Saturation" });
            _saturationBrush = saturationFactory.CreateBrush();

            // Apply animatable saturation effect to foreground visuals
            _foregroundLayerVisual.Effect = _saturationBrush;

            // Create blur effect
            var blurEffect = new GaussianBlurEffect
            {
                Name = "blurEffect",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("backgroundLayerVisual"),
            };

            var blurFactory = _compositor.CreateEffectFactory(blurEffect, new[] { "blurEffect.BlurAmount" });
            _blurBrush = blurFactory.CreateBrush();

            // Apply animatable saturation effect to foreground visuals
            _backgroundLayerVisual.Effect = _blurBrush;

            // Initialize effect animations
            var lin = _compositor.CreateLinearEasingFunction();

            _saturationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _saturationAnimation.InsertKeyFrame(0, 1f, lin);
            _saturationAnimation.InsertKeyFrame(1, 0f, lin);
            _saturationAnimation.Duration = _duration;

            _blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _blurAnimation.InsertKeyFrame(0, 0f, lin);
            _blurAnimation.InsertKeyFrame(1, 20f, lin);
            _blurAnimation.Duration = _duration;
        }

        private Compositor _compositor;
        private ContainerVisual _sceneContainer;
        private LayerVisual _foregroundLayerVisual;
        private LayerVisual _backgroundLayerVisual;
        private float _numVisuals;
        private Vector2 _spriteSize;
        private readonly List<SpriteVisual> _listOfForegroundVisuals = new List<SpriteVisual>();
        private readonly List<SpriteVisual> _listOfBackgroundVisuals = new List<SpriteVisual>();  
        private ScalarKeyFrameAnimation _saturationAnimation;
        private ScalarKeyFrameAnimation _blurAnimation;
        private CompositionEffectBrush _saturationBrush;
        private CompositionEffectBrush _blurBrush;
        private static readonly TimeSpan _duration = TimeSpan.FromSeconds(3);
        private bool _backgroundSwitch;
        private bool _foregroundSwitch;
    }
}
