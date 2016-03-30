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
using Microsoft.UI.Composition.Toolkit;

namespace Effect
{
    public sealed class CompositeEffect : IFrameworkView
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
            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);
            Composite();
        }

        public void Composite()
        {

            // Create the graphics effect          
           var graphicsEffect = new ArithmeticCompositeEffect
            {
                Source1 = new CompositionEffectSourceParameter("source1"),
                Source2 = new SaturationEffect
                {
                    Saturation = 0,
                    Source = new CompositionEffectSourceParameter("source2")
                },
                MultiplyAmount = 0,
                Source1Amount = 0.5f,
                Source2Amount = 0.5f,
                Offset = 0
            };

            // Compile the effect          
            var effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

            // Create and setup an instance of the effect for the visual          
            
            CompositionSurfaceBrush surfaceBrush1 = _compositor.CreateSurfaceBrush();
            LoadImage(surfaceBrush1, new Uri("ms-appx:///Assets/cat.png"));


            CompositionSurfaceBrush surfaceBrush2 = _compositor.CreateSurfaceBrush();
            LoadImage(surfaceBrush2, new Uri("ms-appx:///Assets/dog.png"));            

            var effect = effectFactory.CreateBrush();
            effect.SetSourceParameter("source1", surfaceBrush1);
            effect.SetSourceParameter("source2", surfaceBrush2);

            // Create the visual and add it to the composition tree          
            var visual = _compositor.CreateSpriteVisual();  
            visual.Brush = effect;
            visual.Size = new Vector2(219, 300);
            _root.Children.InsertAtBottom(visual);
        }

        private void LoadImage(CompositionSurfaceBrush brush, Uri uri)
        {
            // Create an image source to load
            CompositionImage imageSource = _imageFactory.CreateImageFromUri(uri);
            brush.Surface = imageSource.Surface;
        }

        // CoreWindow / CoreApplicationView
        private CoreWindow _window;
        private CoreApplicationView _view;

        // Windows.UI.Composition
        private Compositor _compositor;
        private ContainerVisual _root;
        private CompositionTarget _target;

        // Image Loading 
        private CompositionImageFactory _imageFactory;

    }

    public sealed class EffectFactory : IFrameworkViewSource
    {
        IFrameworkView IFrameworkViewSource.CreateView()
        {
            return new CompositeEffect();
        }

        static void Main(string[] args)
        {
            CoreApplication.Run(new EffectFactory());
        }
    }
}



