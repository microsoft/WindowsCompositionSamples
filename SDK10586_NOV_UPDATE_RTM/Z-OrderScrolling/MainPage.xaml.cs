using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
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
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Composition.Toolkit;
using System.Numerics;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Z_ORderScrolling
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Method used to walk throughthe tree doing a search to find a child element of a given type.
        /// </summary>
        /// <typeparam name="T">The type of the element to retrieve.</typeparam>
        /// <param name="rootElement">The root to start the search at.</param>
        /// <returns></returns>
        private static T GetChildElement<T>(DependencyObject rootElement)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(rootElement);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(rootElement, i);
                if ((current.GetType()).Equals(typeof(T)))
                {
                    return (T)Convert.ChangeType(current, typeof(T));
                }
            }

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(rootElement, i);
                T temp = GetChildElement<T>(current);
                if (temp != null)
                    return temp;
            }

            return default(T);
        }

        /// <summary>
        /// MainPage_Loaded is used for all of our initialization to keep the UI thread mostly free.
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(sender as UIElement).Compositor;
            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);
            var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(Scroller);

            var maskedBrush = InitializeCompositeEffect();

            ParallaxingImage.SizeChanged += (s, ev) => { UpdateSizes(); };

            InitializeBackgroundImageVisual(scrollProperties);
            InitializeFrontVisual(scrollProperties, maskedBrush);
            InitializeBehindVisual(scrollProperties, maskedBrush);

            _scrollBar = GetChildElement<ScrollBar>(Scroller);

            UpdateSizes();
        }

        /// <summary>
        /// Create the Visual that will host the profile background image and setup
        /// the animations that will drive it.
        /// </summary>  
        /// /// <param name="scrollProperties">A property set who has Translation.Y specified - typically the return from ElementCompositionPreview.GetScrollViewerManipulationPropertySet(...).</param>
        private void InitializeBackgroundImageVisual(CompositionPropertySet scrollProperties)
        {
            //
            // Get the visual for the background image, and let it parallax up until the ParallaxPoint.
            // ParallaxPoint is later defined as the amount of the image to leave showing.
            //
            _backgroundVisual = ElementCompositionPreview.GetElementVisual(ParallaxingImage);
            _backgroundTranslationAnimation = _compositor.CreateExpressionAnimation(
                "BaseOffset + Vector3(" +
                                      "0," +

                                      "scrollingProperties.Translation.Y > 0 ? 0 :" +
                                            "(ParallaxRatio * -scrollingProperties.Translation.Y) < ParallaxPoint ? " +
                                                "(ParallaxRatio * scrollingProperties.Translation.Y) : (-ParallaxPoint)," +

                                      "0)");
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

            //
            // We want to keep the Name/Title text in the middle of the background image.  To start with,
            // we add the centerpoint - since we move the anchor point to (.5,/5) which XAML layout doesn't
            // expect - and the background offset offset to keep the element positioned with the background.
            // we then lerp between its existing position and its final position based on parallaxed distance
            // traveled.
            //
            _profileContentVisual = ElementCompositionPreview.GetElementVisual(ProfileContent);
            _profileContentTranslationAnimation = _compositor.CreateExpressionAnimation(
                "Vector3(Target.Size.X/2, Target.Size.Y/2 + Background.Offset.Y - Background.Size.Y/2, 0) + " +
                        "Lerp(" +
                                "Vector3(0, 0, 0)," +
                                "Vector3(0, 1+ShowRatio/2 * Background.Size.Y ,0)" +
                                "Clamp( (BaseOffset.Y - Background.Offset.Y)/ParallaxPoint,0,1)" +
                            ")");
            _profileContentTranslationAnimation.SetReferenceParameter("Target", _profileContentVisual);
            _profileContentTranslationAnimation.SetReferenceParameter("Background", _backgroundVisual);
            _profileContentTranslationAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
            _profileContentTranslationAnimation.SetScalarParameter("ShowRatio", _backgroundShowRatio);
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
            // "Terms" and explanation of the following expression:
            //
            //      (CrossoverTranslation + scrollingProperties.Translation.Y)/CrossoverTranslation  
            //
            //              Since scrollingProperties.Translation.Y is negative.  This creates a normalized value that goes from 
            //              0 to 1 between no scrolling and the CrossoverTranslation.
            //
            //      (maxClamp - minClamp)
            //
            //              This calculates the difference between the two clamps.  This will be scaled by the normalized value
            //              above.
            //
            //      minClamp
            //
            //              We add the minClamp back to the whole equation in order to create an equation that runs from minClamp
            //              to maxClamp.
            //      
            _frontPropertiesScalarScaleAnimation = _compositor.CreateExpressionAnimation(
                "Clamp(" +
                            "(CrossoverTranslation + scrollingProperties.Translation.Y)/CrossoverTranslation * (maxClamp-minClamp) + minclamp," +
                            "minClamp," +
                            "maxClamp" +
                       ")");
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
                "initialOffset + 1 * (CrossoverTranslation + scrollingProperties.Translation.Y)");
            _behindTranslateAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
        }


        private void UpdateSizes()
        {
            // 
            // First, calculate all of the new sizes and distances we will need.
            //
            var backgroundImageSize = new Vector2((float)ParallaxingImage.ActualHeight - (float)_scrollBar.ActualWidth, (float)ParallaxingImage.ActualHeight);
            var profileImageSize = new Vector2((float)ParallaxingImage.ActualHeight, (float)ParallaxingImage.ActualHeight) * _initialScaleAmount;
            var crossOverPoint = profileImageSize.Y * _finalScaleAmount / 2 + _followMargin;
            var offset = new Vector3((float)ParallaxingImage.ActualWidth / 2, (float)ParallaxingImage.ActualHeight, 0);
            var parallaxPoint = backgroundImageSize.Y * _backgroundShowRatio;

            // Push the text content down to make room for the image overhanging.
            //
            ContentPanel.Margin = new Thickness(5, (float)ParallaxingImage.ActualHeight + profileImageSize.Y / 2, 5, 5);

            //
            // Resolve all of the property parameters and references on the frontVisual animations and start them.
            //
            _frontVisual.AnchorPoint = new Vector2(.5f, .5f);
            _frontVisual.Offset = offset;
            _frontVisual.Size = profileImageSize;
            _frontPropertiesScalarScaleAnimation.SetScalarParameter("CrossoverTranslation", crossOverPoint);
            _frontVisibilityAnimation.SetScalarParameter("CrossoverTranslation", crossOverPoint);
            _frontVisual.Properties.StartAnimation("ScalarScale", _frontPropertiesScalarScaleAnimation);
            _frontVisual.StartAnimation("Opacity", _frontVisibilityAnimation);


            //
            // Resolve all of the property parameters and references on the backVisual animations and start them.
            //
            _backVisual.AnchorPoint = new Vector2(.5f, .5f);
            _backVisual.Offset = offset;
            _backVisual.Size = profileImageSize;
            _behindTranslateAnimation.SetScalarParameter("initialOffset", offset.Y);
            _behindTranslateAnimation.SetScalarParameter("CrossoverTranslation", crossOverPoint);
            _behindOpacityAnimation.SetScalarParameter("CrossoverTranslation", crossOverPoint);

            _backVisual.StartAnimation("Opacity", _behindOpacityAnimation);
            _backVisual.StartAnimation("Offset.Y", _behindTranslateAnimation);

            //
            // Resolve all property parameters and references on _backgroundVisual
            //
            _backgroundTranslationAnimation.SetScalarParameter("ParallaxPoint", parallaxPoint);
            _backgroundVisual.AnchorPoint = new Vector2(.5f, .5f);
            _backgroundVisual.Size = backgroundImageSize;
            _backgroundVisual.CenterPoint = new Vector3(backgroundImageSize / 2, 1);
            _backgroundTranslationAnimation.SetVector3Parameter("BaseOffset", new Vector3(backgroundImageSize / 2, 0) - new Vector3((float)_scrollBar.ActualWidth,0,0));
            _backgroundVisual.StartAnimation("Offset", _backgroundTranslationAnimation);
            _backgroundVisual.StartAnimation("Scale.X", _backgroundScaleAnimation);
            _backgroundVisual.StartAnimation("Scale.Y", _backgroundScaleAnimation);

            //
            // Resolve all property parameters and references on _profileContentVisual
            //
            _profileContentVisual.Size = new Vector2((float)ProfileContent.ActualWidth, (float)ProfileContent.ActualHeight);
            _profileContentVisual.AnchorPoint = new Vector2(0.5f, 0.5f);
            _profileContentTranslationAnimation.SetScalarParameter("ParallaxPoint", parallaxPoint);
            _profileContentTranslationAnimation.SetVector3Parameter("BaseOffset", new Vector3(backgroundImageSize / 2, 0));
            _profileContentVisual.StartAnimation("Offset", _profileContentTranslationAnimation);
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
            CompositionSurfaceBrush profileBrush = _compositor.CreateSurfaceBrush();
            var image = _imageFactory.CreateImageFromUri(new Uri("ms-appx:///Assets/teched3ae5a27b-4f78-e111-94ad-001ec953730b.jpg"));
            profileBrush.Surface = image.Surface;
            brush.SetSourceParameter("image", profileBrush);

            //
            // Load in the circular mask picture asx a brush using the composition Toolkit.
            //
            CompositionSurfaceBrush maskBrush = _compositor.CreateSurfaceBrush();
            image = _imageFactory.CreateImageFromUri(new Uri("ms-appx:///Assets/CircleMask.png"));
            maskBrush.Surface = image.Surface;
            brush.SetSourceParameter("mask", maskBrush);

            return brush;

        }


        private Compositor _compositor;
        private CompositionImageFactory _imageFactory;

        private SpriteVisual _backVisual;
        private ExpressionAnimation _behindOpacityAnimation;
        private ExpressionAnimation _behindTranslateAnimation;

        private SpriteVisual _frontVisual;
        private ExpressionAnimation _frontPropertiesScalarScaleAnimation;
        private ExpressionAnimation _frontVisibilityAnimation;

        private Visual _backgroundVisual;
        private ExpressionAnimation _backgroundTranslationAnimation;
        private ExpressionAnimation _backgroundScaleAnimation;

        private Visual _profileContentVisual;
        private ExpressionAnimation _profileContentTranslationAnimation;

        private ScrollBar _scrollBar;

        private float _initialScaleAmount = .8f;
        private float _finalScaleAmount = .4f;
        private float _followMargin = 50f;
        private float _backgroundShowRatio = .6f;
        private float _backgroundScaleAmount = .25f;
        private float _parallaxRatio = .6f;
    }
}
