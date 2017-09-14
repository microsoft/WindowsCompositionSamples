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

using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;

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
    }
}
