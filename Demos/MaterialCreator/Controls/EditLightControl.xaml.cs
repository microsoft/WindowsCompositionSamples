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
using System.Diagnostics;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml.Controls;


namespace MaterialCreator
{
    public sealed partial class EditLightControl : UserControl
    {
        LightControl _light;

        const float c_ConstantAttenMultiplier = .1f;
        const float c_LinearAttenMultiplier = .03f;
        const float c_QuadraticAttenMultiplier = .005f;

        public EditLightControl(LightControl light)
        {
            this.InitializeComponent();

            _light = light;

            UpdateUI();
            
            foreach (LightTypes lightType in Enum.GetValues(typeof(LightTypes)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = lightType;
                item.Content = lightType.ToString();
                item.IsSelected = lightType == _light.Light.Type;
                LightTypeSelection.Items.Add(item);
            }
        }

        private void LightTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = LightTypeSelection.SelectedItem as ComboBoxItem;
            LightTypes lightType = (LightTypes)item.Tag;

            PointProperties.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            SpotProperties.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DirectionProperties.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            DefaultProperties.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            switch (lightType)
            {
                case LightTypes.Point:
                    DefaultProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    PointProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                case LightTypes.Spot:
                    DefaultProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    SpotProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    DirectionProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                case LightTypes.Distant:
                    PointProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    DirectionProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                case LightTypes.Ambient:
                    PointProperties.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;

                default:
                    break;
             }

            if (lightType != _light.Light.Type)
            {
                UpdateLight();
            }
        }

        void UpdateUI()
        {
            switch (_light.Light.Type)
            {
                case LightTypes.Point:
                    {
                        Vector3 offset = _light.Light.Offset;
                        XOffset.Value = offset.X;
                        YOffset.Value = offset.Y;
                        ZOffset.Value = offset.Z;
                        ConstantAttenuation.Value = (float)_light.Light.GetProperty("ConstantAttenuation") / c_ConstantAttenMultiplier;
                        LinearAttenuation.Value = (float)_light.Light.GetProperty("LinearAttenuation") / c_LinearAttenMultiplier;
                        QuadraticAttenuation.Value = (float)_light.Light.GetProperty("QuadraticAttenuation") / c_QuadraticAttenMultiplier;
                        LightColor.Color = (Color)_light.Light.GetProperty("Color");
                        IntensitySlider.Value = (float)_light.Light.GetProperty("Intensity") * 100;
                    }
                    break;

                case LightTypes.Spot:
                    {
                        Vector3 offset = _light.Light.Offset;
                        XOffset.Value = offset.X;
                        YOffset.Value = offset.Y;
                        ZOffset.Value = offset.Z;
                        ConstantAttenuation.Value = (float)_light.Light.GetProperty("ConstantAttenuation") / c_ConstantAttenMultiplier;
                        LinearAttenuation.Value = (float)_light.Light.GetProperty("LinearAttenuation") / c_LinearAttenMultiplier;
                        QuadraticAttenuation.Value = (float)_light.Light.GetProperty("QuadraticAttenuation") / c_QuadraticAttenMultiplier;
                        InnerCone.Value = (float)_light.Light.GetProperty("InnerConeAngleInDegrees");
                        InnerConeColor.Color = (Color)_light.Light.GetProperty("InnerConeColor");
                        InnerIntensitySlider.Value = (float)_light.Light.GetProperty("InnerConeIntensity") * 100;
                        OuterCone.Value = (float)_light.Light.GetProperty("OuterConeAngleInDegrees");
                        OuterConeColor.Color = (Color)_light.Light.GetProperty("OuterConeColor");
                        OuterIntensitySlider.Value = (float)_light.Light.GetProperty("OuterConeIntensity") * 100;
                        
                        Vector3 direction = (Vector3)_light.Light.GetProperty("Direction");
                        float dot = Vector3.Dot(new Vector3(0, 0, 1), direction);
                        Vector3 a = (dot * 100) * direction;
                        DirectionX.Value = 300 - a.X;
                        DirectionY.Value = 300 - a.Y;
                    }
                    break;

                case LightTypes.Distant:
                    {
                        LightColor.Color = (Color)_light.Light.GetProperty("Color");
                        IntensitySlider.Value = (float)_light.Light.GetProperty("Intensity") * 100;

                        Vector3 direction = (Vector3)_light.Light.GetProperty("Direction");
                        float dot = Vector3.Dot(new Vector3(0, 0, 1), direction);
                        Vector3 a = (dot * 100) * direction;
                        DirectionX.Value = 300 - a.X;
                        DirectionY.Value = 300 - a.Y;
                    }
                    break;

                case LightTypes.Ambient:
                    {
                        LightColor.Color = (Color)_light.Light.GetProperty("Color");
                        IntensitySlider.Value = (float)_light.Light.GetProperty("Intensity") * 100;
                    }
                    break;

                default:
                    break;
            }

            LightName.Text = _light.Light.Name;
        }

