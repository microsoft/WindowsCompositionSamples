using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace ImplicitAnimations
{
    public static class UIElementExtensionMethods
    {
        public static void EnableLayoutImplicitAnimations(this UIElement element)
        {
            Compositor compositor;
            var result = ElementCompositionPreview.GetElementVisual(element);
            compositor = result.Compositor;
#if SDKVERSION_INSIDER
            var elementImplicitAnimation = compositor.CreateImplicitAnimationCollection();
            elementImplicitAnimation["Offset"] = createOffsetAnimation(compositor);
            result.ImplicitAnimations = elementImplicitAnimation;
#endif
        }

        private static KeyFrameAnimation createOffsetAnimation(Compositor compositor)
        {
            Vector3KeyFrameAnimation kf = compositor.CreateVector3KeyFrameAnimation();
            kf.InsertExpressionKeyFrame(1.0f, "FinalValue");
            kf.Duration = TimeSpan.FromSeconds(0.9);
            return kf;
        }
    }
}
