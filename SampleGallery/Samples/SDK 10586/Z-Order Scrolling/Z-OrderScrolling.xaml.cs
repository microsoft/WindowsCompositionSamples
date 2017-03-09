//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Composition;
using SamplesCommon;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class Z_OrderScrolling : SamplePage
    {
        public Z_OrderScrolling()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        public static string        StaticSampleName    { get { return "Z-Order Scrolling"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Demonstrates how to use a ScrollViewer and ExpressionAnimations to drive Visual properties like Scale, Offset, and Opacity based off of the scroll position. Especially notice how the profile image swaps underneath the header."; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761168"; } }

        /// <summary>
        /// MainPage_Loaded is used for all of our initialization to keep the UI thread mostly free.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(sender as UIElement).Compositor;
            var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(Scroller);

            var uri = new Uri("ms-appx:///Samples/SDK 10586/Z-Order Scrolling/RollingWaves.jpg");
            _blurSurface = ImageLoader.Instance.LoadFromUri(uri, Size.Empty, ApplyBlurEffect);
            ParallaxingImage.Source = uri;
            ParallaxingImage.Brush = InitializeCrossFadeEffect();

            var maskedBrush = InitializeCompositeEffect();

            ParallaxingImage.SizeChanged += (s, ev) => { UpdateSizes(); };

            InitializeBackgroundImageVisual(scrollProperties);
            InitializeFrontVisual(scrollProperties, maskedBrush);
            InitializeBehindVisual(scrollProperties, maskedBrush);

            UpdateSizes();
        }

        /// <summary>
        /// MainPage_Unloaded is used to dispose of the ManagedSurfaces.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _profilePictureSurface.Dispose();
            _circleMaskSurface.Dispose();
            _blurSurface.Dispose();
        }

        /// <summary>
        /// Create the Visual that will host the profile background image and setup
        /// the animations that will drive it.
        /// </summary>  
        /// /// <param name="scrollProperties">A property set who has Translation.Y specified - typically the return from ElementCompositionPreview.GetScrollViewerManipulationPropertySet(...).</param>
        private void InitializeBackgroundImageVisual(CompositionPropertySet scrollProperties)
        {
            //
            // Get the visual for the background image, and let it parallax up until the BackgroundPeekSize.
            // BackgroundPeekSize is later defined as the amount of the image to leave showing.
            //
            _backgroundVisual = ElementCompositionPreview.GetElementVisual(ParallaxingImage);

            //
            // If the scrolling is positive (i.e., bouncing), don't translate at all.  Then check to see if
            // we have parallaxed as far as we should go.  If we haven't, keep parallaxing otherwise use
            // the scrolling translation to keep the background stuck with the background peeking out.
            //
            _backgroundTranslationAnimation = _compositor.CreateExpressionAnimation(
                "BaseOffset + (scrollingProperties.Translation.Y > 0 ? 0 : " +
                    "( (1-ParallaxRatio) * -scrollingProperties.Translation.Y) < BackgroundPeekSize ? " +
                            "(ParallaxRatio * -scrollingProperties.Translation.Y) : -BackgroundPeekSize-scrollingProperties.Translation.Y)");
            _backgroundTranslationAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
            _backgroundTranslationAnimation.SetScalarParameter("ParallaxRatio", _parallaxRatio);

            _backgroundScaleAnimation = _compositor.CreateExpressionAnimation(
                "Lerp(" +
                        "1," +
                        "1+Amount," +
                        "Clamp(scrollingProperties.Translation.Y/50,0,1)" +
                    ")");
            _backgroundScaleAnimation.SetScalarParameter("Amount", _backgroundScaleAmount);
            _backgroundScaleAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

            _backgroundBlurAnimation = _compositor.CreateExpressionAnimation(
                "Clamp(-scrollingProperties.Translation.Y / (BackgroundPeekSize * .5),0,1)");
            _backgroundBlurAnimation.SetScalarParameter("Amount", _backgroundScaleAmount);
            _backgroundBlurAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

            _backgroundInverseBlurAnimation = _compositor.CreateExpressionAnimation(
                "1-Clamp(-scrollingProperties.Translation.Y / (BackgroundPeekSize * .5),0,1)");
            _backgroundInverseBlurAnimation.SetScalarParameter("Amount", _backgroundScaleAmount);
            _backgroundInverseBlurAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);


            //
            // We want to keep the Name/Title text in the middle of the background image.  To start with,
            // we add the centerpoint - since we move the anchor point to (.5,/5) which XAML layout doesn't
            // expect - and the background offset offset to keep the element positioned with the background.
            // we then lerp between its existing position and its final position based on parallaxed distance
            // traveled.
            //
            _profileContentVisual = ElementCompositionPreview.GetElementVisual(ProfileContent);
            _profileContentTranslationAnimation = _compositor.CreateExpressionAnimation(
                "(-scrollingProperties.Translation.Y + Background.Offset.Y + Background.Size.Y/2)/2");
            _profileContentTranslationAnimation.SetReferenceParameter("Background", _backgroundVisual);
            _profileContentTranslationAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

            _profileContentScaleAnimation = _compositor.CreateExpressionAnimation(
                    "Lerp(1,ShrinkRatio, " +
                        "Clamp( (Background.Offset.Y - Background.Size.Y/2) / (Background.Size.Y - BackgroundPeekSize),0,1))"
                );
            _profileContentScaleAnimation.SetScalarParameter("ShrinkRatio", _contentShrinkRatio);
            _profileContentScaleAnimation.SetReferenceParameter("Background", _backgroundVisual);
        }


        /// <summary>
        /// Create the _frontVisual and the animations that drive it.
        /// </summary>
        /// <param name="scrollProperties">A property set who has Translation.Y specified - typically the return from ElementCompositionPreview.GetScrollViewerManipulationPropertySet(...).</param>
        /// <param name="maskedBrush">This is the brush that will be set on _frontVisual</param>
        private void InitializeFrontVisual(CompositionPropertySet scrollProperties, CompositionEffectBrush maskedBrush)
        {
            //
            // Create  the _frontVisual, set the brush on it, and attach it to the scrollViewer.  Setting it as the 
            // child visual on scrollviewer will put it in front of all the scrollViewer content.
            //
            _frontVisual = _compositor.CreateSpriteVisual();
            _frontVisual.Brush = maskedBrush;
            ElementCompositionPreview.SetElementChildVisual(MainGrid, _frontVisual);

            //
            // "Terms" in the following expression:
            //
            //      (CrossoverTranslation + scrollingProperties.Translation.Y)/CrossoverTranslation  
            //
            //              Since scrollingProperties.Translation.Y is negative.  This creates a normalized value that goes from 
            //              0 to 1 between no scrolling and the CrossoverTranslation.
            //
            _frontPropertiesScalarScaleAnimation = _compositor.CreateExpressionAnimation(
                    "Lerp(minClamp, maxClamp, Clamp((CrossoverTranslation + scrollingProperties.Translation.Y)/CrossoverTranslation,0,1))"
                );
            _frontPropertiesScalarScaleAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
            _frontPropertiesScalarScaleAnimation.SetScalarParameter("minClamp", _finalScaleAmount);
            _frontPropertiesScalarScaleAnimation.SetScalarParameter("maxClamp", _initialScaleAmount);

            //
            // The previous equation calculates a scalar which is later bound to ScalarScale in FrontVisual's Properties.  
            // This equation uses that scalar to construct a new vector3 to set to animate the scale of the visual itself.
            //
            var vector3ScaleAnimation = _compositor.CreateExpressionAnimation("Vector3(Properties.ScalarScale, Properties.ScalarScale, 1)");
            _frontVisual.Properties.InsertScalar("ScalarScale", 1);
            vector3ScaleAnimation.SetReferenceParameter("Properties", _frontVisual.Properties);
            _frontVisual.StartAnimation("Scale", vector3ScaleAnimation);

            //
            // This equation controls whether or not the FrontVisual is visibile via opacity.  It uses a simple ternary operator
            // to pick between 100% and 0% opacity based on the position being before the crossover point.
            //
            _frontVisibilityAnimation =
                _compositor.CreateExpressionAnimation("-scrollingProperties.Translation.Y <= CrossoverTranslation ? 1 : 0");
            _frontVisibilityAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

            _frontTranslationAnimation =
                _compositor.CreateExpressionAnimation("scrollingProperties.Translation.Y > 0 ? BaseOffset : BaseOffset-scrollingProperties.Translation.Y");
            _frontTranslationAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

        }

        /// <summary>
        /// Create the _backVisual and the animations that drive it.
        /// </summary>
        /// <param name="scrollProperties">A property set who has Translation.Y specified - typically the return from ElementCompositionPreview.GetScrollViewerManipulationPropertySet(...).</param>
        /// <param name="maskedBrush">This is the brush that will be set on _frontVisual</param>
        private void InitializeBehindVisual(CompositionPropertySet scrollProperties, CompositionEffectBrush maskedBrush)
        {
            //
            // Create  the _frontVisual, set the brush on it, and attach it to the BackGrid.  BackGrid is an empty grid
            // that is visually behind the profile background (the waves). Therefore, setting it as the
            // child visual on the BackGrid will put it behind all the scrollViewer content.
            //
            _backVisual = _compositor.CreateSpriteVisual();
            _backVisual.Brush = maskedBrush;
            ElementCompositionPreview.SetElementChildVisual(BackGrid, _backVisual);

            //
            // This equation controls whether or not the FrontVisual is visibile via opacity.  It uses a simple ternary operator
            // to pick between 100% and 0% opacity based on the position being after the crossover point.
            //
            _backVisual.Scale = new Vector3(_finalScaleAmount, _finalScaleAmount, 1);
            _behindOpacityAnimation = _compositor.CreateExpressionAnimation("-scrollingProperties.Translation.Y <= CrossoverTranslation ? 0 : 1");
            _behindOpacityAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);

            //
            // "Terms" and explanation of the following expression:
            //
            //      (initialOffset - scrollingProperties.Translation.Y)
            //
            //              Since _backVisual is a child of the scroller, the initial position minus the scrolling offset keeps the content
            //              in its original location.
            //              
            //      2 * (CrossoverTranslation + scrollingProperties.Translation.Y)
            //
            //              Since scrollingProperties.Translation.Y is negative when this expression is visibile, this term calculates the 
            //              distance past the crossover point and scales it.  The scale causes the content to move faster than the scrolling.
            //              Since this term evaluates to zero at the crossover point and the term above keeps the content from moving, when 
            //              the visibility swaps between frontVisual and backVisual, they are perfectly aligned.
            //
            _behindTranslateAnimation = _compositor.CreateExpressionAnimation(
            "(BaseOffset - scrollingProperties.Translation.Y) + 2 * (CrossoverTranslation + scrollingProperties.Translation.Y)");
            _behindTranslateAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
        }


        private void UpdateSizes()
        {
            // 
            // First, calculate all of the new sizes and distances we will need.
            //
            var backgroundImageSize = new Vector2((float)ParallaxingImage.ActualWidth, (float)ParallaxingImage.ActualHeight);
            var profileImageSize = new Vector2((float)ParallaxingImage.ActualWidth, (float)ParallaxingImage.ActualHeight) * _initialScaleAmount;
            var crossoverTranslation = (profileImageSize.Y * _finalScaleAmount / 2 + _followMargin) / (1 - _parallaxRatio);
            var offset = new Vector3((float)ParallaxingImage.ActualWidth / 2, (float)ParallaxingImage.ActualHeight, 0);
            var backgroundPeekSize = backgroundImageSize.Y * _backgroundShowRatio;

            // Push the text content down to make room for the image overhanging.
            //
            ContentPanel.Margin = new Thickness(5, (float)ParallaxingImage.ActualHeight + profileImageSize.Y / 2, 5, 5);
            
            //
            // Resolve all of the property parameters and references on the frontVisual animations and start them.
            //
            _frontVisual.AnchorPoint = new Vector2(.5f, .5f);
            _frontVisual.Offset = offset;
            _frontVisual.Size = profileImageSize;
            _frontTranslationAnimation.SetScalarParameter("BaseOffset", offset.Y);
            _frontPropertiesScalarScaleAnimation.SetScalarParameter("CrossoverTranslation", crossoverTranslation);
            _frontVisibilityAnimation.SetScalarParameter("CrossoverTranslation", crossoverTranslation);
            _frontVisual.Properties.StartAnimation("ScalarScale", _frontPropertiesScalarScaleAnimation);
            _frontVisual.StartAnimation("Opacity", _frontVisibilityAnimation);
            _frontVisual.StartAnimation("Offset.Y", _frontTranslationAnimation);


            //
            // Resolve all of the property parameters and references on the backVisual animations and start them.
            //
            _backVisual.AnchorPoint = new Vector2(.5f, .5f);
            _backVisual.Offset = offset;
            _backVisual.Size = profileImageSize;
            _behindTranslateAnimation.SetScalarParameter("BaseOffset", offset.Y);
            _behindTranslateAnimation.SetScalarParameter("CrossoverTranslation", crossoverTranslation);
            _behindOpacityAnimation.SetScalarParameter("CrossoverTranslation", crossoverTranslation);

            _backVisual.StartAnimation("Opacity", _behindOpacityAnimation);
            _backVisual.StartAnimation("Offset.Y", _behindTranslateAnimation);

            //
            // Resolve all property parameters and references on _backgroundVisual
            //
            _backgroundTranslationAnimation.SetScalarParameter("BackgroundPeekSize", backgroundPeekSize);
            _backgroundVisual.Size = backgroundImageSize;
            _backgroundVisual.AnchorPoint = new Vector2(.5f, .5f);
            _backgroundVisual.CenterPoint = new Vector3(backgroundImageSize / 2, 1);
            _backgroundTranslationAnimation.SetScalarParameter("BaseOffset", backgroundImageSize.Y / 2);
            _backgroundVisual.Offset = new Vector3(backgroundImageSize / 2, 0);
            _backgroundVisual.StartAnimation("Offset.Y", _backgroundTranslationAnimation);
            _backgroundVisual.StartAnimation("Scale.X", _backgroundScaleAnimation);
            _backgroundVisual.StartAnimation("Scale.Y", _backgroundScaleAnimation);

            _backgroundBlurAnimation.SetScalarParameter("BackgroundPeekSize", backgroundPeekSize);
            _backgroundInverseBlurAnimation.SetScalarParameter("BackgroundPeekSize", backgroundPeekSize);
            ParallaxingImage.Brush.StartAnimation("Arithmetic.Source1Amount", _backgroundInverseBlurAnimation);
            ParallaxingImage.Brush.StartAnimation("Arithmetic.Source2Amount", _backgroundBlurAnimation);

            //
            // Resolve all property parameters and references on _profileContentVisual
            //
            _profileContentVisual.Size = new Vector2((float)ProfileContent.ActualWidth, (float)ProfileContent.ActualHeight);
            _profileContentVisual.Offset = new Vector3(0);
            _profileContentVisual.Offset = new Vector3(_profileContentVisual.Size / 2, 0);
            _profileContentScaleAnimation.SetScalarParameter("BackgroundPeekSize", backgroundPeekSize);
            _profileContentVisual.StartAnimation("Offset.Y", _profileContentTranslationAnimation);
            _profileContentVisual.StartAnimation("Scale.X", _profileContentScaleAnimation);
            _profileContentVisual.StartAnimation("Scale.Y", _profileContentScaleAnimation);
        }

        /// <summary>
        /// Function is responsible for creating the circular alpha masked profile brush
        /// </summary>
        /// <returns></returns>
        private CompositionEffectBrush InitializeCompositeEffect()
        {
            //
            // Create a simple Composite Effect, using DestinationIn (S * DA), and two named sources - image and mask.
            //
            var effect = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    new CompositionEffectSourceParameter("image"),
                    new CompositionEffectSourceParameter("mask")
                }

            };
            var factory = _compositor.CreateEffectFactory(effect);
            var brush = factory.CreateBrush();

            //
            // Load in the profile picture as a brush using the Composition Toolkit.
            //
            _profilePictureSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Samples/SDK 10586/Z-Order Scrolling/teched3ae5a27b-4f78-e111-94ad-001ec953730b.jpg"));
            brush.SetSourceParameter("image", _profilePictureSurface.Brush);

            //
            // Load in the circular mask picture asx a brush using the composition Toolkit.
            //
            _circleMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
            brush.SetSourceParameter("mask", _circleMaskSurface.Brush);

            return brush;

        }

        /// <summary>
        /// Function is responsible for creating the circular alpha masked profile brush
        /// </summary>
        /// <returns></returns>
        private CompositionEffectBrush InitializeCrossFadeEffect()
        {
            var graphicsEffect = new ArithmeticCompositeEffect
            {
                Name = "Arithmetic",
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source1Amount = 1,
                Source2 = new CompositionEffectSourceParameter("BlurImage"),
                Source2Amount = 0,
                MultiplyAmount = 0
            };

            var factory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "Arithmetic.Source1Amount", "Arithmetic.Source2Amount" });

            CompositionEffectBrush crossFadeBrush = factory.CreateBrush(); ;
            crossFadeBrush.SetSourceParameter("ImageSource", ParallaxingImage.SurfaceBrush);
            crossFadeBrush.SetSourceParameter("BlurImage", _blurSurface.Brush);

            return crossFadeBrush;
        }

        void ApplyBlurEffect(CompositionDrawingSurface surface, CanvasBitmap bitmap, Windows.UI.Composition.CompositionGraphicsDevice device)
        {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Source = bitmap,
                BlurAmount = 40.0f,
                BorderMode = EffectBorderMode.Hard,
            };
            
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(Color.FromArgb(255, 255, 255, 255));
                ds.DrawImage(blurEffect);
            }
        }

        private Compositor _compositor;

        private SpriteVisual _backVisual;
        private ExpressionAnimation _behindOpacityAnimation;
        private ExpressionAnimation _behindTranslateAnimation;

        private SpriteVisual _frontVisual;
        private ExpressionAnimation _frontTranslationAnimation;
        private ExpressionAnimation _frontPropertiesScalarScaleAnimation;
        private ExpressionAnimation _frontVisibilityAnimation;

        private Visual _backgroundVisual;
        private ExpressionAnimation _backgroundTranslationAnimation;
        private ExpressionAnimation _backgroundScaleAnimation;
        private ExpressionAnimation _backgroundBlurAnimation;
        private ExpressionAnimation _backgroundInverseBlurAnimation;

        private Visual _profileContentVisual;
        private ExpressionAnimation _profileContentTranslationAnimation;
        private ExpressionAnimation _profileContentScaleAnimation;

        private ManagedSurface _circleMaskSurface;
        private ManagedSurface _profilePictureSurface;
        private ManagedSurface _blurSurface;

        private float _initialScaleAmount = .8f;
        private float _finalScaleAmount = .4f;
        private float _followMargin = 20f;
        private float _backgroundShowRatio = .7f;
        private float _backgroundScaleAmount = .25f;
        private float _parallaxRatio = .2f;
        private float _contentShrinkRatio = .7f;
    }
}
