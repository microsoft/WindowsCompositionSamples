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

using CompositionSampleGallery.Shared;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class ForegroundFocusEffects : SamplePage
    {
        private SpriteVisual        _destinationSprite;
        private Compositor          _compositor;
        private CompositionScopedBatch
                                    _scopeBatch;
        private ManagedSurface      _maskSurface;

        public enum EffectTypes
        {
            Blur,
            LightenBlur,
            DarkenBlur,
            RainbowBlur,
            Mask,
            VividLight,
            Desaturation,
            Hue,
        }

        public ForegroundFocusEffects()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
        }

        public static string    StaticSampleName => "Foreground Focus Effects"; 
        public override string  SampleName => StaticSampleName;
        public static string    StaticSampleDescription => "Demonstrates how to use a BackDrop effect to deemphasize background content. Click on any thumbnail to trigger the selected effect.";
        public override string  SampleDescription => StaticSampleDescription; 
        public override string  SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761179"; 

        public LocalDataSource Model { set; get; }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThumbnailList.ItemsSource = ThumbnailList.ItemsSource = Model.AggregateDataSources(new ObservableCollection<Thumbnail>[] { Model.Landscapes, Model.Nature });

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

            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create the destinatio sprite, sized to cover the entire list
            _destinationSprite = _compositor.CreateSpriteVisual();
            _destinationSprite.Size = new Vector2((float)ThumbnailList.ActualWidth, (float)ThumbnailList.ActualHeight);

            // Start out with the destination layer invisible to avoid any cost until necessary
            _destinationSprite.IsVisible = false;

            // Create the .png surface
            _maskSurface = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/NormalMapsAndMasks/ForegroundFocusMask.png"));

            ElementCompositionPreview.SetElementChildVisual(ThumbnailList, _destinationSprite);

            // Update the effect to set the appropriate brush 
            UpdateEffect();

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Dispose the sprite and unparent it
            ElementCompositionPreview.SetElementChildVisual(ThumbnailList, null);

            if (_destinationSprite != null)
            {
                _destinationSprite.Dispose();
                _destinationSprite = null;
            }

            if (_maskSurface != null)
            {
                _maskSurface.Dispose();
                _maskSurface = null;
            }
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            CompositionImage image = args.ItemContainer.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
            Thumbnail thumbnail = args.Item as Thumbnail;
            Uri uri = new Uri(thumbnail.ImageUrl);

            // Update the image URI
            image.Source = uri;
        }

        private void ThumbnailList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_destinationSprite != null)
            {
                _destinationSprite.Size = e.NewSize.ToVector2();

                ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
                switch ((EffectTypes)item.Tag)
                {
                    case EffectTypes.Mask:
                        {
                            CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                            CompositionSurfaceBrush surfaceBrush = (CompositionSurfaceBrush)brush.GetSourceParameter("SecondSource");
                            surfaceBrush.CenterPoint = e.NewSize.ToVector2() * .5f;
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private async void ThumbnailList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Thumbnail thumbnail = (Thumbnail)e.ClickedItem;

            // If we're in the middle of an animation, cancel it now
            if (_scopeBatch != null)
            {
                CleanupScopeBatch();
            }

            // We're starting our transition, show the destination sprite
            _destinationSprite.IsVisible = true;

            // Animate from transparent to fully opaque
            ScalarKeyFrameAnimation showAnimation = _compositor.CreateScalarKeyFrameAnimation();
            showAnimation.InsertKeyFrame(0f, 0f);
            showAnimation.InsertKeyFrame(1f, 1f);
            showAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            _destinationSprite.StartAnimation("Opacity", showAnimation);

            // Use whichever effect is currently selected
            ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
            switch ((EffectTypes)item.Tag)
            {
                case EffectTypes.Mask:
                    {
                        CompositionSurfaceBrush brush = ((CompositionEffectBrush)_destinationSprite.Brush).GetSourceParameter("SecondSource") as CompositionSurfaceBrush;
                        Vector2KeyFrameAnimation scaleAnimation = _compositor.CreateVector2KeyFrameAnimation();
                        scaleAnimation.InsertKeyFrame(0f, new Vector2(1.25f, 1.25f));
                        scaleAnimation.InsertKeyFrame(1f, new Vector2(0f, 0f));
                        scaleAnimation.Duration = TimeSpan.FromMilliseconds(2000);
                        brush.StartAnimation("Scale", scaleAnimation);
                        break;
                    }
                case EffectTypes.VividLight:
                    {
                        CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                        ColorKeyFrameAnimation coloAnimation = _compositor.CreateColorKeyFrameAnimation();
                        coloAnimation.InsertKeyFrame(0f, Color.FromArgb(255, 255, 255, 255));
                        coloAnimation.InsertKeyFrame(0f, Color.FromArgb(255, 30, 30, 30));
                        coloAnimation.Duration = TimeSpan.FromMilliseconds(4000);
                        brush.StartAnimation("Base.Color", coloAnimation);
                        break;
                    }
                case EffectTypes.Hue:
                    {
                        CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                        ScalarKeyFrameAnimation rotateAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        rotateAnimation.InsertKeyFrame(0f, 0f);
                        rotateAnimation.InsertKeyFrame(1f, (float)Math.PI);
                        rotateAnimation.Duration = TimeSpan.FromMilliseconds(4000);
                        brush.StartAnimation("Hue.Angle", rotateAnimation);
                        break;
                    }
                case EffectTypes.RainbowBlur:
                    {
                        CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                        ColorKeyFrameAnimation colorAnimation = _compositor.CreateColorKeyFrameAnimation();
                        colorAnimation.InsertKeyFrame(0, Colors.Red);
                        colorAnimation.InsertKeyFrame(.16f, Colors.Orange);
                        colorAnimation.InsertKeyFrame(.32f, Colors.Yellow);
                        colorAnimation.InsertKeyFrame(.48f, Colors.Green);
                        colorAnimation.InsertKeyFrame(.64f, Colors.Blue);
                        colorAnimation.InsertKeyFrame(.80f, Colors.Purple);
                        colorAnimation.InsertKeyFrame(1, Colors.Red);
                        colorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                        colorAnimation.Duration = TimeSpan.FromMilliseconds(5000);
                        brush.StartAnimation("Base.Color", colorAnimation);
                        break;
                    }
                default:
                    break;
            }

            // Create the dialog
            var messageDialog = new MessageDialog(thumbnail.Name);
            messageDialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(DialogDismissedHandler)));

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void DialogDismissedHandler(IUICommand command)
        {
            // Start a scoped batch so we can register to completion event and hide the destination layer
            _scopeBatch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            // Start the hide animation to fade out the destination effect
            ScalarKeyFrameAnimation hideAnimation = _compositor.CreateScalarKeyFrameAnimation();
            hideAnimation.InsertKeyFrame(0f, 1f);
            hideAnimation.InsertKeyFrame(1.0f, 0f);
            hideAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _destinationSprite.StartAnimation("Opacity", hideAnimation);

            // Use whichever effect is currently selected
            ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
            switch ((EffectTypes)item.Tag)
            {
                case EffectTypes.Mask:
                    {
                        CompositionSurfaceBrush brush = ((CompositionEffectBrush)_destinationSprite.Brush).GetSourceParameter("SecondSource") as CompositionSurfaceBrush;
                        Vector2KeyFrameAnimation scaleAnimation = _compositor.CreateVector2KeyFrameAnimation();
                        scaleAnimation.InsertKeyFrame(1f, new Vector2(2.0f, 2.0f));
                        scaleAnimation.Duration = TimeSpan.FromMilliseconds(1000);
                        brush.StartAnimation("Scale", scaleAnimation);
                        break;
                    }
                case EffectTypes.VividLight:
                    {
                        CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                        ColorKeyFrameAnimation coloAnimation = _compositor.CreateColorKeyFrameAnimation();
                        coloAnimation.InsertKeyFrame(1f, Color.FromArgb(255, 100, 100, 100));
                        coloAnimation.Duration = TimeSpan.FromMilliseconds(1500);
                        brush.StartAnimation("Base.Color", coloAnimation);
                        break;
                    }
                case EffectTypes.Hue:
                    {
                        CompositionEffectBrush brush = (CompositionEffectBrush)_destinationSprite.Brush;
                        ScalarKeyFrameAnimation rotateAnimation = _compositor.CreateScalarKeyFrameAnimation();
                        rotateAnimation.InsertKeyFrame(1f, 0f);
                        rotateAnimation.Duration = TimeSpan.FromMilliseconds(1500);
                        brush.StartAnimation("Hue.Angle", rotateAnimation);
                        break;
                    }
                default:
                    break;
            }

            //Scoped batch completed event
            _scopeBatch.Completed += ScopeBatch_Completed;
            _scopeBatch.End();
        }

        private void EffectSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEffect();
        }

        private void UpdateEffect()
        {
            if (_compositor != null)
            {
                ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
                IGraphicsEffect graphicsEffect = null;
                CompositionBrush secondaryBrush = null;
                string[] animatableProperties = null;

                //
                // Create the appropriate effect graph and resources
                //

                switch ((EffectTypes)item.Tag)
                {
                    case EffectTypes.Desaturation:
                        {
                            graphicsEffect = new SaturationEffect()
                            {
                                Saturation = 0.0f,
                                Source = new CompositionEffectSourceParameter("ImageSource")
                            };
                        }
                        break;

                    case EffectTypes.Hue:
                        {
                            graphicsEffect = new HueRotationEffect()
                            {
                                Name = "Hue",
                                Angle = 3.14f,
                                Source = new CompositionEffectSourceParameter("ImageSource")
                            };
                            animatableProperties = new[] { "Hue.Angle" };
                        }
                        break;

                    case EffectTypes.VividLight:
                        {
                            graphicsEffect = new BlendEffect()
                            {
                                Mode = BlendEffectMode.VividLight,
                                Foreground = new ColorSourceEffect()
                                {
                                    Name = "Base",
                                    Color = Color.FromArgb(255,80,40,40)
                                },
                                Background = new CompositionEffectSourceParameter("ImageSource"),
                            };
                            animatableProperties = new[] { "Base.Color" };
                        }
                        break;
                    case EffectTypes.Mask:
                        {
                            graphicsEffect = new CompositeEffect()
                            {
                                Mode = CanvasComposite.DestinationOver,
                                Sources =
                                {
                                    new CompositeEffect()
                                    {
                                        Mode = CanvasComposite.DestinationIn,
                                        Sources =
                                        {

                                            new CompositionEffectSourceParameter("ImageSource"),
                                            new CompositionEffectSourceParameter("SecondSource")
                                        }
                                    },
                                    new ColorSourceEffect()
                                    {
                                        Color = Color.FromArgb(200,255,255,255)
                                    },
                                }
                            };

                            _maskSurface.Brush.Stretch = CompositionStretch.UniformToFill;
                            _maskSurface.Brush.CenterPoint = _destinationSprite.Size * .5f;
                            secondaryBrush = _maskSurface.Brush;
                        }
                        break;
                    case EffectTypes.Blur:
                        {
                            graphicsEffect = new GaussianBlurEffect()
                            {
                                BlurAmount = 20,
                                Source = new CompositionEffectSourceParameter("ImageSource"),
                                Optimization = EffectOptimization.Balanced,
                                BorderMode = EffectBorderMode.Hard,
                            };
                        }
                        break;
                    case EffectTypes.LightenBlur:
                        {
                            graphicsEffect = new ArithmeticCompositeEffect()
                            {
                                Source1Amount = .4f,
                                Source2Amount = .6f,
                                MultiplyAmount = 0,
                                Source1 = new ColorSourceEffect()
                                {
                                    Name = "Base",
                                    Color = Color.FromArgb(255, 255, 255, 255),
                                },
                                Source2 = new GaussianBlurEffect()
                                {
                                    BlurAmount = 20,
                                    Source = new CompositionEffectSourceParameter("ImageSource"),
                                    Optimization = EffectOptimization.Balanced,
                                    BorderMode = EffectBorderMode.Hard,
                                }
                            };
                        }
                        break;
                    case EffectTypes.DarkenBlur:
                        {
                            graphicsEffect = new ArithmeticCompositeEffect()
                            {
                                Source1Amount = .4f,
                                Source2Amount = .6f,
                                MultiplyAmount = 0,
                                Source1 = new ColorSourceEffect()
                                {
                                    Name = "Base",
                                    Color = Color.FromArgb(255, 0, 0, 0),
                                },
                                Source2 = new GaussianBlurEffect()
                                {
                                    BlurAmount = 20,
                                    Source = new CompositionEffectSourceParameter("ImageSource"),
                                    Optimization = EffectOptimization.Balanced,
                                    BorderMode= EffectBorderMode.Hard,
                                }
                            };
                        }
                        break;
                    case EffectTypes.RainbowBlur:
                        {
                            graphicsEffect = new ArithmeticCompositeEffect()
                            {
                                Source1Amount = .3f,
                                Source2Amount = .7f,
                                MultiplyAmount = 0,
                                Source1 = new ColorSourceEffect()
                                {
                                    Name = "Base",
                                    Color = Color.FromArgb(255, 0, 0, 0),
                                },
                                Source2 = new GaussianBlurEffect()
                                {
                                    BlurAmount = 20,
                                    Source = new CompositionEffectSourceParameter("ImageSource"),
                                    Optimization = EffectOptimization.Balanced,
                                    BorderMode = EffectBorderMode.Hard,
                                }
                            };
                            animatableProperties = new[] { "Base.Color" };
                        }
                        break;
                    default:
                        break;
                }

                // Create the effect factory and instantiate a brush
                CompositionEffectFactory _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, animatableProperties);
                CompositionEffectBrush brush = _effectFactory.CreateBrush();

                // Set the destination brush as the source of the image content
                brush.SetSourceParameter("ImageSource", _compositor.CreateBackdropBrush());

                // If his effect uses a secondary brush, set it now
                if (secondaryBrush != null)
                {
                    brush.SetSourceParameter("SecondSource", secondaryBrush);
                }

                // Update the destination layer with the fully configured brush
                _destinationSprite.Brush = brush;
            }
        }

        private void ScopeBatch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            // Scope batch completion event has fired, hide the destination sprite and cleanup the batch
            _destinationSprite.IsVisible = false;

            CleanupScopeBatch();
        }

        private void CleanupScopeBatch()
        {
            if (_scopeBatch != null)
            {
                _scopeBatch.Completed -= ScopeBatch_Completed;
                _scopeBatch.Dispose();
                _scopeBatch = null;
            }
        }
    }
}
