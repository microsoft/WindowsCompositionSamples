using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;

namespace DepthDemo
{
    /// <summary>
    /// Configuration for constant values that are consistent across all scenarios
    /// </summary>
    class ConfigurationConstants
    {
        public ConfigurationConstants() { }

        public static int ZOffsetSpacingIncrement = 20;

        public static Color ShadowColor = Colors.DarkSlateGray;

        public static TimeSpan FocusAnimationDuration = TimeSpan.FromSeconds(0.5);
        public static TimeSpan NavAnimationDuration = TimeSpan.FromSeconds(0.25);

        public static float ShadowOpacity = 0.7f;

        public static float BlurFocusFloat = 10.0f;
    }


    public class DepthTreatmentConfigurations
    {
        private Layer _associatedLayer;

        public Vector3 ChildScale { get; set; }

        public ShadowTreatment ShadowTreatment { get; set; }

        // Focus treatment increase for top layer objects
        private float _focusScaleIncreaseFactor;

        public DepthTreatmentConfigurations(Layer associatedLayer, Vector3 childScale, float focusScaleIncreaseFactor, ShadowTreatment shadowTreatment = null)
        {
            _associatedLayer = associatedLayer;
            _focusScaleIncreaseFactor = focusScaleIncreaseFactor;
            ChildScale = childScale;
            ShadowTreatment = shadowTreatment;
        }

        public CompositionAnimationGroup GetVisualFocusAnimations(Compositor compositor, Layer layer)
        {
            var oldscale = ChildScale;
            var newScale = new Vector3(oldscale.X * _focusScaleIncreaseFactor, oldscale.Y * _focusScaleIncreaseFactor, oldscale.Z);

            // Create AnimationGroup 
            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();

            // Scale
            Vector3KeyFrameAnimation scaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0.0f, oldscale);
            scaleAnimation.InsertKeyFrame(1.0f, newScale);
            scaleAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            scaleAnimation.Target = "Scale";
            animationGroup.Add(scaleAnimation);

            return animationGroup;
        }
    }


    public class ShadowTreatment
    {
        public Color ShadowColor { get { return ConfigurationConstants.ShadowColor; } }
        public int BlurRadius { get; set; }
        public Vector3 Offset { get; set; }

        private int _focusShadowBlurRadiusIncreaseAmount;
        private int _focusShadowOffsetIncreaseAmount;

        public ShadowTreatment(int blurRadius, Vector3 offset, int focusShadowBlurRadiusIncrease,
            int focusShadowOffsetIncrease)
        {
            BlurRadius = blurRadius;
            Offset = offset;

            _focusShadowBlurRadiusIncreaseAmount = focusShadowBlurRadiusIncrease;
            _focusShadowOffsetIncreaseAmount = focusShadowOffsetIncrease;
        }

        public CompositionAnimationGroup GetShadowFocusAnimations(Compositor compositor, Layer layer)
        {
            var newShadowBlurRadius = BlurRadius + _focusShadowBlurRadiusIncreaseAmount;
            var oldShadowOffset = Offset;
            var additionalShadowOffsetAmount = _focusShadowOffsetIncreaseAmount;
            var newShadowOffset = new Vector3(oldShadowOffset.X + additionalShadowOffsetAmount, oldShadowOffset.Y +
                additionalShadowOffsetAmount, oldShadowOffset.Z + additionalShadowOffsetAmount);


            // Create AnimationGroup 
            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();

            // Blur Radius
            ScalarKeyFrameAnimation shadowBlurAnimation = compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.InsertKeyFrame(0.0f, BlurRadius);
            shadowBlurAnimation.InsertKeyFrame(1.0f, newShadowBlurRadius);
            shadowBlurAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            shadowBlurAnimation.Target = "BlurRadius";
            animationGroup.Add(shadowBlurAnimation);

            // Offset
            Vector3KeyFrameAnimation shadowOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            shadowOffsetAnimation.InsertKeyFrame(0.0f, Offset);
            shadowOffsetAnimation.InsertKeyFrame(1.0f, newShadowOffset);
            shadowOffsetAnimation.Duration = ConfigurationConstants.FocusAnimationDuration;
            shadowOffsetAnimation.Target = "Offset";
            animationGroup.Add(shadowOffsetAnimation);

            return animationGroup;
        }
    }
}
