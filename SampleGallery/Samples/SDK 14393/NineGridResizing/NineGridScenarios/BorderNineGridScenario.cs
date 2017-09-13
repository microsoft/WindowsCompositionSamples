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

using ExpressionBuilder;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;

namespace CompositionSampleGallery.Samples.SDK_14393.NineGridResizing.NineGridScenarios
{
    sealed internal class BorderNineGridScenario : INineGridScenario
    {
        private readonly CompositionNineGridBrush _nineGridBrush;
        private readonly SpriteVisual _borderedContent;
        private readonly Compositor _compositor;
        private readonly string _text;

        public BorderNineGridScenario(Compositor compositor, CompositionSurfaceBrush surfaceBrush, Vector2 hostVisualSize, string text)
        {
            _nineGridBrush = compositor.CreateNineGridBrush();
            _nineGridBrush.Source = compositor.CreateColorBrush(Colors.Black);
            _nineGridBrush.SetInsets(10.0f);
            _nineGridBrush.IsCenterHollow = true;

            _borderedContent = compositor.CreateSpriteVisual();
            _borderedContent.Size = hostVisualSize - new Vector2(2 * 10.0f);
            _borderedContent.Offset = new Vector3(10.0f, 10.0f, 0);
            _borderedContent.Brush = surfaceBrush;

            _compositor = compositor;
            _text = text;
        }

        public CompositionBrush Brush
        {
            get
            {
                return _nineGridBrush;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }
        public void Selected(SpriteVisual hostVisual)
        {
            // Set ColorBrush as Source to NineGridBrush with HollowCenter and insert child Content visual
            hostVisual.Brush = _nineGridBrush;
            hostVisual.Children.InsertAtTop(_borderedContent);

            // Run expression animations to manage the size of borderedContent child visual

            var hostNode = hostVisual.GetReference();
            var nineBrush = _nineGridBrush.GetReference();
            var xSizeExpression = hostNode.Size.X - (nineBrush.LeftInset * nineBrush.LeftInsetScale + nineBrush.RightInset * nineBrush.RightInsetScale);
            var ySizeExpression = hostNode.Size.Y - (nineBrush.TopInset * nineBrush.TopInsetScale + nineBrush.BottomInset + nineBrush.BottomInsetScale);
            var xOffsetExpression = nineBrush.LeftInset * nineBrush.LeftInsetScale;
            var yOffsetExpression = nineBrush.TopInset * nineBrush.TopInsetScale;

            _borderedContent.StartAnimation("Size.X", xSizeExpression);
            _borderedContent.StartAnimation("Size.Y", ySizeExpression);
            _borderedContent.StartAnimation("Offset.X", xOffsetExpression);
            _borderedContent.StartAnimation("Offset.Y", yOffsetExpression);
        }    
    }
}
