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

using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class BasicLayoutAndTransforms : SamplePage
    {
        private Visual _xamlRoot;
        private Compositor _compositor;
        private ContainerVisual _root;
        private SpriteVisual _mainImage;
        private SpriteVisual _apIndicator;
        private SpriteVisual _cpIndicator;

        public BasicLayoutAndTransforms()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName    { get { return "Basic Layout and Transforms"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Adjust the sliders to see how CenterPoint and AnchorPoint affect positioning and transforms"; } }

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Set up Composition tree structure
            _xamlRoot = ElementCompositionPreview.GetElementVisual(MainGrid);
            _compositor = _xamlRoot.Compositor;
            _root = _compositor.CreateContainerVisual();
            _mainImage = Image.SpriteVisual;

            ElementCompositionPreview.SetElementChildVisual(ImageContainer, _root);
            _root.Children.InsertAtTop(_mainImage);

            // Add the visual indicators for AnchorPoint and CenterPoint
            _apIndicator = _compositor.CreateSpriteVisual();
            _apIndicator.Size = new Vector2(10, 10);
            _apIndicator.AnchorPoint = new Vector2(0.5f, 0.5f);
            _apIndicator.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Red);

            _cpIndicator = _compositor.CreateSpriteVisual();
            _cpIndicator.Size = new Vector2(10, 10);
            _cpIndicator.AnchorPoint = new Vector2(0.5f, 0.5f);
            _cpIndicator.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Green);

            _root.Children.InsertAtTop(_cpIndicator);
            _root.Children.InsertAtTop(_apIndicator);

            // Add Clip to the content container
            var containerGrid = ElementCompositionPreview.GetElementVisual(ContentGrid);
            containerGrid.Size = new Vector2((float)ContentGrid.ActualWidth, (float)ContentGrid.ActualHeight);
            ContentGrid.SizeChanged += (s, a) =>
            {
                containerGrid.Size = new Vector2((float)ContentGrid.ActualWidth, (float)ContentGrid.ActualHeight);
            };
            containerGrid.Clip = _compositor.CreateInsetClip();

            // Set all sliders to initial values
            AnchorPointXSlider.Value = _mainImage.AnchorPoint.X;
            AnchorPointYSlider.Value = _mainImage.AnchorPoint.Y;
            CenterPointXSlider.Value = _mainImage.CenterPoint.X;
            CenterPointYSlider.Value = _mainImage.CenterPoint.Y;
            RotationSlider.Value = _mainImage.RotationAngleInDegrees;
            ScaleXSlider.Value = _mainImage.Scale.X;
            ScaleYSlider.Value = _mainImage.Scale.Y;

        }


        private void AnchorPointXSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.AnchorPoint = new Vector2((float)slider.Value, _mainImage.AnchorPoint.Y);
        }

        private void AnchorPointYSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.AnchorPoint = new Vector2(_mainImage.AnchorPoint.X, (float)slider.Value);
        }

        private void CenterPointXSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.CenterPoint = new Vector3((float)slider.Value, _mainImage.CenterPoint.Y, _mainImage.CenterPoint.Z);
            _cpIndicator.Offset = _mainImage.CenterPoint;
        }

        private void CenterPointYSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.CenterPoint = new Vector3(_mainImage.CenterPoint.X, (float)slider.Value, _mainImage.CenterPoint.Z);
            _cpIndicator.Offset = _mainImage.CenterPoint;
        }

        private void RotationSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.RotationAngleInDegrees = (float)slider.Value;
        }

        private void ScaleSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.Scale = new Vector3((float)slider.Value, (float)slider.Value, 0);
        }

        private void ScaleXSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.Scale = new Vector3((float)slider.Value, _mainImage.Scale.Y, _mainImage.Scale.Z);
        }

        private void ScaleYSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            _mainImage.Scale = new Vector3(_mainImage.Scale.X, (float)slider.Value, _mainImage.Scale.Z);
        }

    }
}
