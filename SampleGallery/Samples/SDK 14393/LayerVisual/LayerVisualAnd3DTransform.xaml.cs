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
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Collections.Generic;
using SamplesCommon;


namespace CompositionSampleGallery
{
    public class EffectParameters
    {
        public EffectParameters(
            LayerVisualAnd3DTransform page,
            LayerVisualAnd3DTransform.EffectType effectType,
            float valueCache,
            float maxScaleValue = 1.2f)
        {
            _page = page;
            _effectType = effectType;
            _initialValue = valueCache;
            _maxScaleValue = maxScaleValue;
            _value = _initialValue;
        }

        public void InitializeEffectBrush()
        {
            switch (_effectType)
            {
                case LayerVisualAnd3DTransform.EffectType.Saturation:
                    _effectBrush = _page.CreateEffectBrush(
                        _effectType,
                        "saturation",
                        (float)_initialValue,
                        new[] { PropertyName });
                    break;

                case LayerVisualAnd3DTransform.EffectType.HueRotation:
                    _effectBrush = _page.CreateEffectBrush(
                        _effectType,
                        "hueRotation",
                        (float)_initialValue,
                        new[] { PropertyName });
                    break;

                case LayerVisualAnd3DTransform.EffectType.Sepia:
                    _effectBrush = _page.CreateEffectBrush(
                        _effectType,
                        "sepia",
                        (float)_initialValue,
                        new[] { PropertyName });
                    break;

                default:
                    break;
            }   
        }

        public void UpdatePropertyValue()
        {
            switch (_effectType)
            {
                case LayerVisualAnd3DTransform.EffectType.HueRotation:
                    _effectBrush.Properties.InsertScalar(PropertyName, _value * (float)Math.PI * 2.0f);
                    break;
                default:
                    _effectBrush.Properties.InsertScalar(PropertyName, _value);
                    break;
            }
        }

        public void OnPointerEntered()
        {
            if (_scaleAnimation == null)
            {
                _scaleAnimation = _page.Compositor.CreateVector3KeyFrameAnimation();
            }

            _scaleAnimation.InsertKeyFrame(1f, new Vector3(_maxScaleValue, _maxScaleValue, 1.0f));
            _scaleAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            _layerVisual.StartAnimation("Scale", _scaleAnimation);

            if (_effectAnimation == null)
            {
                _effectAnimation = _page.Compositor.CreateScalarKeyFrameAnimation();
            }

            var easeLinear = _page.Compositor.CreateLinearEasingFunction();

            switch (_effectType)
            {
                case LayerVisualAnd3DTransform.EffectType.Saturation:
                    _effectAnimation.InsertKeyFrame(1.0f, 0.0f);
                    break;

                case LayerVisualAnd3DTransform.EffectType.HueRotation:
                    _effectAnimation.InsertKeyFrame(1.0f, (float)Math.PI, easeLinear);
                    break;

                default:
                    _effectAnimation.InsertKeyFrame(1.0f, 1.0f);
                    break;
            }

            _effectAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            _effectBrush.StartAnimation(PropertyName, _effectAnimation);
        }

        public void OnPointerExited()
        {
            if (_scaleAnimation == null)
            {
                _scaleAnimation = _page.Compositor.CreateVector3KeyFrameAnimation();
            }

            _scaleAnimation.InsertKeyFrame(1f, new Vector3(1.0f, 1.0f, 1.0f));
            _scaleAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            _layerVisual.StartAnimation("Scale", _scaleAnimation);

            if (_effectAnimation == null)
            {
                _effectAnimation = _page.Compositor.CreateScalarKeyFrameAnimation();
            }

            var easeLinear = _page.Compositor.CreateLinearEasingFunction();

            switch (_effectType)
            {
                case LayerVisualAnd3DTransform.EffectType.Saturation:
                    _effectAnimation.InsertKeyFrame(1.0f, 1.0f);
                    break;

                case LayerVisualAnd3DTransform.EffectType.HueRotation:
                    _effectAnimation.InsertKeyFrame(1.0f, 0.0f, easeLinear);
                    break;

                default:
                    _effectAnimation.InsertKeyFrame(1.0f, 0.0f);
                    break;
            }

            _effectAnimation.Duration = TimeSpan.FromMilliseconds(1500);
            _effectBrush.StartAnimation(PropertyName, _effectAnimation);
        }

        public ContainerVisual VisualInstance
        {
            get
            {
                return _layerVisual;
            }
            set
            {
                _layerVisual = value;
            }
        }

