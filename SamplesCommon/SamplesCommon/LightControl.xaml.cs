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
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml;

namespace SamplesCommon
{
    public sealed partial class LightControl : UserControl
    {
        Compositor          _compositor;
        CompositionLight    _light;
        List<Visual>        _targetVisualList;
        Visual              _coordinateSpace;

        const float         c_ConstantAttenRange  = 100;
        const float         c_LinearAttenRange    = 100;
        const float         c_QuadraticAttenRange = 100;

        public enum LightTypes
        {
            PointLight,
            SpotLight,
            DistantLight
        }

        public LightControl()
        {
            this.InitializeComponent();

            _targetVisualList = new List<Visual>(8);

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            
            ConstantAttenuation.Maximum = c_ConstantAttenRange;
            ConstantAttenuation.Value = ConstantAttenuation.Maximum;
            LinearAttenuation.Maximum = c_LinearAttenRange;
            LinearAttenuation.Value = 0;
            QuadraticAttenuation.Maximum = c_QuadraticAttenRange;
            QuadraticAttenuation.Value = 0;

            foreach (LightTypes light in Enum.GetValues(typeof(LightTypes)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = light;
                item.Content = light.ToString();
                LightTypeSelection.Items.Add(item);
            }

            LightTypeSelection.SelectedIndex = 0;
        }

        public void AddTargetVisual(Visual target)
        {
            _targetVisualList.Add(target);

            if (_light != null)
            {
                _light.Targets.Add(target);
            }
        }
        
        public Visual CoordinateSpace
        {
            get
            {
                return _coordinateSpace;
            }

            set
            {
                _coordinateSpace = value;
                UpdateLight();
            }
        }

        public Vector3 Offset
        {
            get
            {
                return new Vector3((float)XOffset.Value, (float)YOffset.Value, (float)ZOffset.Value);
            }
            set
            {
                XOffset.Value = value.X;
                YOffset.Value = value.Y;
                ZOffset.Value = value.Z;

                UpdateLight();
            }
        }

        GridLength CreateGridLength(double value, GridUnitType gridUnitType)
        {
#if USING_CSWINRT
            return new GridLength(value, gridUnitType);
#else
            return new GridLength() { Value = value, GridUnitType = gridUnitType };
#endif
        }

        GridLength CreateGridLength(double value)
        {
            return CreateGridLength(value, GridUnitType.Pixel);
        }

        private void LightTypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = LightTypeSelection.SelectedValue as ComboBoxItem;
            LightTypes lightType = (LightTypes)item.Tag;

            switch (lightType)
            {
                case LightTypes.PointLight:
                    XOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    YOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    ZOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    ConstantAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    LinearAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    QuadraticAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    InnerConeRow.Height = CreateGridLength(0);
                    OuterConeRow.Height = CreateGridLength(0);
                    InnerConeColorRow.Height = CreateGridLength(0);
                    OuterConeColorRow.Height = CreateGridLength(0);
                    LightColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    DirectionXRow.Height = CreateGridLength(0);
                    DirectionYRow.Height = CreateGridLength(0);
                    break;

                case LightTypes.SpotLight:
                    XOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    YOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    ZOffsetRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    ConstantAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    LinearAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    QuadraticAttenuationRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    InnerConeRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    OuterConeRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    InnerConeColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    OuterConeColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    LightColorRow.Height = CreateGridLength(0);
                    DirectionXRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    DirectionYRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    break;

                case LightTypes.DistantLight:
                    XOffsetRow.Height = CreateGridLength(0);
                    YOffsetRow.Height = CreateGridLength(0);
                    ZOffsetRow.Height = CreateGridLength(0);
                    ConstantAttenuationRow.Height = CreateGridLength(0);
                    LinearAttenuationRow.Height = CreateGridLength(0);
                    QuadraticAttenuationRow.Height = CreateGridLength(0);
                    InnerConeRow.Height = CreateGridLength(0);
                    OuterConeRow.Height = CreateGridLength(0);
                    InnerConeColorRow.Height = CreateGridLength(0);
                    OuterConeColorRow.Height = CreateGridLength(0);
                    LightColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    DirectionXRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    DirectionYRow.Height = CreateGridLength(1, GridUnitType.Auto);
                    break;
                default:
                    break;
            }

            // Update UI for selected light type
            if (lightType == LightTypes.SpotLight)
            {
                InnerConeRow.Height = CreateGridLength(1, GridUnitType.Auto);
                OuterConeRow.Height = CreateGridLength(1, GridUnitType.Auto);
                InnerConeColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
                OuterConeColorRow.Height = CreateGridLength(1, GridUnitType.Auto);

                LightColorRow.Height = CreateGridLength(0);
            }
            else
            {
                InnerConeRow.Height = CreateGridLength(0);
                OuterConeRow.Height = CreateGridLength(0);
                InnerConeColorRow.Height = CreateGridLength(0);
                OuterConeColorRow.Height = CreateGridLength(0);

                LightColorRow.Height = CreateGridLength(1, GridUnitType.Auto);
            }

            UpdateLight();
        }

