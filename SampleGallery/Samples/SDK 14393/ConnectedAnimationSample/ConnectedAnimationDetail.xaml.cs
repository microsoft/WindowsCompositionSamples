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
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CompositionSampleGallery
{
    public sealed partial class ConnectedAnimationDetail : Page
    {
        SystemNavigationManager _systemNavigationManager = SystemNavigationManager.GetForCurrentView();

        public ConnectedAnimationDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PhotoImage.Source = new BitmapImage(new Uri((string)e.Parameter));

            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("Image");
            if (animation != null)
            {
                PhotoImage.Opacity = 0;
                // Wait for image opened. In future Insider Preview releases, this won't be necessary.
                PhotoImage.ImageOpened += (sender_, e_) =>
                {
                    PhotoImage.Opacity = 1;
                    animation.TryStart(PhotoImage);
                };
            }
        }
    }
}