        public CompositionEffectBrush EffectInstance
        {
            get
            {
                return _effectBrush;
            }
            set
            {
                _effectBrush = value;
            }
        }

        private string PropertyName
        {
            get
            {
                string name = null;
                switch (_effectType)
                {
                    case LayerVisualAnd3DTransform.EffectType.Saturation:
                        name = "saturation.Saturation";
                        break;

                    case LayerVisualAnd3DTransform.EffectType.HueRotation:
                        name = "hueRotation.Angle";
                        break;

                    case LayerVisualAnd3DTransform.EffectType.Sepia:
                        name = "sepia.Intensity";
                        break;

                    default:
                        break;
                }

                return name;
            }
        }

        private readonly LayerVisualAnd3DTransform _page;
        private readonly LayerVisualAnd3DTransform.EffectType _effectType;
        private readonly float _initialValue;
        private float _value;
        private readonly float _maxScaleValue;
        private ContainerVisual _layerVisual;
        private CompositionEffectBrush _effectBrush;
        private Vector3KeyFrameAnimation _scaleAnimation;
        private ScalarKeyFrameAnimation _effectAnimation;
    }

    public sealed partial class LayerVisualAnd3DTransform : SamplePage
    {
        public enum EffectType
        {
            Saturation,
            HueRotation,
            Sepia
        }

        public LayerVisualAnd3DTransform()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        public static string        StaticSampleName => "Layer Visual Sample"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Layer visual in the 3d space with 3D perspective transform"; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868998";

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            double paramHeight;
            double paramWidth;
            GetParamSize(out paramWidth, out paramHeight);

            LeftWallGrid.Width = 0.02 * paramWidth;
            LeftWallGrid.Height = paramHeight;
            RightWallGrid.Width = paramWidth;
            RightWallGrid.Height = paramHeight;

            _roomVisual = ElementCompositionPreview.GetElementVisual(RoomGrid);
            _compositor = _roomVisual.Compositor;

            float scale = GetFullScale();
            if (_roomVisual != null)
            {
                _roomVisual.Scale = new Vector3(scale, scale, 1.0f);
            }

            await LoadImages();

            float flAngle = (float)(Math.PI / -64.0f);
            float perspectiveDistance = (float)RoomGrid.Width * 3;
            Vector3 centerPt = new Vector3(
                (float)(RoomGrid.Width * 0.5f),
                (float)(RoomGrid.Height * 0.5f),
                0);
            Matrix4x4 transformMatrix =
                Matrix4x4.CreateTranslation(-centerPt) *
                Matrix4x4.CreateRotationY(flAngle) *
                new Matrix4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, -1 / perspectiveDistance,
                    0, 0, 0, 1) *
                Matrix4x4.CreateTranslation(centerPt);
            Vector3 offset = new Vector3(transformMatrix.M41 - 8, transformMatrix.M42 - 8, 0);
            _roomVisual.TransformMatrix = transformMatrix * Matrix4x4.CreateTranslation(-offset);

            // Create three categories of panels, each panel has three contents.
            // Each category has a layer visual which contains three layer visual from its contents.
            // Effect on a category and all its sub tree contents works when hit-testing a category.
            // And effect on a content works when hit-testing a content.
            Visual rightWallVisual = ElementCompositionPreview.GetElementVisual(RightWallGrid);

            flAngle = (float)(Math.PI / 64.0f);
            rightWallVisual.TransformMatrix = Matrix4x4.CreateRotationY(flAngle);

            double sectionGridWidthRatio = 1 / 3.0;

            for (int i = 0; i < _fColumnCount; ++i)
            {
                _sectionContentParams[i] = new EffectParameters[_fColumnCount];
            }

            EffectType[] sectionEffectTypes = {
                EffectType.Saturation,
                EffectType.HueRotation,
                EffectType.Sepia };

            float[] sectionPropertyValues = { 1.0f, 0.0f, 0.0f };
            Color[] sectionColors = { Colors.Linen, Colors.Linen, Colors.Linen };
            double sectionWidthRatio = 0.9;
            double delatSectionWidth = RightWallGrid.Width * sectionGridWidthRatio * (1 - sectionWidthRatio) * 0.5;

