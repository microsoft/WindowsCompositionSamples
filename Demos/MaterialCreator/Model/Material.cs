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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{
    public delegate void OnBrushChanged(Material sender);

    [DataContract]
    public class Material
    {
        Compositor                          _compositor;
        ObservableCollection<Light>         _lights;
        GroupLayer                          _groupLayer;
        CompositionEffectBrush              _effectBrush;
        bool                                _layerInvalidated;
        OnBrushChanged                      _brushChangedHandler;
        
        public Material()
        {
            StreamingContext context;
            Initialize(context);
        }

        private void Group_LayerChanged(object sender, string property)
        {
            _layerInvalidated = true;

            if (_brushChangedHandler != null)
            {
                _brushChangedHandler(this);
            }
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _layerInvalidated = true;
            _groupLayer = new GroupLayer();
            _groupLayer.LayerChanged += Group_LayerChanged;
            Lights = new ObservableCollection<Light>();
        }

        public void FinalizeDeserialization()
        {
            _groupLayer.FinalizeDeserialization();
        }

        public async Task LoadResources()
        {
            await _groupLayer.LoadResources();
        }

        public void Initialize(Compositor compositor, OnBrushChanged brushChangedHandler)
        {
            Debug.Assert(compositor != null);

            _compositor = compositor;
            _brushChangedHandler = brushChangedHandler;
        }

        public void Dispose()
        {
            // Need to explicitly dispose the lights to remove the targets
            foreach (Light light in _lights)
            {
                light.Dispose();
            }
        }

        public void PropertyChangeHandler(object sender)
        {
            _layerInvalidated = true;
            _brushChangedHandler(this);
        }
                
        public CompositionBrush Brush
        {
            get
            {
                UpdateBrush();
                return _effectBrush;
            }
        }

        public IGraphicsEffect Effect
        {
            get
            {
                return _groupLayer.GenerateEffectGraph(false);
            }
        }

        [DataMember]
        public ObservableCollection<Light> Lights
        {
            get
            {
                return _lights;
            }
            private set
            {
                if (_lights != value)
                {
                    if (_lights != null)
                    {
                        _lights.CollectionChanged -= OnCollectionChanged;
                    }

                    _lights = value;

                    if (_lights != null)
                    {
                        _lights.CollectionChanged += OnCollectionChanged;
                    }
                }
            }
        }

        [DataMember]
        public ObservableCollection<Layer> Layers
        {
            get
            {
                if (_groupLayer == null)
                {
                    return null;
                }

                return _groupLayer.Layers;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int count = Lights.Count;
            for (int idx = 0; idx < count; idx++)
            {
                Lights[idx].ShowSeparator = idx != (count - 1);
            }
        }

        public void AddLayer(Layer layer)
        {
            if (layer != null)
            {
                _groupLayer.AddLayer(layer);
            }
        }

        public void ReplaceLayer(Layer oldLayer, Layer newLayer)
        {
            _groupLayer.ReplaceLayer(oldLayer, newLayer);
        }

        private async void UpdateBrush()
        {
            if (_layerInvalidated)
            {
                // Clear the old brush
                _effectBrush = null;

                IGraphicsEffect effecGraph = _groupLayer.GenerateEffectGraph(false);
                if (effecGraph != null)
                {
                    try
                    {
                        CompositionEffectFactory effectFactory = _compositor.CreateEffectFactory(effecGraph, null);
                        _effectBrush = effectFactory.CreateBrush();

                        foreach (Layer layer in _groupLayer.Layers)
                        {
                            if (layer.Enabled)
                            {
                                layer.UpdateResourceBindings(_effectBrush);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ContentDialog dialog = new ContentDialog()
                        {
                            Title = "Failed to create effect",
                            Content = String.Format("The effect creation failed with the following error.  Please simplify your material.\r\n\r\n\"{0}\"", e.Message.ToString().Replace("\r\n", " ")),
                            PrimaryButtonText = "OK",
                            IsSecondaryButtonEnabled = false,
                        };

                        await dialog.ShowAsync();

                        // Clear out the old effect
                        _effectBrush = null;
                    }

                    // Clear the flag
                    _layerInvalidated = false;
                }

                _brushChangedHandler(this);
            }
        }
    }
}