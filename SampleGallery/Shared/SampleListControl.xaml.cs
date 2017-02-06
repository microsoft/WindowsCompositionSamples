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
using Windows.UI.Xaml.Data;

namespace CompositionSampleGallery
{
    public sealed partial class SampleListControl : UserControl
    {
        public SampleListControl()
        {
            this.InitializeComponent();

            RefreshSampleList();
        }

        private void SampleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            SampleDefinition definition = e.ClickedItem as SampleDefinition;
            MainPage.Instance.NavigateToPage(typeof(SampleHost), definition);

            SamplesSplitView.IsPaneOpen = false; 
        }

        public void RefreshSampleList()
        {
            IOrderedEnumerable<IGrouping<SampleCategory, SampleDefinition>> result;

            if (MainPage.AreEffectsFast)
            {
                result = from sampleDef in SampleDefinitions.Definitions
                         orderby sampleDef.DisplayName
                         group sampleDef by sampleDef.SampleCategory into sampleGroup
                         orderby sampleGroup.Key
                         select sampleGroup;
            }
            else if (MainPage.AreEffectsSupported)
            {
                result = from sampleDef in SampleDefinitions.Definitions
                         where !sampleDef.RequiresFastEffects
                         orderby sampleDef.DisplayName
                         group sampleDef by sampleDef.SampleCategory into sampleGroup
                         orderby sampleGroup.Key
                         select sampleGroup;
            }
            else
            {
                result = from sampleDef in SampleDefinitions.Definitions
                         where !sampleDef.RequiresEffects
                         orderby sampleDef.DisplayName
                         group sampleDef by sampleDef.SampleCategory into sampleGroup
                         orderby sampleGroup.Key
                         select sampleGroup;
            }

            SampleViewSource.Source = result;
        }
    }
}
