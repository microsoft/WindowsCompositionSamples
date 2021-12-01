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
using System;
using System.Numerics;

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    public sealed partial class AnimationControl : SamplePage
    {
        public AnimationControl()
        {
            InitializeComponent();
            AnimationSetup();
        }

        public static string        StaticSampleName => "Animation Controller"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Play, pause, speed up, slow down, " +
            "or scrub your animation with the AnimationController API"; 
        public override string      SampleDescription => StaticSampleDescription;
        private void AnimationSetup()
        {
            _compositor = CompositionTarget.GetCompositorForCurrentThread();
            _visual = ElementCompositionPreview.GetElementVisual(Rectangle);
            _pause = true;
            _interval = TimeSpan.FromMilliseconds(16); // based on 60f/sec

            var linear = _compositor.CreateLinearEasingFunction();
            int animationDuration = 4;
            float animationMax = 250f;

            // set up main animation
            ElementCompositionPreview.SetIsTranslationEnabled(Rectangle, true);
            _animation = _compositor.CreateVector3KeyFrameAnimation();
            _animation.InsertKeyFrame(0.25f, new Vector3(animationMax, (float)Canvas.GetTop(Rectangle), 0f), linear);
            _animation.InsertKeyFrame(0.5f, new Vector3(animationMax, animationMax, 0f), linear);
            _animation.InsertKeyFrame(0.75f, new Vector3((float)Canvas.GetLeft(Rectangle), animationMax, 0f), linear);
            _animation.InsertKeyFrame(1f, new Vector3((float)Canvas.GetLeft(Rectangle), (float)Canvas.GetTop(Rectangle), 0f), linear);
            _animation.Duration = TimeSpan.FromSeconds(animationDuration);
            _animation.IterationBehavior = AnimationIterationBehavior.Forever;

            // set up dispatcher timer to animate slider
            _sliderAnimator = new DispatcherTimer();
            _sliderAnimator.Tick += SliderBehavior;
            _sliderAnimator.Interval = _interval;

            // initialize amount to change slider value per tick
            _delta = (slider.Maximum / _animation.Duration.TotalMilliseconds) * _interval.TotalMilliseconds;

            // add pointer listeners to slider for smooth scrubbing action
            slider.AddHandler(PointerPressedEvent, new PointerEventHandler(PressedThumb), true);
            slider.AddHandler(PointerReleasedEvent, new PointerEventHandler(ReleasedThumb), true);
        }
        private void EnsureController()
        {
            if (_controller == null)
            {
                // start animation on visual, and grab AnimationController from visual
                _visual.StartAnimation(nameof(Visual.Offset), _animation);
                _controller = _visual.TryGetAnimationController(nameof(Visual.Offset));
                _controller.Pause();
            }
        }
        private void PlayPause_Animation(object sender, RoutedEventArgs e)
        {
            EnsureController();
            if (_pause)
            {
                _controller.Resume();
                PlaySlider();
                PlayIcon.Symbol = Symbol.Pause;
            }
            else
            {
                _controller.Pause();
                StopSlider();
                PlayIcon.Symbol = Symbol.Play;
            }
            _pause = !_pause;
        }
        private void Stop_Animation(object sender, RoutedEventArgs e)
        {
            _pause = false;
            PlayPause_Animation(sender, e);
            _controller.PlaybackRate = 1;
            _controller.Progress = 0;
            slider.Value = 0;
        }
        private void SpeedUp_Animation(object sender, RoutedEventArgs e)
        {
            EnsureController();
            if (Math.Abs(_controller.PlaybackRate) < Math.Abs(AnimationController.MaxPlaybackRate) && !_pause)
            {
                _controller.PlaybackRate *= 2;
            }
        }
        private void SlowDown_Animation(object sender, RoutedEventArgs e)
        {
            EnsureController();
            if (Math.Abs(_controller.PlaybackRate) > Math.Abs(AnimationController.MinPlaybackRate) && !_pause)
            {
                _controller.PlaybackRate /= 2;
            }
        }
        private void Reverse_Animation(object sender, RoutedEventArgs e)
        {
            if (!_pause)
            {
                _controller.PlaybackRate *= -1;
            }
        }

        // helper methods to animate slider
        private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            EnsureController();
            // this enables scrubbing
            _controller.Progress = (float)slider.Value * .01f;
        }
        private void SliderBehavior(object sender, object e)
        {
            // update slider.Value based on time elapsed
            DateTimeOffset currentTime = DateTimeOffset.Now;
            TimeSpan elapsedTime = currentTime - lastTime;
            double numTicks = elapsedTime.TotalMilliseconds / _interval.TotalMilliseconds;
            slider.Value += (_delta * numTicks * _controller.PlaybackRate);

            // logic to loop slider animation
            // if PlaybackRate is less than 0, playing in reverse
            if (_controller.PlaybackRate > 0)
            {
                if (slider.Value == 100)
                {
                    slider.Value = 0;
                }
            }
            else
            {
                if (slider.Value == 0)
                {
                    slider.Value = 100;
                }
            }
            lastTime = currentTime;
        }
        private void PlaySlider()
        {
            if (_sliderAnimator != null)
            {
                _sliderAnimator.Start();
                lastTime = DateTimeOffset.Now;
            }
        }
        private void StopSlider()
        {
            if (_sliderAnimator != null)
                _sliderAnimator.Stop();
        }

        // helper methods for mouse input on the slider for smooth scrubbing action
        private void PressedThumb(object sender, PointerRoutedEventArgs e)
        {
            EnsureController();
            StopSlider();
            _controller.Pause();
        }
        private void ReleasedThumb(object sender, PointerRoutedEventArgs e)
        {
            EnsureController();
            if (!_pause)
            {
                PlaySlider();
                _controller.Resume();
            }
        }

        private Compositor _compositor;
        private Vector3KeyFrameAnimation _animation;
        private AnimationController _controller;
        private Visual _visual;
        private bool _pause;
        private DispatcherTimer _sliderAnimator;
        private TimeSpan _interval;
        private DateTimeOffset lastTime;
        private double _delta;
    }
}
