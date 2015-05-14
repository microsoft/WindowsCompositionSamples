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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace SlideShow
{
#if ENABLE_TESTRANGE

    partial class MainPage
    {
        private void Flatten_Checked(object sender, RoutedEventArgs e)
        {
            _transitionController.LayoutManager.IsFlattened = true;
        }


        private void Flatten_Unchecked(object sender, RoutedEventArgs e)
        {
            _transitionController.LayoutManager.IsFlattened = false;
        }

        private void DepthSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            float min = (float) DepthSliderFar.Value;
            float max = (float) DepthSliderNear.Value;
            if (min > max)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            if (_transitionController != null)
            {
                _transitionController.UpdateDepthRange(min, max);
            }
        }
    }


    partial class LayoutManager
    {
        public bool IsFlattened
        {
            get
            {
                return _isFlattened;
            }

            set
            {
                if (_isFlattened != value)
                {
                    _isFlattened = value;

                    foreach (var tile in _allTiles)
                    {
                        var offset = tile.Offset;
                        tile.Offset = new Vector3(
                            offset.X,
                            offset.Y,
                            _isFlattened ? 0.0f : GetRandomZ());
                    }
                }
            }
        }


        public void UpdateDepthRange(float near, float far)
        {
            Debug.Assert(near >= 0.0f);
            Debug.Assert(far >= near);

            _nearRange = -near;
            _farRange = -far;

            foreach (var tile in _allTiles)
            {
                var offset = tile.Offset;

                if (_isFlattened)
                {
                    offset.Z = 0.0f;
                }
                else if (offset.Z > _nearRange)
                {
                    offset.Z = _nearRange;
                }
                else if (offset.Z < _farRange)
                {
                    offset.Z = _farRange;
                }

                tile.Offset = offset;
            }
        }

        private float GetRandomZ()
        {
            return (float) (_random.NextDouble()) * (_farRange - _nearRange) + _nearRange;
        }

        private float _nearRange = 0;
        private float _farRange = -200;
        private bool _isFlattened;
    }


    partial class TransitionController
    {
        public void UpdateDepthRange(float near, float far)
        {
            _layoutManager.UpdateDepthRange(near, far);
        }
    }

#endif
}