            for (int j = 0; j < _fColumnCount; ++j)
            {
                Windows.UI.Xaml.Controls.Grid section = (Windows.UI.Xaml.Controls.Grid)RightWallGrid.Children[j];
                section.Width = sectionGridWidthRatio * RightWallGrid.Width * sectionWidthRatio;
                section.Height = RightWallGrid.Height * 0.9;
                section.Margin = new Windows.UI.Xaml.Thickness(
                    (1.5 - j * 0.5) * delatSectionWidth,
                    0.05 * paramHeight,
                    (0.5 + j * 0.5) * delatSectionWidth,
                    0.05 * paramHeight);

                double contentGridHeightRatio = 1 / 3.0;
                double contentWidthRatio = 0.8;
                double contentHeightRatio = 0.8;
                double deltaWidth = (1 - contentWidthRatio) * 0.5 * section.Width;
                double deltaHeight = contentGridHeightRatio * section.Height * (1.0 - contentHeightRatio) * 0.5;

                _sectionParams[j] = new EffectParameters(this, sectionEffectTypes[j], sectionPropertyValues[j], 1.1f);
                _sectionParams[j].InitializeEffectBrush();

                Vector2 sectionSize = new Vector2((float)section.Width, (float)section.Height);
                _sectionParams[j].VisualInstance = AddLayerVisual(
                    _sectionParams[j].EffectInstance,
                    sectionSize,
                    RightWallGrid.Children[j]);
                _sectionParams[j].UpdatePropertyValue();

                _sectionParams[j].VisualInstance.CenterPoint = new Vector3(sectionSize.X * 0.5f, sectionSize.Y * 0.5f, 0);
                AddColorSpriteVisual(sectionColors[j], _sectionParams[j].VisualInstance.Size, _sectionParams[j].VisualInstance);

                for (int i = 0; i < _fColumnCount; ++i)
                {
                    Windows.UI.Xaml.Shapes.Rectangle content = (Windows.UI.Xaml.Shapes.Rectangle)section.Children[i];
                    content.Width = section.Width * contentWidthRatio;
                    content.Height = contentGridHeightRatio * section.Height * contentHeightRatio;

                    content.Margin = new Windows.UI.Xaml.Thickness(deltaWidth,
                        (1.5 - i * 0.5) * deltaHeight,
                        deltaWidth,
                        (0.5 + i * 0.5) * deltaHeight);

                    _sectionContentParams[i][j] = new EffectParameters(this,
                        sectionEffectTypes[j], sectionPropertyValues[j]);
                    _sectionContentParams[i][j].InitializeEffectBrush();

                    Vector2 contentASize = new Vector2((float)content.Width, (float)content.Height);

                    _sectionContentParams[i][j].VisualInstance = AddLayerVisual(
                        _sectionContentParams[i][j].EffectInstance,
                        contentASize,
                        new Vector3((float)section.Width * (0.5f - (float)contentWidthRatio * 0.5f),
                            (float)content.Height * i + 1.5f * (float)deltaHeight * (i + 1), 0),
                        _sectionParams[j].VisualInstance);
                    _sectionContentParams[i][j].UpdatePropertyValue();

                    _sectionContentParams[i][j].VisualInstance.CenterPoint =
                        new Vector3(contentASize.X * 0.5f, contentASize.Y * 0.5f, 0);
                    AddImageSpriteVisual(_imageSurfaces[j * _fColumnCount + i].Brush,
                        _sectionContentParams[i][j].VisualInstance.Size, _sectionContentParams[i][j].VisualInstance);
                }
            }

