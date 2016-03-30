//------------------------------------------------------------------------------
//
// Copyright (C) Microsoft. All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class TransitionLibrary
    //
    //  This class create a new Transition to visual "transition to" the given
    //  ContainerVisual that contains a photo.
    //
    //------------------------------------------------------------------------------

    class TransitionLibrary : IDisposable
    {
        public TransitionLibrary(Compositor compositor, LayoutManager layoutManager)
        {
            _layoutManager = layoutManager;
            _windowWidth = 640;
            _windowHeight = 480;
            _zoomScale = 3.0f;
            _random = new Random();

            CreateAnimationTemplates(compositor);
        }


        public void Dispose()
        {
            if (_nearSlideOffsetAnimation != null)
            {
                _nearSlideOffsetAnimation.Dispose();
                _nearSlideOffsetAnimation = null;
            }

            if (_farSlideOffsetAnimation != null)
            {
                _farSlideOffsetAnimation.Dispose();
                _farSlideOffsetAnimation = null;
            }

            if (_slideCenterAnimation != null)
            {
                _slideCenterAnimation.Dispose();
                _slideCenterAnimation = null;
            }

            if (_zoomScaleAnimation != null)
            {
                _zoomScaleAnimation.Dispose();
                _zoomScaleAnimation = null;
            }

            if (_zoomCenterAnimation != null)
            {
                _zoomCenterAnimation.Dispose();
                _zoomCenterAnimation = null;
            }

            if (_zoomOffsetAnimation != null)
            {
                _zoomOffsetAnimation.Dispose();
                _zoomOffsetAnimation = null;
            }

            if (_stackFlyInAnimation != null)
            {
                _stackFlyInAnimation.Dispose();
                _stackFlyInAnimation = null;
            }

            if (_stackFlyOutAnimation != null)
            {
                _stackFlyOutAnimation.Dispose();
                _stackFlyOutAnimation = null;
            }

            if (_stackScaleAnimation != null)
            {
                _stackScaleAnimation.Dispose();
                _stackScaleAnimation = null;
            }

            if (_colorFlashlightAnimation != null)
            {
                _colorFlashlightAnimation.Dispose();
                _colorFlashlightAnimation = null;
            }
        }


        public void UpdateWindowSize(Vector2 value)
        {
            _windowWidth = value.X;
            _windowHeight = value.Y;

            if (_colorFlashlightAnimation != null)
            {
                _colorFlashlightAnimation.SetScalarParameter("windowWidth", _windowWidth);
                _colorFlashlightAnimation.SetScalarParameter("windowHeight", _windowHeight);
            }
        }


        private void GetValues(
            Visual target, 
            out Vector3 targetCenter,
            out Vector3 targetCenterPoint,
            out Vector3 viewportCenter)
        {
            var offset = target.Offset;
            var size = target.Size;


            //
            // Compute the position of the target's center
            //

            targetCenter = new Vector3(
                offset.X + (size.X / 2.0f),
                offset.Y + (size.Y / 2.0f),
                offset.Z);


            //
            // Compute the new scale center-point
            //

            targetCenterPoint = new Vector3(
                targetCenter.X,
                targetCenter.Y,
                0.0f);


            //
            // Compute where to position the viewport so that the target looks centered
            //

            viewportCenter = new Vector3(
                targetCenter.X - (_windowWidth / 2.0f),
                targetCenter.Y - (_windowHeight / 2.0f),
                targetCenter.Z);
        }


        public void CreateZoomAndPanTransition(ContainerVisual targetVisual)
        {
            Vector3 targetCenter, targetCenterPoint, viewportCenter;
            GetValues(targetVisual, out targetCenter, out targetCenterPoint, out viewportCenter);

            _zoomOffsetAnimation.SetVector3Parameter("myViewportCenter", viewportCenter);
            _zoomCenterAnimation.SetVector3Parameter("myTargetCenterPoint", targetCenterPoint);
            _zoomScaleAnimation.SetVector3Parameter("myScale", new Vector3(_zoomScale, _zoomScale, 1.0f));

            var panningVisual = targetVisual.Parent;
            panningVisual.StartAnimation("Offset", _zoomOffsetAnimation);
            panningVisual.StartAnimation("CenterPoint", _zoomCenterAnimation);
            panningVisual.StartAnimation("Scale", _zoomScaleAnimation);
        }


        public void CreateNearSlideTransition(ContainerVisual targetVisual)
        {
            Vector3 targetCenter, targetCenterPoint, viewportCenter;
            GetValues(targetVisual, out targetCenter, out targetCenterPoint, out viewportCenter);

            _nearSlideOffsetAnimation.SetVector3Parameter("myViewportCenter", viewportCenter);
            _slideCenterAnimation.SetVector3Parameter("myTargetCenterPoint", targetCenterPoint);
            _nearSlideScaleAnimation.SetVector3Parameter("myScale", new Vector3(_zoomScale, _zoomScale, 1.0f));

            _slideCenterAnimation.Duration = _nearSlideOffsetAnimation.Duration;

            var panningVisual = targetVisual.Parent;
            panningVisual.StartAnimation("Offset", _nearSlideOffsetAnimation);
            panningVisual.StartAnimation("CenterPoint", _slideCenterAnimation);
            panningVisual.StartAnimation("Scale", _nearSlideScaleAnimation);
        }


        public void CreateFarSlideTransition(ContainerVisual targetVisual)
        {
            Vector3 targetCenter, targetCenterPoint, viewportCenter;
            GetValues(targetVisual, out targetCenter, out targetCenterPoint, out viewportCenter);

            _farSlideOffsetAnimation.SetVector3Parameter("myViewportCenter", viewportCenter);
            _slideCenterAnimation.SetVector3Parameter("myTargetCenterPoint", targetCenterPoint);

            _slideCenterAnimation.Duration = _farSlideOffsetAnimation.Duration;

            var parentVisual = targetVisual.Parent;
            parentVisual.StartAnimation("Offset", _farSlideOffsetAnimation);
            parentVisual.StartAnimation("CenterPoint", _slideCenterAnimation);
            parentVisual.StartAnimation("Scale", _farSlideScaleAnimation);
        }


        public void CreateStackTransition(ContainerVisual targetVisual)
        {
            int stackSize = _random.Next(4, 9);

            Debug.Assert(_stackedTiles == null);
            _stackedTiles = new List<Tile>();
            _stackVisual = targetVisual;

            _stackScaleAnimation.SetVector3Parameter("myScale", new Vector3(_zoomScale, _zoomScale, 1.0f));
            targetVisual.Parent.StartAnimation("Scale", _stackScaleAnimation);

            for (int i = 0; i < stackSize; i++)
            {
                var tile = _layoutManager.GetFarNeighborPreserveSelection();
                _stackedTiles.Add(tile);

                Vector3 offset = tile.Frame.Offset - targetVisual.Offset;
                Vector3 direction = offset / offset.Length();
                Vector3 startDelta = direction * Math.Max(_windowWidth, _windowHeight);

                float rx = (float) (_random.NextDouble() * 100 - 50);
                float ry = (float) (_random.NextDouble() * 100 - 50);
                Vector3 endDelta = -direction * new Vector3(rx, ry, 0.0f);

                _stackFlyInAnimation.SetReferenceParameter("stackVisual", targetVisual);
                _stackFlyInAnimation.SetVector3Parameter("startDelta", startDelta);
                _stackFlyInAnimation.SetVector3Parameter("endDelta", endDelta);
                _stackFlyInAnimation.DelayTime = TimeSpan.FromSeconds(i * 2);

                tile.Frame.StartAnimation("Offset", _stackFlyInAnimation);

                tile.BringToTop();
            }
        }


        public void CreateUnstackTransition(ContainerVisual targetVisual)
        {
            Debug.Assert((_stackedTiles != null) && (_stackedTiles.Count > 0));
            
            for (int i = 0; i < _stackedTiles.Count; i++)
            {
                Vector3 offset = _stackedTiles[i].Frame.Offset - _stackVisual.Offset;
                Vector3 direction = offset / offset.Length();
                Vector3 delta = direction * Math.Max(_windowWidth, _windowHeight);

                _stackFlyOutAnimation.SetVector3Parameter("delta", delta);
                _stackFlyOutAnimation.SetVector3Parameter("originalOffset", _stackedTiles[i].Offset);
                _stackFlyOutAnimation.DelayTime = TimeSpan.FromMilliseconds(1500 + (_stackedTiles.Count - i) * 100);

                _stackedTiles[i].Frame.StartAnimation("Offset", _stackFlyOutAnimation);
            }

            _stackedTiles.Clear();
            _stackedTiles = null;
        }
        

        public void ApplyDesaturation(Tile tile, TransitionDesaturationMode mode)
        {
            switch (mode)
            {
                case TransitionDesaturationMode.ColorFlashlight:
                    Debug.Assert(_colorFlashlightAnimation != null);

                    _colorFlashlightAnimation.SetReferenceParameter("frame", tile.Frame);
                    tile.ApplyDesaturationAnimation(_colorFlashlightAnimation);

                    break;

                case TransitionDesaturationMode.None:
                case TransitionDesaturationMode.Regular:
                    tile.ApplyDesaturationAnimation(tile.IsDesaturated ?
                        CommonAnimations.SlowOffAnimation :
                        CommonAnimations.NormalOnAnimation);

                    break;
            }
        }


        private void CreateAnimationTemplates(Compositor compositor)
        {
            //
            // Near-slide and far-slide animations.
            //

            _nearSlideOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            _nearSlideOffsetAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
            _nearSlideOffsetAnimation.InsertExpressionKeyFrame(1.0f, "vector3(0,0,0) - myViewportCenter");

            _farSlideOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            _farSlideOffsetAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
            _farSlideOffsetAnimation.InsertExpressionKeyFrame(0.9f, "vector3(0,0,0) - myViewportCenter");
            _farSlideOffsetAnimation.InsertExpressionKeyFrame(1.0f, "vector3(0,0,0) - myViewportCenter");

            _slideCenterAnimation = compositor.CreateVector3KeyFrameAnimation();
            _slideCenterAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
            _slideCenterAnimation.InsertExpressionKeyFrame(1.0f, "myTargetCenterPoint");

            _nearSlideScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            _nearSlideScaleAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _nearSlideScaleAnimation.InsertExpressionKeyFrame(1.00f, "myScale");

            _farSlideScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            _farSlideScaleAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _farSlideScaleAnimation.InsertKeyFrame(0.30f, new Vector3(1.0f, 1.0f, 1.0f));
            _farSlideScaleAnimation.InsertKeyFrame(1.00f, new Vector3(1.0f, 1.0f, 1.0f));

            TimeSpan time4sec = TimeSpan.FromSeconds(4);
            TimeSpan time8sec = TimeSpan.FromSeconds(8);
            _nearSlideOffsetAnimation.Duration = time4sec;
            _farSlideOffsetAnimation.Duration = time8sec;
            _slideCenterAnimation.Duration = time4sec;
            _nearSlideScaleAnimation.Duration = time4sec;
            _farSlideScaleAnimation.Duration = time4sec;


            //
            // Zoom animations.
            //

            _zoomScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            _zoomScaleAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _zoomScaleAnimation.InsertKeyFrame(0.40f, new Vector3(1.0f, 1.0f, 1.0f));
            _zoomScaleAnimation.InsertKeyFrame(0.60f, new Vector3(1.0f, 1.0f, 1.0f));
            _zoomScaleAnimation.InsertExpressionKeyFrame(1.00f, "myScale");

            _zoomCenterAnimation = compositor.CreateVector3KeyFrameAnimation();
            _zoomCenterAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _zoomCenterAnimation.InsertExpressionKeyFrame(0.40f, "this.StartingValue");
            _zoomCenterAnimation.InsertExpressionKeyFrame(0.60f, "myTargetCenterPoint");
            _zoomCenterAnimation.InsertExpressionKeyFrame(1.00f, "myTargetCenterPoint");

            _zoomOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            _zoomOffsetAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _zoomOffsetAnimation.InsertExpressionKeyFrame(1.00f, "vector3(0,0,0) - myViewportCenter");

            TimeSpan time12sec = TimeSpan.FromSeconds(12);
            _zoomScaleAnimation.Duration = time12sec;
            _zoomCenterAnimation.Duration = time12sec;
            _zoomOffsetAnimation.Duration = time12sec;


            //
            // Stack animations.
            //

            CubicBezierEasingFunction flyInEasing;
            CubicBezierEasingFunction flyOutEasing;

            flyInEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.0f, 1.0f), new Vector2(0.8f, 1.0f));
            _stackFlyInAnimation = compositor.CreateVector3KeyFrameAnimation();
            _stackFlyInAnimation.InsertExpressionKeyFrame(0.00f, "stackVisual.Offset + startDelta");
            _stackFlyInAnimation.InsertExpressionKeyFrame(1.00f, "stackVisual.Offset + endDelta", flyInEasing);
            _stackFlyInAnimation.Duration = TimeSpan.FromSeconds(2);

            flyOutEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.0f, 0.4f), new Vector2(1.0f, 0.6f));
            _stackFlyOutAnimation = compositor.CreateVector3KeyFrameAnimation();
            _stackFlyOutAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue", flyOutEasing);
            _stackFlyOutAnimation.InsertExpressionKeyFrame(0.50f, "this.StartingValue + delta", flyOutEasing);
            _stackFlyOutAnimation.InsertExpressionKeyFrame(1.00f, "originalOffset", flyOutEasing);
            _stackFlyOutAnimation.Duration = TimeSpan.FromSeconds(2);

            _stackScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            _stackScaleAnimation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");
            _stackScaleAnimation.InsertExpressionKeyFrame(1.00f, "myScale");
            _stackScaleAnimation.Duration = TimeSpan.FromSeconds(6);


            //
            // Color flashlight expression animation.
            // 
            // This expression returns a computes between 0 and 1 as a function of on how close the 
            // center of the frame is to the center of the window. 
            //   - If the frame is at the center of the window, the expression computes 0 (no
            //     desaturation).
            //   - If the frame is more than 300px away from the center of the window, the
            //     expression computes 1 (full desaturation).
            //   - If the frame is within 300px from the center of the window, the expression
            //     computes a value between 0 and 1 relative to how far the frame is from the 300px
            //     boundary (partial desaturation).
            //

            _colorFlashlightAnimation = compositor.CreateExpressionAnimation(
                  "1.0 - min("
                + "    1.0,"
                + "    ("
                + "        ("
                + "            ( frame.Offset.x + (frame.Size.x * 0.5) + grid.Offset.x - (windowWidth * 0.5) )"
                + "          * ( frame.Offset.x + (frame.Size.x * 0.5) + grid.Offset.x - (windowWidth * 0.5) )"
                + "        ) + ("
                + "            ( frame.Offset.y + (frame.Size.y * 0.5) + grid.Offset.y - (windowHeight * 0.5) )"
                + "          * ( frame.Offset.y + (frame.Size.y * 0.5) + grid.Offset.y - (windowHeight * 0.5) )"
                + "        )"
                + "    ) / ( radius * radius )"
                + ")");
            
            _colorFlashlightAnimation.SetReferenceParameter("grid", _layoutManager.GridVisual);
            _colorFlashlightAnimation.SetScalarParameter("radius", 300);
            _colorFlashlightAnimation.SetScalarParameter("windowWidth", _windowWidth);
            _colorFlashlightAnimation.SetScalarParameter("windowHeight", _windowHeight);
        }


        private LayoutManager _layoutManager;
        private float _windowWidth;
        private float _windowHeight;
        private float _zoomScale;
        private Random _random;
        private List<Tile> _stackedTiles;
        private ContainerVisual _stackVisual;

        private Vector3KeyFrameAnimation _nearSlideOffsetAnimation;
        private Vector3KeyFrameAnimation _farSlideOffsetAnimation;
        private Vector3KeyFrameAnimation _slideCenterAnimation;
        private Vector3KeyFrameAnimation _nearSlideScaleAnimation;
        private Vector3KeyFrameAnimation _farSlideScaleAnimation;

        private Vector3KeyFrameAnimation _zoomScaleAnimation;
        private Vector3KeyFrameAnimation _zoomCenterAnimation;
        private Vector3KeyFrameAnimation _zoomOffsetAnimation;

        private Vector3KeyFrameAnimation _stackFlyInAnimation;
        private Vector3KeyFrameAnimation _stackFlyOutAnimation;
        private Vector3KeyFrameAnimation _stackScaleAnimation;

        private ExpressionAnimation _colorFlashlightAnimation;
    }
}
