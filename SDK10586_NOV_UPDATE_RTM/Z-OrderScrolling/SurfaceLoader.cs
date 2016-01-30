using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI.Composition;

namespace Z_ORderScrolling
{
    public delegate CompositionDrawingSurface LoadTimeEffectHandler(CanvasBitmap bitmap, CompositionGraphicsDevice device, Size sizeTarget);

    class SurfaceLoader
    {
        private static bool                         _intialized;
        private static Compositor                   _compositor;
        private static CanvasDevice                 _canvasDevice;
        private static CompositionGraphicsDevice    _compositionDevice;

        static public void Initialize(Compositor compositor)
        {
            Debug.Assert(!_intialized);

            _compositor = compositor;
            _canvasDevice = new CanvasDevice();
            _compositionDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _canvasDevice);

            _intialized = true;
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

            CompositionDrawingSurface surface = _compositionDevice.CreateDrawingSurface(sizeTarget, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                ds.DrawImage(bitmap, new Rect(0, 0, sizeTarget.Width, sizeTarget.Height), new Rect(0, 0, sizeSource.Width, sizeSource.Height));
            }

            return surface;
        }

        static public async Task<CompositionDrawingSurface> LoadFromUri(Uri uri, Size sizeTarget, LoadTimeEffectHandler loadEffectHandler)
        {
            Debug.Assert(_intialized);

            CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(_canvasDevice, uri);
            return loadEffectHandler(bitmap, _compositionDevice, sizeTarget);
        }
    }
}