//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{
    public sealed partial class NavigationFlow : SamplePage
    {
        public static string    StaticSampleName => "Navigation Flow"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates a full custom navigation experience using ConnectedAnimationService and Implicit Animations.";
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868949";

        public NavigationFlow()
        {
            InitializeComponent();
            // Inner frame used to contain source and destination pages
            InnerFrame.Navigate(typeof(NavigationFlowSourcePage));
        }

        private void InnerFrame_Navigated(object sender, NavigationEventArgs e)
        {
            GoBackButton.IsEnabled = InnerFrame.CanGoBack;
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (InnerFrame.CanGoBack)
            {
                InnerFrame.GoBack();
            }
        }

        private void InnerFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Ensure the frame's contents are clipped to its bounds
            InnerFrame.Clip = new RectangleGeometry()
            {
                Rect = new Rect(new Point(), e.NewSize)
            };
        }
    }
}
