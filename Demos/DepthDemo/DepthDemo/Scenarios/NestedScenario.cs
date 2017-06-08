using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DepthDemo.Scenarios
{
    class NestedScenario : Scenario
    {
        private Compositor _compositor;
        private List<SpriteVisual> _visuals;
        private List<Layer> _layers;
        private SpriteVisual _scenarioContainer;
        private Canvas _canvasReference;

        // Focus treatment increase for top layer objects
        public static float _focusScaleIncreaseFactor = 1.2f;
        // Shadow specific
        public static int _focusShadowBlurRadiusIncreaseAmount = 5;
        public static int _focusShadowOffsetIncreaseAmount = 10;

        private static List<LayerConfig> s_layers = new List<LayerConfig>
        {
            new LayerConfig(-2, Color.FromArgb(255,0,153,153), new Vector3(1, 1, 1), 0, new Vector3(0, 0, 0)),
            new LayerConfig(-1, Color.FromArgb(255,0,204,204), new Vector3(1, 1, 1), 5, new Vector3(5,5,-5)),
            new LayerConfig(0, Color.FromArgb(255,0,255,255), new Vector3(1, 1, 1), 9, new Vector3(10,10,-10)),
            new LayerConfig(1, Color.FromArgb(255,102,255,255), new Vector3(1, 1, 1), 15, new Vector3(15,15,-15)),
            new LayerConfig(2, Color.FromArgb(255,204,255,255), new Vector3(1, 1, 1), 30, new Vector3(20,20,-20))
        };

        public NestedScenario(int identifier, Compositor compositor, Canvas canvasReference) : base(identifier)
        {
            _visuals = new List<SpriteVisual>();
            _layers = new List<Layer>();

            _compositor = compositor;
            _canvasReference = canvasReference;

            HelpTextInstructions = "1. Move your finger L/R across the screen as if panning, to trigger the parallax behavior in the nested visuals.";
        }

        public override void CreateLayers()
        {
            // Create layers
            for (int i = 0; i < s_layers.Count; i++)
            {
                var offset = new Vector3(0, 0, (ConfigurationConstants.ZOffsetSpacingIncrement * s_layers[i].ID));
                var size = new Vector2((float)_canvasReference.ActualWidth, (float)_canvasReference.ActualHeight);

                Layer l = new Layer(_compositor, s_layers[i].ID, offset, size, _scenarioContainer, _compositor.CreateColorBrush(s_layers[i].Color));
                _layers.Add(l);

                // Set depth treatment
                ShadowTreatment shadowTreatment = new ShadowTreatment(s_layers[i].ShadowBlurRadius, s_layers[i].ShadowOffset, _focusShadowBlurRadiusIncreaseAmount, _focusShadowOffsetIncreaseAmount);
                DepthTreatmentConfigurations depthTreatment = new DepthTreatmentConfigurations(_layers[i], s_layers[i].Scale, _focusScaleIncreaseFactor, shadowTreatment);
                _layers[i].SetDepthTreatments(depthTreatment);
            }
        }

        public override void ImplementScenario(Compositor compositor, SpriteVisual scenarioContainer)
        {
            _scenarioContainer = scenarioContainer;

            CreateLayers();

            // Create basic sprite visual to add to each layer, with random colors
            for (int i = 0; i < _layers.Count; i++)
            {
                SpriteVisual v = compositor.CreateSpriteVisual();
                v.Brush = compositor.CreateColorBrush(s_layers[i].Color);
                v.Opacity = 1.0f;

                _visuals.Add(v);
                _layers[i].LayerBackingVisual.Children.InsertAtTop(v);
            }

            UpdateSizeAndLayout();

            foreach (Layer l in _layers)
            {
                l.RefreshApplyDepthTreatments();
            }
        }

        public List<SpriteVisual> GetVisuals()
        {
            return _visuals;
        }

        public override void SizeChanged()
        {
            UpdateSizeAndLayout();
        }

        private void UpdateSizeAndLayout()
        {
            // Incremental offset and size starting values
            var previousOffset = new Vector3(10, 10, 0);
            var previousSize = new Vector2((float)_scenarioContainer.Size.X - 200, (float)_scenarioContainer.Size.Y - 200);

            for (int i = 0; i < _layers.Count; i++)
            {
                foreach (SpriteVisual child in _layers[i].LayerBackingVisual.Children)
                {
                    child.Size = previousSize;
                    child.Offset = previousOffset;
                    child.CenterPoint = new Vector3(previousSize.X / 2, previousSize.Y / 2, 0);   // to center scaling

                    previousOffset = new Vector3(previousOffset.X + 50, previousOffset.Y + 50, 0);
                    previousSize = new Vector2(previousSize.X - 100, previousSize.Y - 100);
                }
            }
        }
    }
}
