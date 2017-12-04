using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    // Defines the protocol needed for an abstract sample hosting UI:
    // - Setting the list of sample categories
    // - Navigating to arbitrary UI
    // - Interacting with the navigation back stack
    // - Refreshing the current sample when the current composition capabilities have changed
    public interface ISampleGalleryUIHost
    {
        object SampleCategories { get; set; }

        bool CanGoBack { get; }

        void GoBack();

        event EventHandler BackStackStateChanged;

        void Navigate(Type type, object parameter);
        
        void NotifyCompositionCapabilitiesChanged(bool areEffectsSupported, bool areEffectsFast);
    }


    public enum UIType
    {
        Auto,     // Automatically choose a Pivot or NavigationView depending on the current platform
        NavView,  // Force a NavigationView view to be loaded
        Pivot     // Force a Pivot view to be loaded
    }

    // This control allows a level of indirection so that we can host our samples within either a Pivot control on downlevel
    // platforms, or a NavigationView control if it is available.
    // 
    // The differences between these two controls are abstracted via the ISampleGalleryUIHost interface,
    // which is how external code should interface with this control.
    public class SampleGalleryUIIndirector : UserControl, ISampleGalleryUIHost
    {
        ISampleGalleryUIHost _actualContent;
        private object _sampleCategories;

        public SampleGalleryUIIndirector()
        {
        }

        private void LoadUI(UIType type)
        {
            // Auto-detect the type to load if requested
            if (type == UIType.Auto)
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
                {
                    type = UIType.NavView;
                }
                else
                {
                    type = UIType.Pivot;
                }
            }

            // Save off any properties that are stashed on the UI itself and need to be 
            // forwarded on to the new UI, load it, and then reapply the saved properties
            object oldItemsSource = SampleCategories;


            UIElement actualContent;
            if (type == UIType.Pivot)
            {
                actualContent = new SampleGalleryPivotHost();
            }
            else
            {
                actualContent = new SampleGalleryNavViewHost();
            }


            Content = actualContent;
            _actualContent = (ISampleGalleryUIHost)actualContent;


            SampleCategories = oldItemsSource;
        }


        // Convenience helper for forcing into Pivot mode for testing
        public UIType UIType
        {
            set
            {
                LoadUI(value);
            }
        }

        public object SampleCategories
        {
            get
            {
                return _sampleCategories;
            }
            set
            {
                _sampleCategories = value;
                if (_actualContent != null)
                {
                    _actualContent.SampleCategories = value;
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                return _actualContent.CanGoBack;
            }
        }

        public void GoBack()
        {
            _actualContent.GoBack();
        }

        public event EventHandler BackStackStateChanged
        {
            add
            {
                _actualContent.BackStackStateChanged += value;
            }
            remove
            {
                _actualContent.BackStackStateChanged -= value;
            }
        }

        public void Navigate(Type type, object parameter)
        {
            _actualContent.Navigate(type, parameter);
        }

        public void NotifyCompositionCapabilitiesChanged(bool areEffectsSupported, bool areEffectsFast)
        {
            _actualContent.NotifyCompositionCapabilitiesChanged(areEffectsSupported, areEffectsFast);
        }
    }
}
