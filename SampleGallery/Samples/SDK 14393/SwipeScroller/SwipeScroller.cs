using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using CompositionSampleGallery.Samples.SDK_14393.SwipeScroller.Models;
using CompositionSampleGallery.Shared;

namespace CompositionSampleGallery
{
    public sealed partial class SwipeScroller : SamplePage
    {
        public SwipeScroller()
        {
            this.InitializeComponent();
            Model = new LocalDataSource();
        }

        public static string StaticSampleName { get { return "Swipe Scroller"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription { get { return "Demonstrates how to use InteractionTracker to add a swipe behavior to items inside a ScrollViewer"; } }

        public LocalDataSource Model { set; get; }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<PhotoModel> list = new List<PhotoModel>();
            foreach (Thumbnail thumbnail in Model.Cities)
            {
                list.Add(new PhotoModel()
                {
                    Name = thumbnail.Name,
                    Image = new Uri(thumbnail.ImageUrl),
                    Info = thumbnail.Description,
                    Details = thumbnail.Details
                });
            }
            Items.ItemsSource = list;
        }
    }
}

