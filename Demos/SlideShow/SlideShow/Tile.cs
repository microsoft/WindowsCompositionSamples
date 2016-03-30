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
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.UI.Composition;

namespace SlideShow
{
    //------------------------------------------------------------------------------
    //
    // class Tile
    //
    //  This class represents a single piece of content to be created and positioned
    //  by the LayoutManager.  Each tile has a frame and a picture.
    //
    //------------------------------------------------------------------------------

    class Tile : IDisposable
    {
        public static void Initialize(Compositor compositor)
        {
            s_random = new Random();
            s_compositor = compositor;


            // Create a animatable desaturation effect description

            var effectDesc = new SaturationEffect();
            effectDesc.Name = "myEffect";
            effectDesc.Source = new CompositionEffectSourceParameter("Image");

            s_saturationEffectFactory = compositor.CreateEffectFactory(
                effectDesc,
                new string[] { "myEffect.Saturation" });
        }

        public static void Uninitialize()
        {
            s_saturationEffectFactory = null;

            s_compositor = null;
        }


        public Tile(ContainerVisual parent, float border)
        {
            _parent = parent;
            _border = border;


            //
            // Create a placeholder picture frame:
            // - This is the parent of the actual image.
            // - Configure with a center-point in the middle to allow easy rotation.
            // - Don't add it to the parent yet.  This enables the caller to specify more
            //   properties (such as offset) without animating them before making the Tile visible.
            //

            _frame = s_compositor.CreateSpriteVisual();
            _frame.Brush = s_compositor.CreateColorBrush(s_unselectedFrameColor);
            _frame.Opacity = 1.0f;


            //
            // Since we can't currently animate colors, need to animate opacity instead.  At the
            // least, we should be able to make this an effect and animate that.
            // - Start with opacity = 0 and fade in when selected.
            //

            _selectedFrame = s_compositor.CreateSpriteVisual();
            _selectedFrame.Offset = new Vector3();
            _selectedFrame.Brush = s_compositor.CreateColorBrush(s_selectedFrameColor);
            _selectedFrame.Opacity = 0.0f;
            _frame.Children.InsertAtBottom(_selectedFrame);


            //
            // Begin a default animation for the rotation.
            //

            StartNewRotationAnimation();
        }


        public void Dispose()
        {
            if (_selectedFrame != null)
            {
                _selectedFrame.Dispose();
                _selectedFrame = null;
            }

            if (_content != null)
            {
                _content.Dispose();
                _content = null;
            }

            if (_frame != null)
            {
                _frame.Dispose();
                _frame = null;
            }

            if (_saturationEffect != null)
            {
                _saturationEffect.Dispose();
                _saturationEffect = null;
            }
        }


        public int GridRow
        {
            get
            {
                return _row;
            }

            set
            {
                _row = value;
            }
        }


        public int GridColumn
        {
            get
            {
                return _col;
            }

            set
            {
                _col = value;
            }
        }


        public Vector3 Offset
        {
            get
            {
                return _offset;
            }

            set
            {
                if (_offset != value)
                {
                    _offset = value;

                    if (_isVisible)
                    {
                        //
                        // If visible, build a custom animation to move to the new location.
                        //

                        var kfOffset = _frame.Compositor.CreateVector3KeyFrameAnimation();
                        kfOffset.InsertKeyFrame(1.0f, value);
                        kfOffset.Duration = CommonAnimations.NormalTime;

                        _frame.StartAnimation("Offset", kfOffset);
                    }
                    else
                    {
                        //
                        // If not visible, avoid the delay from animating.
                        //

                        _frame.Offset = value;
                    }
                }
            }
        }


        public Vector2 Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;

                _frame.Size = value;
                _frame.CenterPoint = new Vector3(value.X / 2.0f, value.Y / 2.0f, 0.0f);

                _selectedFrame.Size = value;

