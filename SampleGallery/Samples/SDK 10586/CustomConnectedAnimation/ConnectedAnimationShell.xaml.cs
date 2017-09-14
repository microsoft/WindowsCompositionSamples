using Windows.UI.Xaml.Controls;

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

#if SDKVERSION_14393
            SampleComboBox.ItemsSource = new[] { "XAML Connected Animation", "Custom Connected Animation" };
#else
            SampleComboBox.ItemsSource = new[] { "Custom Connected Animation" };
#endif
            SampleComboBox.SelectedIndex = 0;
        }

        private void SampleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if SDKVERSION_14393
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
