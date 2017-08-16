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

using SamplesCommon;
using System.ComponentModel;
using System.Numerics;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MaterialCreator
{
    public sealed partial class LayerControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Layer _layer;
        SpriteVisual _previewSprite;

        public LayerControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += OnDataContextChanged;
        }

        public Layer Layer
        {
            get { return _layer; }
            set
            {
                if (_layer != value)
                {
                    _layer = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Layer)));
                }
            }
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (e.NewValue is Layer newLayer)
            {
                if (newLayer != null && Layer != newLayer)
                {
                    Initialize(newLayer);

                    if (!Layer.FirstLoadCompleted)
                    {
                        Layer.FirstLoadCompleted = true;
                        ShowLayerPropertyFlyout();
                    }

                    AddLayer.Visibility = newLayer.LayerType == LayerType.Group ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void Initialize(Layer layer)
        {
            Layer = layer;
            Layer.LayerChanged += OnLayerChanged;
            Eyeball.Opacity = Layer.Enabled ? 1 : .2f;

            _previewSprite = ElementCompositionPreview.GetElementVisual(this).Compositor.CreateSpriteVisual();
            _previewSprite.Size = new Vector2((float)PreviewLayer.ActualWidth, (float)PreviewLayer.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(PreviewLayer, _previewSprite);

            OnLayerChanged(this, "Layer");
        }

        private void OnLayerChanged(object sender, string property)
        {
            try
            {
                IGraphicsEffect effectGraph = Layer.GenerateEffectGraph(true);
                CompositionEffectFactory effectFactory = _previewSprite.Compositor.CreateEffectFactory(effectGraph, null);
                _previewSprite.Brush = effectFactory.CreateBrush();
                Layer.UpdateResourceBindings((CompositionEffectBrush)_previewSprite.Brush);
            }
            catch
            {
                _previewSprite.Brush = null;
            }

            LayerDescription.Text = Layer.Description;
        }

        private void LayerVisible_Clicked(object sender, RoutedEventArgs e)
        {
            if (Layer.Enabled)
            {
                Layer.Enabled = false;
                Eyeball.Opacity = .2f;
            }
            else
            {
                Layer.Enabled = true;
                Eyeball.Opacity = 1f;
            }
        }

        private void PreviewLayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_previewSprite != null)
            {
                _previewSprite.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
            }
        }

        public void ShowLayerPropertyFlyout()
        {
            EditLayerControl editControl = new EditLayerControl();
            editControl.Initialize(Layer);

            Flyout fly = new Flyout();
            fly.Content = editControl;
            fly.Opened += Fly_Opened;
            fly.Placement = FlyoutPlacementMode.Left;

            fly.ShowAt(this);
        }

        private void Fly_Opened(object sender, object e)
        {
            Flyout fly = (Flyout)sender;
            EditLayerControl control = (EditLayerControl)fly.Content;
            control.Width = ActualWidth - 2;
        }

        private void AddEffect_Click(object sender, RoutedEventArgs e)
        {
            Layer.AddEffect(new Effect());
        }

        private void DeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            Layer.GroupLayer.RemoveLayer(Layer);
        }
        
        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            LayerButtons.Visibility = Visibility.Visible;
            RootGrid.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemListLowColor"]);
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            LayerButtons.Visibility = Visibility.Collapsed;
            RootGrid.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button ancestor = VisualTreeHelperExtensions.GetFirstAncestorOfType<Button>((DependencyObject)e.OriginalSource);

            if (ancestor == null)
            {
                ShowLayerPropertyFlyout();
            }
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            GroupLayer layer = Layer as GroupLayer;

            ColorLayer colorLayer = new ColorLayer();
            layer.AddLayer(colorLayer);
        }
    }
}