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

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using SamplesCommon;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    /// <summary>
    /// A helper class encapsulating the function and visuals for a Color Bloom transition animation.
    /// </summary>
    public class ColorBloomTransitionHelper : IDisposable
    {
        #region Member variables

        UIElement hostForVisual;
        Compositor _compositor;
        ContainerVisual _containerForVisuals;
        ScalarKeyFrameAnimation _bloomAnimation;
        ManagedSurface _circleMaskSurface;

        #endregion


        #region Ctor

        /// <summary>
        /// Creates an instance of the ColorBloomTransitionHelper. 
        /// Any visuals to be later created and animated will be hosted within the specified UIElement.
        /// </summary>
        public ColorBloomTransitionHelper(UIElement hostForVisual)
        {
            this.hostForVisual = hostForVisual;

            // we have an element in the XAML tree that will host our Visuals
            var visual = ElementCompositionPreview.GetElementVisual(hostForVisual);
            _compositor = visual.Compositor;

            // create a container
            // adding children to this container adds them to the live visual tree
            _containerForVisuals = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(hostForVisual, _containerForVisuals);

            // Create the circle mask
            _circleMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
        }
        #endregion


        #region Public API surface

        public delegate void ColorBloomTransitionCompletedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Indicates that the color bloom transition has completed.
        /// </summary>
        public event ColorBloomTransitionCompletedEventHandler ColorBloomTransitionCompleted;


        /// <summary>
        /// Starts the color bloom transition using the specified color and boundary sizes.
        /// 
        /// The bloom is achieved by creating a circular solid colored visual whose scale is progressively 
        /// animated to fully flood a given area.
        /// 
        /// <param name="color">Using the specified color</param>
        /// <param name="initialBounds">The transition begins with a visual with these bounds and position</param>  
        /// <param name="finalBounds">The transition ends when the visual has "bloomed" to an area of this bounding size</param>
        /// </summary>
        public void Start(Windows.UI.Color color, Rect initialBounds, Rect finalBounds)
        {
            var colorVisual = CreateVisualWithColorAndPosition(color, initialBounds, finalBounds);

            // add our solid colored circle visual to the live visual tree via the container
            _containerForVisuals.Children.InsertAtTop(colorVisual);

            // now that we have a visual, let's run the animation 
            TriggerBloomAnimation(colorVisual);
        }

        /// <summary>
        /// Cleans up any remaining surfaces.
        /// </summary>
        public void Dispose()
        {
            _circleMaskSurface.Dispose();
        }

        #endregion


        #region All the heavy lifting
        /// <summary>
        /// Creates a Visual using the specific color and constraints
        /// </summary>
        private SpriteVisual CreateVisualWithColorAndPosition(Windows.UI.Color color,
                                                              Windows.Foundation.Rect initialBounds,
                                                              Windows.Foundation.Rect finalBounds)
        {

            // init the position and dimensions for our visual
            var width = (float)initialBounds.Width;
            var height = (float)initialBounds.Height;
            var positionX = initialBounds.X;
            var positionY = initialBounds.Y;

            // we want our visual (a circle) to completely fit within the bounding box
            var circleColorVisualDiameter = (float)Math.Min(width, height);

            // the diameter of the circular visual is an essential bit of information
            // in initializing our bloom animation - a one-time thing
            if (_bloomAnimation == null)
                InitializeBloomAnimation(circleColorVisualDiameter / 2, finalBounds); // passing in the radius

            // we are going to some lengths to have the visual precisely placed
            // such that the center of the circular visual coincides with the center of the AppBarButton.
            // it is important that the bloom originate there
            var diagonal = Math.Sqrt(2 * (circleColorVisualDiameter * circleColorVisualDiameter));
            var deltaForOffset = (diagonal - circleColorVisualDiameter) / 2;

            // now we have everything we need to calculate the position (offset) and size of the visual
            var offset = new Vector3((float)positionX + (float)deltaForOffset + circleColorVisualDiameter / 2,
                                     (float)positionY + circleColorVisualDiameter / 2,
                                     0f);
            var size = new Vector2(circleColorVisualDiameter);

            // create the visual with a solid colored circle as brush
            SpriteVisual coloredCircleVisual = _compositor.CreateSpriteVisual();
            coloredCircleVisual.Brush = CreateCircleBrushWithColor(color);
            coloredCircleVisual.Offset = offset;
            coloredCircleVisual.Size = size;

            // we want our scale animation to be anchored around the center of the visual
            coloredCircleVisual.AnchorPoint = new Vector2(0.5f, 0.5f);


            return coloredCircleVisual;

        }


        /// <summary>
        /// Creates a circular solid colored brush that we can apply to a visual
        /// </summary>
        private CompositionEffectBrush CreateCircleBrushWithColor(Windows.UI.Color color)
        {

            var colorBrush = _compositor.CreateColorBrush(color);

            //
            // Because Windows.UI.Composition does not have a Circle visual, we will 
            // work around by using a circular opacity mask
            // Create a simple Composite Effect, using DestinationIn (S * DA), 
            // with a color source and a named parameter source.
            //
            var effect = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new ColorSourceEffect()
                    {
                        Color = color
                    },
                    new CompositionEffectSourceParameter("mask")
                }

            };
            var factory = _compositor.CreateEffectFactory(effect);
            var brush = factory.CreateBrush();

            //
            // Create the mask brush using the circle mask
            //

            brush.SetSourceParameter("mask", _circleMaskSurface.Brush);

            return brush;

        }

        /// <summary>
        /// Creates an animation template for a "color bloom" type effect on a circular colored visual.
        /// This is a sub-second animation on the Scale property of the visual.
        /// 
        /// <param name="initialRadius">the Radius of the circular visual</param>
        /// <param name="finalBounds">the final area to occupy</param>
        /// </summary>
        private void InitializeBloomAnimation(float initialRadius, Rect finalBounds)
        {
            var maxWidth = finalBounds.Width;
            var maxHeight = finalBounds.Height;

            // when fully scaled, the circle must cover the entire viewport
            // so we use the window's diagonal width as our max radius, assuming 0,0 placement
            var maxRadius = (float)Math.Sqrt((maxWidth * maxWidth) + (maxHeight * maxHeight)); // hypotenuse

            // the scale factor is the ratio of the max radius to the original radius
            var scaleFactor = (float)Math.Round(maxRadius / initialRadius, MidpointRounding.AwayFromZero);


            var bloomEase = _compositor.CreateCubicBezierEasingFunction(  //these numbers seem to give a consistent circle even on small sized windows
                    new Vector2(0.1f, 0.4f),
                    new Vector2(0.99f, 0.65f)
                );
            _bloomAnimation = _compositor.CreateScalarKeyFrameAnimation();
            _bloomAnimation.InsertKeyFrame(1.0f, scaleFactor, bloomEase);
            _bloomAnimation.Duration = TimeSpan.FromMilliseconds(800); // keeping this under a sec to not be obtrusive

        }

        /// <summary>
        /// Runs the animation
        /// </summary>
        private void TriggerBloomAnimation(SpriteVisual colorVisual)
        {

            // animate the Scale of the visual within a scoped batch
            // this gives us transactionality and allows us to do work once the transaction completes
            var batchTransaction = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            // as with all animations on Visuals, these too will run independent of the UI thread
            // so if the UI thread is busy with app code or doing layout on state/page transition,
            // these animations still run uninterruped and glitch free
            colorVisual.StartAnimation("Scale.X", _bloomAnimation);
            colorVisual.StartAnimation("Scale.Y", _bloomAnimation);

            batchTransaction.Completed += (sender, args) =>
            {
                // remove this visual from visual tree
                _containerForVisuals.Children.Remove(colorVisual);

                // notify interested parties
                ColorBloomTransitionCompleted(this, EventArgs.Empty);
            };
            
            batchTransaction.End();

        }
        #endregion

    }
}
