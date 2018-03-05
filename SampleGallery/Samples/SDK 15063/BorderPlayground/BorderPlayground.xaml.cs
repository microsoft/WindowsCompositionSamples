//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
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
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;

namespace CompositionSampleGallery
{
    public sealed partial class BorderPlayground : SamplePage
    {
        private Compositor              _compositor;
        private SpriteVisual            _sprite;
        private ManagedSurface          _image;

        enum ImageName
        {
            Checkerboard,
            Flower
        }

        public BorderPlayground()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            this.InitializeComponent();
        }

        public static string    StaticSampleName => "Border Effect"; 
        public override string  SampleName => StaticSampleName;
        public static string    StaticSampleDescription => "Demonstrate different border modes with scaling, offset, and rotation."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868944";

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _sprite = _compositor.CreateSpriteVisual();
            _sprite.Size = new Vector2((float)BorderImage.ActualWidth, (float)BorderImage.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(BorderImage, _sprite);

            IList<ComboBoxItem> imageList = new List<ComboBoxItem>();
            SetComboBoxList<ImageName>(imageList);
            ImageSelector.ItemsSource = imageList;
            ImageSelector.SelectedIndex = 0;

            IList<ComboBoxItem> extendXList = new List<ComboBoxItem>();
            SetComboBoxList<CanvasEdgeBehavior>(extendXList);
            ExtendXBox.ItemsSource = extendXList;
            ExtendXBox.SelectedIndex = 0;

            IList<ComboBoxItem> extendYList = new List<ComboBoxItem>();
            SetComboBoxList<CanvasEdgeBehavior>(extendYList);
            ExtendYBox.ItemsSource = extendYList;
            ExtendYBox.SelectedIndex = 0;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }
        private void SetImageBrush(string uri)
        {
            // Update the sprite
            _image = ImageLoader.Instance.LoadFromUri(new Uri(uri));
            UpdateImageBrush();

            // Update the preview image
            BitmapImage image = new BitmapImage(new Uri(uri));
            ImagePreview.Source = image;
        }

        private void SetComboBoxList<T>(IList<ComboBoxItem> list)
        {
            foreach (T type in Enum.GetValues(typeof(T)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = type;
                item.Content = type.ToString();
                list.Add(item);
            }
        }

        private void Extend_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBorderEffectBrush();
        }

        private void UpdateBorderEffectBrush()
        {
            ComboBoxItem itemX = ExtendXBox.SelectedValue as ComboBoxItem;
            ComboBoxItem itemY = ExtendYBox.SelectedValue as ComboBoxItem;

            if (itemX == null || itemY == null)
            {
                return;
            }

            var borderEffect = new BorderEffect
            {
                ExtendX = (CanvasEdgeBehavior)itemX.Tag,
                ExtendY = (CanvasEdgeBehavior)itemY.Tag,
                Source = new CompositionEffectSourceParameter("source")
            };

            var brush = _compositor.CreateEffectFactory(borderEffect).CreateBrush();
            brush.SetSourceParameter("source", _image.Brush);
            _sprite.Brush = brush;
        }

        private void ImageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem image = ImageSelector.SelectedValue as ComboBoxItem;

            switch ((ImageName)image.Tag)
            {
                case ImageName.Checkerboard:
                    SetImageBrush("ms-appx:///Assets/Other/Checkerboard.png");
                    break;
                case ImageName.Flower:
                    SetImageBrush("ms-appx:///Assets/Nature/Nature-5.jpg");
                    break;
            }

            UpdateBorderEffectBrush();
        }

        void UpdateImageBrush()
        {
            _image.Brush.Scale = new Vector2((float)ScaleX.Value, (float)ScaleY.Value);
            _image.Brush.Offset = new Vector2((float)OffsetX.Value, (float)OffsetY.Value);
            _image.Brush.RotationAngleInDegrees = (float)Rotation.Value;
            _image.Brush.Stretch = CompositionStretch.None;
            _image.Brush.CenterPoint = new Vector2(_sprite.Size.X / 2f, _sprite.Size.Y / 2f);
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_image != null)
            {
                UpdateImageBrush();
            }
        }

        private void BorderImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_sprite != null)
            {
                _sprite.Size = e.NewSize.ToVector2();
            }
        }
    }
}
