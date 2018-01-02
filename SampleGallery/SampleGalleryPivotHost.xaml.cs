using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SampleGalleryPivotHost : ISampleGalleryUIHost
    {
        private Frame _currentItemFrame;

        public SampleGalleryPivotHost()
        {
            this.InitializeComponent();

#if SDKVERSION_16299
            // Apply a customized control template to the pivot
            MainPivot.Template = (ControlTemplate)Application.Current.Resources["PivotControlTemplate"];
#endif
        }

        public object SampleCategories
        {
            get
            {
                return MainPivot.ItemsSource;
            }
            set
            {
                MainPivot.ItemsSource = value;
            }
        }


        public event EventHandler BackStackStateChanged;

        public bool CanGoBack
        {
            get
            {
                return _currentItemFrame != null ? _currentItemFrame.CanGoBack : false;
            }
        }

        public void GoBack()
        {
            _currentItemFrame.GoBack();
            FireBackStackChangedEvent();
        }

        public void Navigate(Type type, object parameter)
        {
            _currentItemFrame?.Navigate(type, parameter);
            FireBackStackChangedEvent();
        }

        public void NotifyCompositionCapabilitiesChanged(bool areEffectsSupported, bool areEffectsFast)
        {
            if (_currentItemFrame != null)
            {
                if (_currentItemFrame.Content is SampleHost host)
                {
                    SamplePage page = (SamplePage)host.Content;
                    page.OnCapabiliesChanged(areEffectsSupported, areEffectsFast);
                }
            }
        }

        private void ItemFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // Cache a reference to the currently displayed item frame.  Note that
            // this will be a different frame instance per pivot item.
            _currentItemFrame = (Frame)sender;
        }


        // When navigating to a pivotitem, reload the main page and hide the back 
        // button
        private void MainPivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            NavigationItem navItem = (NavigationItem)((((PivotItemEventArgs)args).Item).DataContext);
            Frame pivotItemFrame = (Frame)(((PivotItem)args.Item).ContentTemplateRoot);
            
            pivotItemFrame.Navigate(navItem.PageType, navItem);
            pivotItemFrame.BackStack.Clear();

            FireBackStackChangedEvent();
        }

        // Load the category pages into the frame of each PivotItem
        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frame = (Frame)sender;
            NavigationItem navItem = (NavigationItem)(frame.DataContext);
            frame.Navigate(navItem.PageType, navItem);
            frame.BackStack.Clear();

            FireBackStackChangedEvent();
        }

        private void FireBackStackChangedEvent()
        {
            BackStackStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
