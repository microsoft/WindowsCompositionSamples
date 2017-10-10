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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace CompositionSampleGallery
{
    /// <summary>
    /// A model for CompositionEffectBrush. Dictates slider values, as well as changes a given 
    /// property of the effect brush.
    /// </summary>
    public class EffectItem
    {
        // Private Members
        private CompositionEffectFactory _factory;
        private CompositionEffectBrush _brush;

        /// <summary>
        /// The name of the effect. Must match the name given to the effect in the effect definition.
        /// </summary>
        public String EffectName { get; set; }
        /// <summary>
        /// The name of the property. Must match the name of the property.
        /// </summary>
        public String AnimatablePropertyName { get; set; }
        /// <summary>
        /// The minimum value for the animatable property.
        /// </summary>
        public float ValueMin { get; set; }
        /// <summary>
        /// The maximum value for the animatable property.
        /// </summary>
        public float ValueMax { get; set; }

        /// <summary>
        /// Function called to create the effect brush.
        /// </summary>
        public Func<CompositionEffectFactory> CreateEffectFactory { get; set; }

        // Slider specific properties.
        public float SmallChange { get; set; }
        public float LargeChange { get; set; }

        /// <summary>
        /// Returns the effect brush for this model. If a brush has not beeen created, one is created
        /// given CreateEffectFactory has been assigned.
        /// </summary>
        /// <returns>CompositionEffectBrush</returns>
        public CompositionEffectBrush GetEffectBrush()
        {
            if (_brush == null && CreateEffectFactory != null)
            {
                if (_factory == null)
                {
                    _factory = CreateEffectFactory();
                }

                _brush = _factory.CreateBrush();
            }

            return _brush;
        }

        /// <summary>
        /// Changes the animatable property on the effect brush if already created. Does nothing if
        /// there is no animatable property present.
        /// </summary>
        /// <param name="value">Value to assign to the effect brush's animatable property.</param>
        public void ChangeValue(float value)
        {
            if (_brush != null && !String.IsNullOrEmpty(AnimatablePropertyName))
            {
                // Change the value on the effect brush. Unlike a visual, where only the property name
                // is needed, both the effect name and the property value are needed to specify the 
                // effect property.
                _brush.Properties.InsertScalar(EffectName + "." + AnimatablePropertyName, value);
            }
        }

        /// <summary>
        /// Animates the effect brush property from the given starting value to the given ending 
        /// value for the given duration. The default easing funciton is used.
        /// </summary>
        /// <param name="start">Starting value for the effect brush property.</param>
        /// <param name="end">The ending or target value for the effect brush property.</param>
        /// <param name="duration">Desired length of the animation.</param>
        public void Animate(float start, float end, double duration)
        {
            if (_brush != null && !String.IsNullOrEmpty(AnimatablePropertyName))
            {
                // Create our animation.
                var compositor = _brush.Compositor;
                var animation = compositor.CreateScalarKeyFrameAnimation();
                animation.InsertKeyFrame(0.0f, start);
                animation.InsertKeyFrame(1.0f, end);
                animation.Duration = TimeSpan.FromSeconds(duration);

                // Like with directly changing the effect property, the full effect name and effect
                // property name are needed to set an animation.
                _brush.StartAnimation(EffectName + "." + AnimatablePropertyName, animation);
            }
        }

        /// <summary>
        /// Sentinel value for no effect.
        /// </summary>
        public static EffectItem None = new EffectItem() { EffectName = "None" };
    }
}
