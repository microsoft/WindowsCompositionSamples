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


namespace Effect
{
    public sealed class DesaturationEffect : IFrameworkView
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
            Desaturate();
        }
        
        public void Desaturate()
        {
            // Create and setup an instance of the effect for the visual          
            var graphicsDevice = _compositor.DefaultGraphicsDevice;
            var catImage = graphicsDevice.CreateImageFromUri(new Uri("ms-appx:///Assets/cat.png"));

            // Create the graphics effect          
            var graphicsEffect = new SaturationEffect
            {
                Saturation = 0.0f,
                Source = new CompositionEffectSourceParameter("mySource")
            };

            // Compile the effect          
            var effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

            var catEffect = effectFactory.CreateEffect();
            catEffect.SetSourceParameter("mySource", catImage);

            // Create the visual and add it to the composition tree         
            var catVisual = _compositor.CreateEffectVisual();
            catVisual.Effect = catEffect;
            catVisual.Size = new Vector2(219, 300);
            _root.Children.InsertAtBottom(catVisual);
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
            return new DesaturationEffect();
        }

        static void Main(string[] args)
        {
            CoreApplication.Run(new EffectFactory());
        }
    }
}



