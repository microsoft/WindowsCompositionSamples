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
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class MainNavigation : UserControl
    {

        // Category description text
        public const string CategoryDescritpion_SeamlessTransitions = @"How do you delight your users and focus their attention across property changes, page navigations, and layout changes?
                                                                       
Transitioning content across pages and choreographing motion lets you smooth out the experiences and help immerse your customers.";
        public const string CategoryDescription_DynamicHumanInteractions =
            @"When a customer touches, clicks, or pans, how do you make the interaction with your app feel personal and enjoyable?

The Windows UI Platform lets you have natural feeling interactions out of the box, as well as letting you control all aspects of how things look and feel in response to input.";
        public const string CategoryDescription_RealWorldUI =
            @"How do you build an interface that looks natural and intuitive to people who are accustom to living in the real three dimensional world?

By using Lights, Shadows, and responsive Materials, you are empowered to create objects that feel familiar and bridge the gap to the natural physical world.";
        public const string CategoryDescription_ContextualUI =
            @"How do you convey the context, importance, and hierarchy of information to drive focus and to help your customers find their way through your app?

By using effects, depth, and perspective you can not only help your customers navigate the mess, but you can captivate their attention.";
        public const string CategoryDescription_NaturalMotion =
            @"How do you breathe life into your app and make it feel natural to a real human being?

By using Expressions, constructs like springs, and directly animating velocity/acceleration, your can bring experiences to life with motion that feels real and fits with the world we actually live in.";
        public const string CategoryDescription_Conceptual = "A collection of samples that illustrate conceptual topics.";


        void AddNavigationItem(ref List<NavigationItem> menu, String displayName, SampleCategory cat, Type pageType, string categoryDescription="")
        {
            var samples = from sample in SampleDefinitions.Definitions
                          where (sample.SampleCategory == cat)
                          select sample;

            if (samples.Count<SampleDefinition>() > 0)
            {
                menu.Add(new NavigationItem(displayName, cat, pageType, categoryDescription: categoryDescription));
            }
        }

        public MainNavigation()
        {
            this.InitializeComponent();

            // Build a collection used to populate the navigation menu. This is where you can define the display names of
            // each menu item and which page they map to.
            List<NavigationItem> mainMenuList = new List<NavigationItem>();
            mainMenuList.Add(new NavigationItem("Home", SampleCategory.Conceptual /* unused */, typeof(HomePage)));
            AddNavigationItem(ref mainMenuList, "Seamless Transitions",      SampleCategory.SeamlessTransitions,      typeof(BaseCategoryPage),         categoryDescription: CategoryDescritpion_SeamlessTransitions);
            AddNavigationItem(ref mainMenuList, "Dynamic Human Interaction", SampleCategory.DynamicHumanInteractions, typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_DynamicHumanInteractions);
            AddNavigationItem(ref mainMenuList, "Real-World UI",             SampleCategory.RealWorldUI,              typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_RealWorldUI);
            AddNavigationItem(ref mainMenuList, "Contextual UI",             SampleCategory.ContextualUI,             typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_ContextualUI);
            AddNavigationItem(ref mainMenuList, "Natural Motion",            SampleCategory.NaturalMotion,            typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_NaturalMotion);
            AddNavigationItem(ref mainMenuList, "Conceptual",                SampleCategory.Conceptual,               typeof(BaseCategoryPage),         categoryDescription: CategoryDescription_Conceptual);

            CategoriesListView.ItemsSource = mainMenuList;
        }

        private void CategoriesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // navigate to the corresonding page
            NavigationItem navItem = e.ClickedItem as NavigationItem;
            if (navItem != null)
            {
                MainPage.Instance.NavigateToPage(navItem.PageType, navItem);
            }
        }
    }

    public class NavigationItem
    {
        private string _displayName, _featuredSamplesTitle;
        private Type _pageType;
        private SampleCategory _cat;
        private string _categoryDescription;

        public string DisplayName {get {return _displayName;} set { _displayName = value; }}
        public Type PageType {  get { return _pageType; } set { _pageType = value; } }
        public SampleCategory Category { get { return _cat; } set { _cat = value; } }
        public string FeaturedSamplesTitle { get { return _featuredSamplesTitle; } set { _featuredSamplesTitle = value; } }
        public string CategoryDescription { get { return _categoryDescription; } }
        public NavigationItem(string displayName, SampleCategory cat, Type pageType, string featuredSamplesTitle = "", string categoryDescription="")
        {
            _displayName = displayName;
            _pageType = pageType;
            _cat = cat;
            _featuredSamplesTitle = featuredSamplesTitle;
            _categoryDescription = categoryDescription;
        }
    }
}
