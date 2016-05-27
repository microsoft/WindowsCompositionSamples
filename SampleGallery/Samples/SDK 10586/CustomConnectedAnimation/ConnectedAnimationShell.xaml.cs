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
    public sealed partial class ConnectedAnimationShell : SamplePage
    {
        public static string StaticSampleName { get { return "Connected Animation"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription { get { return "Connected animations communicate context across page navigations. Click on one of the thumbnails and see it transition continuously across from one page to another."; } }
        public override string SampleCodeUri { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761164"; } }


        public ConnectedAnimationShell()
        {
            InitializeComponent();

#if SDKVERSION_INSIDER
            SampleComboBox.ItemsSource = new[] { "XAML Connected Animation", "Custom Connected Animation" };
#else
            SampleComboBox.ItemsSource = new[] { "Custom Connected Animation" };
#endif
            SampleComboBox.SelectedIndex = 0;
        }

        private void SampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if SDKVERSION_INSIDER
            if (SampleComboBox.SelectedIndex == 0)
            {
                SamplesFrame.Navigate(typeof(ConnectedAnimationSample));
            }
            else if (SampleComboBox.SelectedIndex == 1)
            {
                SamplesFrame.Navigate(typeof(CustomConnectedAnimation));
            }
#else
            if (SampleComboBox.SelectedIndex == 0)
            {
                SamplesFrame.Navigate(typeof(CustomConnectedAnimation));
            }
#endif
        }
    }
}
