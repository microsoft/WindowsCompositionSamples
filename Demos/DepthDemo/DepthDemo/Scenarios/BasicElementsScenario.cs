using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DepthDemo.Scenarios
{
    class BasicElementsScenario : Scenario
    {
        private Compositor _compositor;
        private List<Visual> _visuals;
        private List<Layer> _layers;
        private SpriteVisual _scenarioContainer;
        private Canvas _canvasReference;
        private Layer _primaryHostLayer;
        private SpriteVisual _bottomLargeVisual;

        private int _numTopVisuals = 6;

        // Focus treatment increase for top layer objects
        public static float _focusScaleIncreaseFactor = 1.1f;
        // Shadow specific
        public static int _focusShadowBlurRadiusIncreaseAmount = 15;
        public static int _focusShadowOffsetIncreaseAmount = 15;

        private static List<LayerConfig> s_layers = new List<LayerConfig>
        {
            new LayerConfig(0, Color.FromArgb(255,0,153,153), new Vector3(1, 1, 1), 30, new Vector3(0, 0, -20)),
            new LayerConfig(1, Color.FromArgb(255,0,204,204), new Vector3(1.3f,1.3f, 1.3f), 60, new Vector3(0, 0, -30))
        };

        public BasicElementsScenario(int identifier, Compositor compositor, Canvas canvasReference) : base(identifier)
        {
            _visuals = new List<Visual>();
            _layers = new List<Layer>();

            _compositor = compositor;
            _canvasReference = canvasReference;

            HelpTextInstructions = "1. Click a visual in the upper portion of the screen to animate it forward with depth. " + System.Environment.NewLine +
                "Click again to send it back to the original position.";
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

            // Get layer 0
            foreach (Layer layer in _layers)
            {
                if (layer.Identifier == 0)
                {
                    _primaryHostLayer = layer;
                }
            }

            // Create larger visual on bottom
            _bottomLargeVisual = compositor.CreateSpriteVisual();
            _bottomLargeVisual.Brush = compositor.CreateColorBrush(s_layers[0].Color);
            _bottomLargeVisual.Opacity = 1.0f;
            _visuals.Add(_bottomLargeVisual);
            _primaryHostLayer.LayerBackingVisual.Children.InsertAtTop(_bottomLargeVisual);

            // Create basic sprite visuals, all on layer of id 0
            for (int i = 0; i < _numTopVisuals; i++)
            {
                SpriteVisual v = compositor.CreateSpriteVisual();
                v.Brush = compositor.CreateColorBrush(s_layers[0].Color);
                v.Opacity = 1.0f;
                _visuals.Add(v);
                _primaryHostLayer.LayerBackingVisual.Children.InsertAtTop(v);
            }

            UpdateSizeAndLayout();

            foreach (Layer l in _layers)
            {
                l.RefreshApplyDepthTreatments();
            }
        }

        public override void CanvasPointerPressed(object sender, PointerRoutedEventArgs e, Canvas canvasReference)
        {
            List<SpriteVisual> excludedVisuals = new List<SpriteVisual>() { _bottomLargeVisual };
            var hitVisual = (SpriteVisual)GetHittestVisual(e.GetCurrentPoint(canvasReference).Position, _visuals, _layers, excludedVisuals);

            // Get parent layer
            Layer parentLayer = GetParentLayer(_layers, hitVisual);

            if (hitVisual != null)
            {
                // Check if the hittested visual was elevated from a previous layer. If it wasn't, elevate. Else move back.
                if (!parentLayer.ElevatedVisuals.Contains(hitVisual))
                {
                    RemoveElevatedVisuals();

                    // Elevate hit tested visual
                    // Get new layer owner
                    var newLayerIndex2 = _layers.IndexOf(parentLayer) + 1;
                    if (newLayerIndex2 >= _layers.Count)
                    {
                        newLayerIndex2 = _layers.Count - 1;
                    }
                    var newLayer = _layers[newLayerIndex2];

                    AnimateVisualToLayer(parentLayer, newLayer, hitVisual);
                    newLayer.ElevatedVisuals.Add(hitVisual);
                }
                else
                {
                    // Animate back to non-elevated state   
                    var newLayerIndex = _layers.IndexOf(parentLayer) - 1;
                    if (newLayerIndex < 0)
                    {
                        newLayerIndex = 0;
                    }
                    var newLayer = _layers[newLayerIndex];

                    AnimateVisualToLayer(parentLayer, newLayer, hitVisual);
                    parentLayer.ElevatedVisuals.Remove(hitVisual);
                }
            }
            else
            {
                RemoveElevatedVisuals();
            }
        }

        private void RemoveElevatedVisuals(bool overrideProjectedShadows = false)
        {
            foreach (Layer layer in _layers)
            {
                if (layer.ElevatedVisuals.Count > 0)
                {
                    foreach (SpriteVisual currentlyElevatedVisual in layer.ElevatedVisuals.ToList())
                    {
                        var newLayerIndex = _layers.IndexOf(layer) - 1;
                        if (newLayerIndex < 0)
                        {
                            newLayerIndex = 0;
                        }

                        AnimateVisualToLayer(layer, _layers[newLayerIndex], currentlyElevatedVisual, overrideProjectedShadows);
                        layer.ElevatedVisuals.Remove(currentlyElevatedVisual);
                    }
                }
            }
        }

        public override void AnimateVisualToLayer(Layer oldLayer, Layer newLayer, SpriteVisual targetVisual, bool overrideProjectedShadows = false)
        {
            if (oldLayer.LayerBackingVisual.Children.Contains(targetVisual))
            {
                // Remove visual from current layer
                oldLayer.LayerBackingVisual.Children.Remove(targetVisual);

                // Add to new layer
                newLayer.LayerBackingVisual.Children.InsertAtTop(targetVisual);

                // Trigger animation to new values
                newLayer.AnimateAddedVisual(targetVisual);
            }
        }

        public override void SizeChanged()
        {
            UpdateSizeAndLayout();
        }

        private void UpdateSizeAndLayout()
        {
            var previousOffset = new Vector3(20, 20, 0);
            foreach (SpriteVisual child in _primaryHostLayer.LayerBackingVisual.Children)
            {
                if (child == _bottomLargeVisual)
                {
                    child.Size = new Vector2((float)_scenarioContainer.Size.X - 40, (float)_scenarioContainer.Size.Y - 20);
                    child.Offset = new Vector3(20, _scenarioContainer.Size.Y / 4, 0);
                    child.CenterPoint = new Vector3(child.Size.X / 2, child.Size.Y / 2, 0);
                }
                else
                {
                    child.Size = new Vector2((_bottomLargeVisual.Size.X - 20 * (_numTopVisuals - 1)) / _numTopVisuals, _bottomLargeVisual.Offset.Y - 40);
                    child.Offset = previousOffset;
                    child.CenterPoint = new Vector3(child.Size.X / 2, child.Size.Y / 2, 0);   // to center scaling
                    previousOffset = new Vector3(previousOffset.X + child.Size.X + 20, previousOffset.Y, 0);
                }
            }
        }
    }
}
