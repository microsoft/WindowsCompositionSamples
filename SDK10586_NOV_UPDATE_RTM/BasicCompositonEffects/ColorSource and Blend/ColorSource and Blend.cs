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
using Windows.UI;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Composition.Toolkit;

namespace Effect
{
    public sealed class ColorSource : IFrameworkView
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

        //------------------------------------------------------------------------------
        //
        // VisualProperties.InitNewComposition
        //
        // This method is called by SetWindow(), where we initialize Composition after
        // the CoreWindow has been created.
        //
        //------------------------------------------------------------------------------

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
            TintedBrush();
        }
        
        public void TintedBrush()
        {
            // Create the graphics effect          
           var graphicsEffect = new BlendEffect
            {
                Mode = BlendEffectMode.Multiply,
                Background = new CompositionEffectSourceParameter("image"),
                Foreground = new ColorSourceEffect
                {
                    Name = "colorSource",
                    Color = Color.FromArgb(255, 255, 255, 255)
                }
            };

            // Compile the effect, specifying that the color should be modifiable.          
            var effectFactory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "colorSource.Color" });

            // Load our image          

            CompositionSurfaceBrush surfaceBrush = _compositor.CreateSurfaceBrush();            
            LoadImage(surfaceBrush, new Uri("ms-appx:///Assets/cat.png"));
            
            // Create a visual with an instance of the effect that tints in red          
            var redEffect = effectFactory.CreateBrush();
            // Set parameters for image, which is not animatable          
            redEffect.SetSourceParameter("image", surfaceBrush);
            // Set properties for color, which is animatable          
            redEffect.Properties.InsertColor("colorSource.Color", Colors.Red);

            var redVisual = _compositor.CreateSpriteVisual();
            redVisual.Brush = redEffect;
            redVisual.Size = new Vector2(219, 300);
            _root.Children.InsertAtBottom(redVisual);

            // Create a visual with an instance of the effect that tints in blue          
            var blueEffect = effectFactory.CreateBrush();
            blueEffect.SetSourceParameter("image", surfaceBrush);
            blueEffect.Properties.InsertColor("colorSource.Color", Colors.Blue);

            var blueVisual = _compositor.CreateSpriteVisual();
            blueVisual.Brush = blueEffect;
            blueVisual.Offset = new Vector3(400, 0, 0);
            blueVisual.Size = new Vector2(219, 300);
            _root.Children.InsertAtBottom(blueVisual);
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
            return new ColorSource();
        }

        static void Main(string[] args)
        {
            CoreApplication.Run(new EffectFactory());
        }
    }
}


