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

using ExpressionBuilder;
using SamplesCommon;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery.PointerEffectTechniques
{

    public abstract class EffectTechniques
    {
        protected Compositor                _compositor;
        protected CompositionEffectFactory  _effectFactory;
        protected LoadTimeEffectHandler     _loadEffectHandler;

        public enum EffectTypes
        {
            Exposure = 0,
            Desaturation,
            Blur,
            SpotLight,
            PointLightFollow,
        }

        public EffectTechniques(Compositor compositor)
        {
            _compositor = compositor;
        }

        public abstract Task<CompositionDrawingSurface> LoadResources();
        public abstract void ReleaseResources();
        public abstract void OnPointerEnter(Vector2 pointerPosition, CompositionImage image);
        public abstract void OnPointerExit(Vector2 pointerPosition, CompositionImage image);
        public virtual void OnPointerMoved(Vector2 pointerPosition, CompositionImage image)
        {

        }

        public virtual CompositionEffectBrush CreateBrush()
        {
            return _effectFactory.CreateBrush();
        }

        public LoadTimeEffectHandler LoadTimeEffectHandler
        {
            get { return _loadEffectHandler; }
        }
    }

    public class DesaturateTechnique : EffectTechniques
    {
        const string _effectName = "SaturationEffect";
        const string _targetProperty = _effectName + ".Saturation";
        ScalarKeyFrameAnimation _enterAnimation;
        ScalarKeyFrameAnimation _exitAnimation;

        public DesaturateTechnique(Compositor compositor) : base(compositor)
        {
        }

#pragma warning disable 1998
        public override async Task<CompositionDrawingSurface> LoadResources()
        {
            // Create the effect template
            var graphicsEffect = new SaturationEffect
            {
                Name = _effectName,
                Saturation = 1.0f,
                Source = new CompositionEffectSourceParameter("ImageSource")
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { _targetProperty });


            // Create the animations
            _enterAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _enterAnimation.InsertKeyFrame(1f, 0f);
            _enterAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _enterAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _enterAnimation.IterationCount = 1;

            _exitAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _exitAnimation.InsertKeyFrame(1f, 1f);
            _exitAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _exitAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _exitAnimation.IterationCount = 1;

            return null;
        }

        public override void ReleaseResources()
        {
            if (_effectFactory != null)
            {
                _effectFactory.Dispose();
                _effectFactory = null;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Dispose();
                _enterAnimation = null;
            }

            if (_exitAnimation != null)
            {
                _exitAnimation.Dispose();
                _exitAnimation = null;
            }
        }

        public override void OnPointerEnter(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation(_targetProperty, _enterAnimation);
        }

        public override void OnPointerExit(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation(_targetProperty, _exitAnimation);
        }
    }

    public class ExposureTechnique : EffectTechniques
    {
        const string _effectName = "ExposureEffect";
        const string _targetProperty = _effectName + ".Exposure";
        ScalarKeyFrameAnimation _enterAnimation;
        ScalarKeyFrameAnimation _exitAnimation;

        public ExposureTechnique(Compositor compositor) : base(compositor)
        {
        }

#pragma warning disable 1998
        public override async Task<CompositionDrawingSurface> LoadResources()
        {
            // Create the effect template
            var graphicsEffect = new ExposureEffect
            {
                Name = _effectName,
                Exposure = 0.0f,
                Source = new CompositionEffectSourceParameter("ImageSource")
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { _targetProperty });


            // Create the animations
            _enterAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _enterAnimation.InsertKeyFrame(1f, 2f);
            _enterAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _enterAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _enterAnimation.IterationCount = 1;

            _exitAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _exitAnimation.InsertKeyFrame(1f, 0f);
            _exitAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _exitAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _exitAnimation.IterationCount = 1;

            return null;
        }

        public override void ReleaseResources()
        {
            if (_effectFactory != null)
            {
                _effectFactory.Dispose();
                _effectFactory = null;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Dispose();
                _enterAnimation = null;
            }

            if (_exitAnimation != null)
            {
                _exitAnimation.Dispose();
                _exitAnimation = null;
            }
        }

        public override void OnPointerEnter(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation(_targetProperty, _enterAnimation);
        }

        public override void OnPointerExit(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation(_targetProperty, _exitAnimation);
        }
    }

    public class BlurTechnique : EffectTechniques
    {
        ScalarKeyFrameAnimation _animationIncreasing;
        ScalarKeyFrameAnimation _animationDecreasing;

        public BlurTechnique(Compositor compositor) : base(compositor)
        {
        }

#pragma warning disable 1998
        public override async Task<CompositionDrawingSurface> LoadResources()
        {
            // Create the effect template
            var graphicsEffect = new ArithmeticCompositeEffect
            {
                Name = "Arithmetic",
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source1Amount = 1,
                Source2 = new CompositionEffectSourceParameter("EffectSource"),
                Source2Amount = 0,
                MultiplyAmount = 0
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "Arithmetic.Source1Amount", "Arithmetic.Source2Amount" });


            // Create the animations
            _animationDecreasing = _compositor.CreateScalarKeyFrameAnimation();
            _animationDecreasing.InsertKeyFrame(1f, 0f);
            _animationDecreasing.Duration = TimeSpan.FromMilliseconds(1000);
            _animationDecreasing.IterationBehavior = AnimationIterationBehavior.Count;
            _animationDecreasing.IterationCount = 1;

            _animationIncreasing = _compositor.CreateScalarKeyFrameAnimation();
            _animationIncreasing.InsertKeyFrame(1f, 1f);
            _animationIncreasing.Duration = TimeSpan.FromMilliseconds(1000);
            _animationIncreasing.IterationBehavior = AnimationIterationBehavior.Count;
            _animationIncreasing.IterationCount = 1;

            _loadEffectHandler = ApplyBlurEffect;

            return null;
        }

        private void ApplyBlurEffect(CompositionDrawingSurface surface, CanvasBitmap bitmap, CompositionGraphicsDevice device)
        {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Source = bitmap,
                BlurAmount = 40.0f,
                BorderMode = EffectBorderMode.Hard,
            };
            
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.DrawImage(blurEffect);
                ds.FillRectangle(new Rect(0, 0, surface.Size.Width, surface.Size.Height), Windows.UI.Color.FromArgb(60, 0, 0, 0));
            }
        }

        public override void ReleaseResources()
        {
            if (_effectFactory != null)
            {
                _effectFactory.Dispose();
                _effectFactory = null;
            }

            if (_animationIncreasing != null)
            {
                _animationIncreasing.Dispose();
                _animationIncreasing = null;
            }

            if (_animationDecreasing != null)
            {
                _animationDecreasing.Dispose();
                _animationDecreasing = null;
            }
        }

        public override void OnPointerEnter(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation("Arithmetic.Source1Amount", _animationDecreasing);
            image.Brush.StartAnimation("Arithmetic.Source2Amount", _animationIncreasing);
        }

        public override void OnPointerExit(Vector2 pointerPosition, CompositionImage image)
        {
            image.Brush.StartAnimation("Arithmetic.Source1Amount", _animationIncreasing);
            image.Brush.StartAnimation("Arithmetic.Source2Amount", _animationDecreasing);
        }
    }

    public class SpotLightTechnique : EffectTechniques
    {
        ManagedSurface _lightMap;
        ExpressionNode _transformExpressionNode;
        ScalarKeyFrameAnimation _enterAnimation;
        ScalarKeyFrameAnimation _exitAnimation;
        CompositionPropertySet _propertySet;

        public SpotLightTechnique(Compositor compositor) : base(compositor)
        {
        }

        public override async Task<CompositionDrawingSurface> LoadResources()
        {
            var graphicsEffect = new ArithmeticCompositeEffect
            {
                Name = "Arithmetic",
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source1Amount = .25f,
                Source2 = new Transform2DEffect
                {
                    Name = "LightMapTransform",
                    Source = new CompositionEffectSourceParameter("LightMap")
                },
                Source2Amount = 0,
                MultiplyAmount = 1
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "LightMapTransform.TransformMatrix" });

            // Create the image
            _lightMap = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/NormalMapsAndMasks/conemap.jpg"));

            // Create the animations
            float sweep = (float)Math.PI / 10f;
            float fullCircle = (float)Math.PI * -2f;
            _enterAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _enterAnimation.InsertKeyFrame(0.1f, fullCircle);
            _enterAnimation.InsertKeyFrame(0.4f, fullCircle + sweep);
            _enterAnimation.InsertKeyFrame(0.8f, fullCircle - sweep);
            _enterAnimation.InsertKeyFrame(1.0f, fullCircle);
            _enterAnimation.Duration = TimeSpan.FromMilliseconds(4500);
            _enterAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _enterAnimation.IterationCount = 1;

            _exitAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _exitAnimation.InsertKeyFrame(1.0f, 0f);
            _exitAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            _exitAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _exitAnimation.IterationCount = 1;

            var propsNode = ExpressionValues.Reference.CreatePropertySetReference("props");
            var propsCenterPointOffset = propsNode.GetVector2Property("CenterPointOffset");
            var propsRotation = propsNode.GetScalarProperty("Rotation");
            var propsScale = propsNode.GetScalarProperty("Scale");
            _transformExpressionNode = EF.CreateTranslation(-propsCenterPointOffset) * 
                                       EF.Matrix3x2(EF.Cos(propsRotation) * propsScale, 
                                                    EF.Sin(propsRotation), 
                                                    -EF.Sin(propsRotation), 
                                                    EF.Cos(propsRotation) * propsScale, 
                                                    0, 
                                                    0) * 
                                       EF.CreateTranslation(propsCenterPointOffset + propsNode.GetVector2Property("Translate"));

            return null;
        }

        public override void ReleaseResources()
        {
            if (_effectFactory != null)
            {
                _effectFactory.Dispose();
                _effectFactory = null;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Dispose();
                _enterAnimation = null;
            }

            if (_exitAnimation != null)
            {
                _exitAnimation.Dispose();
                _exitAnimation = null;
            }

            if (_transformExpressionNode != null)
            {
                _transformExpressionNode.Dispose();
                _transformExpressionNode = null;
            }

            if (_propertySet != null)
            {
                _propertySet.Dispose();
                _propertySet = null;
            }
        }

        public override CompositionEffectBrush CreateBrush()
        {
            CompositionEffectBrush brush = base.CreateBrush();
            brush.SetSourceParameter("LightMap", _lightMap.Brush);

            return brush;
        }

        public override void OnPointerEnter(Vector2 pointerPosition, CompositionImage image)
        {
            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertScalar("Scale", 1.25f);
            _propertySet.InsertScalar("Rotation", 0f);
            _propertySet.InsertVector2("Translate", new Vector2(0f, 0f));
            _propertySet.InsertVector2("CenterPointOffset", new Vector2((float)_lightMap.Size.Width / 2f, 0f));

            _transformExpressionNode.SetReferenceParameter("props", _propertySet);

            image.Brush.StartAnimation("LightMapTransform.TransformMatrix", _transformExpressionNode);
            _propertySet.StartAnimation("Rotation", _enterAnimation);
        }

        public override void OnPointerExit(Vector2 pointerPosition, CompositionImage image)
        {
            _propertySet.StartAnimation("Rotation", _exitAnimation);
        }
    }

    public class PointLightFollowTechnique : EffectTechniques
    {
        ManagedSurface _lightMap;
        ExpressionNode _transformExpressionNode;
        ScalarKeyFrameAnimation _enterAnimation;
        Vector2KeyFrameAnimation _exitAnimation;
        CompositionPropertySet _propertySet;

        public PointLightFollowTechnique(Compositor compositor) : base(compositor)
        {
        }

        public override async Task<CompositionDrawingSurface> LoadResources()
        {
            var graphicsEffect = new ArithmeticCompositeEffect
            {
                Name = "Arithmetic",
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source1Amount = .1f,
                Source2 = new Transform2DEffect
                {
                    Name = "LightMapTransform",
                    Source = new CompositionEffectSourceParameter("LightMap")
                },
                Source2Amount = 0,
                MultiplyAmount = 1
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "LightMapTransform.TransformMatrix" });

            // Create the image
            _lightMap = await ImageLoader.Instance.LoadFromUriAsync(new Uri("ms-appx:///Assets/NormalMapsAndMasks/pointmap.jpg"));

            // Create the animations
            CubicBezierEasingFunction easeIn = _compositor.CreateCubicBezierEasingFunction(new Vector2(0.0f, 0.51f), new Vector2(1.0f, 0.51f));
            _enterAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _enterAnimation.InsertKeyFrame(0.33f, 1.25f, easeIn);
            _enterAnimation.InsertKeyFrame(0.66f, 0.75f, easeIn);
            _enterAnimation.InsertKeyFrame(1.0f, 1.0f, easeIn);
            _enterAnimation.Duration = TimeSpan.FromMilliseconds(5000);
            _enterAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _exitAnimation = _compositor.CreateVector2KeyFrameAnimation();
            _exitAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
            _exitAnimation.Duration = TimeSpan.FromMilliseconds(750);
            _exitAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            _exitAnimation.IterationCount = 1;

            var propsNode = ExpressionValues.Reference.CreatePropertySetReference("props");
            var propsCenterPointOffset = propsNode.GetVector2Property("CenterPointOffset");
            var propsTranslate = propsNode.GetVector2Property("Translate");
            var propsScale = propsNode.GetScalarProperty("Scale");
            _transformExpressionNode = EF.Matrix3x2(propsScale, 
                                                    0, 
                                                    0, 
                                                    propsScale, 
                                                    propsCenterPointOffset.X * (1 - propsScale) + (propsTranslate.X * propsCenterPointOffset.X * 2), 
                                                    propsCenterPointOffset.Y * (1 - propsScale) + (propsTranslate.Y * propsCenterPointOffset.Y * 2));

            return null;
        }

        public override void ReleaseResources()
        {
            if (_effectFactory != null)
            {
                _effectFactory.Dispose();
                _effectFactory = null;
            }

            if (_enterAnimation != null)
            {
                _enterAnimation.Dispose();
                _enterAnimation = null;
            }

            if (_exitAnimation != null)
            {
                _exitAnimation.Dispose();
                _exitAnimation = null;
            }

            if (_transformExpressionNode != null)
            {
                _transformExpressionNode.Dispose();
                _transformExpressionNode = null;
            }

            if (_propertySet != null)
            {
                _propertySet.Dispose();
                _propertySet = null;
            }
        }

        public override CompositionEffectBrush CreateBrush()
        {
            CompositionEffectBrush brush = base.CreateBrush();
            brush.SetSourceParameter("LightMap", _lightMap.Brush);

            return brush;
        }

        public override void OnPointerEnter(Vector2 pointerPosition, CompositionImage image)
        {
            _propertySet = _compositor.CreatePropertySet();
            _propertySet.InsertScalar("Scale", 1f);
            Vector2 positionNormalized = new Vector2((pointerPosition.X / (float)image.Width) - .5f, (pointerPosition.Y / (float)image.Height) - .5f);
            _propertySet.InsertVector2("Translate", positionNormalized);
            _propertySet.InsertVector2("CenterPointOffset", new Vector2(128, 128));

            _transformExpressionNode.SetReferenceParameter("props", _propertySet);

            image.Brush.StartAnimation("LightMapTransform.TransformMatrix", _transformExpressionNode);
            _propertySet.StartAnimation("Scale", _enterAnimation);
        }

        public override void OnPointerExit(Vector2 pointerPosition, CompositionImage image)
        {
            _propertySet.StartAnimation("Translate", _exitAnimation);
            _propertySet.StopAnimation("Scale");
        }

        public override void OnPointerMoved(Vector2 pointerPosition, CompositionImage image)
        {
            Vector2 positionNormalized = new Vector2((pointerPosition.X / (float)image.Width) - .5f, (pointerPosition.Y / (float)image.Height) - .5f);
            _propertySet.InsertVector2("Translate", positionNormalized);
        }
    }
}
