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

namespace CompositionSampleGallery.Pages
{

    public sealed partial class SearchResultsPage : Page
    {
        public SearchResultsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var searchText = (string)e.Parameter;

            // Name matches
            var nameMatches = from sampleDef in SampleDefinitions.Definitions
                          where sampleDef.DisplayName.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0
                          select sampleDef;

            // Tag matches
            var tagMatches = from sampleDef in SampleDefinitions.Definitions
                          where (sampleDef.Tags != null && sampleDef.Tags.Any(str => str.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0))
                          select sampleDef;

            HeaderTextBlock.Text = "Results for '" + searchText + "':";
            MatchingTitleSampleList.SetHeaderText("Matches by Name (" + nameMatches.Count() + ")");
            MatchingTagSampleList.SetHeaderText("Matches by Tag (" + tagMatches.Count() + ")");
            if (nameMatches.Count() > 0)
            {
                MatchingTitleSampleList.SetNoContentTextVisibility(false);
                MatchingTitleSampleList.ItemsSource = nameMatches.OrderByDescending(i => i.DisplayName.StartsWith(searchText, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.DisplayName);
            }
            else
            {
                MatchingTitleSampleList.SetNoContentTextVisibility(true);
            }

            if (tagMatches.Count() > 0)
            {
                MatchingTagSampleList.SetNoContentTextVisibility(false);
                MatchingTagSampleList.ItemsSource = tagMatches.OrderByDescending(i => i.DisplayName.StartsWith(searchText, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.DisplayName);
            }
            else
            {
                MatchingTagSampleList.SetNoContentTextVisibility(true);
            }
        }
    }
}