            for (int j = 0; j < RightWallGrid.Children.Count; ++j)
            {
                Windows.UI.Xaml.Controls.Grid section = (Windows.UI.Xaml.Controls.Grid)RightWallGrid.Children[j];
                EffectParameters sectionPara = _sectionParams[j];

                section.PointerEntered += (s, v) =>
                {
                    sectionPara.OnPointerEntered();
                };
                section.PointerExited += (s, v) =>
                {
                    sectionPara.OnPointerExited();
                };

                for (int i = 0; i < section.Children.Count; ++i)
                {
                    Windows.UI.Xaml.Shapes.Rectangle content = (Windows.UI.Xaml.Shapes.Rectangle)section.Children[i];
                    EffectParameters contentPara = _sectionContentParams[i][j];

                    content.PointerEntered += (s, v) =>
                    {
                        sectionPara.OnPointerExited();
                        contentPara.OnPointerEntered();
                    };
                    content.PointerExited += (s, v) =>
                    {
                        sectionPara.OnPointerEntered();
                        contentPara.OnPointerExited();
                    };
                    content.PointerMoved += (s, V) =>
                    {
                        sectionPara.OnPointerExited();
                    };
                }
            }
        }

        public CompositionEffectBrush CreateEffectBrush(
            EffectType effectType,
            string effectName,
            float propertyValue,
            IEnumerable<string> properties)
        {
            IGraphicsEffect effectDesc = null;

            switch (effectType)
            {
                case EffectType.Saturation:
                    effectDesc = new SaturationEffect()
                    {
                        Name = effectName,
                        Saturation = propertyValue,
                        Source = new CompositionEffectSourceParameter("source")
                    };
                    break;

                case EffectType.HueRotation:
                    effectDesc = new HueRotationEffect()
                    {
                        Name = effectName,
                        Angle = propertyValue,
                        Source = new CompositionEffectSourceParameter("source")
                    };
                    break;

                case EffectType.Sepia:
                    effectDesc = new SepiaEffect()
                    {
                        Name = effectName,
                        Intensity = propertyValue,
                        Source = new CompositionEffectSourceParameter("source")
                    };
                    break;
            }

            CompositionEffectFactory effectFactory = _compositor.CreateEffectFactory(effectDesc, properties);
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();

            return effectBrush;
        }

        async private Task LoadImages()
        {
            string[] imageNames = { "Abstract/Abstract-1", "Abstract/Abstract-2", "Abstract/Abstract-4", "Landscapes/Landscape-1",
                "Landscapes/Landscape-2", "Landscapes/Landscape-4", "Nature/Nature-1", "Nature/Nature-2",
                "Nature/Nature-4" };

            for (int i = 0; i < imageNames.Length; ++i)
            {
                var uri = new Uri($"ms-appx:///Assets/{imageNames[i]}.jpg");
                _imageSurfaces[i] = await ImageLoader.Instance.LoadFromUriAsync(uri);
            }
        }

        public Compositor Compositor
        {
            get
            {
                return _compositor;
            }         
        }

        private ContainerVisual AddLayerVisual(
            CompositionEffectBrush effectBrush,
            Vector2 size,
            Windows.UI.Xaml.UIElement parent)
        {
            var layerVisual = _compositor.CreateLayerVisual();
            layerVisual.Effect = effectBrush;
            layerVisual.Size = size;
            ElementCompositionPreview.SetElementChildVisual(parent, layerVisual);

            return layerVisual;
        }

        private ContainerVisual AddLayerVisual(
            CompositionEffectBrush effectBrush,
            Vector2 size,
            Vector3 offset,
            ContainerVisual parent)
        {
            var layerVisual = _compositor.CreateLayerVisual();
            layerVisual.Effect = effectBrush;
            layerVisual.Size = size;
            layerVisual.Offset = offset;
            parent.Children.InsertAtTop(layerVisual);

            return layerVisual;
        }

        private void AddColorSpriteVisual(
            Color color,
            Vector2 size,
            ContainerVisual parent)
        {
            var colorSpriteVisual = _compositor.CreateSpriteVisual();
            colorSpriteVisual.Brush = _compositor.CreateColorBrush(color);
            colorSpriteVisual.Size = size;
            parent.Children.InsertAtTop(colorSpriteVisual);
        }

        private void AddImageSpriteVisual(
            CompositionSurfaceBrush brush,
            Vector2 size,
            ContainerVisual parent)
        {
            var imageSpriteVisual = _compositor.CreateSpriteVisual();
            brush.Stretch = CompositionStretch.UniformToFill;
            imageSpriteVisual.Brush = brush;
            imageSpriteVisual.Size = size;
            parent.Children.InsertAtTop(imageSpriteVisual);
        }

        private void GetParamSize(out double width, out double height)
        {
            width = RoomGrid.Width / 1.02;
            height = RoomGrid.Height;
        }

        private float GetFullScale()
        {
            double paramHeight;
            double paramWidth;
            GetParamSize(out paramWidth, out paramHeight);

            float scaleX = (float)(this.ActualWidth * 0.96f / paramWidth);
            float scaleY = (float)(this.ActualHeight * 0.96f / paramHeight);

            return Math.Min(scaleX, scaleY);
        }
        private void SamplePage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            float scale = GetFullScale();
            if (_roomVisual != null)
            {
                _roomVisual.Scale = new Vector3(scale, scale, 1.0f);
            }
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            foreach (var surface in _imageSurfaces)
            {
                surface.Dispose();
            }
        }

        private const int _fColumnCount = 3;
        private Compositor _compositor;
        private Visual _roomVisual;
        private ManagedSurface[] _imageSurfaces = new ManagedSurface[_fColumnCount * _fColumnCount];
        private EffectParameters[] _sectionParams = new EffectParameters[_fColumnCount];
        private EffectParameters[][] _sectionContentParams = new EffectParameters[_fColumnCount][];
    }
}
