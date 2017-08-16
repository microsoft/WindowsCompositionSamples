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
using Windows.UI;

namespace MaterialCreator
{
    [DataContract]
    public class ColorLayer : Layer
    {
        Color _color;

        public ColorLayer() : base()
        {
            StreamingContext context;
            Initialize(context);
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _type = LayerType.Color;
            _inputEffect = new ColorSourceEffect();
            // Default to white, otherwise ColorPicker will start with #00000000 on a new layer.
            // Since Alpha is not enabled on the ColorPicker, the Alpha will remain 0 no matter the chosen color.
            Color = Colors.White;
        }

        [DataMember]
        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;

                    ((ColorSourceEffect)_inputEffect).Color = value;
                    PropertyChangeHandler(this, nameof(Color));
                }
            }
        }

    }


}