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

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using System.Numerics;
using System;
using Windows.UI.Xaml.Hosting;
using SamplesCommon;

namespace CompositionSampleGallery
{
    public sealed partial class ShadowsAdvanced : SamplePage
    {
        const int   _rows = 2;
        const int   _columns = 6;
        const int   _itemLength = 160;
        const int   _gridMargin = 100;
        const int   _itemMargin = 10;
        const float _initialShadowBlurRadius = 15.0f;
        const float _initialShadowOpacity = 0.5f;
        int   _shadowHighestZ = 2;
        int   _contentHighestZ = 3;

        private UIElement[,] _content = new UIElement[_rows, _columns];
        private CompositionShadow[,] _shadows = new CompositionShadow[_rows, _columns];

        // Sample metadata
        public static string    StaticSampleName => "Advanced Shadows"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates advanced shadow scenarios."; 
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=868954";

        public ShadowsAdvanced()
        {
            this.InitializeComponent();
        }

        private void ShadowsAdvanced_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Determine width and heigth of the canvas
            float width = (_gridMargin * 2) + (_columns * _itemLength) + (_columns * _itemMargin);
            float height = (_gridMargin * 2) + (_rows * _itemLength) + (_rows * _itemMargin);
            MainCanvas.Width = width;
            MainCanvas.Height = height;

            BuildGrid();
        }

        /// <summary>
        /// Builds the grid of boxes/items to have drop shadow on.
        /// </summary>
        private void BuildGrid()
        {
            MainCanvas.Background = new SolidColorBrush(Colors.DimGray);
            int imageIdx = 1;


            //
            // Create as many squares as necessary for the given number of rows and columns
            // Square size is fixed and defined with _itemLength (length of a side of the square)
            //

            for (int row = 0; row < _rows; ++row)
            {
                for (int column = 0; column < _columns; ++column, ++imageIdx)
                {
                    // Create a new square with a solid color
                    var content = new Rectangle();
                    content.Fill = new SolidColorBrush(Colors.OldLace);
                    content.Width = _itemLength;
                    content.Height = _itemLength;

                    // Place the rectangle in a 2D array of UIElement
                    _content[row, column] = content;

                    // Create a new shadow the same size of the square
                    var shadow = new CompositionShadow();
                    shadow.Width = _itemLength;
                    shadow.Height = _itemLength;
                    shadow.ShadowOpacity = _initialShadowOpacity;
                    shadow.BlurRadius = _initialShadowBlurRadius;


                    //
                    // Determine the position of an abstract "light" source (no actual light object created here)
                    // in relation to the position of the square in the scene.
                    //

                    var lightXPosition = (.5f * ((_columns + 1) * (_itemLength + _itemMargin))) - (.5f * (_itemLength + _itemMargin));
                    Vector3 lightPositionVector = new Vector3(lightXPosition, 0, -1);
                    Vector3 itemPositionVector = new Vector3(CalculateOffset(column), CalculateOffset(row), 0);
                    Vector3 lightDirection = Vector3.Normalize(itemPositionVector - lightPositionVector);

                    // Determine the offset of the shadow
                    shadow.OffsetY = 10.0f * (row + 1);
                    shadow.OffsetX = 10.0f * lightDirection.X;
                    shadow.OffsetZ = 0.0f;

                    shadow.Visual.CenterPoint = new Vector3(_itemLength * .5f, _itemLength * .5f, 0);

                    // Store the shadow in a 2D array of CompositionShadow
                    _shadows[row, column] = shadow;

                    // Add the shadow to the Canvas first
                    MainCanvas.Children.Add(shadow);
                    SetOffsets(shadow, row, column, 0);

                    // Add the square to the canvas next
                    MainCanvas.Children.Add(content);
                    SetOffsets(content, row, column, 1);

                    // Attach event handlers to the square for hovering over and off the square
                    content.PointerEntered += Content_PointerEntered;
                    content.PointerExited += Content_PointerExited;

                    SetupAnimations(content);
                }
            }
        }
        private void SetupAnimations(Object item)
        {
            var shadow = GetShadowFromContent(item);
            var content = ElementCompositionPreview.GetElementVisual(item as UIElement);

            ElementCompositionPreview.SetIsTranslationEnabled((UIElement)item, true);
            var ContentProperties = ElementCompositionPreview.GetElementVisual(item as UIElement).Properties;
            ContentProperties.InsertVector3("Translation", Vector3.Zero);

            var compositor = content.Compositor;

            var implicitAnimationShadow = compositor.CreateImplicitAnimationCollection();
            var implicitAnimationVisual = compositor.CreateImplicitAnimationCollection();

            //Translation Animation 
            var translationAnimation = compositor.CreateVector3KeyFrameAnimation();
            translationAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            translationAnimation.Duration = TimeSpan.FromSeconds(1);
            translationAnimation.Target = "Translation";

            //Scale Animation Shadow 
            var shadowScaleAnimation = compositor.CreateVector3KeyFrameAnimation();
            shadowScaleAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            shadowScaleAnimation.Duration = TimeSpan.FromSeconds(1);
            shadowScaleAnimation.Target = "Scale";


            // Animate shadow (change opacity)

            var shadowOpacityAnimation = compositor.CreateScalarKeyFrameAnimation();
            shadowOpacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            shadowOpacityAnimation.Duration = TimeSpan.FromSeconds(1);
            shadowOpacityAnimation.Target = "Opacity";
            
            //BlurRadius Animation 

            var shadowBlurAnimation = compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            shadowBlurAnimation.Duration = TimeSpan.FromSeconds(1);
            shadowBlurAnimation.Target = "BlurRadius";

            //Associating animations with triggers 
            implicitAnimationShadow["BlurRadius"] = shadowBlurAnimation;
            implicitAnimationShadow["Opacity"] = shadowOpacityAnimation;
            implicitAnimationShadow["Scale"] = shadowScaleAnimation;

            implicitAnimationVisual["Translation"] = translationAnimation;
            

            //Applying Implicit Animations to objects 
            content.Properties.ImplicitAnimations = implicitAnimationVisual;
            shadow.Visual.ImplicitAnimations = implicitAnimationShadow;
            shadow.DropShadow.ImplicitAnimations = implicitAnimationShadow;
            
        }

