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
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CompositionSampleGallery
{
    public sealed partial class Social : UserControl
    {
        public Social()
        {
            this.InitializeComponent();
        }

        private async void InsiderPanel_Tapped(object sender, TappedRoutedEventArgs e){ await Launcher.LaunchUriAsync(new Uri("https://insider.windows.com/")); }

        private async void TwitterPanel_Tapped(object sender, TappedRoutedEventArgs e){ await Launcher.LaunchUriAsync(new Uri("https://twitter.com/WindowsUI")); }

        private async void GitHubPanel_Tapped(object sender, TappedRoutedEventArgs e){ await Launcher.LaunchUriAsync(new Uri("https://github.com/Microsoft/WindowsUIDevLabs")); }
    }
}
