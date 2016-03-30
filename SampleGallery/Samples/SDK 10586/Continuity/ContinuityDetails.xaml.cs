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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using System;
using Windows.UI.Core;

namespace CompositionSampleGallery
{
    public sealed partial class ContinuityDetails : Page
    {
        private ContinuityTransition
                                    _currentTransition;
        private Continuity.DetailsInfo
                                    _detailsInfo;
        private SampleHost          _host;

        public ContinuityDetails()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _currentTransition = (ContinuityTransition)e.Parameter;
            _detailsInfo = (Continuity.DetailsInfo)_currentTransition.Payload;
            _host = (SampleHost)_currentTransition.Host;

            Title.Text = _detailsInfo.thumbanil.Name;
            DetailText.Text = _detailsInfo.thumbanil.Description;

            // Enable the back button
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = _host.ContentFrame.CanGoBack ?
                                                AppViewBackButtonVisibility.Visible : 
                                                AppViewBackButtonVisibility.Collapsed;
            
            SystemNavigationManager.GetForCurrentView().BackRequested += ContinuityDetails_BackRequested;
        }

        private void ContinuityDetails_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                // Unregister the handler
                SystemNavigationManager.GetForCurrentView().BackRequested -= ContinuityDetails_BackRequested;

                // We are about to transition to a new page.  Cancel any outstanding transitions.
                if (_currentTransition != null)
                {
                    if (!_currentTransition.Completed)
                    {
                        _currentTransition.Cancel();
                    }
                    _currentTransition = null;
                }

                // Setup the new transition and trigger the navigation
                ContinuityTransition transition = new ContinuityTransition();
                transition.Initialize(_host, ThumbnailImage, _detailsInfo);
                _host.ContentFrame.Navigate(typeof(Continuity), transition);

                // We've got it handled
                e.Handled = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentTransition != null)
            {
                Continuity.DetailsInfo info = (Continuity.DetailsInfo)_currentTransition.Payload;
                
                // Update the Thumbnail image to point to the proper album art
                ThumbnailImage.Source = new Uri(info.thumbanil.ImageUrl);

                // Kick off the transition now that the page has loaded
                _currentTransition.Start(MyGrid, ThumbnailImage, MyScroller, MyScroller);
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
        }
    }
}
