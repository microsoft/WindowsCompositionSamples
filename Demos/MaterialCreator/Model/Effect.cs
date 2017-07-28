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

using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Windows.Graphics.Effects;

namespace MaterialCreator
{
    [DataContract]
    public class Effect : INotifyPropertyChanged
    {
        public static Type[] SupportedEffectTypes = {
            typeof(ColorMatrixEffect),
            typeof(ContrastEffect),
            typeof(ExposureEffect),
            typeof(GammaTransferEffect),
            typeof(GrayscaleEffect),
            typeof(GaussianBlurEffect),
            typeof(HueRotationEffect),
            typeof(InvertEffect),
            typeof(OpacityEffect),
            typeof(SaturationEffect),
            typeof(SepiaEffect),
            typeof(TemperatureAndTintEffect),
            typeof(TintEffect),
        };

        public event PropertyChangedEventHandler PropertyChanged;

        IGraphicsEffect                 _effect;
        Layer                   _layer;

        Type                            _effectType;
        bool                            _enabled;

        public Effect()
        {
            _enabled = true;
            EffectType = SupportedEffectTypes[0];
        }

        public Layer Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
            }
        }

        public bool FirstLoadCompleted { get; set; }

        public string DisplayName
        {
            get
            {
                // Strip the namespaces from the type
                string name = EffectType.ToString();

                if (name.LastIndexOf('.') > 0)
                {
                    name = name.Substring(name.LastIndexOf('.') + 1);
                }

                return name;
            }
        }

        [DataMember]
        public string EffectTypeByString
        {
            get
            {
                return _effect.GetType().AssemblyQualifiedName;
            }

            set
            {
                EffectType = Type.GetType(value);
            }
        }

        [DataMember]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;

                    Layer?.InvalidateLayer("Effect");
                }
            }
        }

        [DataMember]
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (PropertyInfo info in _effect.GetType().GetProperties())
                {
                    if (Helpers.SkipProperty(info.Name))
                    {
                        continue;
                    }

                    object obj = info.GetValue(_effect);
                    if (obj != null)
                    {
                        dict.Add(info.Name, Helpers.ToString(obj));
                    }
                }

                return dict;
            }

            set
            {
                foreach (string key in value.Keys)
                {
                    PropertyInfo info = _effect.GetType().GetProperties().FirstOrDefault(x => x.Name == key);
                    SetProperty(key, Helpers.FromString(info.PropertyType, value[key]));
                }
            }
        }

        public Type EffectType
        {
            get { return _effectType; }

            set
            {
                Debug.Assert(SupportedEffectTypes.Contains(value), "Unsupported effect type");

                if (_effectType != value)
                {
                    _effectType = value;
                    _effect = (IGraphicsEffect)Activator.CreateInstance(_effectType);

                    Layer?.InvalidateLayer("Effect");

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                }
            }
        }

        public IGraphicsEffect GraphicsEffect
        {
            get
            {
                return _effect;
            }
        }

        public void SetProperty(string property, object value)
        {
            PropertyInfo info = _effect.GetType().GetProperties().FirstOrDefault(x => x.Name == property);
            if (info != null)
            {
                object currentValue = info.GetValue(_effect);

                if (!currentValue.Equals(value))
                {
                    info.SetValue(_effect, value);

                    Layer?.InvalidateLayer("Effect");
                }
            }
        }

        public object GetEffectProperty(string property, object value)
        {
            return _effect.GetType().GetProperties().FirstOrDefault(x => x.Name == property).GetValue(_effect);
        }
    }
}
