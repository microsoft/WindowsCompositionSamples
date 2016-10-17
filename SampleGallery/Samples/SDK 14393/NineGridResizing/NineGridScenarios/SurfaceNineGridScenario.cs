using Windows.UI.Composition;

namespace CompositionSampleGallery.Samples.SDK_14393.NineGridResizing.NineGridScenarios
{
    sealed internal class SurfaceNineGridScenario : INineGridScenario
    {
        private readonly CompositionNineGridBrush _nineGridBrush;
        private readonly string _text;

        public SurfaceNineGridScenario(Compositor compositor, CompositionSurfaceBrush surfaceBrush, string text)
        {
            _nineGridBrush = compositor.CreateNineGridBrush();
            _nineGridBrush.Source = surfaceBrush;
            _nineGridBrush.SetInsets(60.0f);

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
