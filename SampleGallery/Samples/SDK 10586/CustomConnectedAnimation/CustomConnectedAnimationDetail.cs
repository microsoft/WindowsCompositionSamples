﻿//*********************************************************
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
using Windows.UI.Core;

using Windows.Foundation;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class CustomConnectedAnimationDetail : Page
    {
        private ConnectedTransition
                                    _currentTransition;
        private CustomConnectedAnimation.DetailsInfo
                                    _detailsInfo;
        private Frame               _host;

        public CustomConnectedAnimationDetail()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _currentTransition = (ConnectedTransition)e.Parameter;
            _detailsInfo = (CustomConnectedAnimation.DetailsInfo)_currentTransition.Payload;
            _host = _currentTransition.Host as Frame;

            Title.Text = _detailsInfo.thumbanil.Name;
            DetailText.Text = _detailsInfo.thumbanil.Description;

            // Enable the back button
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = _host.CanGoBack ?
                                                AppViewBackButtonVisibility.Visible : 
                                                AppViewBackButtonVisibility.Collapsed;
            
            SystemNavigationManager.GetForCurrentView().BackRequested += CustomConnectedAnimationDetail_BackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Unregister the handler
            SystemNavigationManager manager = SystemNavigationManager.GetForCurrentView();
            manager.BackRequested -= CustomConnectedAnimationDetail_BackRequested;
            manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void CustomConnectedAnimationDetail_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
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
                ConnectedTransition transition = new ConnectedTransition();
                transition.Initialize(_host, ThumbnailImage, _detailsInfo);

                _host.Navigate(typeof(CustomConnectedAnimation), transition);

                // We've got it handled
                e.Handled = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentTransition != null)
            {
                CustomConnectedAnimation.DetailsInfo info = (CustomConnectedAnimation.DetailsInfo)_currentTransition.Payload;
                
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
