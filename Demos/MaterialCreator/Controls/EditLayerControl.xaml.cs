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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MaterialCreator
{
    public sealed partial class EditLayerControl : UserControl
    {
        ObservableCollection<string> _blendModes;
        
        public EditLayerControl()
        {
            this.InitializeComponent();
        }

        public Layer Layer { get; set; }
        
        public void Initialize(Layer layer)
        {
            Layer = layer;

            // Create the blendmode collection
            _blendModes = new ObservableCollection<string>(Helpers.GetBlendModes());

            // Create the layer types dropdown
            ComboBoxItem item;
            foreach (LayerType layerType in Enum.GetValues(typeof(LayerType)))
            {
                item = new ComboBoxItem();
                item.Tag = layerType;
                item.Content = layerType.ToString();

                if ((Layer == null && layerType == LayerType.Color) ||
                    (Layer.LayerType == layerType))
                {
                    item.IsSelected = true;
                }
                LayerTypeCombo.Items.Add(item);
            }

            foreach (EdgeMode edgeMode in Enum.GetValues(typeof(EdgeMode)))
            {
                item = new ComboBoxItem();
                item.Tag = edgeMode;
                item.Content = edgeMode.ToString();

                if (Layer == null || Layer.LayerType != LayerType.Image ||
                    (Layer.LayerType == LayerType.Image && ((ImageLayer)Layer).EdgeMode == edgeMode))
                {
                    item.IsSelected = true;
                }

                EdgeModeCombo.Items.Add(item);
            }

            UpdateUI();
        }

        private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (Layer != null)
            {
               ((ColorLayer)Layer).Color = args.NewColor;
            }
        }

        private void UpdateUI()
        {
            if (Layer.LayerType == LayerType.Color)
            {
                ColorPanel.Visibility = Visibility.Visible;
                ColorPicker.Color = ((ColorLayer)Layer).Color;
            }
            else if (Layer.LayerType == LayerType.Image)
            {
                ImagePanel.Visibility = Visibility.Visible;
                string file = System.IO.Path.GetFileName(((ImageLayer)Layer).FilePath);
                if (file == null)
                {
                    file = "<Load an image>";
                }
                Filename.Text = file;

                ImageLayer layer = (ImageLayer)Layer;
                EdgeModeCombo.SelectedValue = layer.EdgeMode.ToString();

                ImagePropertyPanel.Children.Clear();
                UpdateImageProperties(ImagePropertyPanel, layer);
                EdgeModePanel.Children.Clear();
                UpdateEdgeModeProperties(EdgeModePanel, layer);
            }
            else if (Layer.LayerType == LayerType.Lighting)
            {
                LightingPanel.Visibility = Visibility.Visible;

                LightingLayer lightLayer = ((LightingLayer)Layer);

                if (lightLayer.FilePath != null)
                {
                    NormalMapFilename.Text = System.IO.Path.GetFileName(lightLayer.FilePath);

                    NormalMapPropertyPanel.Children.Clear();
                    UpdateImageProperties(NormalMapPropertyPanel, (ImageLayer)Layer);
                    UpdateEdgeModeProperties(NormalMapPropertyPanel, (ImageLayer)Layer);
                }

                UpdateLightingProperties(lightLayer);
            }

            LayerName.Text = Layer.Description;
            BlendType.SelectedValue = Layer.BlendMode;
            OpacitySlider.Value = (int)(Layer.Opacity * 100);
        }

        private void UpdateImageProperties(StackPanel panel, ImageLayer layer)
        {
            PropertyInfo info;

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "Offset");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "Scale");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "RotationAngleInDegrees");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "AnchorPoint");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "Stretch");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);

            info = layer.Brush.GetType().GetProperties().FirstOrDefault(x => x.Name == "HorizontalAlignmentRatio");
            Helpers.AddPropertyToPanel(layer.Brush, layer, panel, info);
            
        }

        private void UpdateLightingProperties(LightingLayer layer)
        {
            PropertyInfo info;

            LightingPropertyPanel.Children.Clear();

            SceneLightingEffect lighting = layer.LightingEffect;

            info = lighting.GetType().GetProperties().FirstOrDefault(x => x.Name == "AmbientAmount");
            Helpers.AddPropertyToPanel(lighting, layer, LightingPropertyPanel, info);

            info = lighting.GetType().GetProperties().FirstOrDefault(x => x.Name == "DiffuseAmount");
            Helpers.AddPropertyToPanel(lighting, layer, LightingPropertyPanel, info);

            info = lighting.GetType().GetProperties().FirstOrDefault(x => x.Name == "SpecularAmount");
            Helpers.AddPropertyToPanel(lighting, layer, LightingPropertyPanel, info);

            info = lighting.GetType().GetProperties().FirstOrDefault(x => x.Name == "SpecularShine");
            Helpers.AddPropertyToPanel(lighting, layer, LightingPropertyPanel, info);

            info = lighting.GetType().GetProperties().FirstOrDefault(x => x.Name == "ReflectanceModel");
            Helpers.AddPropertyToPanel(lighting, layer, LightingPropertyPanel, info);
        }

        private void UpdateEdgeModeProperties(StackPanel panel, ImageLayer layer)
        {
            PropertyInfo info;
            
            if (layer.EdgeMode == EdgeMode.Ninegrid)
            {
                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "LeftInset");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "TopInset");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "RightInset");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "BottomInset");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "LeftInsetScale");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "TopInsetScale");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "RightInsetScale");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "BottomInsetScale");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);

                info = layer.NineGrid.GetType().GetProperties().FirstOrDefault(x => x.Name == "IsCenterHollow");
                Helpers.AddPropertyToPanel(layer.NineGrid, layer, panel, info);
            }
            else
            {
                info = layer.BorderEffect.GetType().GetProperties().FirstOrDefault(x => x.Name == "ExtendX");
                Helpers.AddPropertyToPanel(layer.BorderEffect, layer, panel, info);

                info = layer.BorderEffect.GetType().GetProperties().FirstOrDefault(x => x.Name == "ExtendY");
                Helpers.AddPropertyToPanel(layer.BorderEffect, layer, panel, info);
            }
        }
                
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                Filename.Text = file.Name;
                ((ImageLayer)Layer).File = file;
            }
        }

        private void LayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorPanel.Visibility = Visibility.Collapsed;
            ImagePanel.Visibility = Visibility.Collapsed;
            LightingPanel.Visibility = Visibility.Collapsed;

            ComboBoxItem item = LayerTypeCombo.SelectedItem as ComboBoxItem;
            LayerType layerType = (LayerType)item.Tag;

            if (Layer.LayerType != layerType)
            {
                Layer layer = null;
                switch (layerType)
                {
                    case LayerType.Backdrop:
                        {
                            layer = new BackdropLayer();
                        }
                        break;

                    case LayerType.BackdropHost:
                        {
                            layer = new BackdropHostLayer();
                        }
                        break;

                    case LayerType.Color:
                        {
                            layer = new ColorLayer();
                        }
                        break;

                    case LayerType.Image:
                        { 
                            layer = new ImageLayer();
                        }
                        break;

                    case LayerType.Lighting:
                        {
                            layer = new LightingLayer();
                        }
                        break;

                    case LayerType.Group:
                        {
                            layer = new GroupLayer();
                        }
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }

                // Set the new layer
                Layer.GroupLayer.ReplaceLayer(Layer, layer);

                Layer = layer;
                
                // Update the edit control UI
                UpdateUI();
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Layer != null)
            {
                Layer.Opacity = ((float)OpacitySlider.Value) / 100;
            }
        }

        private void LayerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Layer != null)
            {
                Layer.Description = LayerName.Text;
            }
        }

        private void EdgeModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string edgeMode = (string)EdgeModeCombo.SelectedValue;

            if (Layer.LayerType == LayerType.Image)
            {
                ImageLayer layer = (ImageLayer)Layer;
                layer.EdgeMode = (EdgeMode)Enum.Parse(typeof(EdgeMode), edgeMode);

                UpdateUI();
            }
        }

        private void DeleteNormalMap_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(Layer.LayerType == LayerType.Lighting);

            LightingLayer layer = (LightingLayer)Layer;

            if (layer.FilePath != null)
            {
                ((LightingLayer)Layer).File = null;
                NormalMapFilename.Text = "";
                NormalMapPropertyPanel.Children.Clear();
            }
        }
    }
}
