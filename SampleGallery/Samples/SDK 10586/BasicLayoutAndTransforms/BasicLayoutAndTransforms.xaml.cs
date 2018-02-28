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

using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Collections.Generic;

namespace CompositionSampleGallery
{
    public sealed partial class BasicLayoutAndTransforms : SamplePage
    {
        private Visual _xamlRoot;
        private Compositor _compositor;
        private ContainerVisual _root;
        private ContainerVisual _indicatorContainer;
        private SpriteVisual _mainImage;
        private SpriteVisual _apIndicator;
        private SpriteVisual _cpIndicator;

        public BasicLayoutAndTransforms()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Basic Layout and Transforms"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Adjust the sliders to see how CenterPoint and AnchorPoint affect positioning and transforms"; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868942";

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Acquire Compositor and set up basic visual tree structure
            _xamlRoot = ElementCompositionPreview.GetElementVisual(MainGrid);
            _compositor = _xamlRoot.Compositor;
            _root = _compositor.CreateContainerVisual();
            _mainImage = Image.SpriteVisual;

            ElementCompositionPreview.SetElementChildVisual(ImageContainer, _root);
            _root.Children.InsertAtTop(_mainImage);


            // Add visual indicators to show the position of AnchorPoint and CenterPoint
            _indicatorContainer = _compositor.CreateContainerVisual();

            _apIndicator = _compositor.CreateSpriteVisual();
            _apIndicator.Size = new Vector2(10, 10);
            _apIndicator.AnchorPoint = new Vector2(0.5f, 0.5f);
            _apIndicator.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Red);

            _cpIndicator = _compositor.CreateSpriteVisual();
            _cpIndicator.Size = new Vector2(10, 10);
            _cpIndicator.AnchorPoint = new Vector2(0.5f, 0.5f);
            _cpIndicator.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Green);

            _root.Children.InsertAtTop(_indicatorContainer);
            _indicatorContainer.Children.InsertAtTop(_cpIndicator);
            _indicatorContainer.Children.InsertAtTop(_apIndicator);


            // Specify a clip to prevent image from overflowing into the sliders list
            Visual containerGrid = ElementCompositionPreview.GetElementVisual(ContentGrid);
            containerGrid.Size = new Vector2((float)ContentGrid.ActualWidth, (float)ContentGrid.ActualHeight);
            ContentGrid.SizeChanged += (s, a) =>
            {
                containerGrid.Size = new Vector2((float)ContentGrid.ActualWidth, (float)ContentGrid.ActualHeight);
            };
            containerGrid.Clip = _compositor.CreateInsetClip();


            // Create list of properties to add as sliders
            var list = new List<TransformPropertyModel>();
            list.Add(new TransformPropertyModel(AnchorPointXAction) { PropertyName = "AnchorPoint - X (Red)", MinValue = -1, MaxValue = 2, StepFrequency = 0.01f, Value = _mainImage.AnchorPoint.X });
            list.Add(new TransformPropertyModel(AnchorPointYAction) { PropertyName = "AnchorPoint - Y (Red)", MinValue = -1, MaxValue = 2, StepFrequency = 0.01f, Value = _mainImage.AnchorPoint.Y });
            list.Add(new TransformPropertyModel(CenterPointXAction) { PropertyName = "CenterPoint - X (Green)", MinValue = -600, MaxValue = 600, StepFrequency = 1f, Value = _mainImage.CenterPoint.X });
            list.Add(new TransformPropertyModel(CenterPointYAction) { PropertyName = "CenterPoint - Y (Green)", MinValue = -600, MaxValue = 600, StepFrequency = 1f, Value = _mainImage.CenterPoint.Y });
            list.Add(new TransformPropertyModel(RotationAction) { PropertyName = "Rotation (in Degrees)", MinValue = 0, MaxValue = 360, StepFrequency = 1f, Value = _mainImage.RotationAngleInDegrees });
            list.Add(new TransformPropertyModel(ScaleXAction) { PropertyName = "Scale - X", MinValue = 0, MaxValue = 3, StepFrequency = 0.01f, Value = _mainImage.Scale.X });
            list.Add(new TransformPropertyModel(ScaleYAction) { PropertyName = "Scale - Y", MinValue = 0, MaxValue = 3, StepFrequency = 0.01f, Value = _mainImage.Scale.Y });
            list.Add(new TransformPropertyModel(OffsetXAction) { PropertyName = "Offset - X", MinValue = -200, MaxValue = 200, StepFrequency = 1f, Value = _mainImage.Offset.X });
            list.Add(new TransformPropertyModel(OffsetYAction) { PropertyName = "Offset - Y", MinValue = -200, MaxValue = 200, StepFrequency = 1f, Value = _mainImage.Offset.Y });

            XamlItemsControl.ItemsSource = list;

        }

        private void AnchorPointXAction(float value)
        {
            _mainImage.AnchorPoint = new Vector2(value, _mainImage.AnchorPoint.Y);
        }
        private void AnchorPointYAction(float value)
        {
            _mainImage.AnchorPoint = new Vector2(_mainImage.AnchorPoint.X, value);
        }
        private void CenterPointXAction(float value)
        {
            _mainImage.CenterPoint = new Vector3(value, _mainImage.CenterPoint.Y, _mainImage.CenterPoint.Z);
            _cpIndicator.Offset = _mainImage.CenterPoint;
        }
        private void CenterPointYAction(float value)
        {
            _mainImage.CenterPoint = new Vector3(_mainImage.CenterPoint.X, value, _mainImage.CenterPoint.Z);
            _cpIndicator.Offset = _mainImage.CenterPoint;
        }
        private void RotationAction(float value)
        {
            _mainImage.RotationAngleInDegrees = value;
        }
        private void ScaleXAction(float value)
        {
            _mainImage.Scale = new Vector3(value, _mainImage.Scale.Y, 0);
        }
        private void ScaleYAction(float value)
        {
            _mainImage.Scale = new Vector3(_mainImage.Scale.X, value, 0);
        }
        private void OffsetXAction(float value)
        {
            _mainImage.Offset = new Vector3((float)value, _mainImage.Offset.Y, _mainImage.Offset.Z);
            _indicatorContainer.Offset = _mainImage.Offset;
        }
        private void OffsetYAction(float value)
        {
            _mainImage.Offset = new Vector3(_mainImage.Offset.X, (float)value, _mainImage.Offset.Z);
            _indicatorContainer.Offset = _mainImage.Offset;
        }
    }
}
