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

using CompositionSampleGallery.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public class MainNavigationViewModel
    {
        private static MainNavigationViewModel s_instance;
        public ISampleGalleryUIHost _hostingUI;
        private List<NavigationItem> _mainMenuList;

        // Category description text
        public const string CategoryDescription_Light = 
            @"Light has a way of drawing our attention. It’s warm and inviting; it’s fluid and purposeful. Light creates atmosphere and a sense of place, and it’s a practical tool to illuminate information.  These samples show some examples of bringing Light into your UI.";
        public const string CategoryDescription_Depth =
            @"Think about the frame that contains your information. Now break it apart, and reinvent how things relate to each other within a more layered, physical environment. This is how we’ll keep people in their flow – by giving them more space.  These samples show a variety of techniques for bringing the concept of Depth into your UI.";
        public const string CategoryDescription_Motion =
            @"Think of motion design like a movie. Seamless transitions keep you focused on the story, and bring experiences to life. We can invite that feeling into our designs, leading people from one task to the next with cinematic ease.  These samples show different ways in which motion can enhance your UI.";
        public const string CategoryDescription_Material =
            @"The things that surround us in the real world are sensory and invigorating. They bend, stretch, bounce, shatter, and glide. Those material qualities translate to digital environments, making people want to reach out and touch our designs.  These samples show how to bring new Materials in your UI.";
        public const string CategoryDescription_Scale =
            @"The industry lives and breathes 2D design. Now’s the time to expand our toolkit for more dimensions. We’re scaling our design system to work across devices, inviting innovation across new forms. And we’re looking to you to help us imagine this new world.  These samples show some building blocks for making custom UI that is tailored for different devices.";
        public const string CategoryDescription_APIReference =
            @"In addition to the samples that display the Fluent building blocks in UI, some simple API reference samples are provided to ramp up and learn about basic API capabilities.";


        void AddNavigationItem(
            List<NavigationItem> menu,
            String displayName,
            SampleCategory cat, 
            Type pageType, 
            string categoryDescription="", 
            bool addEvenIfNoMatchingSamples = false, 
            string thumbnail="")
        {
            var samples = from sample in SampleDefinitions.Definitions
                          where (sample.SampleCategory == cat)
                          select sample;

            if ((samples.Count<SampleDefinition>() > 0) || addEvenIfNoMatchingSamples)
            {
                menu.Add(new NavigationItem(displayName, cat, pageType, categoryDescription: categoryDescription, thumbnail:thumbnail));
            }
        }

        public MainNavigationViewModel(ISampleGalleryUIHost hostingUI)
        {
            _hostingUI = hostingUI;

            _hostingUI.BackStackStateChanged += (object sender, EventArgs args) =>
            {
                // Show or hide the global back button
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    _hostingUI.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;
            };

            SystemNavigationManager.GetForCurrentView().BackRequested += (object backSender, BackRequestedEventArgs backArgs) =>
            {
                _hostingUI.GoBack();
            };



            // Build a collection used to populate the navigation menu. This is where you can define the display names of
            // each menu item and which page they map to.
            _mainMenuList = new List<NavigationItem>();
            AddNavigationItem(_mainMenuList, "Home",            SampleCategory.None,                    typeof(HomePage),                 addEvenIfNoMatchingSamples: true,                         thumbnail: "ms-appx:///Assets/CategoryIcons/table_home_icon.png");
            AddNavigationItem(_mainMenuList, "Light",           SampleCategory.Light,                   typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Light,           thumbnail: "ms-appx:///Assets/CategoryIcons/table_light_icon_bw.png");
            AddNavigationItem(_mainMenuList, "Depth",           SampleCategory.Depth,                   typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Depth,           thumbnail: "ms-appx:///Assets/CategoryIcons/table_depth_icon_bw.png");
            AddNavigationItem(_mainMenuList, "Motion",          SampleCategory.Motion,                  typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Motion,          thumbnail: "ms-appx:///Assets/CategoryIcons/table_motion_icon_bw.png");
            AddNavigationItem(_mainMenuList, "Material",        SampleCategory.Material,                typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Material,        thumbnail: "ms-appx:///Assets/CategoryIcons/table_material_icon_bw.png");
            AddNavigationItem(_mainMenuList, "Scale",           SampleCategory.Scale,                   typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Scale,           thumbnail: "ms-appx:///Assets/CategoryIcons/table_scale_icon_bw.png");
            AddNavigationItem(_mainMenuList, "API Reference",   SampleCategory.APIReference,            typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_APIReference,    thumbnail: "ms-appx:///Assets/CategoryIcons/table_reference_icon.png");

            s_instance = this;
        }

        public List<NavigationItem> MainMenuList => _mainMenuList;

        public static void NavigateToSample(object sender, ItemClickEventArgs e)
        {
            NavigateToSample((SampleDefinition)e.ClickedItem);
        }

        public static void NavigateToSample(SampleDefinition sample)
        {            
            s_instance._hostingUI.Navigate(typeof(SampleHost), sample);
        }

        public static void ShowSearchResults(string queryText)
        {
            s_instance._hostingUI.Navigate(typeof(SearchResultsPage), queryText);
        }

        public static void ShowSettings()
        {
            s_instance._hostingUI.Navigate((typeof(Settings)), null);
        }
    }

    public class NavigationItem
    {
        private string _thumbnail;
        private string _displayName;
        private string _featuredSamplesTitle;
        private Type _pageType;
        private SampleCategory _cat;
        private string _categoryDescription;

        public string DisplayName {get {return _displayName;} set { _displayName = value; }}
        public Type PageType {  get { return _pageType; } set { _pageType = value; } }
        public SampleCategory Category { get { return _cat; } set { _cat = value; } }
        public string FeaturedSamplesTitle { get { return _featuredSamplesTitle; } set { _featuredSamplesTitle = value; } }
        public string CategoryDescription { get { return _categoryDescription; } }
        public string ThumbnailUri { get { return _thumbnail; } }
        public NavigationItem(
            string displayName, 
            SampleCategory cat, 
            Type pageType, 
            string categoryDescription, 
            string thumbnail)
        {
            _displayName = displayName;
            _pageType = pageType;
            _cat = cat;
            _featuredSamplesTitle = "";
            _categoryDescription = categoryDescription;
            _thumbnail = thumbnail;
        }
    }
}
