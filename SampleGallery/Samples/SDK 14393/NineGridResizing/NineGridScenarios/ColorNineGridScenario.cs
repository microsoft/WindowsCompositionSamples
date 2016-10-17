using Windows.UI;
using Windows.UI.Composition;

namespace CompositionSampleGallery.Samples.SDK_14393.NineGridResizing.NineGridScenarios
{
    sealed internal class ColorNineGridScenario : INineGridScenario
    {
        private readonly CompositionNineGridBrush _nineGridBrush;
        private readonly string _text;

        public ColorNineGridScenario(Compositor compositor, string text)
        {
            _nineGridBrush = compositor.CreateNineGridBrush();
            _nineGridBrush.Source = compositor.CreateColorBrush(Colors.Black);
            _nineGridBrush.SetInsets(10.0f);
            _nineGridBrush.IsCenterHollow = true;

            _text = text;
        }

        public CompositionBrush Brush
        {
            get
            {
                return _nineGridBrush;
            }
        }
        public string Text
        {
            get
            {
                return _text;
            }
        }

        public void Selected(SpriteVisual hostVisual)
        {
            hostVisual.Brush = _nineGridBrush;
        }
    }
}