        private void UpdateLight()
        {
            if (LightTypeSelection.SelectedValue != null)
            {
                ComboBoxItem item = LightTypeSelection.SelectedItem as ComboBoxItem;
                LightTypes lightType = (LightTypes)item.Tag;

                _light.Light.Type = lightType;

                switch (lightType)
                {
                    case LightTypes.Point:
                        {
                            _light.Light.Offset = new Vector3((float)XOffset.Value, (float)YOffset.Value, (float)ZOffset.Value); 
                            _light.Light.SetProperty("ConstantAttenuation", (float)ConstantAttenuation.Value * c_ConstantAttenMultiplier);
                            _light.Light.SetProperty("LinearAttenuation", (float)LinearAttenuation.Value * c_LinearAttenMultiplier);
                            _light.Light.SetProperty("QuadraticAttenuation", (float)QuadraticAttenuation.Value * c_QuadraticAttenMultiplier);
                            _light.Light.SetProperty("Color", LightColor.Color);
                            _light.Light.SetProperty("Intensity", (float)IntensitySlider.Value / 100f);
                        }
                        break;

                    case LightTypes.Spot:
                        {
                            _light.Light.Offset = new Vector3((float)XOffset.Value, (float)YOffset.Value, (float)ZOffset.Value); 
                            _light.Light.SetProperty("ConstantAttenuation", (float)ConstantAttenuation.Value * c_ConstantAttenMultiplier);
                            _light.Light.SetProperty("LinearAttenuation", (float)LinearAttenuation.Value * c_LinearAttenMultiplier);
                            _light.Light.SetProperty("QuadraticAttenuation", (float)QuadraticAttenuation.Value * c_QuadraticAttenMultiplier);
                            _light.Light.SetProperty("InnerConeAngleInDegrees", (float)InnerCone.Value);
                            _light.Light.SetProperty("InnerConeColor", InnerConeColor.Color);
                            _light.Light.SetProperty("InnerConeIntensity", (float)InnerIntensitySlider.Value / 100f);
                            _light.Light.SetProperty("OuterConeAngleInDegrees", (float)OuterCone.Value);
                            _light.Light.SetProperty("OuterConeColor", OuterConeColor.Color);
                            _light.Light.SetProperty("OuterConeIntensity", (float)OuterIntensitySlider.Value / 100f);

                            Vector3 lookAt = new Vector3((float)DirectionX.Value, (float)DirectionY.Value, 0);
                            Vector3 offset = new Vector3(300, 300, 100);
                            _light.Light.SetProperty("Direction", Vector3.Normalize(lookAt - offset));
                        }
                        break;

                    case LightTypes.Distant:
                        {
                            _light.Light.SetProperty("Color", LightColor.Color);
                            _light.Light.SetProperty("Intensity", (float)IntensitySlider.Value / 100f);

                            Vector3 lookAt = new Vector3((float)DirectionX.Value, (float)DirectionY.Value, 0);
                            Vector3 offset = new Vector3(300, 300, 100);
                            _light.Light.SetProperty("Direction", Vector3.Normalize(lookAt - offset));
                        }
                        break;

                    case LightTypes.Ambient:
                        {
                            _light.Light.SetProperty("Color", LightColor.Color);
                            _light.Light.SetProperty("Intensity", (float)IntensitySlider.Value / 100f);
                        }
                        break;

                    default:
                        Debug.Assert(false, "Unexpected type");
                        break;
                }
            }
        }

        private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            UpdateLight();
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (InnerCone != null && OuterCone != null)
            {
                if (((Slider)sender).Name == "InnerCone")
                {
                    OuterCone.Value = Math.Max(InnerCone.Value, OuterCone.Value);
                }

                if (((Slider)sender).Name == "OuterCone")
                {
                    InnerCone.Value = Math.Min(InnerCone.Value, OuterCone.Value);
                }
            }

            UpdateLight();
        }

        private void LightName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _light.Light.Name = LightName.Text;
        }
    }
}