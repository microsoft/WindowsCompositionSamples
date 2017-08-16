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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI.Composition;

namespace MaterialCreator
{
    public delegate void LayerChangedHandler(object sender, string property);

    public enum LayerType
    {
        Color,
        Image,
        Backdrop,
        BackdropHost,
        Lighting,
        Group,
    }

    public enum EdgeMode
    {
        Tiling,
        Ninegrid,
    }

    [DataContract]
    [KnownType(typeof(Effect))]
    [KnownType(typeof(ColorLayer))]
    [KnownType(typeof(ImageLayer))]
    [KnownType(typeof(BackdropLayer))]
    [KnownType(typeof(BackdropHostLayer))]
    [KnownType(typeof(LightingLayer))]
    [KnownType(typeof(GroupLayer))]
    public abstract class Layer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event LayerChangedHandler LayerChanged;

        private static int      _layerCount = 0;
        private GroupLayer      _group;
        private string          _description = "";
        private bool            _enabled;
        private string          _blendMode;
        private bool            _showSeparator;
        protected LayerType     _type;
        private ObservableCollection<Effect> _effectList;

        protected string _id;
        protected IGraphicsEffect   _inputEffect;
        protected OpacityEffect     _opacityEffect;
        private IGraphicsEffect     _chainedEffect;
        private IGraphicsEffect     _blendEffect;

        public Layer()
        {
            Effects = new ObservableCollection<Effect>();

            StreamingContext context;
            Initialize(context);
            Description = "New Layer";
            BlendMode = "Normal";
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _id = "layer" + _layerCount++.ToString();
            _enabled = true;
        }

        public void Initialize(Layer oldLayer)
        {
            Description = oldLayer.Description;
            _group = oldLayer.GroupLayer;
            LayerChanged = oldLayer.LayerChanged;

            foreach (Effect effect in oldLayer.Effects)
            {
                AddEffect(effect);
            }
        }

        public virtual void FinalizeDeserialization()
        {
            FirstLoadCompleted = true;

            foreach (Effect effect in Effects)
            {
                effect.FirstLoadCompleted = true;
                effect.Layer = this;
            }
        }

        public async virtual Task LoadResources()
        {
            await Task.CompletedTask;
        }

        public GroupLayer GroupLayer
        {
            get { return _group; }
            set
            {
                _group = value;
            }
        }

        public virtual ObservableCollection<Layer> LayerCollection
        {
            get
            {
                return null;
            }
        }

        public virtual bool IsActive()
        {
            return Enabled;
        }

        public bool FirstLoadCompleted { get; set; }

        public LayerType LayerType
        {
            get { return _type; }
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

        public virtual void Update()
        {

        }

        [DataMember]
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                if (value != _description)
                {
                    _description = value;
                    PropertyChangeHandler(this, nameof(Description));
                }
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
                    PropertyChangeHandler(this, nameof(Enabled));
                }
            }
        }

        public virtual void UpdateResourceBindings(CompositionEffectBrush brush)
        {

        }

        [DataMember]
        public string BlendMode
        {
            get
            {
                return _blendMode;
            }

            set
            {
                if (_blendMode != value)
                {
                    _blendMode = value;
                    PropertyChangeHandler(this, nameof(BlendMode));
                }
            }
        }


        [DataMember]
        public float Opacity
        {
            get { return _opacityEffect == null ? 1 : _opacityEffect.Opacity; }
            set
            {
                if (Opacity != value)
                {
                    if (value == 1)
                    {
                        _opacityEffect = null;
                    }
                    else
                    {
                        if (_opacityEffect == null)
                        {
                            _opacityEffect = new OpacityEffect();
                        }
                        _opacityEffect.Opacity = value;
                    }

                    PropertyChangeHandler(this, nameof(Opacity));
                }
            }
        }

        [DataMember]
        public ObservableCollection<Effect> Effects
        {
            get
            {
                return _effectList;
            }
            private set
            {
                if (_effectList != value)
                {
                    if (_effectList != null)
                    {
                        _effectList.CollectionChanged -= OnEffectsCollectionChanged;
                    }

                    _effectList = value;

                    if (_effectList != null)
                    {
                        _effectList.CollectionChanged += OnEffectsCollectionChanged;
                    }
                }
            }
        }

        private void OnEffectsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PropertyChangeHandler(this, nameof(Effects));
        }

        private IGraphicsEffect EffectRoot
        {
            get
            {
                IGraphicsEffect effectRoot = _inputEffect;

                if (effectRoot != null)
                {
                    if (_chainedEffect != null)
                    {
                        effectRoot = _chainedEffect;
                    }

                    if (_opacityEffect != null)
                    {
                        _opacityEffect.Source = effectRoot;
                        effectRoot = _opacityEffect;
                    }
                }

                return effectRoot;
            }
        }


        public IGraphicsEffect GenerateEffectGraph(bool layerOnly)
        {
            if (layerOnly)
            {
                // Only requested the effect for this layer, not the effect for the whole graph
                LinkLayerEffects(null);
                return EffectRoot;
            }
            else
            {
                // Update the entire graph from this layer on and return the effect
                Update();

                if (_blendEffect == null)
                {
                    return EffectRoot;
                }
                else
                {
                    return _blendEffect;
                }
            }
        }

        public void AddEffect(Effect effect)
        {
            if (effect != null)
            {
                effect.Layer = this;

                Effects.Add(effect);
            }
        }

        public void LinkLayerEffects(Layer nextLayer)
        {
            IGraphicsEffect source = _inputEffect;
            _chainedEffect = null;

            // Link the effect chain
            foreach (var effect in Effects)
            {
                if (effect.Enabled)
                {
                    effect.GraphicsEffect.GetType().GetProperties().FirstOrDefault(x => x.Name == "Source").SetValue(effect.GraphicsEffect, source);
                    source = effect.GraphicsEffect;
                    _chainedEffect = effect.GraphicsEffect;
                }
            }

            if (nextLayer != null)
            {
                IGraphicsEffect layerEffect = EffectRoot;
                IGraphicsEffect blendEffect;

                blendEffect = Helpers.GetEffectFromBlendMode(_blendMode);

                if (blendEffect is BlendEffect)
                {
                    BlendEffect blend = (BlendEffect)blendEffect;
                    blend.Foreground = nextLayer.GenerateEffectGraph(false);
                    blend.Background = layerEffect;
                    _blendEffect = blend;
                }
                else if (blendEffect is CompositeEffect)
                {
                    CompositeEffect composite = (CompositeEffect)blendEffect;
                    composite.Sources.Add(nextLayer.GenerateEffectGraph(false));
                    composite.Sources.Add(layerEffect);
                    _blendEffect = composite;
                }
                else if (blendEffect is ArithmeticCompositeEffect)
                {
                    ArithmeticCompositeEffect arith = (ArithmeticCompositeEffect)blendEffect;
                    arith.Source1 = layerEffect;
                    arith.Source2 = nextLayer.GenerateEffectGraph(false);
                    _blendEffect = arith;
                }
                else
                {
                    Debug.Assert(false);
                }
            }
            else
            {
                _blendEffect = null;
            }
        }

        protected void PropertyChangeHandler(object sender, string propertyName)
        {
            // Data-binding support
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            InvalidateLayer(propertyName);
        }

        public void InvalidateLayer(string propertyName)
        {
            LayerChanged?.Invoke(this, propertyName);
        }
    }
}
