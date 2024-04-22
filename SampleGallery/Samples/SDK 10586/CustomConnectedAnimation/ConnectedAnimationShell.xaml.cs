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

﻿using Microsoft.UI.Xaml.Controls;

namespace CompositionSampleGallery
{
    public sealed partial class ConnectedAnimationShell : SamplePage
    {
        public static string        StaticSampleName => "Connected Animation"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Connected animations communicate context across page navigations. Click on one of the thumbnails and see it transition continuously across from one page to another."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761164"; 


        public ConnectedAnimationShell()
        {
            InitializeComponent();

            SampleComboBox.ItemsSource = new[] { "XAML Connected Animation" };
            SampleComboBox.SelectedIndex = 0;
        }

        private void SampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SampleComboBox.SelectedIndex == 0)
            {
                SamplesFrame.Navigate(typeof(ConnectedAnimationSample));
            }
        }
    }
}
