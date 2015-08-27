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
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI.Composition;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas;

namespace Effect
{
    public sealed class MaskEffect : IFrameworkView
    {
        void IFrameworkView.Initialize(CoreApplicationView view)
        {
            _view = view;
        }

        void IFrameworkView.SetWindow(CoreWindow window)
        {
            _window = window;
            InitNewComposition();
        }

        void IFrameworkView.Load(string unused) { }

        void IFrameworkView.Run()
        {
            _window.Activate();
            _window.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
        }

        void IFrameworkView.Uninitialize()
        {
            _window = null;
            _view = null;
        }


        void InitNewComposition()
        {
            //
            // Set up Windows.UI.Composition Compositor, root ContainerVisual, and associate with
            // the CoreWindow.
            //

            _compositor = new Compositor();
            _root = _compositor.CreateContainerVisual();
            _target = _compositor.CreateTargetForCurrentView();
            _target.Root = _root;
            Mask();
        }


        public void Mask()
        {
            // Create the graphics effect          
            var graphicsEffect = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new CompositionEffectSourceParameter("image"),
                    new Transform2DEffect
                    {
                        Name = "maskTransform",
                        Source = new CompositionEffectSourceParameter("mask")
                    }
                }
            };

            // Compile the effect, specifying that the color should be modifiable.          
            var effectFactory = _compositor.CreateEffectFactory(
                graphicsEffect, new[] { "maskTransform.TransformMatrix" });

            // Create our effect and set its inputs          
            var effect = effectFactory.CreateEffect();
            var graphicsDevice = _compositor.DefaultGraphicsDevice;

            var profilePicture = graphicsDevice.CreateImageFromUri(
                new Uri("ms-appx:///Assets/dog.png"));
            effect.SetSourceParameter("image", profilePicture);

            var circleMaskImage = graphicsDevice.CreateImageFromUri(
                new Uri("ms-appx:///Assets/CircleMask.png"));
            effect.SetSourceParameter("mask", circleMaskImage);

            // Create a property set holding the scale we'll be animating          
            var propertySet = _compositor.CreatePropertySet();
            propertySet.InsertScalar("size", 0.0f);

            // Define the scale's animation          
            var scaleAnimation = _compositor.CreateScalarKeyFrameAnimation();
            var linearEasing = _compositor.CreateLinearEasingFunction();
            scaleAnimation.InsertKeyFrame(0.0f, 0.0f, linearEasing);
            scaleAnimation.InsertKeyFrame(1.0f, 1.0f, linearEasing);
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(5000);
            propertySet.ConnectAnimation("size", scaleAnimation).Start();

            // Generate a Matrix4x4 from the size and bind it to the transform matrix          
            var transformExpression = _compositor.CreateExpressionAnimation(
            "Matrix3x2(props.size, 0, 0, props.size, 0, 0)");
            transformExpression.SetReferenceParameter("props", propertySet);
            effect.Properties.ConnectAnimation("maskTransform.TransformMatrix", transformExpression);

            // Create a visual with an instance of the effect          
            var visual = _compositor.CreateEffectVisual();
            visual.Effect = effect;
            visual.Size = new Vector2(219, 229);
            _root.Children.InsertAtBottom(visual);
        }

        // CoreWindow / CoreApplicationView
        private CoreWindow _window;
        private CoreApplicationView _view;

        // Windows.UI.Composition
        private Compositor _compositor;
        private ContainerVisual _root;
        private CompositionTarget _target;
    }

    public sealed class EffectFactory : IFrameworkViewSource
    {
        IFrameworkView IFrameworkViewSource.CreateView()
        {
            return new MaskEffect();
        }

        static void Main(string[] args)
        {
            CoreApplication.Run(new EffectFactory());
        }
    }
}



