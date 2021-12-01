﻿//*********************************************************
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

using System.Collections;

using Microsoft.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class FeaturedSampleHomePage : UserControl
    {
        public FeaturedSampleHomePage()
        {
            this.InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)FeaturedSampleList.ItemsSource; }
            set { FeaturedSampleList.ItemsSource = value; }
        }
        // Navigate up to the parent frame and trigger a navigation
        private void FeaturedSamplesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            MainNavigationViewModel.NavigateToSample(sender, e);
        }
    }
}