using CompositionSampleGallery.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace CompositionSampleGallery
{

    public sealed partial class ConnectedAnimationSample : Page
    {
        public LocalDataSource Model { get; } = new LocalDataSource();

        static string _navigatedUri;
        static bool _usingCustomParameters;

        Compositor _compositor;


        public ConnectedAnimationSample()
        {
            InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            CustomParametersCheckBox.IsChecked = _usingCustomParameters;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Don't use vertical entrance animation with connected animation
            if (e.NavigationMode == NavigationMode.Back)
            {
                EntranceTransition.FromVerticalOffset = 0;
            }

            //Hide the back button on the list page as there is no where to go back to. 
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void ItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var container = ItemsGridView.ContainerFromItem(e.ClickedItem) as GridViewItem;
            if (container != null)
            {
                var root = (FrameworkElement)container.ContentTemplateRoot;
                var image = (UIElement)root.FindName("Image");

                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Image", image);
            }

            var item = (Thumbnail)e.ClickedItem;

            // Add a fade out effect
            Transitions = new TransitionCollection();
            Transitions.Add(new ContentThemeTransition());

            Frame.Navigate(typeof(ConnectedAnimationDetail), _navigatedUri = item.ImageUrl);
        }

        private void ItemsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_navigatedUri != null)
            {
                // May be able to perform backwards Connected Animation
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("Image");
                if (animation != null)
                {
                    var item = Model.Items.Where(compare => compare.ImageUrl == _navigatedUri).First();

                    ItemsGridView.ScrollIntoView(item, ScrollIntoViewAlignment.Default);
                    ItemsGridView.UpdateLayout();

                    var container = ItemsGridView.ContainerFromItem(item) as GridViewItem;
                    if (container != null)
                    {
                        var root = (FrameworkElement)container.ContentTemplateRoot;
                        var image = (Image)root.FindName("Image");

                        // Wait for image opened. In future Insider Preview releases, this won't be necessary.
                        image.Opacity = 0;
                        image.ImageOpened += (sender_, e_) =>
                        {
                            image.Opacity = 1;
                            animation.TryStart(image);
                        };
                    }
                    else
                    {
                        animation.Cancel();
                    }
                }

                _navigatedUri = null;
            }
        }

        private void CustomParametersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
            connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.5);
            connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.42f, 0.0f),
                new Vector2(0.58f, 1.0f)
                );

            _usingCustomParameters = true;
        }

        private void CustomParametersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // These are the default values for ConnectedAnimationService in Windows 10 Anniversary
            var connectedAnimationService = ConnectedAnimationService.GetForCurrentView();
            connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.33);
            connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.3f, 0.3f),
                new Vector2(0.0f, 1.0f)
                );

            _usingCustomParameters = false;
        }
    }
}
