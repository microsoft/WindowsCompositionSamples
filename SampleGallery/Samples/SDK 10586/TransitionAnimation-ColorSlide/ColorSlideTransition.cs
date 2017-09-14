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
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColorSlideTransition : SamplePage
    {

        public static string    StaticSampleName => "Color slide"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates how to use Visuals and Animations to create a color slide effect during page or state transitions. Click on one of the items in the Pivot control to trigger the color slide effect."; 
        public override string  SampleDescription => StaticSampleDescription; 
        public override string  SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761173"; 
        #region Private member variables

        PropertySet _colorsByPivotItem;
        ColorSlideTransitionHelper transition;

        #endregion


        #region Ctor

        public ColorSlideTransition()
        {
            this.InitializeComponent();

            this.InitializeColors();

            this.InitializeTransitionHelper();
        }

        #endregion


        #region Initializers

        /// <summary>
        /// Prepopulate a set of colors, indexed by where on the Pivot they will play a role
        /// </summary>
        private void InitializeColors()
        {
            _colorsByPivotItem = new PropertySet();
            _colorsByPivotItem.Add("Pictures", Windows.UI.Colors.Orange);
            _colorsByPivotItem.Add("ContactInfo", Windows.UI.Colors.Lavender);
            _colorsByPivotItem.Add("Download", Windows.UI.Colors.GreenYellow);
            _colorsByPivotItem.Add("Comment", Windows.UI.Colors.DeepSkyBlue);
        }


        /// <summary>
        /// All of the Color slide transition functionality is encapsulated in this handy helper
        /// which we will init once
        /// </summary>
        private void InitializeTransitionHelper()
        {
            // we pass in the UIElement that will host our Visuals
            transition = new ColorSlideTransitionHelper(hostForVisual);

            // when the transition completes, we need to know so we can update other property values
            transition.ColorSlideTransitionCompleted += ColorSlideTransitionCompleted;
        }


        #endregion


        #region Event handlers

        /// <summary>
        /// Event handler for the Click event on the header. 
        /// In response this function will trigger a Color slide transition animation.
        /// This is achieved by creating a circular solid colored visual directly underneath the
        /// Pivot header which was clicked, and animating its scale so that it floods a designated bounding box. 
        /// </summary>
        private void Header_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var header = sender as AppBarButton;

            var currentPivotItem = rootPivot.SelectedItem as PivotItem;
            var contentHeight = rootPivot.RenderSize.Height - header.RenderSize.Height;
            var contentWidth = rootPivot.RenderSize.Width;

            var headerPosition = header.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var finalBounds = new Windows.Foundation.Rect()  // maps to a rectangle the size of the pivot content
            {
                Width = contentWidth,
                Height = contentHeight,
                X = Window.Current.Bounds.Width,
                Y = headerPosition.Y + header.RenderSize.Height
            };


            transition.Start((Windows.UI.Color)_colorsByPivotItem[header.Name],  // the color for the colored slide
                                       finalBounds);                             // the area to fill over the animation duration
        }


        /// <summary>
        /// Makes the content of the currently selected pivot item visible, 
        /// and content of every other pivot item invisible
        /// </summary>
        private void ColorSlideTransitionCompleted(object sender, EventArgs e)
        {
            var currentPivotItem = rootPivot.SelectedItem as PivotItem;
            foreach (PivotItem pivotItem in rootPivot.Items)
            {
                if (pivotItem == currentPivotItem)
                    (pivotItem.Content as FrameworkElement).Visibility = Visibility.Visible;
                else
                    (pivotItem.Content as FrameworkElement).Visibility = Visibility.Collapsed;
            }

        }


        /// <summary>
        /// In response to a XAML layout event on the Grid (named UICanvas) we will apply a clip
        /// to ensure all Visual animations stay within the bounds of the Grid, and doesn't bleed into
        /// the top level Frame belonging to the Sample Gallery. Probably not a factor in most other cases.
        /// </summary>
        private void UICanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var uiCanvasLocation = UICanvas.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));
            var clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(uiCanvasLocation, e.NewSize)
            };
            UICanvas.Clip = clip;
        }

        #endregion


    }
}
