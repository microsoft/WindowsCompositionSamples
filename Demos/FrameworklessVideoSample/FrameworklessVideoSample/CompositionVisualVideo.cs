
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
using Windows.Media.Core;
using Windows.Media.Playback;

namespace CompositionVisualVideo
{
    class VideoPlayer : IFrameworkView
    {
        // Media set up

        //------------------------------------------------------------------------------
        //
        // StartPlayback
        //
        // News up a MediaPlayer, creates a playback item then 
        // MediaPlayer hands out a surface that can be put on brush
        // We call this below when we set up the tree init the composition
        //
        //------------------------------------------------------------------------------
        private void StartPlayback()
        {


            // MediaPlayer set up with a create from URI
            _mediaPlayer = new MediaPlayer();

            // Get a source from a URI. This could also be from a file via a picker or a stream

            var source = MediaSource.CreateFromUri(new Uri("http://go.microsoft.com/fwlink/?LinkID=809007&clcid=0x409"));
            var item = new MediaPlaybackItem(source);
            _mediaPlayer.Source = item;

            // MediaPlayer supports many of the starndard MediaElement vars like looping
            _mediaPlayer.IsLoopingEnabled = true;

            // Get the surface from MediaPlayer and put it on a brush
            _videoSurface = _mediaPlayer.GetSurface(_compositor);
            _videoVisual.Brush = _compositor.CreateSurfaceBrush(_videoSurface.CompositionSurface);

            // Play the video on app run.

            PlayVideo();

        }

        //------------------------------------------------------------------------------
        //
        // Play and Pause
        //
        // Sets up private PlayVideo() and PauseVideo() methods wrapping public MediaPlayer methods
        // which match MediaElement methods. We only use PlayVideo() in this sample
        //
        //------------------------------------------------------------------------------
        private void PlayVideo()
        {
            // MediaPlayer public play method. By wrapping this in a private you can add behaviors. 
            _mediaPlayer.Play();
        }

        private void PauseVideo()
        {
            // MediaPlayer pause method. By wrapping this you can add behaviors later 
            _mediaPlayer.Pause();
        }


        // Frameworkless App Set Up: tree and visuals including those that host video

        //------------------------------------------------------------------------------
        //
        // VideoPlayer.Initialize
        //
        // This method is called during startup to associate the IFrameworkView with the
        // CoreApplicationView.
        //
        //------------------------------------------------------------------------------

        void IFrameworkView.Initialize(CoreApplicationView view)
        {
            _view = view;

        }

        //------------------------------------------------------------------------------
        //
        // VideoPlayer.SetWindow
        //
        // This method is called when the CoreApplication has created a new CoreWindow,
        // allowing the application to configure the window and start producing content
        // to display.
        //
        //------------------------------------------------------------------------------


        void IFrameworkView.SetWindow(CoreWindow window)
        {
            _window = window;
            InitNewComposition();

        }


        //------------------------------------------------------------------------------
        //
        // VideoPlayer.Load
        //
        // This method is called when a specific page is being loaded in the
        // application.  It is not used for this application.
        //
        //------------------------------------------------------------------------------

        void IFrameworkView.Load(string unused)
        {

        }

        //------------------------------------------------------------------------------
        //
        // VideoPlayer.Run
        //
        // This method is called by CoreApplication.Run() to actually run the
        // dispatcher's message pump.
        //
        //------------------------------------------------------------------------------

        void IFrameworkView.Run()
        {
            _window.Activate();
            _window.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
        }

        //------------------------------------------------------------------------------
        //
        // VideoPlayer.Uninitialize
        //
        // This method is called during shutdown to disconnect the CoreApplicationView,
        // and CoreWindow from the IFrameworkView.
        //
        //------------------------------------------------------------------------------

        void IFrameworkView.Uninitialize()
        {
            _window = null;
            _view = null;
        }

        //------------------------------------------------------------------------------
        //
        // VideoPlayer.InitNewComposition
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

            _compositionTarget = _compositor.CreateTargetForCurrentView();
            _compositionTarget.Root = _root;


            // Create a few visuals for our window

            {
                _root.Children.InsertAtTop(CreateChildElement());
            }


        }


        //------------------------------------------------------------------------------
        //
        // VideoPlayer.CreateChildElement
        //
        // Creates a child to represent a visible element in our application then hangs
        // a SpriteVisual videoHost off it to host the brush that holds the video.
        //
        //------------------------------------------------------------------------------

        Visual CreateChildElement()
        {
            // ContainerVisual to allow for a simple tree.

            var element = _compositor.CreateContainerVisual();
            element.Size = new Vector2(100.0f, 100.0f);

            //
            // Position this visual within our window
            //
            element.Offset = new Vector3(100f, 100f, 0f);

            // Create the child SpriteVisual to host video. It's size is the video size.
            // Puts it in the tree then adds the brush with the video surface.

            var videoHost = _compositor.CreateSpriteVisual();
            videoHost.Size = new Vector2(600.0f, 500.0f);
            element.Children.InsertAtTop(videoHost);
            _videoVisual = videoHost;

            // This calls the method that wraps all of playback business

            StartPlayback();


            return element;
        }



        // CoreWindow / CoreApplicationView
        private CoreWindow _window;
        private CoreApplicationView _view;

        // Windows.UI.Composition
        private Compositor _compositor;
        private CompositionTarget _compositionTarget;
        private ContainerVisual _root;

        // Media Helpers
        private MediaPlayer _mediaPlayer;
        private SpriteVisual _videoVisual;
        private MediaPlayerSurface _videoSurface;

    }


    public sealed class VideoPlayerFactory : IFrameworkViewSource
    {
        //------------------------------------------------------------------------------
        //
        // VideoPlayerFactory.CreateView
        //
        // This method is called by CoreApplication to provide a new IFrameworkView for
        // a CoreWindow that is being created.
        //
        //------------------------------------------------------------------------------

        IFrameworkView IFrameworkViewSource.CreateView()
        {
            return new VideoPlayer();
        }


        //------------------------------------------------------------------------------
        // main
        // Application main
        //
        //------------------------------------------------------------------------------

        static int Main(string[] args)
        {
            CoreApplication.Run(new VideoPlayerFactory());

            return 0;
        }
    }
}

