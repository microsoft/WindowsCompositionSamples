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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;

using Microsoft.UI.Composition.Toolkit;

namespace CompositionImageSample
{
    class CompositionImageSample : IFrameworkView
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

        void IFrameworkView.Load(string unused)
        {

        }

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
            _compositor = new Compositor();

            _root = _compositor.CreateContainerVisual();

            _compositionTarget = _compositor.CreateTargetForCurrentView();
            _compositionTarget.Root = _root;

            CreateChildElement();
        }

        void CreateChildElement()
        {
            Uri localUri = new Uri("ms-appx:///Assets/StoreLogo.png");
            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);
            CompositionImageOptions options = new CompositionImageOptions()
            {
                DecodeWidth = 400,
                DecodeHeight = 400,
            };

            _image = _imageFactory.CreateImageFromUri(localUri, options);
            var visual = _compositor.CreateSpriteVisual();
            visual.Size = new Vector2(400.0f, 400.0f);
            visual.Brush = _compositor.CreateSurfaceBrush(_image.Surface);
            _root.Children.InsertAtTop(visual);

            // If for some reason the image fails to load, replace the brush with
            // a red solid color.
            _image.ImageLoaded += (CompositionImage sender, CompositionImageLoadStatus status) =>
            {
                if (status != CompositionImageLoadStatus.Success)
                {
                    visual.Brush = _compositor.CreateColorBrush(Colors.Red);
                }
            };
        }

        // CoreWindow / CoreApplicationView
        private CoreWindow _window;
        private CoreApplicationView _view;

        // Windows.UI.Composition
        private Compositor _compositor;
        private CompositionTarget _compositionTarget;
        private ContainerVisual _root;

        // Microsoft.UI.Composition.Toolkit
        private CompositionImageFactory _imageFactory;
        private CompositionImage _image;
    }


    public sealed class CompositionImageSampleFactory : IFrameworkViewSource
    {
        IFrameworkView IFrameworkViewSource.CreateView()
        {
            return new CompositionImageSample();
        }

        static int Main(string[] args)
        {
            CoreApplication.Run(new CompositionImageSampleFactory());

            return 0;
        }
    }
}
