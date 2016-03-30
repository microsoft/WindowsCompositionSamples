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

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI;
using Windows.UI.Composition;

namespace SamplesCommon
{
    public delegate CompositionDrawingSurface LoadTimeEffectHandler(CanvasBitmap bitmap, CompositionGraphicsDevice device, Size sizeTarget);

    public class SurfaceLoader
    {
        private static bool                         _intialized;
        private static Compositor                   _compositor;
        private static CanvasDevice                 _canvasDevice;
        private static CompositionGraphicsDevice    _compositionDevice;

        static public void Initialize(Compositor compositor)
        {
            Debug.Assert(!_intialized || compositor == _compositor);

            if (!_intialized)
            {
                _compositor = compositor;
                _canvasDevice = new CanvasDevice();
                _compositionDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);

                _intialized = true;
            }
        }

        static public void Uninitialize()
        {
            _compositor = null;

            if (_compositionDevice != null)
            {
                _compositionDevice.Dispose();
                _compositionDevice = null;
            }

            if(_canvasDevice != null)
            {
                _canvasDevice.Dispose();
                _canvasDevice = null;
            }

            _intialized = false;
        }

        static public bool IsInitialized
        {
            get
            {
                return _intialized;
            }
        }

        static public async Task<CompositionDrawingSurface> LoadFromUri(Uri uri)
        {
            return await LoadFromUri(uri, Size.Empty);
        }

        static public async Task<CompositionDrawingSurface> LoadFromUri(Uri uri, Size sizeTarget)
        {
            Debug.Assert(_intialized);
            
            CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(_canvasDevice, uri);
            Size sizeSource = bitmap.Size;

            if (sizeTarget.IsEmpty)
            {
                sizeTarget = sizeSource;
            }

            CompositionDrawingSurface surface = _compositionDevice.CreateDrawingSurface(sizeTarget, 
                                                            DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                ds.DrawImage(bitmap, new Rect(0, 0, sizeTarget.Width, sizeTarget.Height), new Rect(0, 0, sizeSource.Width, sizeSource.Height));
            }

            return surface;
        }

        static public CompositionDrawingSurface LoadText(string text, Size sizeTarget, CanvasTextFormat textFormat, Color textColor, Color bgColor)
        {
            Debug.Assert(_intialized);
            
            CompositionDrawingSurface surface = _compositionDevice.CreateDrawingSurface(sizeTarget, 
                                                            DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(bgColor);
                ds.DrawText(text, new Rect(0, 0, sizeTarget.Width, sizeTarget.Height), textColor, textFormat);
            }

            return surface;
        }

        static public async Task<CompositionDrawingSurface> LoadFromUri(Uri uri, Size sizeTarget, LoadTimeEffectHandler loadEffectHandler)
        {
            Debug.Assert(_intialized);

            if (loadEffectHandler != null)
            {
                CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(_canvasDevice, uri);
                return loadEffectHandler(bitmap, _compositionDevice, sizeTarget);
            }
            else
            {
                return await LoadFromUri(uri, sizeTarget);
            }
        }
    }
}