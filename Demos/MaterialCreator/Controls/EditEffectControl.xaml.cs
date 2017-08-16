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
using System.Reflection;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{
    public sealed partial class EditEffectControl : UserControl
    {
        Effect _effect;

        public EditEffectControl()
        {
            this.InitializeComponent();
        }

        public void Initialize(Effect effect)
        {
            Debug.Assert(effect != null);
            _effect = effect;

            ComboBoxItem item;
            foreach (Type effectType in Effect.SupportedEffectTypes)
            {
                item = new ComboBoxItem();
                item.Tag = effectType;
                item.Content = effectType.Name.ToString();
                item.IsSelected = effectType.Name == _effect.EffectType.Name;
                EffectType.Items.Add(item);
            }

            UpdateEffectProperties();
        }
        
        void UpdateEffectProperties()
        {
            Type effectType = (Type)EffectType.SelectedValue;

            EffectPropertyPanel.Children.Clear();

            foreach (PropertyInfo info in effectType.GetProperties())
            {
                if (Helpers.SkipProperty(info.Name))
                {
                    continue;
                }

                Helpers.AddPropertyToPanel(_effect.GraphicsEffect, _effect, EffectPropertyPanel, info);
            }
        }

        private void EffectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _effect.EffectType = (Type)EffectType.SelectedValue;

            UpdateEffectProperties();
        }
    }
}
