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

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace CompositionSampleGallery
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
