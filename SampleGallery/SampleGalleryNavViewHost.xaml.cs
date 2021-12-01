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

﻿using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Linq;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CompositionSampleGallery
{
    public sealed partial class SampleGalleryNavViewHost : UserControl
    {
        object _itemsSource;
        private SampleDefinition _dummySampleDefinition;
        public SampleGalleryNavViewHost()
        {
            this.InitializeComponent();
            NavView.PointerPressed += PointerPressedHandler;
        }

        private void PointerPressedHandler(object sender, PointerRoutedEventArgs e)
        {
                     
            if (e.GetCurrentPoint((UIElement)sender).Properties.IsXButton1Pressed)
            {
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.GoBack();
                }
            }
            else if (e.GetCurrentPoint((UIElement)sender).Properties.IsXButton2Pressed)
            {
                if (ContentFrame.CanGoForward)
                {
                    ContentFrame.GoForward();
                }
            }
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
                    foreach (NavigationItem item in (ICollection<NavigationItem>)value)
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

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        public void Navigate(Type type, object parameter)
        {
            ContentFrame.Navigate(type, parameter);
        }

        public void NotifyCompositionCapabilitiesChanged(bool areEffectsSupported, bool areEffectsFast)
        {
            if (ContentFrame.Content is SampleHost host)
            {
                SamplePage page = (SamplePage)host.Content;
            }
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
            }
            else
            {
                // The NavigationViewItemInvokedEventArgs only give us the string label of the item that was invoked.  Rather than writing
                // awkward code to string compare the arg against each category name, we instead can just fetch the selected item
                // off of the navigation view.  However, if the selection is actually changing as a result of this invocation, the NavView doesn't
                // reflect that change until *after* this event has fired.  So, use the thread's DispatcherQueue to defer the navigation until 
                // after the nav view has processed the potential selection change.

                DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.High, () => { NavigateToSelectedItem(); });
            }
        }

        private void NavigateToSelectedItem()
        {
            NavigationItem navItem = (NavigationItem)((NavigationViewItem)NavView.SelectedItem).DataContext;

            Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            properties.Add("TargetView", navItem.Category.ToString());
            CompositionSampleGallery.AppTelemetryClient.TrackEvent("Navigate", properties, null);
            Navigate(navItem.PageType, navItem);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matches = from sampleDef in SampleDefinitions.Definitions
                              where sampleDef.DisplayName.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0
                                 || (sampleDef.Tags != null && sampleDef.Tags.Any(str => str.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0))
                              select sampleDef;

                if (matches.Count() > 0)
                {
                    SearchBox.ItemsSource = matches.OrderByDescending(i => i.DisplayName.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.DisplayName);
                }
                else
                {
                    _dummySampleDefinition = new SampleDefinition("No results found", null, SampleType.Reference, SampleCategory.APIReference, false, false);
                    SearchBox.ItemsSource = new SampleDefinition[] { _dummySampleDefinition };
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText) && args.ChosenSuggestion == null)
            {
                MainNavigationViewModel.ShowSearchResults(args.QueryText);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (((SampleDefinition)(args.SelectedItem)) == _dummySampleDefinition)
            {
                SearchBox.Text = "";
            }
            else
            {
                MainNavigationViewModel.NavigateToSample((SampleDefinition)args.SelectedItem);
            }
        }

        private void SearchBox_AccessKeyInvoked(UIElement sender, Microsoft.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            SearchBox.Focus(FocusState.Keyboard);
        }
    }
}
