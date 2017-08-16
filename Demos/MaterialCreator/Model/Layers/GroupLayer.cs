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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace MaterialCreator
{
    [DataContract]
    public class GroupLayer : Layer
    {
        ObservableCollection<Layer> _layers;

        public GroupLayer() : base()
        {
            StreamingContext context;
            Initialize(context);
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _type = LayerType.Group;
            _layers = new ObservableCollection<Layer>();
            _layers.CollectionChanged += OnLayerCollectionChanged;
        }

        public override ObservableCollection<Layer> LayerCollection
        {
            get
            {
                return _layers;
            }
        }

        [DataMember]
        public ObservableCollection<Layer> Layers
        {
            get { return _layers; }

            private set
            {
                if (_layers != value)
                {
                    if (_layers != null)
                    {
                        _layers.CollectionChanged -= OnLayerCollectionChanged;
                    }

                    _layers = value;

                    if (_layers != null)
                    {
                        _layers.CollectionChanged += OnLayerCollectionChanged;
                    }

                    GroupLayerChanged(this, "Layers");
                }
            }
        }

        public override bool IsActive()
        {
            if (!Enabled)
            {
                return false;
            }

            bool isActiveSublayer = false;

            // Need at least one active sublayer
            foreach (Layer layer in _layers)
            {
                if (layer.IsActive())
                {
                    isActiveSublayer = true;
                    break;
                }
            }

            return isActiveSublayer;
        }

        public void AddLayer(Layer layer)
        {
            layer.GroupLayer = this;
            layer.LayerChanged += GroupLayerChanged;

            _layers.Insert(0, layer);
        }

        private void GroupLayerChanged(object sender, string property)
        {
            Update();

            PropertyChangeHandler(this, property);
        }

        public void RemoveLayer(Layer layer)
        {
            layer.LayerChanged -= GroupLayerChanged;
            _layers.Remove(layer);
        }

        public void ReplaceLayer(Layer oldLayer, Layer newLayer)
        {
            // Transfer relevent state to new layer
            newLayer.Initialize(oldLayer);

            Layers[Layers.IndexOf(oldLayer)] = newLayer;
        }

        public override void Update()
        {
            _inputEffect = null;

            if (_layers == null)
            {
                return;
            }

            // Link all the layer effects together, hooking up the blend bindings across layers
            Layer layer;
            Layer lastEnabled = null;

            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                layer = _layers[i];

                if (layer.IsActive())
                {
                    layer.LinkLayerEffects(lastEnabled == null ? null : lastEnabled);
                    lastEnabled = layer;
                }
            }

            if (lastEnabled != null)
            {
                // Get the last enabled layer
                _inputEffect = lastEnabled.GenerateEffectGraph(false);
            }
        }

        public override void UpdateResourceBindings(CompositionEffectBrush brush)
        {
            foreach (Layer layer in _layers)
            {
                if (layer.Enabled)
                {
                    layer.UpdateResourceBindings(brush);
                }
            }
        }

        private void OnLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int count = Layers.Count;
            for (int idx = 0; idx < count; idx++)
            {
                Layers[idx].ShowSeparator = idx != (count - 1);
            }

            PropertyChangeHandler(this, nameof(Layers));
        }

        public override void FinalizeDeserialization()
        {
            base.FinalizeDeserialization();

            foreach (Layer layer in Layers)
            {
                layer.GroupLayer = this;
                layer.LayerChanged += GroupLayerChanged;

                layer.FinalizeDeserialization();
            }
        }

        public async override Task LoadResources()
        {
            base.FinalizeDeserialization();

            foreach (Layer layer in Layers)
            {
                await layer.LoadResources();
            }
        }
    }
}
