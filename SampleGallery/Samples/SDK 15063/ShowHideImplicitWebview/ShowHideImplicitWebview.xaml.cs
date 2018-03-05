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
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Shapes;

namespace CompositionSampleGallery
{
    public sealed partial class ShowHideImplicitWebview : SamplePage
    {
        private Compositor _compositor;
        private Visual _image1, _image2, _image3;
        private Vector3 _primaryImageOffset, _secondaryImageOffset, _tertiaryImageOffset;
        private Vector3 _primaryImageScale, _secondaryImageScale, _tertiaryImageScale;
        private Ellipse _currentPrimary, _currentSecondary, _currentTertiary;
        private Dictionary<Ellipse, ImageItem> imageDictionary;
        private static string _rightArrowGlyph = "\u2190"; 
        private static string _leftArrowGlyph = "\u2192";
        private bool _circlesVisible = true;
        private ImplicitAnimationCollection _implicitAnimationCollection;

        public static string        StaticSampleName => "Implicit Show/Hide Webview"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to apply an implicit show/hide animation on a " +
                                                                    "webview UI element in a realistic app scenario."; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868955";


        public ShowHideImplicitWebview()
        {
            this.InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            imageDictionary = new Dictionary<Ellipse, ImageItem>();

            _primaryImageScale = new Vector3(1, 1, 1);
            _secondaryImageScale = new Vector3(0.8f, 0.8f, 0.8f);
            _tertiaryImageScale = new Vector3(0.6f, 0.6f, 0.6f);

            this.CreateImageObjects();

            // Implicit show animation for webview
            var showWebviewAnimation = _compositor.CreateScalarKeyFrameAnimation();
            showWebviewAnimation.InsertKeyFrame(0.0f, 0.0f);
            showWebviewAnimation.InsertKeyFrame(1.0f, 1.0f);
            showWebviewAnimation.Target = nameof(Visual.Opacity);
            showWebviewAnimation.Duration = TimeSpan.FromSeconds(0.5f);
            ElementCompositionPreview.SetImplicitShowAnimation(PageWebview, showWebviewAnimation);

            // Implicit hide animation for webview
            var hideWebviewAnimation = _compositor.CreateScalarKeyFrameAnimation();
            hideWebviewAnimation.InsertKeyFrame(0.0f, 1.0f);
            hideWebviewAnimation.InsertKeyFrame(1.0f, 0.0f);
            hideWebviewAnimation.Target = nameof(Visual.Opacity);
            hideWebviewAnimation.Duration = TimeSpan.FromSeconds(0.5f);
            ElementCompositionPreview.SetImplicitHideAnimation(PageWebview, hideWebviewAnimation);

            // Implicit show animation for the images
            var showImagesAnimation = _compositor.CreateScalarKeyFrameAnimation();
            showImagesAnimation.InsertKeyFrame(0.0f, 0.0f);
            showImagesAnimation.InsertKeyFrame(1.0f, 1.0f);
            showImagesAnimation.Target = nameof(Visual.Opacity);
            showImagesAnimation.Duration = TimeSpan.FromSeconds(1.0f);
            ElementCompositionPreview.SetImplicitShowAnimation(CircleCanvas, showImagesAnimation);

            // Offset and scale implicit animation set up for images
            _implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
            // Offset implicit animation
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromSeconds(1.0f);
            offsetAnimation.Target = nameof(Visual.Offset);
            // Scale implicit animation
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            scaleAnimation.Duration = TimeSpan.FromSeconds(1.0f);
            scaleAnimation.Target = nameof(Visual.Scale);
            // Add to collection
            _implicitAnimationCollection["Offset"] = offsetAnimation;
            _implicitAnimationCollection["Scale"] = scaleAnimation;
        }

        /// <summary>
        /// Helper to create image objects
        /// </summary>
        private void CreateImageObjects()
        {
            ImageItem imageItem1 = new ImageItem("https://en.wikipedia.org/wiki/Slug");
            ImageItem imageItem2 = new ImageItem("https://en.wikipedia.org/wiki/Nymphalidae");
            ImageItem imageItem3 = new ImageItem("https://en.wikipedia.org/wiki/Dahlia#Flower_type");

            imageDictionary.Add(Image1, imageItem1);
            imageDictionary.Add(Image2, imageItem2);
            imageDictionary.Add(Image3, imageItem3);
        }

        /// <summary>
        /// Update layout and webivew on page load
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _primaryImageOffset = new Vector3((float)(6 * (CircleCanvas.ActualWidth / 18)), (float)CircleCanvas.ActualHeight / 5, 0);
            _secondaryImageOffset = new Vector3((float)(1 * (CircleCanvas.ActualWidth / 14)), (float)CircleCanvas.ActualHeight / 7, 20);
            _tertiaryImageOffset = new Vector3((float)(20 * (CircleCanvas.ActualWidth / 30)), (float)CircleCanvas.ActualHeight / 10, 40);

            // Get backing visuals
            _image1 = ElementCompositionPreview.GetElementVisual(Image1);
            _image2 = ElementCompositionPreview.GetElementVisual(Image2);
            _image3 = ElementCompositionPreview.GetElementVisual(Image3);

            // Update XAML element visibility to trigger show animation
            Image1.Visibility = Visibility.Visible;
            Image2.Visibility = Visibility.Visible;
            Image3.Visibility = Visibility.Visible;

            _currentPrimary = Image1;
            _currentSecondary = Image2;
            _currentTertiary = Image3;

            UpdateVisualLayout();
            UpdateWebview(_currentPrimary);
        }

