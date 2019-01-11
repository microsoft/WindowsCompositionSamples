using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    class NavViewXCBB : XamlCompositionBrushBase
    {
        public NavViewXCBB() { }

        protected override void OnConnected()
        {
            Compositor compositor = Window.Current.Compositor;

            // Use LBG if Api contract exists 
            var customBlue = Color.FromArgb((byte)(255 * 0.7), 0, 178, 240);  // Match color to hex used by FeaturedSamples and titlebar
            CompositionBrush brush;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                brush = compositor.CreateLinearGradientBrush();
                var colorStop1 = compositor.CreateColorGradientStop(0.0f, customBlue); 
                var colorStop2 = compositor.CreateColorGradientStop(0.8f, Colors.White);

                ((CompositionLinearGradientBrush)brush).ColorStops.Add(colorStop1);
                ((CompositionLinearGradientBrush)brush).ColorStops.Add(colorStop2);
            }
            else
            {
                brush = compositor.CreateColorBrush(customBlue);
            }
            
            CompositionBrush = brush;
        }

        protected override void OnDisconnected()
        {
            if (CompositionBrush != null)
            {
                CompositionBrush.Dispose();
            }
        }

    }
}
