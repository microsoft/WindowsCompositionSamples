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
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColorBloomTransition : SamplePage
    {

        public static string    StaticSampleName => "Color bloom"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates how to use Visuals and Animations to create a color bloom effect during page or state transitions. Click on one of the items in the Pivot control to trigger the color bloom effect."; 
        public override string  SampleDescription => StaticSampleDescription; 
        public override string  SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761176"; 

        #region Private member variables

        PropertySet _colorsByPivotItem;
        ColorBloomTransitionHelper transition;
        Queue<PivotItem> pendingTransitions = new Queue<PivotItem>();
        #endregion


        #region Ctor

        public ColorBloomTransition()
        {
            this.InitializeComponent();

            this.InitializeColors();

            this.InitializeTransitionHelper();

            this.Unloaded += ColorBloomTransition_Unloaded;
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
        /// All of the Color Bloom transition functionality is encapsulated in this handy helper
        /// which we will init once
        /// </summary>
        private void InitializeTransitionHelper()
        {
            // we pass in the UIElement that will host our Visuals
            transition = new ColorBloomTransitionHelper(hostForVisual);

            // when the transition completes, we need to know so we can update other property values
            transition.ColorBloomTransitionCompleted += ColorBloomTransitionCompleted;
        }

        
        #endregion


        #region Event handlers

        /// <summary>
        /// Event handler for the Click event on the header. 
        /// In response this function will trigger a Color Bloom transition animation.
        /// This is achieved by creating a circular solid colored visual directly underneath the
        /// Pivot header which was clicked, and animating its scale so that it floods a designated bounding box. 
        /// </summary>
        private void Header_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var header = sender as AppBarButton;

            var headerPosition = header.TransformToVisual(UICanvas).TransformPoint(new Windows.Foundation.Point(0d, 0d));

            var initialBounds = new Windows.Foundation.Rect()  // maps to a rectangle the size of the header
            {
                Width = header.RenderSize.Width,
                Height = header.RenderSize.Height,
                X = headerPosition.X,
                Y = headerPosition.Y
            };

            var finalBounds = Window.Current.Bounds;  // maps to the bounds of the current window


            transition.Start((Windows.UI.Color)_colorsByPivotItem[header.Name],  // the color for the circlular bloom
                                 initialBounds,                                  // the initial size and position
                                       finalBounds);                             // the area to fill over the animation duration

            // Add item to queue of transitions
            var pivotItem = (PivotItem)rootPivot.Items.Single(i => ((AppBarButton)((PivotItem)i).Header).Name.Equals(header.Name));
            pendingTransitions.Enqueue(pivotItem);

            // Make the content visible immediately, when first clicked. Subsequent clicks will be handled by Pivot Control
            var content = (FrameworkElement)pivotItem.Content;
            if (content.Visibility == Visibility.Collapsed)
            {
                content.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// Updates the background of the layout panel to the same color whose transition animation just completed.
        /// </summary>
        private void ColorBloomTransitionCompleted(object sender, EventArgs e)
        {
            // Grab an item off the pending transitions queue
            var item = pendingTransitions.Dequeue();
            
            // now remember, that bloom animation was just transitional
            // so we need to explicitly set the correct color as background of the layout panel
            var header = (AppBarButton)item.Header;
            UICanvas.Background = new SolidColorBrush((Windows.UI.Color)_colorsByPivotItem[header.Name]);
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

        /// <summary>
        /// Cleans up remaining surfaces when the page is unloaded.
        /// </summary>
        private void ColorBloomTransition_Unloaded(object sender, RoutedEventArgs e)
        {
            transition.Dispose();
        }

        #endregion


    }
}