        /// <summary>
        /// Show webview on navigation complete, which will trigger show implicit animation
        /// </summary>
        private void PageWebview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            PageWebview.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Updates circle image positions on grid resize
        /// Also update portrait layout values if applicable
        /// </summary>
        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualLayout();

            if(LeftStackPanel.Visibility == Visibility.Collapsed && PageWebview.Visibility == Visibility.Visible)
            {
                MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions[1].Width = new GridLength(3, GridUnitType.Star);
                MainGrid.ColumnDefinitions[2].Width = new GridLength(20, GridUnitType.Star);

                // Update glyph
                ViewMoreButtonIcon.Glyph = _rightArrowGlyph;
            }
        }

        /// <summary>
        /// Click listener for navigation on portrait layout
        /// </summary>
        private void ViewMoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_circlesVisible)
            { 
                LeftStackPanel.Visibility = Visibility.Collapsed;
                PageWebview.Visibility = Visibility.Visible;
                MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions[1].Width = new GridLength(3, GridUnitType.Star);
                MainGrid.ColumnDefinitions[2].Width = new GridLength(20, GridUnitType.Star);

                // Update glyph
                ViewMoreButtonIcon.Glyph = _rightArrowGlyph;
            }
            else
            {
                LeftStackPanel.Visibility = Visibility.Visible;
                PageWebview.Visibility = Visibility.Collapsed;
                MainGrid.ColumnDefinitions[0].Width = new GridLength(20, GridUnitType.Star);
                MainGrid.ColumnDefinitions[1].Width = new GridLength(3, GridUnitType.Star);
                MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);

                // Update glyph
                ViewMoreButtonIcon.Glyph = _leftArrowGlyph;
            }
            _circlesVisible = !_circlesVisible;
        }

        /// <summary>
        /// Navigates the webview to the info URL of a passed image
        /// </summary>
        private void UpdateWebview(Ellipse imageLookup)
        {
            PageWebview.Navigate(new Uri(imageDictionary[imageLookup].GetWebviewString()));
        }

        /// <summary>
        /// Helper to update circle image size/positioning without animation
        /// </summary>
        private void UpdateVisualLayout()
        {
            if(_currentPrimary != null)
            {
                var primary = ElementCompositionPreview.GetElementVisual(_currentPrimary);
                var secondary = ElementCompositionPreview.GetElementVisual(_currentSecondary);
                var tertiary = ElementCompositionPreview.GetElementVisual(_currentTertiary);

                primary.Offset = _primaryImageOffset;
                primary.Scale = _primaryImageScale;

                secondary.Offset = _secondaryImageOffset;
                secondary.Scale = _secondaryImageScale;

                tertiary.Offset = _tertiaryImageOffset;
                tertiary.Scale = _tertiaryImageScale;
            }
        }

        /// <summary>
        /// Helper to animate circle images to new positions given new desired position information
        /// Triggers implicit offset/scale animations
        /// </summary>
        private void AnimateImages(Visual newPrimary, Ellipse newPrimaryImage, Visual newSecondary, Ellipse newSecondaryImage, Visual newTertiary, Ellipse newTertiaryImage)
        {
            // Connect implicit animations
            newPrimary.ImplicitAnimations = _implicitAnimationCollection;
            newSecondary.ImplicitAnimations = _implicitAnimationCollection;
            newTertiary.ImplicitAnimations = _implicitAnimationCollection;

            // Update values to trigger implicit animations

            newPrimary.Offset = _primaryImageOffset;
            newPrimary.Scale = _primaryImageScale;

            newSecondary.Offset = _secondaryImageOffset;
            newSecondary.Scale = _secondaryImageScale;

            newTertiary.Offset = _tertiaryImageOffset;
            newTertiary.Scale = _tertiaryImageScale;

            // Update current order
            _currentPrimary = newPrimaryImage;
            _currentSecondary = newSecondaryImage;
            _currentTertiary = newTertiaryImage;

            // Reorder visual tree
            var xamlImage1Storage = Image1;
            var xamlImage2Storage = Image2;
            var xamlImage3Storage = Image3;
            CircleCanvas.Children.Clear();
            CircleCanvas.Children.Add(_currentTertiary);
            CircleCanvas.Children.Add(_currentSecondary);
            CircleCanvas.Children.Add(_currentPrimary);
        }

        /// <summary>
        /// On image click, animate circle images to appropriate view and update webview
        /// </summary>
        private void Image3_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.AnimateImages(_image3, Image3, _image1, Image1, _image2, Image2);
            PageWebview.Visibility = Visibility.Collapsed;
            this.UpdateWebview(Image3);
        }

        /// <summary>
        /// On image click, animate circle images to appropriate view and update webview
        /// </summary>
        private void Image2_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.AnimateImages(_image2, Image2, _image3, Image3, _image1, Image1);
            PageWebview.Visibility = Visibility.Collapsed;
            this.UpdateWebview(Image2);
        }

        /// <summary>
        /// On image click, animate circle images to appropriate view and update webview
        /// </summary>
        private void Image1_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.AnimateImages(_image1, Image1, _image2, Image2, _image3, Image3);
            PageWebview.Visibility = Visibility.Collapsed;
            this.UpdateWebview(Image1);
        }
    }

    /// <summary>
    /// Stores information about an image 
    /// </summary>
    public class ImageItem
    {
        private string webviewString;

        public ImageItem( string webviewString)
        {
            this.webviewString = webviewString;
        }
        
        public string GetWebviewString()
        {
            return webviewString;
        }
    }
}