        private void UpdateLight()
        {
            if (LightTypeSelection.SelectedValue != null)
            {
                ComboBoxItem item = LightTypeSelection.SelectedValue as ComboBoxItem;
                LightTypes lightType = (LightTypes)item.Tag;

                if (_light != null)
                {
                    _light.Targets.RemoveAll();
                }

                switch (lightType)
                {
                    case LightTypes.PointLight:
                        {
                            PointLight light = _compositor.CreatePointLight();
                            light.CoordinateSpace = CoordinateSpace;
                            light.Offset = new Vector3((float)XOffset.Value, (float)YOffset.Value, (float)ZOffset.Value);
                            light.ConstantAttenuation = (float)ConstantAttenuation.Value / c_ConstantAttenRange;
                            light.LinearAttenuation = (float)LinearAttenuation.Value / c_LinearAttenRange * .1f;
                            light.QuadraticAttenuation = (float)QuadraticAttenuation.Value / c_QuadraticAttenRange * .01f;
                            light.Color = LightColor.Color;

                            _light = light;
                        }
                        break;

                    case LightTypes.SpotLight:
                        {
                            SpotLight light = _compositor.CreateSpotLight();
                            light.CoordinateSpace = CoordinateSpace;
                            light.Offset = new Vector3((float)XOffset.Value, (float)YOffset.Value, (float)ZOffset.Value);
                            light.ConstantAttenuation = (float)ConstantAttenuation.Value / c_ConstantAttenRange;
                            light.LinearAttenuation = (float)LinearAttenuation.Value / c_LinearAttenRange *.1f;
                            light.QuadraticAttenuation = (float)QuadraticAttenuation.Value / c_QuadraticAttenRange * .01f;
                            light.InnerConeAngleInDegrees = (float)InnerCone.Value;
                            light.InnerConeColor = InnerConeColor.Color;
                            light.OuterConeAngleInDegrees = (float)OuterCone.Value;
                            light.OuterConeColor = OuterConeColor.Color;

                            Vector3 lookAt = new Vector3((float)DirectionX.Value, (float)DirectionY.Value, 0);
                            Vector3 direction = Vector3.Normalize(lookAt - new Vector3(300, 300, 300));
                            light.Direction = direction;
                            
                            _light = light;
                        }
                        break;

                    case LightTypes.DistantLight:
                        {
                            DistantLight light = _compositor.CreateDistantLight();
                            light.CoordinateSpace = CoordinateSpace;
                            light.Color = LightColor.Color;

                            Vector3 lookAt = new Vector3((float)DirectionX.Value, (float)DirectionY.Value, 0);
                            Vector3 offset = new Vector3(300, 300, 100);
                            light.Direction = Vector3.Normalize(lookAt - offset);

                            _light = light;
                        }
                        break;

                    default:
                        Debug.Assert(false, "Unexpected type");
                        break;
                }

                // Add the visual to the light target collection
                foreach (Visual target in _targetVisualList)
                {
                    _light.Targets.Add(target);
                }
            }
        }
        
        private void ColorMixer_ColorChanged(object sender, SamplesCommon.ColorEventArgs args)
        {
            UpdateLight();
        }

        private void Slider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
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
    }
}
