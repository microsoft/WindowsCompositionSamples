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
using System.Runtime.Serialization;
using Windows.Storage;
using Windows.UI.Composition.Effects;

namespace MaterialCreator
{

    public class LightingLayer : ImageLayer
    {
        BorderEffect _borderEffect;

        public LightingLayer() : base()
        {
            StreamingContext context;
            Initialize(context);
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _type = LayerType.Lighting;
            _borderEffect = (BorderEffect)_inputEffect;
            _inputEffect = new SceneLightingEffect()
            {
                AmbientAmount = .3f,
                DiffuseAmount = 1,
                SpecularAmount = .2f,
                SpecularShine = 16,
            };

            _properties.Add(new LayerProperty("AmbientAmount", _inputEffect));
            _properties.Add(new LayerProperty("DiffuseAmount", _inputEffect));
            _properties.Add(new LayerProperty("SpecularAmount", _inputEffect));
            _properties.Add(new LayerProperty("SpecularShine", _inputEffect));
            _properties.Add(new LayerProperty("BrdfType", _inputEffect));
        }

        public override StorageFile File
        {
            set
            {
                SceneLightingEffect lighting = (SceneLightingEffect)_inputEffect;

                if (value != null)
                {
                    lighting.NormalMapSource = _borderEffect;
                }
                else
                {
                    lighting.NormalMapSource = null;
                }

                base.File = value;
            }
        }

        public override void SetProperty(string property, object value)
        {
            if (!SetPropertyHelper(new object[] { _borderEffect }, property, value))
            {
                base.SetProperty(property, value);
            }
        }

        public override BorderEffect BorderEffect
        {
            get { return _borderEffect; }
        }

        public SceneLightingEffect LightingEffect
        {
            get { return (SceneLightingEffect)_inputEffect; }
        }
    }
}