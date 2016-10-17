using Windows.UI.Composition;

namespace CompositionSampleGallery.Samples.SDK_14393.NineGridResizing
{
    public interface INineGridScenario
    {
        void Selected(SpriteVisual hostVisual);

        CompositionBrush Brush { get;  }

        string Text { get; }
    }
}
