using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;

namespace DepthDemo
{
    public class Layer
    {
        private Compositor _compositor;
        private SpriteVisual _backingVisual;
        private CompositionAnimationGroup _shadowAnimationGroup;
        private CompositionAnimationGroup _visualAnimationGroup;
        private CompositionAnimationGroup _reversedShadowAnimationGroup;
        private CompositionAnimationGroup _reversedVisualAnimationGroup;


        public int Identifier { get; set; }
        public Vector3 Offset { get; set; }
        public CompositionColorBrush LayerColor;
        public DepthTreatmentConfigurations DepthTreatment { get; set; }
        public List<Visual> FocusedVisuals { get; set; }
        public List<Visual> ElevatedVisuals { get; set; }
        public SpriteVisual LayerBackingVisual
        {
            get { return _backingVisual; }
            set { _backingVisual = LayerBackingVisual; }
        }


        public Layer(Compositor compositor, int identifier, Vector3 offset, Vector2 size,
            ContainerVisual parent, CompositionColorBrush layerColor)
        {
            _compositor = compositor;
            LayerColor = layerColor;
            FocusedVisuals = new List<Visual>();
            ElevatedVisuals = new List<Visual>();

            SpriteVisual x = _compositor.CreateSpriteVisual();
            x.Size = new Vector2(size.X, size.Y);
            x.Offset = Offset = offset;
            Identifier = identifier;
            x.Comment = Identifier.ToString();

            _backingVisual = x;
            parent.Children.InsertAtTop(x);
        }

        public void SetDepthTreatments(DepthTreatmentConfigurations depthTreatmentConfig)
        {
            DepthTreatment = depthTreatmentConfig;
            RefreshApplyDepthTreatments();

            // Shadow Focus Animation
            _shadowAnimationGroup = DepthTreatment.ShadowTreatment.GetShadowFocusAnimations(_compositor, this);
            // Additional Focus Animations
            _visualAnimationGroup = DepthTreatment.GetVisualFocusAnimations(_compositor, this);

            // Create reversed animation groups to run on unfocus
            _reversedShadowAnimationGroup = _compositor.CreateAnimationGroup();
            _reversedVisualAnimationGroup = _compositor.CreateAnimationGroup();
            foreach (KeyFrameAnimation animation in _shadowAnimationGroup)
            {
                animation.Direction = AnimationDirection.Reverse;
                _reversedShadowAnimationGroup.Add(animation);
            }
            foreach (KeyFrameAnimation animation in _visualAnimationGroup)
            {
                animation.Direction = AnimationDirection.Reverse;
                _reversedVisualAnimationGroup.Add(animation);
            }
        }

        public void RefreshApplyDepthTreatments()
        {
            // Apply treatments to all children in layer
            var children = _backingVisual.Children;
            foreach (Visual child in children)
            {
                if (DepthTreatment.ShadowTreatment != null && child.GetType() == typeof(SpriteVisual))
                {
                    var shadowTreatment = DepthTreatment.ShadowTreatment;

                    DropShadow shadow = _compositor.CreateDropShadow();
                    shadow.BlurRadius = shadowTreatment.BlurRadius;
                    shadow.Offset = shadowTreatment.Offset;
                    shadow.Color = shadowTreatment.ShadowColor;
                    shadow.Opacity = ConfigurationConstants.ShadowOpacity;

                    ((SpriteVisual)child).Shadow = shadow;
                }
            }
        }

        public void AnimateFocusTreatment(SpriteVisual target)
        {
            FocusedVisuals.Add(target);

            target.Shadow.StartAnimationGroup(_shadowAnimationGroup);
            target.StartAnimationGroup(_visualAnimationGroup);
        }

        public void AnimationUnfocusTreatment(SpriteVisual target)
        {
            if (FocusedVisuals.Contains(target))
            {
                if (target.Shadow != null && _reversedShadowAnimationGroup != null)
                {
                    target.Shadow.StartAnimationGroup(_reversedShadowAnimationGroup);
                }
                if (_reversedVisualAnimationGroup != null)
                {
                    target.StartAnimationGroup(_reversedVisualAnimationGroup);
                }

                FocusedVisuals.Remove(target);
            }
        }

        public void AnimateAddedVisual(SpriteVisual target)
        {
            float currShadowBlurRadius = 0f;
            Vector3 currShadowOffset = new Vector3();

            if (target.Shadow != null)
            {
                currShadowBlurRadius = ((DropShadow)(target.Shadow)).BlurRadius;
                currShadowOffset = ((DropShadow)(target.Shadow)).Offset;
            }
            else
            {
                DropShadow shadow = _compositor.CreateDropShadow();
                shadow.Color = ConfigurationConstants.ShadowColor;
                shadow.Opacity = ConfigurationConstants.ShadowOpacity;
                target.Shadow = shadow;
            }

            // Create AnimationGroup for shadow change animation
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();

            // Animate shadow blur radius change
            ScalarKeyFrameAnimation shadowBlurAnimation = _compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.InsertKeyFrame(0.0f, currShadowBlurRadius);
            shadowBlurAnimation.InsertKeyFrame(1.0f, DepthTreatment.ShadowTreatment.BlurRadius);
            shadowBlurAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            shadowBlurAnimation.Target = "BlurRadius";
            animationGroup.Add(shadowBlurAnimation);

            // Animate shadow offset change
            Vector3KeyFrameAnimation shadowOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            shadowOffsetAnimation.InsertKeyFrame(0.0f, currShadowOffset);
            shadowOffsetAnimation.InsertKeyFrame(1.0f, DepthTreatment.ShadowTreatment.Offset);
            shadowOffsetAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            shadowOffsetAnimation.Target = "Offset";
            animationGroup.Add(shadowOffsetAnimation);

            target.Shadow.StartAnimationGroup(animationGroup);

            AnimateToLayerHelper(target);
        }

        private void AnimateToLayerHelper(SpriteVisual target)
        {
            // Create AnimationGroup for target visual propery animation
            CompositionAnimationGroup animationGroup = _compositor.CreateAnimationGroup();

            // Scale
            Vector3KeyFrameAnimation scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0.0f, target.Scale);
            scaleAnimation.InsertKeyFrame(1.0f, this.DepthTreatment.ChildScale);
            scaleAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            scaleAnimation.Target = "Scale";
            animationGroup.Add(scaleAnimation);

            target.StartAnimationGroup(animationGroup);


            // Update item color to match items in new layer. Preserve content if any.
            var brushType = target.Brush.GetType();
            if (brushType != typeof(CompositionSurfaceBrush) && brushType != typeof(CompositionEffectBrush))
            {
                target.Brush = LayerColor;
            }
        }
    }
}
