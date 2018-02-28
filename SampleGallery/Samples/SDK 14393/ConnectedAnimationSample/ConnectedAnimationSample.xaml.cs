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
using System;
using System.Linq;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
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
                    var item = Model.Landscapes.Where(compare => compare.ImageUrl == _navigatedUri).First();

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
            connectedAnimationService.DefaultDuration = TimeSpan.FromSeconds(0.8);
            connectedAnimationService.DefaultEasingFunction = _compositor.CreateCubicBezierEasingFunction(
                new Vector2(0.41f, 0.52f),
                new Vector2(0.00f, 0.94f)
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
