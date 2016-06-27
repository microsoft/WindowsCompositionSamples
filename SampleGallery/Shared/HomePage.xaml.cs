using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace CompositionSampleGallery
{
    public class FeaturedSample
    {
        string _name;
        string _description;
        string _imageUrl;

        public FeaturedSample(string name, string description, string imageUrl)
        {
            _name = name;
            _description = description;
            _imageUrl = imageUrl;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
        }
    }

    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();

            FeaturedSample[] featuredSamples = null;
#if SDKVERSION_INSIDER
            featuredSamples = new FeaturedSample[] 
                {
                    new FeaturedSample(ThumbnailLighting.StaticSampleName,  "Demonstrates how to apply Image Lighting to ListView Items.  Switch between different combinations of light types(point, spot, distant) and lighting properties such as diffuse and specular.", "ms-appx:///Assets/ThumbnailLighting.jpg" ),
                    new FeaturedSample(PullToAnimate.StaticSampleName,      "Demonstrates the use of InteractionTracker to drive smooth animations of effect properties.", "ms-appx:///Assets/PullToAnimate.jpg" ),
                    new FeaturedSample(ShadowPlayground.StaticSampleName,   "Experiment with the available properties on the DropShadow object to create interesting shadows.", "ms-appx:///Assets/ShadowPlayground.jpg" ),
                };
#else
            featuredSamples = new FeaturedSample[]
                {
                    new FeaturedSample(ColorBloomTransition.StaticSampleName,       "Demonstrates how to use Visuals and Animations to create a color bloom effect during page or state transitions.", "ms-appx:///Assets/ColorBloom.jpg"),
                    new FeaturedSample(ConnectedAnimationShell.StaticSampleName,    "Connected animations communicate context across page navigations. Click on one of the thumbnails and see it transition continuously across from one page navigate to another.", "ms-appx:///Assets/ContinuityAnimations.jpg"),
                    new FeaturedSample(ZoomWithPerspective.StaticSampleName,        "Demonstrates how to apply and animate a perspective transform.", "ms-appx:///Assets/ZoomPerspective.jpg"),
                };
#endif
            FeaturedSampleList.ItemsSource = featuredSamples;
        }

        private void FeaturedSampleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            FeaturedSample sample = (FeaturedSample)e.ClickedItem;

            foreach (SampleDefinition definition in SampleDefinitions.Definitions)
            {
                if (sample.Name == definition.Name)
                {
                    ((Frame)Window.Current.Content).Navigate(typeof(SampleHost), definition);
                    break;
                }
            }
        }
    }
}
