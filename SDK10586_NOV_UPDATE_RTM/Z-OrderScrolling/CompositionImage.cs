using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Z_ORderScrolling
{
    public sealed class CompositionImage : Control
    {
        private bool                        _unloaded;
        private Compositor                  _compositor;
        private SpriteVisual                _sprite;
        private Uri                         _uri;
        private CompositionDrawingSurface   _surface;
        private CompositionDrawingSurface   _effectSurface;
        private CompositionEffectBrush      _effectBrush;
        private LoadTimeEffectHandler       _loadEffectDelegate;
        
        public CompositionImage()
        {
            this.DefaultStyleKey = typeof(CompositionImage);
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.Loading += CompImage_Loading;
            this.Unloaded += CompImage_Unloaded;
            this.SizeChanged += CompImage_SizeChanged;
        }

        private void ReleaseContent()
        {
            if (_surface != null)
            {
                _surface.Dispose();
                _surface = null;
            }

            if (_effectSurface != null)
            {
                _effectSurface.Dispose();
                _effectSurface = null;
            }

            if (_effectBrush != null)
            {
                _effectBrush.Dispose();
                _effectBrush = null;
            }
        }

        private void CompImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_sprite != null)
            {
                ApplyStretchingPolicy(e.NewSize);

                _sprite.Size = e.NewSize.ToVector2();
            }
        }

        private void CompImage_Loading(FrameworkElement sender, object args)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _sprite = _compositor.CreateSpriteVisual();
            _sprite.Size = new Vector2((float)ActualWidth, (float)ActualHeight);

            ElementCompositionPreview.SetElementChildVisual(this, _sprite);
        }

        private void CompImage_Unloaded(object sender, RoutedEventArgs e)
        {
            _unloaded = true;

            if (_sprite != null)
            {
                _sprite.Dispose();
                _sprite = null;
            }
        }

        private void ApplyStretchingPolicy(Size targetSize)
        {
            if (_surface != null)
            {
                // Uniform stretching only for now
                double aspect = _surface.Size.Height / _surface.Size.Width;
                if (_surface.Size.Width > _surface.Size.Height)
                {
                    Height = targetSize.Width * aspect;
                }
                else
                {
                    Width = targetSize.Height * aspect;
                }
            }
        }

        public Uri Uri
        {
            get { return _uri; }
            set
            {
                if (_uri != value)
                {
                    _uri = value;
                    OnUriChanged();
                }
            }
        }

        public LoadTimeEffectHandler LoadTimeEffectHandler
        {
            get { return _loadEffectDelegate; }
            set
            {
                _loadEffectDelegate = value;
            }
        }

        private async void OnUriChanged()
        {
            if (_uri == null)
            {
                ReleaseContent();
                return;
            }

            _surface = await SurfaceLoader.LoadFromUri(_uri);
            if (_loadEffectDelegate != null)
            {
                _effectSurface = await SurfaceLoader.LoadFromUri(_uri, Size.Empty, _loadEffectDelegate);
            }

            // Async operations may take a while.  If we've unloaded, return now.
            if (_unloaded)
            {
                ReleaseContent();
                return;
            }

            // Apply stretching policy
            ApplyStretchingPolicy(new Size(ActualWidth, ActualHeight));

            if (_effectBrush == null)
            {
                CompositionSurfaceBrush brush = (CompositionSurfaceBrush)_sprite.Brush;

                if (brush != null)
                {
                    brush.Surface = _surface;
                }
                else
                {
                    _sprite.Brush = _compositor.CreateSurfaceBrush(_surface);
                }
            }
            else
            {
                _effectBrush.SetSourceParameter("ImageSource", _compositor.CreateSurfaceBrush(_surface));

                if (_effectSurface != null)
                {
                    _effectBrush.SetSourceParameter("EffectSource", _compositor.CreateSurfaceBrush(_effectSurface));
                }
                _sprite.Brush = _effectBrush;
            }
        }

        public async void SetEffectBrush(CompositionEffectBrush brush)
        {
            // Release previous brush if set
            if (_effectBrush != null)
            {
                _effectBrush.Dispose();
                _effectBrush = null;
            }

            if (_effectSurface != null)
            {
                _effectSurface.Dispose();
                _effectSurface = null;
            }

            _effectBrush = brush;

            // Set the new brush
            if (_sprite != null)
            {
                // Release current brush
                if (_sprite.Brush != null)
                {
                    _sprite.Brush.Dispose();
                }

                if (_surface != null)
                {
                    // If the effect brush is cleared, create a surface brush
                    if (_effectBrush == null)
                    {
                        _sprite.Brush = _compositor.CreateSurfaceBrush(_surface);
                    }
                    else
                    {
                        _effectBrush.SetSourceParameter("ImageSource", _compositor.CreateSurfaceBrush(_surface));

                        if (_loadEffectDelegate != null)
                        {                           
                            _effectSurface = await SurfaceLoader.LoadFromUri(_uri, Size.Empty, _loadEffectDelegate);
                            _effectBrush.SetSourceParameter("EffectSource", _compositor.CreateSurfaceBrush(_effectSurface));
                        }
                        _sprite.Brush = _effectBrush;
                    }
                }
            }
        }

        public CompositionEffectBrush EffectBrush
        {
            get { return _effectBrush; }
        }
    }
}
