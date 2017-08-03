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
using Windows.UI.Composition;

namespace MaterialCreator
{
    public class BackdropLayer : Layer
    {
        public BackdropLayer() : base()
        {
            StreamingContext context;
            Initialize(context);
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            _type = LayerType.Backdrop;
            _inputEffect = new BorderEffect()
            {
                Name = _id,
                Source = new CompositionEffectSourceParameter(_id + "Image"),
            };
        }

        public override void UpdateResourceBindings(CompositionEffectBrush brush)
        {
            brush.SetSourceParameter(_id + "Image", brush.Compositor.CreateBackdropBrush());
        }
    }
}