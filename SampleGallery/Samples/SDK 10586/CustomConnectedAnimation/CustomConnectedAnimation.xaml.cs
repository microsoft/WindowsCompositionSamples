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
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class CustomConnectedAnimation : Page
    {
        private ConnectedTransition    _currentTransition;

        public struct ContinuityData
        {
            public SpriteVisual sprite;
            public Frame        frame;
            public UIElement    parent;
            public CompositionImage image;
        }

        public struct DetailsInfo
        {
            public Thumbnail thumbanil;
        }

        public CustomConnectedAnimation()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

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
            ConnectedTransition transition = new ConnectedTransition();
            transition.Initialize(Frame, image, info);

            Frame.Navigate(typeof(CustomConnectedAnimationDetail), transition);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Store the incoming parameter
            if (e.Parameter is ConnectedTransition)
            {
                _currentTransition = (ConnectedTransition)e.Parameter;
            }
            else
            {
                // Should not run ConnectedTransition
                _currentTransition = null;
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
