using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

            _systemNavigationManager.BackRequested += ConnectedAnimationDetail_BackRequested;
            _systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void ConnectedAnimationDetail_BackRequested(object sender, BackRequestedEventArgs e)
        {
            _systemNavigationManager.BackRequested -= ConnectedAnimationDetail_BackRequested;
            _systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Image", PhotoImage);

            Frame.GoBack();
        }
    }
}
