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
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.Foundation;

namespace ContinuitySample
{
    /// <summary>
    /// Detail Page where Fruits details appear. 
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        //Variables used on this page
        Compositor _compositor;
        ContainerVisual _mainGridVisual;
        Item _parameterObject;
        Frame _rootFrame;
        public DetailsPage()
        {
            this.InitializeComponent();

           _mainGridVisual = ElementCompositionPreview.GetElementVisual(Window.Current.Content) as ContainerVisual;
            _compositor = _mainGridVisual.Compositor;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (null == e.Parameter)
                return;

            _rootFrame = Window.Current.Content as Frame;

            _parameterObject = e.Parameter as Item;
            DetailText.Text = _parameterObject.Description;

            //Add app back button to the page for user to go back. 
            if (_rootFrame.CanGoBack)
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            else
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += DetailsPage_BackRequested;
        }

        private void DetailsPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {

            if (_rootFrame.CanGoBack && !e.Handled)
            {
                //Set up visuals so that it is reparented to root frame and visible when pages change 
                ContainerVisual newContainerVisual;
                ContinuityHelper.SetupContinuity(_parameterObject.imageContainerVisual, HeroRectangle, out newContainerVisual);
                e.Handled = true;
                _parameterObject.imageContainerVisual = newContainerVisual;
                
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= DetailsPage_BackRequested;
                //Go back to previous page
                _rootFrame.GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContainerVisual tempConatinerVisual;
            //Run animations and re-parenting when page is loaded and elements are available on page. 
            ContinuityHelper.InitiateContinuity(HeroRectangle, _parameterObject.imageContainerVisual, out tempConatinerVisual);

            //set Application Parameters to move between pages. 
            _parameterObject.imageContainerVisual = tempConatinerVisual;
            var coordinate = HeroRectangle.TransformToVisual(Window.Current.Content);
            
            DetailText.Visibility = Visibility.Visible;
        }
    }
}
