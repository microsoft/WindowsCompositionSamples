using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CompositionSampleGallery
{
    public sealed partial class SampleGalleryNavViewHost : UserControl, ISampleGalleryUIHost
    {
        object _itemsSource;

        public SampleGalleryNavViewHost()
        {
            this.InitializeComponent();
        }

        public object SampleCategories
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                NavView.MenuItems.Clear();
                _itemsSource = value;

                // Wrap each NavigationItem with a corresponding NavigationViewItem so that it can 
                // have its text and icon displayed appropriately.
                if (_itemsSource != null)
                {
                    List<NavigationViewItem> newItems = new List<NavigationViewItem>();
                    foreach (NavigationItem item in (ICollection)value)
                    {
                        NavigationViewItem navItem = new NavigationViewItem();
                        navItem.Content = item.DisplayName;
                        BitmapIcon icon = new BitmapIcon();

                        if (!String.IsNullOrEmpty(item.ThumbnailUri))
                        {
                            icon.UriSource = new Uri(item.ThumbnailUri);
                            navItem.Icon = icon;
                        }

                        navItem.DataContext = item;

                        NavView.MenuItems.Add(navItem);
                    }
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                return ContentFrame.CanGoBack;
            }
        }

        public void GoBack()
        {
            ContentFrame.GoBack();
            FireBackStackChangedEvent();
        }

        public event EventHandler BackStackStateChanged;

        public void Navigate(Type type, object parameter)
        {
            ContentFrame.Navigate(type, parameter);
            FireBackStackChangedEvent();
        }

        public void NotifyCompositionCapabilitiesChanged(bool areEffectsSupported, bool areEffectsFast)
        {
            if (ContentFrame.Content is SampleHost host)
            {
                SamplePage page = (SamplePage)host.Content;
                page.OnCapabiliesChanged(areEffectsSupported, areEffectsFast);
            }
        }

        private void FireBackStackChangedEvent()
        {
            BackStackStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            NavigateToSelectedItem();
        }

        private void NavViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Navigate(typeof(Settings), null);
                ClearBackstack();
            }
            else
            {
                // The NavigationViewItemInvokedEventArgs only give us the string label of the item that was invoked.  Rather than writing
                // awkward code to string compare the arg against each category name, we instead can just fetch the selected item
                // off of the navigation view.  However, if the selection is actually changing as a result of this invocation, the NavView doesn't
                // reflect that change until *after* this event has fired.  So, use the thread's DispatcherQueue to defer the navigation until 
                // after the nav view has processed the potential selection change.

                DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.High, () => { NavigateToSelectedItem(); } );                
            }
        }

        private void NavigateToSelectedItem()
        {
            NavigationItem navItem = (NavigationItem)((NavigationViewItem)NavView.SelectedItem).DataContext;

            Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            properties.Add("TargetView", navItem.Category.ToString());
            Shared.AppTelemetryClient.TrackEvent("Navigate", properties, null);
            Navigate(navItem.PageType, navItem);
            ClearBackstack();
        }

        private void ClearBackstack()
        {
            // Reset the backstack when a new category is selected to avoid having to coordinate the cateogory 
            // selection as we navigate back through the backstack
            ContentFrame.BackStack.Clear();
            FireBackStackChangedEvent();
        }
    }
}
