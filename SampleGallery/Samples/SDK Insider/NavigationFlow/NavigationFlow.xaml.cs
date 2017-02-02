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
        public static string StaticSampleName { get { return "Navigation Flow"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription
        {
            get
            {
                return "Demonstrates a full custom navigation experience using ConnectedAnimationService and Implicit Animations.";
            }
        }

        public NavigationFlow()
        {
            InitializeComponent();
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
