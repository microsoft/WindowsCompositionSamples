//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
//
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI;
using SamplesCommon;
using static SamplesCommon.ImageLoader;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EffectEditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public enum EffectType
        {
            NoEffect,
            AlphaMask,
            Arithmetic,
            Blend,
            ColorSource,
            Contrast,
            Exposure,
            GammaTransfer,
            Grayscale,
            HueRotation,
            Invert,
            Saturation,
            Sepia,
            TemperatureAndTint,
            Transform2D,

            NumEffectTypes
        }

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = this;
        }


        void InitializeValues()
        {
            EffectSelector.SelectedIndex = 0;
            ArithmeticMultiply.Value = 0.5f;
            ArithmeticSource1.Value = 0.5f;
            ArithmeticSource2.Value = 0.5f;
            ArithmeticOffset.Value = 0.0f;
            BlendModeSelector.SelectedIndex = 0;
            ColorSourceRed.Value = 1.0f;
            ColorSourceGreen.Value = 1.0f;
            ColorSourceBlue.Value = 1.0f;
            ColorSourceAlpha.Value = 1.0f;
            Contrast.Value = 0.5f;
            Exposure.Value = 0.5f;
            GammaTransferChannelSelector.SelectedIndex = 0;
            GammaAmplitude.Value = 1.0f;
            GammaExponent.Value = 1.0f;
            GammaOffset.Value = 0.0f;
            HueRotation.Value = 0.5f;
            Saturation.Value = 0.5f;
            SepiaAlphaModeSelector.SelectedIndex = 0;
            Sepia.Value = 0.5f;
            Temperature.Value = 0.5f;
            Tint.Value = 0.5f;
        }

        private CompositionSurfaceBrush CreateBrushFromAsset(string name, out Size size)
        {
            CompositionDrawingSurface surface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/" + name)).Surface;
            size = surface.Size;
            return m_compositor.CreateSurfaceBrush(surface);
        }

        private CompositionSurfaceBrush CreateBrushFromAsset(string name)
        {
            Size size;
            return CreateBrushFromAsset(name, out size);
        }

        private void MainGridLoaded(object sender, RoutedEventArgs e)
        {
            m_compositor = ElementCompositionPreview.GetElementVisual(MainGrid).Compositor;
            m_root = m_compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(MainGrid, m_root);

            ImageLoader.Initialize(m_compositor);

            Size imageSize;
            m_noEffectBrush = CreateBrushFromAsset(
                "Bruno'sFamily2015 (13)-X2.jpg",
                out imageSize);
            m_imageAspectRatio = (imageSize.Width == 0 && imageSize.Height == 0) ? 1 : imageSize.Width / imageSize.Height;

            m_sprite = m_compositor.CreateSpriteVisual();
            ResizeImage(new Size(MainGrid.ActualWidth, MainGrid.ActualHeight));
            m_root.Children.InsertAtTop(m_sprite);

            // Image with alpha channel as an mask.
            var alphaMaskEffectDesc = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new CompositionEffectSourceParameter("Image"),
                    new Transform2DEffect
                    {
                        Name = "MaskTransform",
                        Source = new CompositionEffectSourceParameter("Mask")
                    }
                }
            };
            m_alphaMaskEffectBrush = m_compositor.CreateEffectFactory(
                alphaMaskEffectDesc,
                new[] { "MaskTransform.TransformMatrix" }
            ).CreateBrush();
            m_alphaMaskEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);
            m_alphaMaskEffectBrush.SetSourceParameter(
                "Mask",
                CreateBrushFromAsset("CircleMask.png"));

            // Arithmetic operations between two images.
            var arithmeticEffectDesc = new ArithmeticCompositeEffect
            {
                Name = "effect",
                ClampOutput = false,
                Source1 = new CompositionEffectSourceParameter("Source1"),
                Source2 = new CompositionEffectSourceParameter("Source2")
            };
            m_arithmeticEffectBrush = m_compositor.CreateEffectFactory(
                arithmeticEffectDesc,
                new[]
                {
                    "effect.MultiplyAmount",
                    "effect.Source1Amount",
                    "effect.Source2Amount",
                    "effect.Offset"
                }
            ).CreateBrush();
            m_arithmeticEffectBrush.SetSourceParameter(
                "Source1",
                m_noEffectBrush);
            m_arithmeticEffectBrush.SetSourceParameter(
                "Source2",
                CreateBrushFromAsset("_P2A8041.jpg"));

            // Creates a blend effect that combines two images.
            var foregroundBrush = CreateBrushFromAsset("Checkerboard_100x100.png");
            m_blendEffectBrushes = new CompositionEffectBrush[m_supportedBlendModes.Length];
            for (int i = 0; i < m_supportedBlendModes.Length; i++)
            {
                var blendEffectDesc = new BlendEffect
                {
                    Mode = m_supportedBlendModes[i],
                    Background = new CompositionEffectSourceParameter("Background"),
                    Foreground = new CompositionEffectSourceParameter("Foreground")
                };
                m_blendEffectBrushes[i] = m_compositor.CreateEffectFactory(
                    blendEffectDesc
                ).CreateBrush();
                m_blendEffectBrushes[i].SetSourceParameter(
                    "Background",
                    m_noEffectBrush);
                m_blendEffectBrushes[i].SetSourceParameter(
                    "Foreground",
                    foregroundBrush);
            }

            // Generates an image containing a solid color.
            var colorSourceEffectDesc = new ColorSourceEffect // FloodEffect
            {
                Name = "effect"
            };
            m_colorSourceEffectBrush = m_compositor.CreateEffectFactory(
                colorSourceEffectDesc,
                new[] { "effect.Color" }
            ).CreateBrush();

            // Changes the contrast of an image.
            var contrastEffectDesc = new ContrastEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_contrastEffectBrush = m_compositor.CreateEffectFactory(
                contrastEffectDesc,
                new[] { "effect.Contrast" }
            ).CreateBrush();
            m_contrastEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Changes the exposure of an image.
            var exposureEffectDesc = new ExposureEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_exposureEffectBrush = m_compositor.CreateEffectFactory(
                exposureEffectDesc,
                new[] { "effect.Exposure" }
            ).CreateBrush();
            m_exposureEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Alters the colors of an image by applying a per-channel gamma transfer function.
            var gammaTransferEffectDesc = new GammaTransferEffect
            {
                Name = "effect",
                RedDisable = false,
                GreenDisable = false,
                BlueDisable = false,
                AlphaDisable = false,
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_gammaTransferEffectBrush = m_compositor.CreateEffectFactory(
                gammaTransferEffectDesc,
                new[]
                {
                    "effect.RedAmplitude",
                    "effect.RedExponent",
                    "effect.RedOffset",
                    "effect.GreenAmplitude",
                    "effect.GreenExponent",
                    "effect.GreenOffset",
                    "effect.BlueAmplitude",
                    "effect.BlueExponent",
                    "effect.BlueOffset"
                }
            ).CreateBrush();
            m_gammaTransferEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Converts an image to monochromatic gray.
            var grayscaleEffectDesc = new GrayscaleEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_grayscaleEffectBrush = m_compositor.CreateEffectFactory(
                grayscaleEffectDesc
            ).CreateBrush();
            m_grayscaleEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Alters the color of an image by rotating its hue values.
            var hueRotationEffectDesc = new HueRotationEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_hueRotationEffectBrush = m_compositor.CreateEffectFactory(
                hueRotationEffectDesc,
                new[] { "effect.Angle" }
            ).CreateBrush();
            m_hueRotationEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Inverts the colors of an image.
            var invertEffectDesc = new InvertEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_invertEffectBrush = m_compositor.CreateEffectFactory(
                invertEffectDesc
            ).CreateBrush();
            m_invertEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Alters the saturation of an image.
            var saturationEffectDesc = new SaturationEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_saturateEffectBrush = m_compositor.CreateEffectFactory(
                saturationEffectDesc,
                new[] { "effect.Saturation" }
            ).CreateBrush();
            m_saturateEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Converts an image to sepia tones.
            var supportedAlphaModes = new[]
            {
                CanvasAlphaMode.Premultiplied,
                CanvasAlphaMode.Straight
            };
            m_sepiaEffectBrushes = new CompositionEffectBrush[supportedAlphaModes.Length];
            for (int i = 0; i < supportedAlphaModes.Length; i++)
            {
                var sepiaEffectDesc = new SepiaEffect
                {
                    Name = "effect",
                    AlphaMode = supportedAlphaModes[i],
                    Source = new CompositionEffectSourceParameter("Image")
                };
                m_sepiaEffectBrushes[i] = m_compositor.CreateEffectFactory(
                    sepiaEffectDesc,
                    new[] { "effect.Intensity" }
                ).CreateBrush();
                m_sepiaEffectBrushes[i].SetSourceParameter(
                    "Image",
                    m_noEffectBrush);
            }

            // Adjusts the temperature and/or tint of an image.
            var temperatureAndTintEffectDesc = new TemperatureAndTintEffect
            {
                Name = "effect",
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_temperatureAndTintEffectBrush = m_compositor.CreateEffectFactory(
                temperatureAndTintEffectDesc,
                new[]
                {
                    "effect.Temperature",
                    "effect.Tint"
                }
            ).CreateBrush();
            m_temperatureAndTintEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // Applies a 2D affine transform matrix to an image.
            var transform2DEffectDesc = new Transform2DEffect
            {
                TransformMatrix = new Matrix3x2(
                    -1, 0,
                    0, 1,
                    m_sprite.Size.X, 0),
                Source = new CompositionEffectSourceParameter("Image")
            };
            m_transform2DEffectBrush = m_compositor.CreateEffectFactory(
                transform2DEffectDesc
            ).CreateBrush();
            m_transform2DEffectBrush.SetSourceParameter(
                "Image",
                m_noEffectBrush);

            // For simplying UI states switch, put effect parameter grids in an array
            m_effectParamsGrids = new Grid[(int)EffectType.NumEffectTypes];
            m_effectParamsGrids[(int)EffectType.NoEffect] = null;
            m_effectParamsGrids[(int)EffectType.AlphaMask] = AlphaMaskParams;
            m_effectParamsGrids[(int)EffectType.Arithmetic] = ArithmeticParams;
            m_effectParamsGrids[(int)EffectType.Blend] = BlendParams;
            m_effectParamsGrids[(int)EffectType.ColorSource] = ColorSourceParams;
            m_effectParamsGrids[(int)EffectType.Contrast] = ContrastParams;
            m_effectParamsGrids[(int)EffectType.Exposure] = ExposureParams;
            m_effectParamsGrids[(int)EffectType.GammaTransfer] = GammaTransferParams;
            m_effectParamsGrids[(int)EffectType.Grayscale] = null;
            m_effectParamsGrids[(int)EffectType.HueRotation] = HueRotationParams;
            m_effectParamsGrids[(int)EffectType.Invert] = null;
            m_effectParamsGrids[(int)EffectType.Saturation] = SaturationParams;
            m_effectParamsGrids[(int)EffectType.Sepia] = SepiaParams;
            m_effectParamsGrids[(int)EffectType.TemperatureAndTint] = TemperatureAndTintParams;
            m_effectParamsGrids[(int)EffectType.Transform2D] = null;

            // Same as grids
            m_effectBrushes = new CompositionBrush[(int)EffectType.NumEffectTypes];
            m_effectBrushes[(int)EffectType.NoEffect] = m_noEffectBrush;
            m_effectBrushes[(int)EffectType.AlphaMask] = m_alphaMaskEffectBrush;
            m_effectBrushes[(int)EffectType.Arithmetic] = m_arithmeticEffectBrush;
            m_effectBrushes[(int)EffectType.Blend] = m_blendEffectBrushes[m_activeBlendMode];
            m_effectBrushes[(int)EffectType.ColorSource] = m_colorSourceEffectBrush;
            m_effectBrushes[(int)EffectType.Contrast] = m_contrastEffectBrush;
            m_effectBrushes[(int)EffectType.Exposure] = m_exposureEffectBrush;
            m_effectBrushes[(int)EffectType.GammaTransfer] = m_gammaTransferEffectBrush;
            m_effectBrushes[(int)EffectType.Grayscale] = m_grayscaleEffectBrush;
            m_effectBrushes[(int)EffectType.HueRotation] = m_hueRotationEffectBrush;
            m_effectBrushes[(int)EffectType.Invert] = m_invertEffectBrush;
            m_effectBrushes[(int)EffectType.Saturation] = m_saturateEffectBrush;
            m_effectBrushes[(int)EffectType.Sepia] = m_sepiaEffectBrushes[m_activeSepiaAlphaMode];
            m_effectBrushes[(int)EffectType.TemperatureAndTint] = m_temperatureAndTintEffectBrush;
            m_effectBrushes[(int)EffectType.Transform2D] = m_transform2DEffectBrush;

            this.InitializeValues();
        }

        private void MainGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_sprite != null)
            {
                ResizeImage(e.NewSize);
            }
        }

        private void ResizeImage(Size windowSize)
        {
            double visibleWidth = windowSize.Width - EffectControls.Width;
            double visibleHeight = windowSize.Height;
            double newWidth = visibleWidth;
            double newHeight = visibleHeight;

            newWidth = newHeight * m_imageAspectRatio;
            if (newWidth > visibleWidth)
            {
                newWidth = visibleWidth;
                newHeight = newWidth / m_imageAspectRatio;
            }

            m_sprite.Offset = new Vector3(
                (float)(EffectControls.Width + (visibleWidth - newWidth) / 2),
                (float)((visibleHeight - newHeight) / 2),
                0.0f);
            m_sprite.Size = new Vector2(
                (float)newWidth,
                (float)newHeight);
        }

        string ChannelName(int index)
        {
            string channel;
            switch (index)
            {
                case 0:
                    channel = "Red";
                    break;
                case 1:
                    channel = "Green";
                    break;
                case 2:
                    channel = "Blue";
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return channel;
        }

        public EffectType ActiveEffectType
        {
            get
            {
                return m_activeEffectType;
            }
            set
            {
                m_activeEffectType = value;

                foreach (Grid paramsGrid in m_effectParamsGrids)
                {
                    if (paramsGrid != null)
                    {
                        paramsGrid.Visibility = Visibility.Collapsed;
                    }
                }

                Grid selectedParamsGrid = m_effectParamsGrids[(int)m_activeEffectType];
                if (selectedParamsGrid != null)
                {
                    selectedParamsGrid.Visibility = Visibility.Visible;
                }

                m_sprite.Brush = m_effectBrushes[(int)m_activeEffectType];

                EffectControls.UpdateLayout();
            }
        }

        public int ActiveBlendMode
        {
            get
            {
                return m_activeBlendMode;
            }
            set
            {
                m_activeBlendMode = value;
                m_sprite.Brush = m_effectBrushes[(int)EffectType.Blend]
                    = m_blendEffectBrushes[m_activeBlendMode];
            }
        }

        public int ActiveSepiaAlphaMode
        {
            get
            {
                return m_activeSepiaAlphaMode;
            }
            set
            {
                m_activeSepiaAlphaMode = value;
                m_sprite.Brush = m_effectBrushes[(int)EffectType.Sepia]
                    = m_sepiaEffectBrushes[m_activeSepiaAlphaMode];
            }
        }

        public IReadOnlyList<BlendEffectMode> SupportedBlendModes
        {
            get
            {
                return m_supportedBlendModes;
            }
        }

        private void OnArithmeticMultiplyValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_arithmeticEffectBrush.Properties.InsertScalar(
                "effect.MultiplyAmount",
                (float)(e.NewValue));
        }

        private void OnArithmeticSource1ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_arithmeticEffectBrush.Properties.InsertScalar(
                "effect.Source1Amount",
                (float)(e.NewValue));
        }

        private void OnArithmeticSource2ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_arithmeticEffectBrush.Properties.InsertScalar(
                "effect.Source2Amount",
                (float)(e.NewValue));
        }

        private void OnArithmeticOffsetValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_arithmeticEffectBrush.Properties.InsertScalar(
                "effect.Offset",
                (float)(e.NewValue));
        }

        private void OnColorSourceRedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_colorSource.R = (byte)(e.NewValue * 255);
            m_colorSourceEffectBrush.Properties.InsertColor(
                "effect.Color",
                m_colorSource);
        }

        private void OnColorSourceGreenValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_colorSource.G = (byte)(e.NewValue * 255);
            m_colorSourceEffectBrush.Properties.InsertColor(
                "effect.Color",
                m_colorSource);
        }

        private void OnColorSourceBlueValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_colorSource.B = (byte)(e.NewValue * 255);
            m_colorSourceEffectBrush.Properties.InsertColor(
                "effect.Color",
                m_colorSource);
        }

        private void OnColorSourceAlphaValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_colorSource.A = (byte)(e.NewValue * 255);
            m_colorSourceEffectBrush.Properties.InsertColor(
                "effect.Color",
                m_colorSource);
        }

        private void OnContrastValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_contrastEffectBrush.Properties.InsertScalar(
                "effect.Contrast",
                (float)e.NewValue);
        }

        private void OnExposureValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_exposureEffectBrush.Properties.InsertScalar(
                "effect.Exposure",
                (float)e.NewValue);
        }

        private void OnGammaTransferChannelSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_activeEffectType == EffectType.GammaTransfer)
            {
                m_sprite.Brush = m_gammaTransferEffectBrush;
            }
        }

        private void OnGammaAmplitudeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_gammaTransferEffectBrush.Properties.InsertScalar(
                "effect." + ChannelName(GammaTransferChannelSelector.SelectedIndex) + "Amplitude",
                (float)(e.NewValue));
        }

        private void OnGammaExponentValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_gammaTransferEffectBrush.Properties.InsertScalar(
                "effect." + ChannelName(GammaTransferChannelSelector.SelectedIndex) + "Exponent",
                (float)(e.NewValue));
        }

        private void OnGammaOffsetValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_gammaTransferEffectBrush.Properties.InsertScalar(
                "effect." + ChannelName(GammaTransferChannelSelector.SelectedIndex) + "Offset",
                (float)(e.NewValue));
        }

        private void OnHueRotationValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_hueRotationEffectBrush.Properties.InsertScalar(
                "effect.Angle",
                (float)(e.NewValue * Math.PI * 2));
        }

        private void OnSaturationValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_saturateEffectBrush.Properties.InsertScalar(
                "effect.Saturation",
                (float)e.NewValue);
        }

        private void OnSepiaValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_sepiaEffectBrushes[m_activeSepiaAlphaMode].Properties.InsertScalar(
                "effect.Intensity",
                (float)e.NewValue);
        }

        private void OnTemperatureValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_temperatureAndTintEffectBrush.Properties.InsertScalar(
                "effect.Temperature",
                (float)e.NewValue);
        }

        private void OnTintValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            m_temperatureAndTintEffectBrush.Properties.InsertScalar(
                "effect.Tint",
                (float)e.NewValue);
        }

        private void OnAlphaMaskAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (AlphaMaskAnimation.IsOn)
            {
                var propertySet = m_compositor.CreatePropertySet();
                propertySet.InsertScalar("Size", 0.0f);

                // An animation for scaling the mask

                var scaleAnimation = m_compositor.CreateScalarKeyFrameAnimation();
                var linearEasing = m_compositor.CreateLinearEasingFunction();
                scaleAnimation.InsertKeyFrame(0.0f, 0.0f, linearEasing);
                scaleAnimation.InsertKeyFrame(0.4f, 1.0f, linearEasing);
                scaleAnimation.InsertKeyFrame(0.6f, 1.0f, linearEasing);
                scaleAnimation.InsertKeyFrame(1.0f, 0.0f, linearEasing);
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(5000);
                scaleAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                propertySet.StartAnimation("Size", scaleAnimation);

                var transformExpression = m_compositor.CreateExpressionAnimation(
                    "Matrix3x2(Props.Size, 0, 0, Props.Size, 0, 0)");
                transformExpression.SetReferenceParameter("Props", propertySet);
                m_alphaMaskEffectBrush.StartAnimation("MaskTransform.TransformMatrix",
                    transformExpression);
            }
            else
            {
                m_alphaMaskEffectBrush.StopAnimation("MaskTransform.TransformMatrix");
            }
        }

        private void OnContrastAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (ContrastAnimation.IsOn)
            {
                Contrast.IsEnabled = false;

                // Animates the contrast from 0 to 1 and back to 0

                var animation = m_compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(0.0f, "0.0f");
                animation.InsertExpressionKeyFrame(0.5f, "1.0f");
                animation.InsertExpressionKeyFrame(1.0f, "0.0f");
                animation.Duration = TimeSpan.FromMilliseconds(5000);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                m_contrastEffectBrush.StartAnimation("effect.Contrast", animation);
            }
            else
            {
                m_contrastEffectBrush.StopAnimation("effect.Contrast");

                Contrast.IsEnabled = true;
            }
        }

        private void OnExposureAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (ExposureAnimation.IsOn)
            {
                Exposure.IsEnabled = false;

                var animation = m_compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(0.0f, "0.0f");
                animation.InsertExpressionKeyFrame(0.5f, "1.0f");
                animation.InsertExpressionKeyFrame(1.0f, "0.0f");
                animation.Duration = TimeSpan.FromMilliseconds(5000);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                m_exposureEffectBrush.StartAnimation("effect.Exposure", animation);
            }
            else
            {
                m_exposureEffectBrush.StopAnimation("effect.Exposure");

                Exposure.IsEnabled = true;
            }
        }

        private void OnHueRotationAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (HueRotationAnimation.IsOn)
            {
                HueRotation.IsEnabled = false;

                var animation = m_compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(0.0f, "0.0f");
                animation.InsertExpressionKeyFrame(0.5f, "PI * 2");
                animation.InsertExpressionKeyFrame(1.0f, "0.0f");
                animation.Duration = TimeSpan.FromMilliseconds(5000);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                m_hueRotationEffectBrush.StartAnimation("effect.Angle", animation);
            }
            else
            {
                m_hueRotationEffectBrush.StopAnimation("effect.Angle");

                HueRotation.IsEnabled = true;
            }
        }

        private void OnSepiaAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (SepiaAnimation.IsOn)
            {
                Sepia.IsEnabled = false;

                var animation = m_compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(0.0f, "0.0f");
                animation.InsertExpressionKeyFrame(0.5f, "1.0f");
                animation.InsertExpressionKeyFrame(1.0f, "0.0f");
                animation.Duration = TimeSpan.FromMilliseconds(5000);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                m_sepiaEffectBrushes[m_activeSepiaAlphaMode].StartAnimation(
                    "effect.Intensity",
                    animation);
            }
            else
            {
                m_sepiaEffectBrushes[m_activeSepiaAlphaMode].StopAnimation("effect.Intensity");

                Sepia.IsEnabled = true;
            }
        }

        private void OnSaturationAnimationToggled(object sender, RoutedEventArgs e)
        {
            if (SaturationAnimation.IsOn)
            {
                Saturation.IsEnabled = false;

                var animation = m_compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(0.0f, "0.0f");
                animation.InsertExpressionKeyFrame(0.5f, "1.0f");
                animation.InsertExpressionKeyFrame(1.0f, "0.0f");
                animation.Duration = TimeSpan.FromMilliseconds(5000);
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                m_saturateEffectBrush.StartAnimation("effect.Saturation", animation);
            }
            else
            {
                m_saturateEffectBrush.StopAnimation("effect.Saturation");

                Saturation.IsEnabled = true;
            }
        }

        private Compositor m_compositor;
        private ContainerVisual m_root;
        private SpriteVisual m_sprite;

        private CompositionSurfaceBrush m_noEffectBrush;
        private CompositionEffectBrush m_alphaMaskEffectBrush;
        private CompositionEffectBrush m_arithmeticEffectBrush;
        private CompositionEffectBrush[] m_blendEffectBrushes;
        private CompositionEffectBrush m_colorSourceEffectBrush;
        private CompositionEffectBrush m_contrastEffectBrush;
        private CompositionEffectBrush m_exposureEffectBrush;
        private CompositionEffectBrush m_grayscaleEffectBrush;
        private CompositionEffectBrush m_gammaTransferEffectBrush;
        private CompositionEffectBrush m_hueRotationEffectBrush;
        private CompositionEffectBrush m_invertEffectBrush;
        private CompositionEffectBrush m_saturateEffectBrush;
        private CompositionEffectBrush[] m_sepiaEffectBrushes;
        private CompositionEffectBrush m_temperatureAndTintEffectBrush;
        private CompositionEffectBrush m_transform2DEffectBrush;

        private Color m_colorSource;

        private BlendEffectMode[] m_supportedBlendModes = new[]
        {
            BlendEffectMode.Multiply,
            BlendEffectMode.Screen,
            BlendEffectMode.Darken,
            BlendEffectMode.Lighten,
            BlendEffectMode.ColorBurn,
            BlendEffectMode.LinearBurn,
            BlendEffectMode.DarkerColor,
            BlendEffectMode.LighterColor,
            BlendEffectMode.ColorDodge,
            BlendEffectMode.LinearDodge,
            BlendEffectMode.Overlay,
            BlendEffectMode.SoftLight,
            BlendEffectMode.HardLight,
            BlendEffectMode.VividLight,
            BlendEffectMode.LinearLight,
            BlendEffectMode.PinLight,
            BlendEffectMode.HardMix,
            BlendEffectMode.Difference,
            BlendEffectMode.Exclusion,
            BlendEffectMode.Subtract,
            BlendEffectMode.Division
        };

        private Grid[] m_effectParamsGrids;
        private CompositionBrush[] m_effectBrushes;

        private EffectType m_activeEffectType = EffectType.NoEffect;
        private int m_activeBlendMode = 0;
        private int m_activeSepiaAlphaMode = 0;

        private double m_imageAspectRatio;
    }
}
