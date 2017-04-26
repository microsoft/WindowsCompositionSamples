using Windows.UI.Xaml;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShyHeader : SamplePage
    {
        CompositionPropertySet _props;
        CompositionPropertySet _scrollerPropertySet;
        Compositor _compositor;
        private SpriteVisual _blurredBackgroundImageVisual;
        public ShyHeader()
        {
            this.InitializeComponent();
            Loaded += SamplePage_Loaded;
            SizeChanged += Page_SizeChanged;
        }

        public static string StaticSampleName { get { return "Shy Header"; } }
        public override string SampleName { get { return StaticSampleName; } }
        public override string SampleDescription { get { return "Demonstrates how to use ExpressionAnimations with a ScrollViewer to create a shinking header tied to scroll position."; } }
        public override string SampleCodeUri { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761172"; } }

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the PropertySet that contains the scroll values from MyScrollViewer
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(MyScrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            // Create a PropertySet that has values to be referenced in the ExpressionAnimations below
            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", 150);
            _props.InsertScalar("scaleFactor", 0.7f);

            // Create a blur effect to be animated based on scroll position
            var blurEffect = new GaussianBlurEffect()
            {
                Name = "blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };

            var blurBrush = _compositor.CreateEffectFactory(blurEffect,
                new[] { "blur.BlurAmount" })
                .CreateBrush();

            blurBrush.SetSourceParameter("source", _compositor.CreateBackdropBrush());

            // Create a Visual for applying the blur effect
            _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();
            _blurredBackgroundImageVisual.Brush = blurBrush;
            _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);

            // Insert the blur visual at the right point in the Visual Tree
            ElementCompositionPreview.SetElementChildVisual(OverlayRectangle, _blurredBackgroundImageVisual);

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionAnimation progressAnimation = _compositor.CreateExpressionAnimation("clamp(-scrollingProperties.Translation.Y/props.clampSize, 0, 1)");
            progressAnimation.SetReferenceParameter("scrollingProperties", _scrollerPropertySet);
            progressAnimation.SetReferenceParameter("props", _props);
            _props.StartAnimation("progress", progressAnimation);

            // Create and start an ExpressionAnimation to animate blur radius between 0 and 15 based on progress
            ExpressionAnimation blurAnimation = _compositor.CreateExpressionAnimation("lerp(0, 15, props.progress)");
            blurAnimation.SetReferenceParameter("props", _props);
            _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

            // Get the backing visual for the header so that its properties can be animated
            Visual headerVisual = ElementCompositionPreview.GetElementVisual(Header);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionAnimation headerTranslationAnimation = _compositor.CreateExpressionAnimation("(props.progress < 1 ? 0 : -scrollingProperties.Translation.Y - props.clampSize)");
            headerTranslationAnimation.SetReferenceParameter("props", _props);
            headerTranslationAnimation.SetReferenceParameter("scrollingProperties", _scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            // Create and start an ExpressionAnimation to scale the header during overpan
            ExpressionAnimation headerScaleAnimation = _compositor.CreateExpressionAnimation("lerp(1, 1.25, clamp(scrollingProperties.Translation.Y/50, 0, 1))");
            headerScaleAnimation.SetReferenceParameter("scrollingProperties", _scrollerPropertySet);
            headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);
            headerVisual.StartAnimation("Scale.X", headerScaleAnimation);

            //Set the header's CenterPoint to ensure the overpan scale looks as desired
            headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

            // Get the backing visual for the photo in the header so that its properties can be animated
            Visual photoVisual = ElementCompositionPreview.GetElementVisual(BackgroundRectangle);

            // Create and start an ExpressionAnimation to opacity fade out the image behind the header
            ExpressionAnimation imageOpacityAnimation = _compositor.CreateExpressionAnimation("1-props.progress");
            imageOpacityAnimation.SetReferenceParameter("props", _props);
            photoVisual.StartAnimation("opacity", imageOpacityAnimation);

            // Get the backing visual for the profile picture visual so that its properties can be animated
            Visual profileVisual = ElementCompositionPreview.GetElementVisual(ProfileImage);

            // Create and start an ExpressionAnimation to scale the profile image with scroll position
            ExpressionAnimation scaleAnimation = _compositor.CreateExpressionAnimation("lerp(1, props.scaleFactor, props.progress)");
            scaleAnimation.SetReferenceParameter("props", _props);
            profileVisual.StartAnimation("scale.x", scaleAnimation);
            profileVisual.StartAnimation("scale.y", scaleAnimation);

            // Get backing visuals for the text blocks so that their properties can be animated
            Visual blurbVisual = ElementCompositionPreview.GetElementVisual(Blurb);
            Visual subtitleVisual = ElementCompositionPreview.GetElementVisual(SubtitleBlock);
            Visual moreVisual = ElementCompositionPreview.GetElementVisual(MoreText);

            // Create an ExpressionAnimation that moves between 1 and 0 with scroll progress, to be used for text block opacity
            ExpressionAnimation textOpacityAnimation = _compositor.CreateExpressionAnimation("clamp(1-(props.progress*2), 0, 1)");
            textOpacityAnimation.SetReferenceParameter("props", _props);

            // Start opacity and scale animations on the text block visuals
            blurbVisual.StartAnimation("Opacity", textOpacityAnimation);
            blurbVisual.StartAnimation("scale.x", scaleAnimation);
            blurbVisual.StartAnimation("scale.y", scaleAnimation);

            subtitleVisual.StartAnimation("Opacity", textOpacityAnimation);
            subtitleVisual.StartAnimation("scale.x", scaleAnimation);
            subtitleVisual.StartAnimation("scale.y", scaleAnimation);

            moreVisual.StartAnimation("Opacity", textOpacityAnimation);
            moreVisual.StartAnimation("scale.x", scaleAnimation);
            moreVisual.StartAnimation("scale.y", scaleAnimation);

            // Get the backing visuals for the text and button containers so that their properites can be animated
            Visual textVisual = ElementCompositionPreview.GetElementVisual(TextContainer);
            Visual buttonVisual = ElementCompositionPreview.GetElementVisual(ButtonPanel);

            // When the header stops scrolling it is 150 pixels offscreen.  We want the text header to end up with 50 pixels of its content
            // offscreen which means it needs to go from offset 0 to 100 as we traverse through the scrollable region
            ExpressionAnimation contentOffsetAnimation = _compositor.CreateExpressionAnimation("props.progress * 100");
            contentOffsetAnimation.SetReferenceParameter("props", _props);
            textVisual.StartAnimation("offset.y", contentOffsetAnimation);

            ExpressionAnimation buttonOffsetAnimation = _compositor.CreateExpressionAnimation("props.progress * -100");
            buttonOffsetAnimation.SetReferenceParameter("props", _props);
            buttonOffsetAnimation.SetReferenceParameter("scrollingProperties", _scrollerPropertySet);
            buttonVisual.StartAnimation("Offset.Y", buttonOffsetAnimation);
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_blurredBackgroundImageVisual != null)
            {
                _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);
            }
        }

    }
}
