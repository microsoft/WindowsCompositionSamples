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
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    public sealed partial class ImagePopupViewer : UserControl
    {
        Compositor                  _compositor;
        CompositionEffectBrush      _crossFadeBrush;
        CompositionSurfaceBrush     _previousSurfaceBrush;
        CompositionScopedBatch      _crossFadeBatch;
        ConnectedTransition        _transition;
        string                      _initialPhoto;
        static ImagePopupViewer     _viewerInstance;
        static Grid                 _hostGrid;
        Func<object, bool, Uri>     _imageUriGetterFunc;

        /// <summary>
        /// Private constructor as Show() is responsible for creating an instance
        /// </summary>
        private ImagePopupViewer(Func<object, bool, Uri> photoGetter, string initialPhoto)
        {
            this.InitializeComponent();

            _imageUriGetterFunc = photoGetter;
            _transition = new ConnectedTransition();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            this.Loaded += ImagePopupViewer_Loaded;
            this.Unloaded += ImagePopupViewer_Unloaded;

            // Bring the selected item into view
            _initialPhoto = initialPhoto;
            
            // Hide until the content is available
            this.Opacity = 0;
            BackgroundImage.ImageOpened += BackgroundImage_FirstOpened;

            // Disable the placeholder as we'll be using a transition
            PrimaryImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            BackgroundImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            BackgroundImage.LoadTimeEffectHandler = SampleImageColor;
            BackgroundImage.SharedSurface = true;

            // Create a crossfade brush to animate image transitions
            IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect()
            {
                Name = "CrossFade",
                Source1Amount = 0,
                Source2Amount = 1,
                MultiplyAmount = 0,
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source2 = new CompositionEffectSourceParameter("ImageSource2"),
            };

            CompositionEffectFactory factory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "CrossFade.Source1Amount", "CrossFade.Source2Amount" });
            _crossFadeBrush = factory.CreateBrush();

        }

        private void BackgroundImage_FirstOpened(object sender, RoutedEventArgs e)
        {
            // Image loaded, let's show the content
            this.Opacity = 1;

            // Show the content now that we should have something.
            ScalarKeyFrameAnimation fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeInAnimation.InsertKeyFrame(0, 0);
            fadeInAnimation.InsertKeyFrame(1, 1);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            BackgroundImage.SpriteVisual.StartAnimation("Opacity", fadeInAnimation);
            ElementCompositionPreview.GetElementVisual(ImageList).StartAnimation("Opacity", fadeInAnimation);

            // Start a slow UV scale to create movement in the background image
            Vector2KeyFrameAnimation scaleAnimation = _compositor.CreateVector2KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0, new Vector2(1.1f, 1.1f));
            scaleAnimation.InsertKeyFrame(.5f, new Vector2(2.0f, 2.0f));
            scaleAnimation.InsertKeyFrame(1, new Vector2(1.1f, 1.1f));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(40000);
            scaleAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            CompositionDrawingSurface surface = (CompositionDrawingSurface)BackgroundImage.SurfaceBrush.Surface;
            BackgroundImage.SurfaceBrush.CenterPoint = new Vector2((float)surface.Size.Width, (float)surface.Size.Height) * .5f;
            BackgroundImage.SurfaceBrush.StartAnimation("Scale", scaleAnimation);

            // Start the animation of the cross-fade brush so they're in sync
            _previousSurfaceBrush = _compositor.CreateSurfaceBrush();
            _previousSurfaceBrush.StartAnimation("Scale", scaleAnimation);

            BackgroundImage.ImageOpened -= BackgroundImage_FirstOpened;
        }

        private void BackgroundImage_ImageChanged(object sender, RoutedEventArgs e)
        {
            if (_crossFadeBatch == null)
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(1000);

                // Create the animations for cross-fading
                ScalarKeyFrameAnimation fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeInAnimation.InsertKeyFrame(0, 0);
                fadeInAnimation.InsertKeyFrame(1, 1);
                fadeInAnimation.Duration = duration;

                ScalarKeyFrameAnimation fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeOutAnimation.InsertKeyFrame(0, 1);
                fadeOutAnimation.InsertKeyFrame(1, 0);
                fadeOutAnimation.Duration = duration;

                // Create a batch object so we can cleanup when the cross-fade completes.
                _crossFadeBatch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Set the sources
                _crossFadeBrush.SetSourceParameter("ImageSource", BackgroundImage.SurfaceBrush);
                _crossFadeBrush.SetSourceParameter("ImageSource2", _previousSurfaceBrush);

                // Animate the source amounts to fade between
                _crossFadeBrush.StartAnimation("CrossFade.Source1Amount", fadeInAnimation);
                _crossFadeBrush.StartAnimation("CrossFade.Source2Amount", fadeOutAnimation);

                // Update the image to use the cross fade brush
                BackgroundImage.Brush = _crossFadeBrush;

                _crossFadeBatch.Completed += Batch_CrossFadeCompleted;
                _crossFadeBatch.End();
            }

            // Unhook the handler
            BackgroundImage.ImageOpened -= BackgroundImage_ImageChanged;
        }

        private void Batch_CrossFadeCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            BackgroundImage.Brush = BackgroundImage.SurfaceBrush;

            // Dispose the image
            ((CompositionDrawingSurface)_previousSurfaceBrush.Surface).Dispose();
            _previousSurfaceBrush.Surface = null;

            // Clear out the batch
            _crossFadeBatch = null;
        }

        private void ImagePopupViewer_Loaded(object sender, RoutedEventArgs e)
        {
            // Update the sources
            BackgroundImage.Source = new Uri(_initialPhoto);
            PrimaryImage.Source = new Uri(_initialPhoto);

            // Ensure the source thumbnail is in view
            ImageList.ScrollIntoView(_initialPhoto);
        }

        private void ImagePopupViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_hostGrid != null)
            {
                ElementCompositionPreview.SetElementChildVisual(_hostGrid.Children[0], null);
                _hostGrid.Children.Remove(_viewerInstance);
                _hostGrid = null;
            }
            _viewerInstance = null;
        }

        public object ItemsSource
        {
            get { return ImageList.ItemsSource; }
            set { ImageList.ItemsSource = value; }
        }


        private Color ExtractPredominantColor(Color[] colors, Size size)
        {
            Dictionary<uint, int> dict = new Dictionary<uint, int>();
            uint maxColor = 0xff000000;

            // Take a small sampling of the decoded pixels, looking for the most common color
            int pixelSamples = Math.Min(2000, colors.Length);
            int skipPixels = colors.Length / pixelSamples;

            for (int pixel = colors.Length - 1; pixel >= 0; pixel -= skipPixels)
            {
                Color c = colors[pixel];

                // Quantize the colors to bucket the groupings better
                c.R -= (byte)(c.R % 10);
                c.G -= (byte)(c.G % 10);
                c.B -= (byte)(c.B % 10);

                // Determine the saturation and value for the color
                int max = Math.Max(c.R, Math.Max(c.G, c.B));
                int min = Math.Min(c.R, Math.Min(c.G, c.B));
                int saturation = (int)(((max == 0) ? 0 : (1f - (1f * min / max))) * 255);
                int value = (int)((max / 255f) * 255);

                if (c.A > 0)
                {
                    uint color = (uint)((255 << 24) | (c.R << 16) | (c.G << 8) | (c.B << 0));

                    // Weigh saturated, high value colors more heavily
                    int weight = saturation + value;

                    if (dict.ContainsKey(color))
                    {
                        dict[color] += weight;
                    }
                    else
                    {
                        dict.Add(color, weight);
                    }
                }
            }

            // Determine the predominant color
            int maxValue = 0;
            foreach (KeyValuePair<uint, int> pair in dict)
            {
                if (pair.Value > maxValue)
                {
                    maxColor = pair.Key;
                    maxValue = pair.Value;
                }
            }

            // Convert to the final color value
            return Color.FromArgb((byte)(maxColor >> 24), (byte)(maxColor >> 16),
                                   (byte)(maxColor >> 8), (byte)(maxColor >> 0));
        }

        private void SampleImageColor(CompositionDrawingSurface surface, CanvasBitmap bitmap, CompositionGraphicsDevice device)
        {
            // Extract the color to tint the blur with
            Color predominantColor = ExtractPredominantColor(bitmap.GetPixelColors(), bitmap.Size);

            // Create a heavily blurred version of the image
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Source = bitmap,
                BlurAmount = 20.0f
            };

            Size size = surface.Size;
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                Rect destination = new Rect(0, 0, size.Width, size.Height);
                ds.FillRectangle(destination, predominantColor);
                ds.DrawImage(blurEffect, destination, new Rect(0, 0, size.Width, size.Height), .6f);
            }
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            CompositionImage image = args.ItemContainer.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
            Uri imageSource = _imageUriGetterFunc(args.Item, false);

            // Set the URI source, and size to the large target image
            image.Source = imageSource;
        }

        private void ImageList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListViewItem item = (ListViewItem)ImageList.ContainerFromItem(e.ClickedItem);

            // If we near the edges of the list, scroll more into view
            GeneralTransform coordinate = item.TransformToVisual(ImageList);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            if ((position.X + item.ActualWidth >= ImageList.ActualWidth) ||
                (position.X - item.ActualWidth <= 0))
            {
                double delta = position.X - item.ActualWidth <= 0 ? -item.ActualWidth : item.ActualWidth;
                delta *= 1.5;

                ScrollViewer scroller = ImageList.GetFirstDescendantOfType<ScrollViewer>();
                scroller.ChangeView(scroller.HorizontalOffset + delta, null, null);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
        }

        private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageList.SelectedItem != null)
            {
                ListViewItem item = (ListViewItem)ImageList.ContainerFromItem(ImageList.SelectedItem);
                Uri imageSource = _imageUriGetterFunc(item.Content, true);

                if (_crossFadeBatch == null)
                {
                    // Save the previous image for a cross-fade
                    _previousSurfaceBrush.Surface = BackgroundImage.SurfaceBrush.Surface;
                    _previousSurfaceBrush.CenterPoint = BackgroundImage.SurfaceBrush.CenterPoint;
                    _previousSurfaceBrush.Stretch = BackgroundImage.SurfaceBrush.Stretch;

                    // Load the new background image
                    BackgroundImage.ImageOpened += BackgroundImage_ImageChanged;
                }

                // Update the images
                BackgroundImage.Source = imageSource;
                PrimaryImage.Source = imageSource;

                if (!_transition.Completed)
                {
                    _transition.Cancel();
                }

                // Kick off a connected animation to animate from it's current position to it's new location
                CompositionImage image = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(item);
                _transition.Initialize(this, image, null);
                _transition.Start(this, PrimaryImage, null, null);
            }
        }

        internal static void Show(string photo, object itemSource, Func<object, bool, Uri> photoGetter, Thickness margin, UIElement page)
        {
            if (_viewerInstance != null)
            {
                throw new InvalidOperationException("Already displaying a photoviewer popup");
            }

            _hostGrid = VisualTreeHelperExtensions.GetFirstDescendantOfType<Grid>(page);

            if (_hostGrid != null)
            {
                _viewerInstance = new ImagePopupViewer(photoGetter, photo);

                // dialog needs to span all rows in the grid
                _viewerInstance.SetValue(Grid.RowSpanProperty, (_hostGrid.RowDefinitions.Count > 0 ? _hostGrid.RowDefinitions.Count : 1));
                _viewerInstance.SetValue(Grid.ColumnSpanProperty, (_hostGrid.ColumnDefinitions.Count > 0 ? _hostGrid.ColumnDefinitions.Count : 1));

                _hostGrid.Children.Add(_viewerInstance);

                _viewerInstance.ItemsSource = itemSource;
            }
            else
            {
                throw new ArgumentException("can't find a top level grid");
            }
        }
    }
}
