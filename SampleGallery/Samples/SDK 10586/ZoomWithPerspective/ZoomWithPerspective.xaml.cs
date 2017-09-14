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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    public sealed partial class ZoomWithPerspective : SamplePage
    {
        private Compositor          _compositor;
        private bool                _zoomed;

        public ZoomWithPerspective()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();

            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        }

        public static string    StaticSampleName => "Zoom With Perspective"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates how to apply and animate a perspective transform. Click on one of the thumbnails to see the effect applied."; 
        public override string  SampleDescription => StaticSampleDescription; 
        public override string  SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761174"; 

        public LocalDataSource Model
        {
            get; set;
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            CompositionImage image = args.ItemContainer.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
            Thumbnail thumbnail = args.Item as Thumbnail;

            // Update the image URI
            image.Source = new Uri(thumbnail.ImageUrl);
        }

        private void ThumbnailList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Thumbnail thumbnail = (Thumbnail)e.ClickedItem;
            ListView listView = (ListView)sender;
            ListViewItem listItem = (ListViewItem)listView.ContainerFromItem(e.ClickedItem);

            if (_zoomed)
            {
                Visual root = ElementCompositionPreview.GetElementVisual(ThumbnailList);

                //
                // Animate the rotation and offset back to the starting values
                //

                ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
                rotationAnimation.InsertKeyFrame(1, 0f);
                rotationAnimation.Duration = TimeSpan.FromMilliseconds(1000);
                root.StartAnimation("RotationAngleInDegrees", rotationAnimation);

                Vector3KeyFrameAnimation offsetAnimaton = _compositor.CreateVector3KeyFrameAnimation();
                offsetAnimaton.InsertKeyFrame(1, new Vector3(0, 0, 0));
                offsetAnimaton.Duration = TimeSpan.FromMilliseconds(1000);
                root.StartAnimation("Offset", offsetAnimaton);

                _zoomed = false;
            }
            else
            {
                //
                // Calculate the absolute offset to the item that was clicked.  We will use that for centering
                // the zoom in.
                //

                GeneralTransform coordinate = listItem.TransformToVisual(listView);
                Vector2 clickedItemCenterPosition = coordinate.TransformPoint(new Point(0, 0)).ToVector2() +
                                                    new Vector2((float)listItem.ActualWidth / 2, (float)listItem.ActualHeight / 2);


                //
                // Calculate the offset we want to animate up/down/in for the zoom based on the center point of the target and the 
                // size of the panel/viewport.
                //

                Vector2 targetOffset = new Vector2((float)listView.ActualWidth / 2, (float)listView.ActualHeight / 2) - clickedItemCenterPosition;


                //
                // Get the root panel and set it up for the rotation animation.  We're rotating the listview around the Y-axis relative
                // to the center point of the panel.
                //

                Visual root = ElementCompositionPreview.GetElementVisual(ThumbnailList);
                root.Size = new Vector2((float)ThumbnailList.ActualWidth, (float)ThumbnailList.ActualHeight);
                root.CenterPoint = new Vector3(root.Size.X / 2, root.Size.Y / 2, 0);
                root.RotationAxis = new Vector3(0, 1, 0);

                // Kick off the rotation animation
                ScalarKeyFrameAnimation rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
                rotationAnimation.InsertKeyFrame(0, 0);
                rotationAnimation.InsertKeyFrame(1, targetOffset.X > 0 ? -45f : 45f);
                rotationAnimation.Duration = TimeSpan.FromMilliseconds(1000);
                root.StartAnimation("RotationAngleInDegrees", rotationAnimation);

                // Calcuate the offset for the point we are zooming towards
                const float zoomFactor = .8f;
                Vector3 zoomedOffset = new Vector3(targetOffset.X, targetOffset.Y, (float)PerspectivePanel.ActualWidth * zoomFactor) * zoomFactor;

                Vector3KeyFrameAnimation offsetAnimaton = _compositor.CreateVector3KeyFrameAnimation();
                offsetAnimaton.InsertKeyFrame(0, new Vector3(0, 0, 0));
                offsetAnimaton.InsertKeyFrame(1, zoomedOffset);
                offsetAnimaton.Duration = TimeSpan.FromMilliseconds(1000);
                root.StartAnimation("Offset", offsetAnimaton);

                _zoomed = true;
            }
        }

        private void RootPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Resize the clip to be the full dimensions of the panel
            MyClip.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);

            PerspectivePanel.PerspectiveDepth = e.NewSize.Width;
        }
    }
}
