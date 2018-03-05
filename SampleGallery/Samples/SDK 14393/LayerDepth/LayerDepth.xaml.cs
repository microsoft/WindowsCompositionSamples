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
using ExpressionBuilder;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery
{
    public sealed partial class LayerDepth : SamplePage
    {
        private readonly List<Layer> layers = new List<Layer>();
        private int selectedLayerIndex;
        private Compositor _compositor;
        private ContainerVisual _rootVisual;
        private CompositionPropertySet _animationPropertySet;

        public LayerDepth()
        {
            InitializeViewModel();
            InitializeComponent();
        }

        public static string    StaticSampleName => "Layer Depth"; 
        public override string  SampleName => StaticSampleName;
        public static string    StaticSampleDescription => "Demonstrates how to achieve a depth-of-field effect with LayerVisual and EffectBrush."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868997";

        public IReadOnlyList<Layer> Layers { get { return layers; } }

        public int SelectedLayerIndex
        {
            get { return selectedLayerIndex; }
            set
            {
                if (selectedLayerIndex != value)
                {
                    selectedLayerIndex = value;

                    if (_animationPropertySet != null)
                    {
                        var currentZAnimation = _animationPropertySet.Compositor.CreateScalarKeyFrameAnimation();
                        currentZAnimation.InsertKeyFrame(1, value);
                        currentZAnimation.Duration = TimeSpan.FromSeconds(1.2f);
                        _animationPropertySet.StartAnimation("currentZ", currentZAnimation);
                    }
                }
            }
        }

        private void InitializeViewModel()
        {
            // Divide the pictures among three layers
            layers.Add(new Layer(2010));
            layers.Add(new Layer(2011));
            layers.Add(new Layer(2012));
            selectedLayerIndex = layers.Count / 2;

            int layerIndex = 0;
            // Divide the images from the data source over the layer items
            // Recycle data source images if necessary
            var datasource = (new LocalDataSource()).Nature;

            for (int i=0; i<layers.Count*4; i++)
            {
                var uri = new Uri(datasource[i % datasource.Count].ImageUrl);
                var layer = layers[layerIndex];
                layer.AddItem(uri);

                layerIndex = (layerIndex + 1) % layers.Count;
            }

            this.DataContext = this;
        }

        private void CreateCompositionScene(Compositor compositor)
        {
            // Create the property driving the animations
            _animationPropertySet = _compositor.CreatePropertySet();
            _animationPropertySet.InsertScalar("currentZ", selectedLayerIndex);

            // Create the layer effect
            var layerEffectDesc = new GaussianBlurEffect
            {
                Name = "blur",
                BorderMode = EffectBorderMode.Hard,
                BlurAmount = 0,
                Source = new SaturationEffect
                {
                    Name = "saturation",
                    Saturation = 1,
                    Source = new CompositionEffectSourceParameter("source")
                }
            };

            var layerEffectFactory = _compositor.CreateEffectFactory(layerEffectDesc,
                new[] { "blur.BlurAmount", "saturation.Saturation" });

            // Create the host visual
            _rootVisual = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(compositionHostPanel, _rootVisual);

            // Create the scene visuals
            for (int layerIndex = 0; layerIndex < layers.Count; ++layerIndex)
            {
                var layer = layers[layerIndex];
                layer.CreateVisuals(compositor);

                _rootVisual.Children.InsertAtBottom(layer.LayerVisual);
                layer.LayerVisual.Effect = layerEffectFactory.CreateBrush();
                SetupLayerAnimations(layer, layerIndex);
            }

            UpdateVisualLayout();
        }

        private void SetupLayerAnimations(Layer layer, float layerZ)
        {
            var layerVisual = layer.LayerVisual;
            var itemContainerVisual = layer.ItemContainerVisual;

            var compositor = layerVisual.Compositor;
            var currentZNode = _animationPropertySet.GetReference().GetScalarProperty("currentZ");

            // Opacity and saturation go to zero with as |deltaZ| -> 1
            var opacityAndSaturationExpression = EF.Max(0, 1 - EF.Abs(currentZNode - layerZ));
            layerVisual.StartAnimation(nameof(layerVisual.Opacity), opacityAndSaturationExpression);
            layerVisual.Effect.Properties.StartAnimation("saturation.Saturation", opacityAndSaturationExpression);

            // Scale changes with deltaZ (perspective-like)
            var scaleExpression = EF.Vector3(EF.Pow(1.5f, currentZNode - layerZ), EF.Pow(1.5f, currentZNode - layerZ), 0);
            itemContainerVisual.StartAnimation(nameof(layerVisual.Scale), scaleExpression);

            // Blur increases with |deltaZ|
            var blurAmountExpression = EF.Abs(currentZNode - layerZ) * 10;
            layerVisual.Effect.Properties.StartAnimation("blur.BlurAmount", blurAmountExpression);
        }

        private void UpdateVisualLayout()
        {
            if (_rootVisual == null)
            {
                return;
            }
            
            foreach (var layer in layers)
            {
                var layerVisual = layer.LayerVisual;
                layerVisual.Size = new Vector2(
                    (float)compositionHostPanel.ActualWidth,
                    (float)compositionHostPanel.ActualHeight);
                layer.ItemContainerVisual.Offset = new Vector3(
                    (float)compositionHostPanel.ActualWidth / 2,
                    (float)compositionHostPanel.ActualHeight / 2, 0);
            }
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            CreateCompositionScene(_compositor);
        }

        private void compositionHostPanel_LayoutUpdated(object sender, object e)
        {
            UpdateVisualLayout();
        }

        /// <summary>
        /// A group of pictures associated to a year and subject to a layer effect.
        /// </summary>
        public sealed class Layer
        {
            private readonly int year;
            private readonly List<LayerItem> items = new List<LayerItem>();
            private ContainerVisual itemContainerVisual;
            private LayerVisual layerVisual;

            public string YearLabel
            {
                get { return year.ToString(); }
            }

            public ContainerVisual ItemContainerVisual
            {
                get { return itemContainerVisual; }
            }

            public LayerVisual LayerVisual
            {
                get { return layerVisual; }
            }

            public Layer(int year)
            {
                this.year = year;
            }
            
            public void AddItem(Uri imageUri)
            {
                items.Add(new LayerItem(imageUri));
            }

            public void CreateVisuals(Compositor compositor)
            {
                layerVisual = compositor.CreateLayerVisual();
                itemContainerVisual = compositor.CreateContainerVisual();
                layerVisual.Children.InsertAtTop(itemContainerVisual);
                for (int i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    var itemVisual = item.CreateVisual(compositor);
                    itemVisual.Size = new Vector2(300, 200);
                    itemVisual.Offset = new Vector3((i % 2 - 1) * itemVisual.Size.X, (i / 2 - 1) * itemVisual.Size.Y, 0);
                    itemContainerVisual.Children.InsertAtTop(itemVisual);
                }
            }
        }

        /// <summary>
        /// A single picture in a layer.
        /// </summary>
        private sealed class LayerItem
        {
            private ManagedSurface _surface;
            private SpriteVisual   _visual;

            public LayerItem(Uri imageUri)
            {
                _surface = ImageLoader.Instance.LoadFromUri(imageUri);
            }

            public SpriteVisual CreateVisual(Compositor compositor)
            {
                _visual = compositor.CreateSpriteVisual();
                var brush = _surface.Brush;
                brush.Stretch = CompositionStretch.UniformToFill;
                _visual.Brush = brush;
                return _visual;
            }
        }
    }
}
