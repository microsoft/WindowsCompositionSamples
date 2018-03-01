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

using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class TransparentWindow : SamplePage
    {
        Compositor      _compositor;
        SpriteVisual    _hostSprite;

        private enum EffectTypes
        {
            None,
            ColorSwap,
            ColorMask,
            RedFilter,
            ColorHighlight,
            Bloom,
        }

        public TransparentWindow()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }

        public static string        StaticSampleName => "Transparent Window"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates a few different transparent window effects";
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868956";

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate the Effect combobox
            IList<ComboBoxItem> effectList = new List<ComboBoxItem>();
            foreach (EffectTypes type in Enum.GetValues(typeof(EffectTypes)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = type;
                item.Content = type.ToString();
                effectList.Add(item);
            }

            EffectSelection.ItemsSource = effectList;
            EffectSelection.SelectedIndex = 0;

            _hostSprite = _compositor.CreateSpriteVisual();
            _hostSprite.Size = new Vector2((float)BackgroundGrid.ActualWidth, (float)BackgroundGrid.ActualHeight);

            ElementCompositionPreview.SetElementChildVisual(BackgroundGrid, _hostSprite);

            UpdateEffect();
        }

        private void EffectSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_hostSprite != null)
            {
                UpdateEffect();
            }
        }

        private void BackgroundGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_hostSprite != null)
            {
                _hostSprite.Size = e.NewSize.ToVector2();
            }
        }

        private void UpdateEffect()
        {
            ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
            switch ((EffectTypes)item.Tag)
            {
                case EffectTypes.ColorSwap:
                    {
                        Matrix5x4 mat = new Matrix5x4()
                        {
                            M11 = 0,  M12 = 0f, M13 = 1f, M14 = 0,
                            M21 = 0f, M22 = 1f, M23 = 0f, M24 = 0,
                            M31 = 1f, M32 = 0f, M33 = 0f, M34 = 0,
                            M41 = 0f, M42 = 0f, M43 = 0f, M44 = 1,
                            M51 = 0,  M52 = 0,  M53 = 0,  M54 = 0
                        };

                        IGraphicsEffect graphicsEffect = new ColorMatrixEffect()
                        {
                            ColorMatrix = mat,
                            Source = new CompositionEffectSourceParameter("ImageSource")
                        };

                        // Create the effect factory and instantiate a brush
                        CompositionEffectFactory _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, null);
                        CompositionEffectBrush brush = _effectFactory.CreateBrush();

                        // Set the destination brush as the source of the image content
                        brush.SetSourceParameter("ImageSource", _compositor.CreateHostBackdropBrush());

                        _hostSprite.Brush = brush;
                    }
                    break;

                case EffectTypes.ColorMask:
                    {
                        Matrix5x4 mat = new Matrix5x4()
                        {
                            M11 = 0.8f, M12 = 0f,   M13 = 0f,   M14 = 1.0f / 3,
                            M21 = 0f,   M22 = 0.8f, M23 = 0f,   M24 = 1.0f / 3,
                            M31 = 0f,   M32 = 0f,   M33 = 0.8f, M34 = 1.0f / 3,
                            M41 = 0f,   M42 = 0f,   M43 = 0f,   M44 = 0,
                            M51 = 0,    M52 = 0,    M53 = 0,    M54 = 0
                        };

                        IGraphicsEffect graphicsEffect = new ColorMatrixEffect()
                        {
                            ColorMatrix = mat,
                            Source = new CompositionEffectSourceParameter("ImageSource")
                        };


                        // Create the effect factory and instantiate a brush
                        CompositionEffectFactory _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, null);
                        CompositionEffectBrush brush = _effectFactory.CreateBrush();

                        // Set the destination brush as the source of the image content
                        brush.SetSourceParameter("ImageSource", _compositor.CreateHostBackdropBrush());

                        _hostSprite.Brush = brush;
                    }
                    break;

                case EffectTypes.RedFilter:
                    {
                        Matrix5x4 mat = new Matrix5x4()
                        {
                            M11 = 1,  M12 = 0f, M13 = 0f, M14 = 0,
                            M21 = 0f, M22 = 0f, M23 = 0f, M24 = 0,
                            M31 = 0f, M32 = 0f, M33 = 0f, M34 = 0,
                            M41 = 0f, M42 = 0f, M43 = 0f, M44 = 1,
                            M51 = 0,  M52 = 0,  M53 = 0,  M54 = 0
                        };

                        IGraphicsEffect graphicsEffect = new ColorMatrixEffect()
                        {
                            ColorMatrix = mat,
                            Source = new CompositionEffectSourceParameter("ImageSource")
                        };

                        // Create the effect factory and instantiate a brush
                        CompositionEffectFactory _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, null);
                        CompositionEffectBrush brush = _effectFactory.CreateBrush();

                        // Set the destination brush as the source of the image content
                        brush.SetSourceParameter("ImageSource", _compositor.CreateHostBackdropBrush());

                        _hostSprite.Brush = brush;
                    }
                    break;

                case EffectTypes.ColorHighlight:
                    {
                        IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect()
                        {
                            MultiplyAmount = 0,
                            Source1Amount = 1,
                            Source2Amount = 1,
                            Source1 = new GammaTransferEffect()
                            {
                                RedExponent = 7,
                                BlueExponent = 7,
                                GreenExponent = 7,
                                RedAmplitude = 3,
                                GreenAmplitude = 3,
                                BlueAmplitude = 3,
                                Source = new CompositionEffectSourceParameter("ImageSource")
                            },
                            Source2 = new SaturationEffect()
                            {
                                Saturation = 0,
                                Source = new CompositionEffectSourceParameter("ImageSource")
                            }
                        };

                        // Create the effect factory and instantiate a brush
                        CompositionEffectFactory _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, null);
                        CompositionEffectBrush brush = _effectFactory.CreateBrush();

                        // Set the destination brush as the source of the image content
                        brush.SetSourceParameter("ImageSource", _compositor.CreateHostBackdropBrush());

                        _hostSprite.Brush = brush;
                    }
                    break;
                case EffectTypes.Bloom:
                    {
                        var bloomEffectDesc = new ArithmeticCompositeEffect
                        {
                            Name = "Bloom",
                            Source1Amount = 1,
                            Source2Amount = 2,
                            MultiplyAmount = 0,

                            Source1 = new CompositionEffectSourceParameter("source"),
                            Source2 = new GaussianBlurEffect
                            {
                                Name = "Blur",
                                BorderMode = EffectBorderMode.Hard,
                                BlurAmount = 40,

                                Source = new BlendEffect
                                {
                                    Mode = BlendEffectMode.Multiply,

                                    Background = new CompositionEffectSourceParameter("source2"),
                                    Foreground = new CompositionEffectSourceParameter("source2"),
                                },
                            },
                        };

                        var bloomEffectFactory = _compositor.CreateEffectFactory(bloomEffectDesc,
                            new[] { "Bloom.Source2Amount", "Blur.BlurAmount" });
                        var brush = bloomEffectFactory.CreateBrush();

                        var backdropBrush = _compositor.CreateHostBackdropBrush();
                        brush.SetSourceParameter("source", backdropBrush);
                        brush.SetSourceParameter("source2", backdropBrush);

                        // Setup some animations for the bloom effect
                        ScalarKeyFrameAnimation blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        blurAnimation.InsertKeyFrame(0, 0);
                        blurAnimation.InsertKeyFrame(.5f, 2);
                        blurAnimation.InsertKeyFrame(1, 0);
                        blurAnimation.Duration = TimeSpan.FromMilliseconds(5000);
                        blurAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        ScalarKeyFrameAnimation bloomAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        bloomAnimation.InsertKeyFrame(0, 0);
                        bloomAnimation.InsertKeyFrame(.5f, 40);
                        bloomAnimation.InsertKeyFrame(1, 0);
                        bloomAnimation.Duration = TimeSpan.FromMilliseconds(5000);
                        bloomAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

                        brush.StartAnimation("Bloom.Source2Amount", blurAnimation);
                        brush.StartAnimation("Blur.BlurAmount", bloomAnimation);

                        _hostSprite.Brush = brush;
                    }
                    break;

                case EffectTypes.None:
                default:
                    _hostSprite.Brush = _compositor.CreateHostBackdropBrush();
                    break;
            }
        }

    }
}
