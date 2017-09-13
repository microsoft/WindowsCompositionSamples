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

using Windows.UI.Composition;

namespace CompositionSampleGallery.Samples.SDK_14393.NineGridResizing.NineGridScenarios
{
    sealed internal class SurfaceNineGridScenario : INineGridScenario
    {
        private readonly CompositionNineGridBrush _nineGridBrush;
        private readonly string _text;

        public SurfaceNineGridScenario(Compositor compositor, CompositionSurfaceBrush surfaceBrush, string text)
        {
            _nineGridBrush = compositor.CreateNineGridBrush();
            _nineGridBrush.Source = surfaceBrush;
            _nineGridBrush.SetInsets(60.0f);

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
            hostVisual.Brush = _nineGridBrush;
        }
    }
}
