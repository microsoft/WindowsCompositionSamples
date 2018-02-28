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

using CompositionSampleGallery.Samples.SDK_14393.SwipeScroller.Models;
using CompositionSampleGallery.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace CompositionSampleGallery
{
    public sealed partial class SwipeScroller : SamplePage
    {
        public SwipeScroller()
        {
            this.InitializeComponent();
            Model = new LocalDataSource();
        }

        public static string    StaticSampleName => "Swipe Scroller"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates how to use InteractionTracker to add a swipe behavior to items inside a ScrollViewer"; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=869004";

        public LocalDataSource Model { set; get; }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<PhotoModel> list = new List<PhotoModel>();
            foreach (Thumbnail thumbnail in Model.AggregateDataSources(new ObservableCollection<Thumbnail>[] { Model.Landscapes, Model.Nature }))
            {
                list.Add(new PhotoModel()
                {
                    Name = thumbnail.Name,
                    Image = new Uri(thumbnail.ImageUrl),
                    Info = thumbnail.Description
                });
            }
            Items.ItemsSource = list;
        }
    }
}

