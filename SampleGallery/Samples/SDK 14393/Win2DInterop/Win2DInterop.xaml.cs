using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Win2DInterop : SamplePage
    {
        public Win2DInterop()
        {
            this.InitializeComponent();
        }

        public static string StaticSampleName { get { return "Win2D Interop"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription { get { return "Drawing a Win2D effect into a CompositionDrawingSurface so it can be used as part of a composition effect graph. A noise bitmap is generated using the Win2D TurbulenceEffectNoise effect and then used as the alpha channel of the content bitmap."; } }

        #region UI Event Handlers

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NoiseOctavesSlider.Value = _noiseOctaves;
            NoiseFrequencySlider.Value = _noiseFrequency.X;
            if (_noiseType == TurbulenceEffectNoise.FractalSum)
            {
                NoiseTypeComboBox.SelectedValue = "Fractal Sum";
            }
            else
            {
                NoiseTypeComboBox.SelectedValue = "Turbulence";
            }

            var fireAndForget = SetupEffect();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeEffect();
        }

        private void Placeholder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeEffect();
        }

        private void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            var fireAndForget = SetupEffect();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }
            _noiseOctaves = (int)NoiseOctavesSlider.Value;
            _noiseFrequency = new Vector2((float)NoiseFrequencySlider.Value, (float)NoiseFrequencySlider.Value);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }
            if (NoiseTypeComboBox.SelectedValue as string == "Fractal Sum")
            {
                _noiseType = TurbulenceEffectNoise.FractalSum;
            }
            else
            {
                _noiseType = TurbulenceEffectNoise.Turbulence;
            }
        }

        #endregion

        private bool _initialized = false;

        private int _noiseOctaves = 4;
        private Vector2 _noiseFrequency = new Vector2(0.05f, 0.05f);
        private TurbulenceEffectNoise _noiseType = TurbulenceEffectNoise.Turbulence;

        private TurbulenceEffect _noiseEffect = null;
        private LuminanceToAlphaEffect _noiseLuminanceEffect = null;
        private ManagedSurface _noiseSurface;

        private ManagedSurface _contentSurface = null;
        private CompositeEffect _combinedEffect = null;
        private CompositionEffectBrush _combinedEffectBrush = null;
        private SpriteVisual _outputVisual = null;

        private Vector2 EffectSize
        {
            get
            {
                if (!double.IsNaN(Placeholder.ActualWidth) && !double.IsNaN(Placeholder.ActualHeight) && Placeholder.ActualWidth > 0 && Placeholder.ActualHeight > 0)
                {
                    return new Vector2((float)Placeholder.ActualWidth, (float)Placeholder.ActualHeight);
                }
                else
                {
                    return Vector2.Zero;
                }
            }
        }

        private async Task SetupEffect()
        {
            DisposeEffect();

            _noiseEffect = new TurbulenceEffect();
            _noiseEffect.Octaves = _noiseOctaves;
            _noiseEffect.Frequency = _noiseFrequency;
            _noiseEffect.Noise = _noiseType;
            _noiseEffect.Tileable = true;
            _noiseEffect.Size = EffectSize;

            _noiseLuminanceEffect = new LuminanceToAlphaEffect();
            _noiseLuminanceEffect.Source = _noiseEffect;

            _noiseSurface = ImageLoader.Instance.LoadInteropSurface(EffectSize.ToSize(), NoiseDrawHandler);
            if (_noiseEffect == null)
            {
                DisposeEffect();
                return;
            }
            _noiseSurface.Brush.Stretch = CompositionStretch.Fill;

            var placeholderVisual = ElementCompositionPreview.GetElementVisual(Placeholder);
            var compositor = placeholderVisual.Compositor;

            _contentSurface = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/Landscapes/Landscape-1.jpg"));

            if (_contentSurface == null)
            {
                DisposeEffect();
                return;
            }

            _contentSurface.Brush.Stretch = CompositionStretch.UniformToFill;
            _combinedEffect = new CompositeEffect()
            {
                Mode = Microsoft.Graphics.Canvas.CanvasComposite.SourceOut,
                Sources =
                {
                    new CompositionEffectSourceParameter("Noise"),
                    new CompositionEffectSourceParameter("Content"),
                }
            };
            var combinedEffectFactory = compositor.CreateEffectFactory(_combinedEffect);
            _combinedEffectBrush = combinedEffectFactory.CreateBrush();
            _combinedEffectBrush.SetSourceParameter("Noise", _noiseSurface.Brush);
            _combinedEffectBrush.SetSourceParameter("Content", _contentSurface.Brush);

            _outputVisual = compositor.CreateSpriteVisual();
            _outputVisual.Brush = _combinedEffectBrush;
            _outputVisual.Size = EffectSize;

            ElementCompositionPreview.SetElementChildVisual(Placeholder, _outputVisual);

            _initialized = true;
        }

        private void NoiseDrawHandler(CompositionDrawingSurface surface, CompositionGraphicsDevice device)
        {
            if (_noiseLuminanceEffect == null || surface.Size.Width == 0 || surface.Size.Height == 0)
            {
                return;
            }
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(Colors.Transparent);
                ds.DrawImage(_noiseLuminanceEffect);
            }
        }

        private void DisposeEffect()
        {
            if (_noiseSurface != null)
            {
                _noiseSurface.Dispose();
                _noiseSurface = null;
            }
            if (_noiseLuminanceEffect != null)
            {
                _noiseLuminanceEffect.Dispose();
                _noiseLuminanceEffect = null;
            }
            if (_noiseEffect != null)
            {
                _noiseEffect.Dispose();
                _noiseEffect = null;
            }
            if (_outputVisual != null)
            {
                _outputVisual.Dispose();
                _outputVisual = null;
            }
            if (_combinedEffectBrush != null)
            {
                _combinedEffectBrush.Dispose();
                _combinedEffectBrush = null;
            }
            if (_combinedEffect != null)
            {
                _combinedEffect.Dispose();
                _combinedEffect = null;
            }
            if (_contentSurface != null)
            {
                _contentSurface.Dispose();
                _contentSurface = null;
            }
            _initialized = false;
        }

        private void ResizeEffect()
        {
            if (!_initialized)
            {
                return;
            }

            _outputVisual.Size = EffectSize;
        }
    }
}
