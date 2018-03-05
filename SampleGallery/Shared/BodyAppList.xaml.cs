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

namespace CompositionSampleGallery
{
    public sealed partial class BodyAppList : UserControl
    {

        public object ItemsSource
        {
            get
            {
                return FullSampleList.ItemsSource;
            }
            set
            {
                if (value != null)
                {
                    FullSampleList.ItemsSource = value;                    
                }
            }
        }

        public BodyAppList()
        {
            this.InitializeComponent();
        }

        private void FullSampleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            MainNavigationViewModel.NavigateToSample(sender, e);
        }

        public void SetHeaderText(string text)
        {
            HeaderItem.Content = text;
        }

        public void SetNoContentTextVisibility(bool isVisible)
        {
            if (isVisible)
            {
                NoResultsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                NoResultsTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}


