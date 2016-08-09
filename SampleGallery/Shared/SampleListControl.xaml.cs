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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class SampleListControl : UserControl
    {
        public SampleListControl()
        {
            this.InitializeComponent();

            var result = from sampleDef in SampleDefinitions.Definitions
                         orderby sampleDef.DisplayName
                         group sampleDef by sampleDef.SampleCategory into sampleGroup
                         orderby sampleGroup.Key
                         select sampleGroup;
            SampleViewSource.Source = result;
        }

        private void SampleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            SampleDefinition definition = e.ClickedItem as SampleDefinition;
            MainPage.Instance.NavigateToPage(typeof(SampleHost), definition);

            SamplesSplitView.IsPaneOpen = false; 
        }
    }
}