                if (_content != null)
                {
                    _content.Size = new Vector2(value.X - (2 * _border), value.Y - (2 * _border));
                }
            }
        }


        public ContainerVisual Frame
        {
            get
            {
                return _frame;
            }
        }


        public bool IsDesaturated { get; set; }


        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    _selectedFrame.StartAnimation(
                        "Opacity",
                        value ?
                            CommonAnimations.NormalOnAnimation :
                            CommonAnimations.SlowOffAnimation);
                }
            }
        }


        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }

            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;

                    if (value)
                    {
                        Debug.Assert(_frame.Parent == null);
                        _parent.Children.InsertAtTop(_frame);
                    }
                    else
                    {
                        Debug.Assert(_frame.Parent == _parent);
                        _parent.Children.Remove(_frame);
                    }
                }
            }
        }


        public void BringToTop()
        {
            Debug.Assert(_frame.Parent == _parent);
            _parent.Children.Remove(_frame);
            _parent.Children.InsertAtTop(_frame);
        }


        public Photo Photo
        {
            get { return _photo; }

            set
            {
                _photo = value;

                //
                // Create individual effect instance to allow per-Tile animations.
                //

                _saturationEffect = s_saturationEffectFactory.CreateBrush();
                _saturationEffect.SetSourceParameter("Image", s_compositor.CreateSurfaceBrush(value.ThumbnailImage.Surface));
                _saturationEffect.Properties.InsertScalar(
                    "myEffect.Saturation",
                    IsDesaturated ? 0.0f : 1.0f);

                _content = s_compositor.CreateSpriteVisual();
                _content.Brush = _saturationEffect;


                //
                // Insert the new visual as a child of our frame.
                //

                var frameSize = _frame.Size;
                _content.Offset = new Vector3(_border, _border, 0.0f);
                _content.Size = new Vector2(frameSize.X - (2 * _border), frameSize.Y - (2 * _border));

                _frame.Children.InsertAtTop(_content);
            }
        }

        private void StartNewRotationAnimation()
        {
            Debug.Assert(!_rotateAnimating, 
                "Don't start new rotation while still animating previous rotation");

            //
            // Build a custom animation that takes a random amount of time to move to the new angle
            // value (in degrees).
            //

            var angleInDegrees = ((float)s_random.NextDouble()) * 20.0f - 10.0f;

            var rotationAnimation = s_compositor.CreateScalarKeyFrameAnimation();
#if false
            rotationAnimation.InsertKeyFrame(1.0f, angleInDegrees);
#else
            var angleInRadians = angleInDegrees * TWO_PI / 360.0f;
            rotationAnimation.InsertKeyFrame(1.0f, angleInRadians);
#endif

            var duration = s_random.Next(2000, 5000);
            rotationAnimation.Duration = TimeSpan.FromMilliseconds(duration);

            _desiredRotationInDegrees = angleInDegrees;


            //
            // Start new animator playing.
            //

            var scopedBatch = s_compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            scopedBatch.Completed += RotationAnimation_Completed;

#if false
            _frame.StartAnimation("RotationAngleInDegrees", rotationAnimation);
#else
            _frame.StartAnimation("RotationAngle", rotationAnimation);
#endif
            scopedBatch.End();

            _rotateAnimating = true;
        }

        private const float TWO_PI = 2.0f * (float) Math.PI;

        private void RotationAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            Debug.Assert(_rotateAnimating);
            _rotateAnimating = false;

#if false
            Debug.Assert(_desiredRotationInDegrees - _frame.RotationAngleInDegrees < 0.001f,
                "Ensure within epsilon of expected value");
#else
            var angleInDegrees = _frame.RotationAngle * 360.0f / TWO_PI;
            Debug.Assert(_desiredRotationInDegrees - angleInDegrees < 0.001f,
                "Ensure within epsilon of expected value");
#endif

            StartNewRotationAnimation();
        }


        public void ApplyDesaturationAnimation(CompositionAnimation animation)
        {
            if (_saturationEffect != null)
            {
                _saturationEffect.Properties.StartAnimation(
                    "myEffect.Saturation",
                    animation);
            }
        }

        private static readonly Color s_unselectedFrameColor = Colors.White;
        private static readonly Color s_selectedFrameColor = Colors.Orange;

        private static Random s_random;
        private static Compositor s_compositor;

        private int _row;
        private int _col;
        private Photo _photo;
        private Vector3 _offset;
        private Vector2 _size;
        private float _border;
        private bool _isSelected;
        private bool _isVisible;
        private ContainerVisual _parent;
        private SpriteVisual _frame;
        private SpriteVisual _selectedFrame;
        private bool _rotateAnimating;
        private float _desiredRotationInDegrees;

        private static CompositionEffectFactory s_saturationEffectFactory;

        private SpriteVisual _content;
        private CompositionEffectBrush _saturationEffect;
    }
}
