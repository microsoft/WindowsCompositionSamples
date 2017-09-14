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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class FeaturedSamples : UserControl
    {

        public String Title
        {
            set
            {
                if(value != null)
                {
                    Title1TextBlock.Text = value;
                }
            }
        }

        public String Description
        {
            set
            {
                if (value != null)
                {
                    DescriptionTextBlock.Text = value;
                }
            }
        }

        public object ItemsSource
        {
            get
            {
                return NewSamplesList.ItemsSource;
            }
            set
            {
                if (value != null)
                {
                    NewSamplesList.ItemsSource = value;
                }
            }
        }


        public FeaturedSamples()
        {
            this.InitializeComponent();
            NewSamplesList.ItemClick += MainPage.FeaturedSampleList_ItemClick;
        }

        // We don't want the featured sample list to show if there aren't any samples 
        // to feature so we'll hide it
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<CompositionSampleGallery.SampleDefinition> items = (IEnumerable < CompositionSampleGallery.SampleDefinition > )(this.ItemsSource);

            if(items.Count<SampleDefinition>() == 0)    
                this.Visibility = Visibility.Collapsed;
        }

        // Navigate up to the parent frame and trigger a navigation
        private void NewSamplesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            MainNavigationViewModel.NavigateToSample(sender, e);
        }
    }
}
