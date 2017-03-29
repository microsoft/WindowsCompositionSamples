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
using System.Diagnostics;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Composition;

namespace SamplesCommon
{
    public class ManagedSurface
    {
        private CompositionDrawingSurface   _surface;
        private IContentDrawer               _drawer;
        private CompositionSurfaceBrush     _brush;

        public ManagedSurface(CompositionDrawingSurface surface)
        {
            Debug.Assert(surface != null);
            _surface = surface;

            ImageLoader.Instance.RegisterSurface(this);
        }

        public void Dispose()
        {
            if (_surface != null)
            {
                _surface.Dispose();
                _surface = null;
            }

            if (_brush != null)
            {
                _brush.Dispose();
                _brush = null;
            }

            _drawer = null;

            ImageLoader.Instance.UnregisterSurface(this);
        }

        public CompositionDrawingSurface Surface
        {
            get { return _surface; }
        }

        public CompositionSurfaceBrush Brush
        {
            get
            {
                if (_brush == null)
                {
                    _brush = _surface.Compositor.CreateSurfaceBrush(_surface);
                }

                return _brush;
            }
        }

        public Size Size
        {
            get
            {
                return (_surface != null) ? _surface.Size : Size.Empty;
            }
        }
        
        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, IContentDrawer drawer)
        {
            Debug.Assert(_surface != null);

            _drawer = drawer;
            await _drawer.Draw(device, drawingLock, _surface, _surface.Size);
        }

        public async void OnDeviceReplaced(object sender, object e)
        {
            DeviceReplacedEventArgs args = (DeviceReplacedEventArgs)e;
            await ReloadContent(args.GraphicsDevce, args.DrawingLock);
        }

        private async Task ReloadContent(CompositionGraphicsDevice device, Object drawingLock)
        {
            await _drawer.Draw(device, drawingLock, _surface, _surface.Size);
        }
    }
}
