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
using System.Numerics;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

using Microsoft.Graphics.Canvas.UI.Composition;

namespace SamplesCommon
{
    internal class CircleDrawer : IContentDrawer
    {
        private float _radius;
        private Color _color;

        public CircleDrawer(float radius, Color color)
        {
            _radius = radius;
            _color = color;
        }

        public float Radius
        {
            get { return _radius; }
        }

        public Color Color
        {
            get { return _color; }
        }

#pragma warning disable 1998
        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(Colors.Transparent);
                ds.FillCircle(new Vector2(_radius, _radius), _radius, _color);
            }
        }
    }
}
