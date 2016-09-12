using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

namespace SamplesCommon.ImageLoader
{
    public interface ICircleSurface : IDisposable
    {
        IImageLoader ImageLoader { get; }
        ICompositionSurface Surface { get; }
        float Radius { get; set; }
        Color Color { get; set; }
        Size Size { get; }
        void RedrawSurface();
    }

    class CircleSurface : ICircleSurface
    {
        private IImageLoaderInternal _imageLoader;
        private CompositionDrawingSurface _surface;
        private float _radius;
        private Color _color;

        public IImageLoader ImageLoader { get { return _imageLoader; } }
        public ICompositionSurface Surface { get { return _surface; } }
        public Size Size { get { return _surface.Size; } }
        
        public float Radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value)
                {
                    _radius = value;
                    _imageLoader.ResizeSurface(_surface, new Size(_radius * 2, _radius * 2));
                    RedrawSurface();
                }
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    RedrawSurface();
                }
            }
        }

        public CircleSurface(IImageLoaderInternal imageLoader, float radius, Color color)
        {
            _imageLoader = imageLoader;
            _radius = radius;
            _color = color;
            _surface = _imageLoader.CreateSurface(new Size(radius * 2, radius * 2));

            _imageLoader.DeviceReplacedEvent += OnDeviceReplaced;
        }

        private void OnDeviceReplaced(object sender, object e)
        {
            RedrawSurface();
        }

        public void RedrawSurface()
        {
            _imageLoader.DrawIntoSurface(_surface, (surface, device) =>
            {
                using (var session = CanvasComposition.CreateDrawingSession(surface))
                {
                    session.Clear(Colors.Transparent);
                    session.FillCircle(new Vector2(_radius, _radius), _radius, _color);
                }
            });
        }

        public void Dispose()
        {
            _surface.Dispose();
            _imageLoader.DeviceReplacedEvent -= OnDeviceReplaced;
            _surface = null;
            _imageLoader = null;
        }
    }
}
