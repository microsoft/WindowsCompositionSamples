using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DepthDemo
{
    public abstract class Scenario
    {
        public Scenario(int identifier)
        {
            Identifier = identifier;
            IsActive = false;
        }

        public struct LayerConfig
        {
            public int ID;
            public Color Color;
            public Vector3 Scale;
            public int ShadowBlurRadius;
            public Vector3 ShadowOffset;

            public LayerConfig(int id, Color color, Vector3 scale, int shadowBlurRadius, Vector3 shadowOffset)
            {
                ID = id;
                Color = color;
                Scale = scale;
                ShadowBlurRadius = shadowBlurRadius;
                ShadowOffset = shadowOffset;
            }
        }

        /// <summary>
        /// String describing how to interact with the scenario
        /// </summary>
        public string HelpTextInstructions { get; set; }

        /// <summary>
        /// Boolean indicating whether the scenario is active or not. 'Active' indicates
        /// the end user can see the scenario in the app.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Identifier for the scenario
        /// </summary>
        public int Identifier { get; set; }

        /// <summary>
        /// Used to define new behavior when animating a visual from one conceptual layer to another,
        /// on a per scenario basis.
        /// </summary>
        public virtual void AnimateVisualToLayer(Layer oldLayer, Layer newLayer, SpriteVisual targetVisual, bool overrideProjectedShadows = false)
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

        public Visual GetHittestVisual(Point pointerPosition, List<Visual> visuals, List<Layer> layers, List<SpriteVisual> excludedVisuals)
        {
            return GetHittestVisualHelper(pointerPosition, visuals, layers, excludedVisuals);
        }

        public Visual GetHittestVisual(Point pointerPosition, List<SpriteVisual> visuals, List<Layer> layers, List<SpriteVisual> excludedVisuals)
        {
            List<Visual> visualsList = new List<Visual>();
            foreach (Visual visual in visuals)
            {
                visualsList.Add((Visual)visual);
            }

            return GetHittestVisualHelper(pointerPosition, visualsList, layers, excludedVisuals);
        }

        public Visual GetHittestVisual(Point pointerPosition, VisualCollection visuals, List<Layer> layers, List<SpriteVisual> excludedVisuals)
        {
            List<Visual> visualsList = new List<Visual>();
            foreach (Visual visual in visuals)
            {
                visualsList.Add(visual);
            }

            return GetHittestVisualHelper(pointerPosition, visualsList, layers, excludedVisuals);
        }

        private Visual GetHittestVisualHelper(Point pointerPosition, List<Visual> visuals, List<Layer> layers, List<SpriteVisual> excludedVisuals)
        {
            if (excludedVisuals == null) { excludedVisuals = new List<SpriteVisual>(); }
            List<Visual> hitVisuals = new List<Visual>();
            foreach (Visual visual in visuals)
            {
                if (!excludedVisuals.Contains(visual))
                {
                    var visualXLowerBound = visual.Offset.X;
                    var visualXUpperBound = visual.Offset.X + visual.Size.X;
                    var visualYLowerBound = visual.Offset.Y;
                    var visualYUpperBound = visual.Offset.Y + visual.Size.Y;

                    // Check if clicked
                    if (pointerPosition.X >= visualXLowerBound && pointerPosition.X <= visualXUpperBound &&
                    pointerPosition.Y >= visualYLowerBound && pointerPosition.Y <= visualYUpperBound)
                    {
                        hitVisuals.Add(visual);
                    }
                }
            }

            if (hitVisuals.Count >= 1)
            {
                if (hitVisuals.Count == 1)
                {
                    return hitVisuals.First();
                }
                else
                {
                    // Go through each layer starting at the end (highest) and get the topmost hit visual
                    for (int i = layers.Count - 1; i >= 0; i--)
                    {
                        foreach (Visual v in hitVisuals)
                        {
                            if (layers[i].LayerBackingVisual.Children.Contains(v))
                            {
                                // Convert back to SV or CV
                                if (v.GetType().Name.Equals("SpriteVisual"))
                                {
                                    return (SpriteVisual)v;
                                }
                                else if (v.GetType().Name.Equals("ContainerVisual"))
                                {
                                    return (ContainerVisual)v;
                                }
                                return v;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Given a visual, get it's parent Layer object
        /// </summary>
        public Layer GetParentLayer(List<Layer> layers, Visual visual)
        {
            foreach (Layer layer in layers)
            {
                if (layer.LayerBackingVisual.Children.Contains(visual))
                {
                    return layer;
                }
            }
            return null;
        }

        /// <summary>
        /// Create conceptual layers for the scenario, in order to define layer-specific behavior
        /// </summary>
        abstract public void CreateLayers();

        /// <summary>
        /// Size changed listener helper method
        /// </summary>
        abstract public void SizeChanged();

        /// <summary>
        /// Used to actually create the components of the scenario. Separated from the constructor
        /// in order to provide greater control over scenarios.
        /// </summary>
        abstract public void ImplementScenario(Compositor compositor, SpriteVisual scenarioContainer);

        /// <summary>
        /// Scenario-specific behavior for canvas pointer moved input
        /// </summary>
        virtual public void CanvasPointerMoved(object sender, PointerRoutedEventArgs e, Canvas canvasReference) { }

        /// <summary>
        /// Scenario-specific behavior for canvas pointer pressed input
        /// </summary>
        virtual public void CanvasPointerPressed(object sender, PointerRoutedEventArgs e, Canvas canvasReference) { }

        /// <summary>
        /// Scenario-specific behavior for canvas double tapped input
        /// </summary>
        virtual public void CanvasDoubleTapped(object sender, DoubleTappedRoutedEventArgs e, Canvas canvasReference) { }

    }
}
