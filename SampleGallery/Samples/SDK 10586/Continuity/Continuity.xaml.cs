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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class Continuity : SamplePage
    {
        private SampleHost              _host;
        private ContinuityTransition    _currentTransition;

        public struct ContinuityData
        {
            public SpriteVisual sprite;
            public SampleHost   host;
            public UIElement    parent;
            public CompositionImage image;
        }

        public struct DetailsInfo
        {
            public Thumbnail thumbanil;
        }

        public Continuity()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public static string    StaticSampleName    { get { return "Continuity"; } }
        public override string  SampleName          { get { return StaticSampleName; } }
        public override string  SampleDescription   { get { return "Connected animations communicate context across page navigations. Click on one of the thumbnails and see it transition continuously across from one page navigate to another."; } }
        public override string  SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761164"; } }

        public LocalDataSource Model
        {
            get; set;
            }

        private void ThumbnailList_Click(object sender, ItemClickEventArgs e)
        {
            GridView gridView = (GridView)sender;
            GridViewItem item = (GridViewItem)gridView.ContainerFromItem(e.ClickedItem);
            CompositionImage image = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(item);

            // We are about to transition to a new page.  Cancel any outstanding transitions.
            if (_currentTransition != null)
            {
                if (!_currentTransition.Completed)
                {
                    _currentTransition.Cancel();
                }
                _currentTransition = null;
            }

            DetailsInfo info;
            info.thumbanil = (Thumbnail)e.ClickedItem;

            // Setup the new transition and trigger the navigation
            ContinuityTransition transition = new ContinuityTransition();
            transition.Initialize(_host, image, info);
            _host.ContentFrame.Navigate(typeof(ContinuityDetails), transition);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Store the incoming parameter
            if (e.Parameter is SampleHost)
            {
                _host = (SampleHost)e.Parameter;
            }
            else if (e.Parameter is ContinuityTransition)
            {
                _currentTransition = (ContinuityTransition)e.Parameter;
            }

            //Hide the back button on the list page as there is no where to go back to. 
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
        }

        private void ThumbnailList_Loaded(object sender, RoutedEventArgs args)
        {
            if (_currentTransition != null)
            {
                DetailsInfo info = (DetailsInfo)_currentTransition.Payload;
                GridViewItem item = (GridViewItem)ThumbnailList.ContainerFromItem(info.thumbanil);
                CompositionImage image = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(item);
                ScrollViewer scrollViewer = VisualTreeHelperExtensions.GetFirstDescendantOfType<ScrollViewer>(ThumbnailList);

                // Kick off the transition now that the page has loaded
                _currentTransition.Start(MyGrid, image, scrollViewer, ThumbnailList);
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }
    }
}