        private void Content_PointerExited(object sender, PointerRoutedEventArgs e)
        {
           
            ElementCompositionPreview.SetIsTranslationEnabled((UIElement)sender, true);
            var ContentProperties = ElementCompositionPreview.GetElementVisual((UIElement)sender).Properties;
            ContentProperties.InsertVector3("Translation", Vector3.Zero);

            // Animate Shadow (make it smaller)
            var shadow = GetShadowFromContent(sender);
            shadow.Visual.Scale = new Vector3(1.0f);
            shadow.DropShadow.Opacity = _initialShadowOpacity;
            shadow.DropShadow.BlurRadius = _initialShadowBlurRadius;

            // Set the ZIndex to lowest
            Canvas.SetZIndex((UIElement)sender, 1);
            Canvas.SetZIndex(shadow, 0);
        }

        private void Content_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var content = ElementCompositionPreview.GetElementVisual((UIElement)sender);

            var compositor = content.Compositor;
            
            var ContentProperties = ElementCompositionPreview.GetElementVisual((UIElement)sender).Properties;
            
            // Get shadow and set the proper Z values for visual and shadow
            var shadow = GetShadowFromContent(sender);
            Canvas.SetZIndex((UIElement)sender, _contentHighestZ += 2);
            Canvas.SetZIndex(shadow, _shadowHighestZ += 2);

            shadow.DropShadow.Opacity = _initialShadowOpacity;
            shadow.DropShadow.BlurRadius = _initialShadowBlurRadius;
            ContentProperties.InsertVector3("Translation", new Vector3(0, 0, 25.0f));
            shadow.Visual.Scale = new Vector3(1.25f, 1.25f, 0.0f);
        }

        private CompositionShadow GetShadowFromContent(object content)
        {
            for (int row = 0; row < _rows; ++row)
            {
                for (int column = 0; column < _columns; ++column)
                {
                    if (content == _content[row, column])
                    {
                        return _shadows[row, column];
                    }
                }
            }

            return null;
        }

        private static float CalculateOffset(int index)
        {
            return _gridMargin + (_itemLength * index) + (_itemMargin * index);
        }

        private static void SetOffsets(UIElement element, int row, int column, int zIndex)
        {
            Canvas.SetTop(element, CalculateOffset(row));
            Canvas.SetLeft(element, CalculateOffset(column));
            Canvas.SetZIndex(element, zIndex);
        }
       
    }
}
