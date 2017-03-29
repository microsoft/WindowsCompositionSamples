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
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;

namespace SamplesCommon
{
    internal class TextDrawer : IContentDrawer
    {
        private string _text;
        private CanvasTextFormat _textFormat;
        private Color _textColor;
        private Color _backgroundColor;

        public TextDrawer(string text, CanvasTextFormat textFormat, Color textColor, Color bgColor)
        {
            _text = text;
            _textFormat = textFormat;
            _textColor = textColor;
            _backgroundColor = bgColor;
        }

        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(_backgroundColor);
                ds.DrawText(_text, new Rect(0, 0, surface.Size.Width, surface.Size.Height), _textColor, _textFormat);
            }
        }
    }
}
