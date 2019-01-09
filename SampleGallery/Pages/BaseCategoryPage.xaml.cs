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
using ExpressionBuilder;
using SamplesCommon;
using System.Linq;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery
{
    public sealed partial class BaseCategoryPage : Page
    {
        public BaseCategoryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Determine which category we're in
            NavigationItem navItem = (NavigationItem)e.Parameter;
            if(navItem != null)
            {
                // populate itemscontrols
                // Featured samples
                var result = from sampleDef in SampleDefinitions.Definitions
                             where (sampleDef.SampleCategory == navItem.Category)
                             && (sampleDef.Featured == true)
                             select sampleDef;
                FeaturedSampleControl.ItemsSource = result;

                // Full sample list
                result = from sampleDef in SampleDefinitions.Definitions
                         where (sampleDef.SampleCategory == navItem.Category)
                         select sampleDef;
                FullSampleList.ItemsSource = result;

                // Populate the featured-samples text
                if (navItem.FeaturedSamplesTitle != "")
                {
                    FeaturedSampleControl.Title = navItem.FeaturedSamplesTitle;
                }
                else
                {
                    FeaturedSampleControl.Title = "Featured " + navItem.DisplayName + " Samples";
                }

                // Populate the category description textblock
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run() { Text = navItem.CategoryDescription });
                CategoryDescriptionTextBlock.Blocks.Add(paragraph);

            }
        }

        /*
         * On page load, add logic for sticky 'featured samples' header
         */ 
        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the backing visual for the header so that its properties can be animated
            Visual headerVisual = ElementCompositionPreview.GetElementVisual(FeaturedSampleControl);
            var compositor = headerVisual.Compositor;

            // Set Z of Featured Samples so content scrolls behind it
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)MainGrid.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex((UIElement)headerContainer, 1);

            // Get scrollviewer
            ScrollViewer myScrollViewer = MainGrid.GetFirstDescendantOfType<ScrollViewer>();
            var _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(myScrollViewer);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var _props = compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", (float)FeaturedSampleControl.ActualHeight);

            // Bind property references
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionNode headerTranslationAnimation =  -scrollingProperties.Translation.Y;
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);
        }
    }
}
