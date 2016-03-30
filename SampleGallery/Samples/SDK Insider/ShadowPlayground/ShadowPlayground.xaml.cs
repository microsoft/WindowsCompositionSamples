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

using SamplesCommon;
using SamplesCommon.ImageLoader;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class ShadowPlayground : SamplePage
    {
        private Visual _shadowContainer;
        private Compositor _compositor;
        private SpriteVisual _imageVisual;
        private DropShadow _shadow;
        private CompositionImage _image;
        private IImageLoader _imageLoader;
        private IManagedSurface _imageMaskSurface;
        private CompositionMaskBrush _maskBrush;
        private bool _isMaskEnabled;
        private bool _isAnimationEnabled;

        private const int ANIMATION_DURATION = 1; //animation duration in seconds

        public ShadowPlayground()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName    { get { return "Shadow Playground"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Experiment with the available properties on the DropShadow object to create interesting shadows."; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761177"; } }

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get backing visual from shadow container and interop compositor
            _shadowContainer = ElementCompositionPreview.GetElementVisual(ShadowContainer);
            _compositor = _shadowContainer.Compositor;

            // Get CompositionImage, its sprite visual
            _image = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(ShadowContainer);
            _imageVisual = _image.SpriteVisual;
           
            // Add drop shadow to image visual
            _shadow = _compositor.CreateDropShadow();
            _imageVisual.Shadow = _shadow;

            // Initialize sliders to shadow defaults - with the exception of offset
            BlurRadiusSlider.Value  = _shadow.BlurRadius;   //defaults to 9.0f
            OffsetXSlider.Value     = _shadow.Offset.X;     //defaults to 0
            OffsetYSlider.Value     = _shadow.Offset.Y;     //defaults to 0
            RedSlider.Value         = _shadow.Color.R;      //defaults to 0 (black.R)
            GreenSlider.Value       = _shadow.Color.G;      //defaults to 0 (black.G) 
            BlueSlider.Value        = _shadow.Color.B;      //defaults to 0 (black.B) 

            // Load mask asset onto surface using helpers in SamplesCommon
            _imageLoader = ImageLoaderFactory.CreateImageLoader(_compositor);
            _imageMaskSurface = _imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Assets/CircleMask.png"));

            // Create surface brush for mask
            CompositionSurfaceBrush mask = _compositor.CreateSurfaceBrush();
            mask.Surface = _imageMaskSurface.Surface;
            
            // Get surface brush from composition image
            CompositionSurfaceBrush source = _image.SurfaceBrush as CompositionSurfaceBrush;

            // Create mask brush for toggle mask functionality
            _maskBrush = _compositor.CreateMaskBrush();
            _maskBrush.Mask = mask;
            _maskBrush.Source = source;

            // Initialize toggle mask and animation to false
            _isMaskEnabled = false;
            _isAnimationEnabled = false;
          
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_imageMaskSurface != null)
            {
                _imageMaskSurface.Dispose();
            }

            if (_imageLoader != null)
            {
                _imageLoader.Dispose();
            }
        }

        private void MaskButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            if (_isMaskEnabled) //then remove mask
            {
                _image.Brush = _maskBrush.Source; //set set composition image's brush to (the initial) surfacebrush (source) 
                _shadow.Mask = null; //remove mask from shadow

            }
            else //add mask
            {
                _image.Brush = _maskBrush; //set composition image's brush to maskbrush
                _shadow.Mask = _maskBrush.Mask; //add mask to shadow

            }

            // Update bool
            _isMaskEnabled = !_isMaskEnabled;
        }

        private void RedSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Set shadow's red color component to slider value
            if(!_isAnimationEnabled)
            {
                byte red = (byte)e.NewValue;
                _shadow.Color = Color.FromArgb(255, red, _shadow.Color.G, _shadow.Color.B);
            }
        }

        private void GreenSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Set shadow's green color component to slider value
            if (!_isAnimationEnabled)
            {
                byte green = (byte)e.NewValue;
                _shadow.Color = Color.FromArgb(255, _shadow.Color.R, green, _shadow.Color.B);
            }  
        }

        private void BlueSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Set shadow's blue color component to slider value
            if (!_isAnimationEnabled)
            {
                byte blue = (byte)e.NewValue;
                _shadow.Color = Color.FromArgb(255, _shadow.Color.R, _shadow.Color.G, blue);
            }
        }

        private void OffsetXSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Change shadow's horizontal offset based on slider value
            if (!_isAnimationEnabled)
            {
                var offset_x = (float)e.NewValue;
                _shadow.Offset = new Vector3(offset_x, _shadow.Offset.Y, _shadow.Offset.Z);
            }
        }

        private void OffsetYSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Change shadow's vertical offset based on slider value
            if (!_isAnimationEnabled)
            {
                var offset_y = (float)e.NewValue;
                _shadow.Offset = new Vector3(_shadow.Offset.X, offset_y, _shadow.Offset.Z);
            }
        }

        private void BlurRadiusSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            // Set shadow's blur radius to slider value
            if (!_isAnimationEnabled)
            {
                var blur_radius = (float)e.NewValue;
                _shadow.BlurRadius = blur_radius;
            }
        }

        private void AnimationCheckBox_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Enable or disable animation depending on whether box is checked
            CheckBox cb = (CheckBox)sender;
            if (cb.IsChecked.HasValue)
            {
                _isAnimationEnabled = (bool)cb.IsChecked;
            }
        }

        
        /** Event Handlers for Animation **/

        private void BlurRadiusSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate blur radius from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;

                ScalarKeyFrameAnimation blurAnimation = _compositor.CreateScalarKeyFrameAnimation();
                blurAnimation.InsertKeyFrame(1.0f, (float)sliderControl.Value);
                blurAnimation.Duration = new TimeSpan(0, 0, ANIMATION_DURATION);
                _shadow.StartAnimation("BlurRadius", blurAnimation);
            }
        }

        private void StartVector3Animation(String propertyName, Vector3 endValue)
        {
            // Create and start a Vector3 KeyFrame animation on shadow with given begin and end values
            Vector3KeyFrameAnimation animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, endValue);
            animation.Duration = new TimeSpan(0, 0, ANIMATION_DURATION);
            _shadow.StartAnimation(propertyName, animation);
        }

        private void OffsetXSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate shadow's x-offset from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;
                StartVector3Animation("Offset",
                    new Vector3((float)sliderControl.Value, _shadow.Offset.Y, _shadow.Offset.Z));
            }
        }

        private void OffsetYSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate shadow's y-offset from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;
                StartVector3Animation("Offset",
                    new Vector3(_shadow.Offset.X, (float)sliderControl.Value, _shadow.Offset.Z));
            }
        }

        private void StartColorAnimation(Color endValue)
        {
            // Create and start a color animation from begin to end values
            ColorKeyFrameAnimation animation = _compositor.CreateColorKeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, endValue);
            animation.Duration = new TimeSpan(0, 0, ANIMATION_DURATION);
            _shadow.StartAnimation("Color", animation);
        }

        private void RedSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate shadow's red color channel from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;
                StartColorAnimation(
                    Color.FromArgb(255, (byte)sliderControl.Value, _shadow.Color.G, _shadow.Color.B));
            }
        }

        private void GreenSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate shadow's green color channel from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;
                StartColorAnimation(
                    Color.FromArgb(255, _shadow.Color.R, (byte)sliderControl.Value, _shadow.Color.B));
            }
        }

        private void BlueSlider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Animate shadow's blue color channel from current value to slider value
            if (_isAnimationEnabled)
            {
                Slider sliderControl = (Slider)sender;
                StartColorAnimation(
                    Color.FromArgb(255, _shadow.Color.R, _shadow.Color.G, (byte)sliderControl.Value));
            }
        }
    }
}
