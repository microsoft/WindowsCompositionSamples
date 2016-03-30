using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;

namespace SamplesCommon.ImageLoader
{
    public interface IManagedSurface : IDisposable
    {
        IImageLoader ImageLoader { get; }
        ICompositionSurface Surface { get; }
        Uri Source { get; }
        Size Size { get; }
        IAsyncAction RedrawSurface();
        IAsyncAction RedrawSurface(Uri uri);
        IAsyncAction RedrawSurface(Uri uri, Size size);
        void Resize(Size size);
    }

    class ManagedSurface : IManagedSurface
    {
        private IImageLoaderInternal _imageLoader;
        private CompositionDrawingSurface _surface;
        private Uri _uri;

        public IImageLoader ImageLoader
        {
            get
            {
                return _imageLoader;
            }
        }

        public ICompositionSurface Surface
        {
            get
            {
                return _surface;
            }
        }

        public Uri Source
        {
            get
            {
                return _uri;
            }
        }

        public Size Size
        {
            get
            {
                if (_surface != null)
                {
                    return _surface.Size;
                }
                else
                {
                    return Size.Empty;
                }
            }
        }

        public ManagedSurface(IImageLoaderInternal imageLoader, Uri uri, Size size)
        {
            _imageLoader = imageLoader;
            _uri = uri;
            _surface = _imageLoader.CreateSurface(size);

            _imageLoader.DeviceReplacedEvent += OnDeviceReplaced;
        }

        private async void OnDeviceReplaced(object sender, object e)
        {
            Debug.WriteLine("CompositionImageLoader - Redrawing ManagedSurface from Device Replaced");
            await RedrawSurface();
        }

        public IAsyncAction RedrawSurface()
        {
            return RedrawSurfaceWorker(_uri, Size.Empty).AsAsyncAction();
        }

        public IAsyncAction RedrawSurface(Uri uri)
        {
            return RedrawSurfaceWorker(uri, Size.Empty).AsAsyncAction();
        }

        public IAsyncAction RedrawSurface(Uri uri, Size size)
        {
            return RedrawSurfaceWorker(uri, size).AsAsyncAction();
        }

        public void Resize(Size size)
        {
            _imageLoader.ResizeSurface(_surface, size);
        }

        private async Task RedrawSurfaceWorker(Uri uri, Size size)
        {
            _uri = uri;
            await _imageLoader.DrawSurface(_surface, _uri, size);
        }

        public void Dispose()
        {
            _surface.Dispose();
            _imageLoader.DeviceReplacedEvent -= OnDeviceReplaced;
            _surface = null;
            _imageLoader = null;
            _uri = null;
        }
    }
}
