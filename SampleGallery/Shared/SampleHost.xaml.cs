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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class SampleHost : Page
    {
        private SampleDefinition _sampleDefinition;

        public SampleDefinition SampleDefinition
        {
            get { return _sampleDefinition; }
            set { _sampleDefinition = value;  }
        }

        public SampleHost()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SampleDefinition definition = (SampleDefinition)e.Parameter;
            SampleDefinition = definition;
            ContentFrame.Navigate(definition.Type, this);
        }

        public void TagHyperlink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var inline = ((Hyperlink)sender).Inlines[0];
            var run = (Run)inline;
            var searchString = run.Text;

            if (!String.IsNullOrEmpty(searchString))
            {
                MainNavigationViewModel.ShowSearchResults(searchString);
            }
        }
    }
}
