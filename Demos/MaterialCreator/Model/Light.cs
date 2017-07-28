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

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using Windows.UI;
using Windows.UI.Composition;

namespace MaterialCreator
{
    public enum LightTypes
    {
        Point,
        Spot,
        Distant,
        Ambient,
    }

    [DataContract]
    public class Light : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Compositor          _compositor;
        CompositionLight    _light;
        LightTypes          _type;
        bool                _enabled;
        bool                _showSeparator;
        Visual              _target;
        Dictionary<string, string> _properties;
        Vector3             _offset;
        string              _name;

        public Light(Compositor compositor, Visual container)
        {
            _enabled = true;
            Type = LightTypes.Point;
            Name = "New Light";
            Initialize(compositor, container);
        }

        public void Initialize(Compositor compositor, Visual container)
        {
            Debug.Assert(compositor != null);
            _compositor = compositor;
            UpdateLight();
            Target = container;
            
            if (_properties != null)
            {
                // Force the light to have deserialized values applied now that we know the light has been created
                Properties = _properties;

                // This one is special, so we can keep "desired static offset" separate from a dynamic one when the user has mouse following enabled.
                if (_type == LightTypes.Point || _type == LightTypes.Spot)
                {
                    SetProperty("Offset", Offset);
                }
            }
        }

        public void Dispose()
        {
            if (_light != null)
            {
                _light.Dispose();
                _light = null;
            }
        }

        public bool ShowSeparator
        {
            get { return _showSeparator; }
            set
            {
                if (_showSeparator != value)
                {
                    _showSeparator = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSeparator)));
                }
            }
        }

        [DataMember]
        public LightTypes Type
        {
            get { return _type; }
            set
            {
                if (_type != value || _light == null)
                {
                    _type = value;
                    
                    UpdateLight();
                    UpdateTarget();
                    PropertyChangeHandler(this, nameof(Color));
                }
            }
        }

        [DataMember]
        public Vector3 Offset
        {
            get { return _offset; }
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                }

                // Always do this because it could be the first time it's being set 
                // on a new light type - even if it's equal to the previous value
                SetProperty("Offset", value);
            }
        }

        private void UpdateLight()
        {
            if (_compositor == null) return;

            if (_light != null)
            {
                _light.Targets.RemoveAll();
                _light.Dispose();
                _light = null;
            }
            
            switch (_type)
            {
                case LightTypes.Point:
                    _light = _compositor.CreatePointLight();
                    break;
                case LightTypes.Spot:
                    _light = _compositor.CreateSpotLight();
                    break;
                case LightTypes.Distant:
                    _light = _compositor.CreateDistantLight();
                    break;
                case LightTypes.Ambient:
                    _light = _compositor.CreateAmbientLight();
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void UpdateTarget()
        {
            if (_light == null) return;

            _light.Targets.RemoveAll();

            if (_enabled && _target != null)
            {
                _light.Targets.Add(_target);
                if (_type != LightTypes.Ambient)
                {
                    SetProperty("CoordinateSpace", _target);
                }
            }
        }

        public Color Color
        {
            get
            {
                if (_light != null)
                {
                    switch (_type)
                    {
                        case LightTypes.Point:
                            return ((PointLight)_light).Color;
                        case LightTypes.Spot:
                            return ((SpotLight)_light).InnerConeColor;
                        case LightTypes.Distant:
                            return ((DistantLight)_light).Color;
                        case LightTypes.Ambient:
                            return ((AmbientLight)_light).Color;
                    }
                }

                return Colors.Black;
            }
        }

        public Visual Target
        {
            get { return _target; }
            set
            {
                if (_target != value)
                {
                    _target = value;
                    UpdateTarget();
                }
            }
        }

        public void SetProperty(string property, object value)
        {
            if (_light == null) return;

            if (property.EndsWith("Color"))
            {
                PropertyChangeHandler(this, nameof(Color));
            }

            PropertyInfo info = _light.GetType().GetProperties().FirstOrDefault(x => x.Name == property);
            
            object currentValue = info.GetValue(_light);
            if ((currentValue == null && value != null) || !currentValue.Equals(value))
            {
                info.SetValue(_light, value);
                PropertyChangeHandler(this, property);
            }
        }

        public object GetProperty(string property)
        {
            PropertyInfo info = _light.GetType().GetProperties().FirstOrDefault(x => x.Name == property);

            return info.GetValue(_light);
        }

        protected void PropertyChangeHandler(object sender, string propertyName)
        {
            // Data-binding support
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DataMember]
        public bool Enabled
        {
            get { return _enabled; }

            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    UpdateTarget();
                }
            }
        }
        
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChangeHandler(this, nameof(Name));
                }
            }
        }

        [DataMember]
        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (PropertyInfo info in _light.GetType().GetProperties())
                {
                    if (Helpers.SkipProperty(info.Name) ||
                        // Maybe Offset could be in the SkipProperty method, but I think maybe it's common in Comp(?), so scoping to Light only until I know for sure.
                        string.Compare(info.Name, "Offset", true) == 0)
                    {
                        continue;
                    }

                    object obj = info.GetValue(_light);
                    if (obj != null)
                    {
                        dict.Add(info.Name, Helpers.ToString(obj));
                    }
                }

                return dict;
            }

            set
            {
                _properties = value;

                if (_light != null)
                {
                    foreach (string key in value.Keys)
                    {
                        PropertyInfo info = _light.GetType().GetProperties().FirstOrDefault(x => x.Name == key);
                        SetProperty(key, Helpers.FromString(info.PropertyType, value[key]));
                    }
                }
            }
        }
    }
}