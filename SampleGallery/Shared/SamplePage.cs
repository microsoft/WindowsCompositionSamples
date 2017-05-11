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
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public abstract class SamplePage : Page
    {
        public abstract string SampleDescription { get; }
        public abstract string SampleName { get; }
        public virtual string SampleCodeUri { get { return "https://github.com/Microsoft/composition/";} }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += SamplePage_BackRequested;

            if (e.Parameter is SampleHost)
            {
                SampleHost host = (SampleHost)e.Parameter;

                host.SampleDescription.Text = SampleDescription;
                host.SampleName.Text = SampleName;
                host.SampleCode.NavigateUri = new Uri(SampleCodeUri);
            }
        }

        private void SamplePage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame pivotItemFrame = MainNavigationViewModel.GetPivotFrame(this);
            if (pivotItemFrame != null)
            {
                e.Handled = true;
                (pivotItemFrame).GoBack();
            }
        }

        public virtual void OnCapabiliesChanged(bool areEffectSupported, bool areEffectsFast)
        {

        }
    }
}
