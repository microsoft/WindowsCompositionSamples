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

            var brush = compositor.CreateLinearGradientBrush();
            var colorStop1 = compositor.CreateColorGradientStop(0.0f, Color.FromArgb((byte)(255*0.7), 0, 178, 240));  // Match color to hex used by FeaturedSamples and titlebar
            var colorStop2 = compositor.CreateColorGradientStop(0.8f, Colors.White);

            brush.ColorStops.Add(colorStop1);
            brush.ColorStops.Add(colorStop2);

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
