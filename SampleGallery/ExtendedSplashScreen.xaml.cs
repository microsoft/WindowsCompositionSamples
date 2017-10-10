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

using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An extended splash screen to add more personality to our sample gallery.
    /// Plus it showcases animations that run independent of the UI thread in a 
    /// situation where the UI thread is typically busy initializing app UI.
    /// </summary>
    public sealed partial class ExtendedSplashScreen : Page
    {
        private Rect _splashImageBounds;
        
        public ExtendedSplashScreen(SplashScreen splashScreen)
        {
            InitializeComponent();

            if (splashScreen != null)
            {
                _splashImageBounds = splashScreen.ImageLocation;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the main page
            MainPage page = new MainPage(_splashImageBounds);

            // ... and navigate to the Main Page
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                Window.Current.Content = rootFrame = new Frame();
            }

            rootFrame.Content = page;
        }
    }
}
