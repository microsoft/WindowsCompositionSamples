using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace CompositionSampleGallery
{

    public sealed partial class NavigationFlowDestinationPage : Page
    {
        private Compositor _compositor;

        public NavigationFlowDestinationPage()
        {
            InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            var topBorderOffsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            topBorderOffsetAnimation.Duration = TimeSpan.FromSeconds(0.45);
            topBorderOffsetAnimation.Target = "Translation.Y";
            topBorderOffsetAnimation.InsertKeyFrame(0, -450.0f);
            topBorderOffsetAnimation.InsertKeyFrame(1, 0);

            ElementCompositionPreview.SetIsTranslationEnabled(TopBorder, true);
            ElementCompositionPreview.SetImplicitShowAnimation(TopBorder, topBorderOffsetAnimation);

            var listContentOffsetAnimation = _compositor.CreateScalarKeyFrameAnimation();
            listContentOffsetAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            listContentOffsetAnimation.DelayTime = TimeSpan.FromSeconds(0.2);
            listContentOffsetAnimation.Duration = TimeSpan.FromSeconds(0.45);
            listContentOffsetAnimation.Target = "Translation.Y";
            listContentOffsetAnimation.InsertKeyFrame(0, 50.0f);
            listContentOffsetAnimation.InsertKeyFrame(1, 0);

            var listContentOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
            listContentOpacityAnimation.Duration = TimeSpan.FromSeconds(0.4);
            listContentOpacityAnimation.Target = "Opacity";
            listContentOpacityAnimation.InsertKeyFrame(0, 0);
            listContentOpacityAnimation.InsertKeyFrame(0.25f, 0);
            listContentOpacityAnimation.InsertKeyFrame(1, 1);

            var listContentShowAnimations = _compositor.CreateAnimationGroup();
            listContentShowAnimations.Add(listContentOffsetAnimation);
            listContentShowAnimations.Add(listContentOpacityAnimation);

            ElementCompositionPreview.SetIsTranslationEnabled(ListContent, true);
            ElementCompositionPreview.SetImplicitShowAnimation(ListContent, listContentShowAnimations);

            var listContentExitAnimation = _compositor.CreateScalarKeyFrameAnimation();
            listContentExitAnimation.Target = "Translation.Y";
            listContentExitAnimation.InsertKeyFrame(1, 30);
            listContentExitAnimation.Duration = TimeSpan.FromSeconds(0.4);

            ElementCompositionPreview.SetIsTranslationEnabled(ListContent, true);
            ElementCompositionPreview.SetImplicitHideAnimation(ListContent, listContentExitAnimation);

            var topBorderExitAnimation = _compositor.CreateScalarKeyFrameAnimation();
            topBorderExitAnimation.Target = "Translation.Y";
            topBorderExitAnimation.InsertKeyFrame(1, -30);
            topBorderExitAnimation.Duration = TimeSpan.FromSeconds(0.4);

            ElementCompositionPreview.SetIsTranslationEnabled(TopBorder, true);
            ElementCompositionPreview.SetImplicitHideAnimation(TopBorder, topBorderExitAnimation);

            var fadeOut = _compositor.CreateScalarKeyFrameAnimation();
            fadeOut.Target = "Opacity";
            fadeOut.InsertKeyFrame(1, 0);
            fadeOut.Duration = TimeSpan.FromSeconds(0.4);

            // Set Z index to force this page to the top during the hide animation
            Canvas.SetZIndex(this, 1);
            ElementCompositionPreview.SetImplicitHideAnimation(this, fadeOut);
        }

        private void DestinationPage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BorderDest", BorderDest);
            Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ConnectedAnimationService
                .GetForCurrentView()
                .GetAnimation("BorderSource")
                .TryStart(BorderDest, new[] { DescriptionRoot });

            ItemTextBlock.Text = $"Item {(int)e.Parameter}";
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("BorderDest", BorderDest);
        }
    }
}
