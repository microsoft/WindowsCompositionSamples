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

using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    /// <summary>
    /// Creates a number of visuals
    /// Creates ImplicitAnimationMaps and shares between the visuals
    /// Adds a group of animations to the ImplicitAnimationMaps
    /// Implicitly kicks off the animations on offset and scale changes
    /// Creates Circle/Spiral/Ellipse/Collapsed Layouts
    /// </summary>
    public sealed partial class ImplicitAnimationTransformer : SamplePage
    {
        // Windows.UI.Composition
        private Compositor _compositor;
        private ContainerVisual _root;
        private List<ManagedSurface> _imageList;

        // Constants
        private const float _posX = 600;
        private const float _posY = 400;
        private const float _circleRadius = 300;
        private const float _ellipseRadiusX = 400;
        private const float _ellipseRadiusY = 200;
        private const double _spiralOrientation = 5;
        private const double _spiralTightness = 0.8;
        private const int _distance = 60;
        private const int _rowCount = 13;
        private const int _columnCount = 13;

        //Helper
        private readonly Random randomBrush = new Random();

        public ImplicitAnimationTransformer()
        {
            this.InitializeComponent();
            this.InitializeComposition();
        }

        public static string    StaticSampleName => "Implicit Animations"; 
        public override string  SampleName => StaticSampleName; 
        public static string    StaticSampleDescription => "Demonstrates how to use Animations triggered by property changes. Choose different layouts by clicking the buttons below. The layout of visual elements are transformed using implicit animations that are triggered on offset change. The last button allows you to apply scale implicit animation to any layout that is triggered by scale change.";
        public override string  SampleDescription => StaticSampleDescription;
        public override string  SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761163"; 

        /// <summary>
        /// Initialize Composition
        /// </summary>
        private void InitializeComposition()
        {

            // Retrieve an instance of the Compositor from the backing Visual of the Page
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create a root visual from the Compositor
            _root = _compositor.CreateContainerVisual();

            // Set the root ContainerVisual as the XAML Page Visual           
            ElementCompositionPreview.SetElementChildVisual(this, _root);

            // Assign initial values to variables used to store updated offsets for the visuals          
            float posXUpdated = _posX;
            float posYUpdated = _posY;


            //Create a list of image brushes that can be applied to a visual
            string[] imageNames = { "60Banana.png", "60Lemon.png", "60Vanilla.png", "60Mint.png", "60Orange.png", "110Strawberry.png", "60SprinklesRainbow.png" };
            _imageList = new List<ManagedSurface>(10);
            for (int k = 0; k < imageNames.Length; k++)
            {
                ManagedSurface surface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/" + imageNames[k]));
                _imageList.Add(surface);
            }

            // Create nxn matrix of visuals where n=row/ColumnCount-1 and passes random image brush to the function
            // that creates a visual
            for (int i = 1; i < _rowCount; i++)
            {
                posXUpdated = i * _distance;
                for (int j = 1; j < _columnCount; j++)
                {
                    CompositionSurfaceBrush brush = _imageList[randomBrush.Next(_imageList.Count)].Brush;

                    posYUpdated = j * _distance;
                    _root.Children.InsertAtTop(CreateChildElement(brush, posXUpdated, posYUpdated));
                }
            }

            // Update the default animation state
            UpdateAnimationState(EnableAnimations.IsChecked == true);
        }

        /// <summary>
        /// Creates a visible element in our application
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <returns> </returns>
        Visual CreateChildElement(CompositionSurfaceBrush brush, float positionX, float positionY)
        {

            // Each element consists of a single Sprite visual
            SpriteVisual visual = _compositor.CreateSpriteVisual();

            // Create a SpriteVisual with size, offset and center point
            visual.Size = new Vector2(50, 50);
            visual.Offset = new Vector3(positionX, positionY, 0);
            visual.CenterPoint = new Vector3(visual.Size.X / 2.0f, visual.Size.Y / 2.0f, 0.0f);

            //apply the random image brush to a visual
            visual.Brush = brush;

            return visual;
        }

        /// <summary>
        ///  This method implicitly animates the visual elements into a Grid layout
        ///  on offset change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridLayout(object sender, RoutedEventArgs e)
        {
            //get the position of the initial grid element
            float posXgrid = _posX;
            float posYgrid = _posY;

            List<Vector3> newOffset = new List<Vector3>();

            // Calculate the position for each visual in the grid layout
            for (int i = 1; i < _rowCount; i++)
            {
                posXgrid = i * _distance;
                for (int j = 1; j < _columnCount; j++)
                {
                    posYgrid = j * _distance;
                    newOffset.Add(new Vector3(posXgrid, posYgrid, 0));
                }
            }
            //counter for adding elements to the grid
            int k = 0;

            foreach (var child in _root.Children)
            {
                child.Offset = newOffset[k];
                k++;

            }
        }

        /// <summary>
        ///  This method implicitly animates the visual elements into a circle layout
        ///  on offset change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CircleLayout(object sender, RoutedEventArgs e)
        {
            //
            // Define initial angle of each element on the spiral
            //
            double theta = 0;
            double thetaRadians = 0;

            foreach (var child in _root.Children)
            {
                // Change the Offset property of the visual. This will trigger the implicit animation that is associated with the Offset change.
                // The position of the element on the circle is defined using parametric equation:
                child.Offset = new Vector3((float)(_circleRadius * Math.Cos(thetaRadians)) + _posX, (float)(_circleRadius * Math.Sin(thetaRadians) + _posY), 0);

                // Update the angle to be used for the next visual element
                theta += 2.5;
                thetaRadians = theta * Math.PI / 180F;
            }
        }

        /// <summary>
        /// This method implicitly animates the visual elements into a spiral layout
        /// on offset change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpiralLayout(object sender, RoutedEventArgs e)
        {

            // Define initial angle of each element on the spiral
            double theta = 0;
            double thetaOrientationRadians = _spiralOrientation * Math.PI / 180F;

            foreach (var child in _root.Children)
            {
                // Change the Offset property of the visual. This will trigger the implicit animation that is associated with the Offset change.
                // Define the position of the visual on the spiral using parametric equation:
                // x = beta*cos(theta + alpha); y = beta*sin(theta + alpha ) 
                child.Offset = new Vector3((float)(_spiralTightness * theta * (Math.Cos(thetaOrientationRadians))) + _posX, (float)(_spiralTightness * theta * (Math.Sin(thetaOrientationRadians))) + _posY, 0);

                // Update the angle to be used for the next visual element
                theta += 4;
                thetaOrientationRadians = (theta + _spiralOrientation) * Math.PI / 180F;
            }
        }

        /// <summary>
        ///This method implicitly animates and rotates the visual elements into an ellipse layout
        /// on offset change  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EllipseLayout(object sender, RoutedEventArgs e)
        {
            // Define initial angle of each element on the ellipse
            double theta = 0;
            double thetaRadians = 0;

            foreach (var child in _root.Children)
            {
                // Change the Offset property of the visual. This will trigger the implicit animation that is associated with the Offset change.
                // The position of the element on the ellipse is defined using parametric equation:
                // x = alpha * cos(theta) ; y = beta*sin(theta)
                child.Offset = new Vector3((float)(_ellipseRadiusX * Math.Cos(thetaRadians)) + _posX, (float)(_ellipseRadiusY * Math.Sin(thetaRadians)) + _posY, 0);


                // Update the angle to be used for the next visual element
                theta += 2.5;
                thetaRadians = theta * Math.PI / 180F;
            }
        }

        /// <summary>
        /// This method animates collapsing all the visual elements into the same position
        /// and implicitly kicks off opacity animation on offset change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollapseLayout(object sender, RoutedEventArgs e)
        {
            foreach (var child in _root.Children)
            {
                // Change the Offset property of the visual. This will trigger the implicit animation that is associated with the Offset change.
                // Define the same position for each visual
                child.Offset = new Vector3(_posX, _posY, 0);
            }
        }
        /// <summary>
        /// This method implictly kicks off animation on scale change 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scale(object sender, RoutedEventArgs e)
        {
            foreach (var child in _root.Children)
            {
                if (child.Scale == Vector3.One)
                {
                    // Change the scale property of the visual will trigger the 
                    // implicit animation that is associated with the scale change
                    child.Scale = new Vector3(0.5f, 0.5f, 1.0f);
                }
                else
                {
                    child.Scale = Vector3.One;
                }
            }
        }

        /// <summary>
        /// Creates offset animation that can be applied to a visual
        /// </summary>
        Vector3KeyFrameAnimation CreateOffsetAnimation()
        {
            var _offsetKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
            _offsetKeyFrameAnimation.Target = "Offset";
            
            // Final Value signifies the target value to which the visual will animate
            // in this case it will be defined by new offset
            _offsetKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            _offsetKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

            return _offsetKeyFrameAnimation;
        }

        /// <summary>
        /// Creates scale animation that can be applied to a visual
        /// </summary>
        CompositionAnimationGroup CreateScaleAnimation()
        {
            var scaleKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleKeyFrameAnimation.Target = "Scale";
            scaleKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            scaleKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

            var rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotationAnimation.Target = "RotationAngleInDegrees";
            rotationAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 45.0f");
            rotationAnimation.Duration = TimeSpan.FromSeconds(3);

            var animationGroup = _compositor.CreateAnimationGroup();

            // AnimationGroup associates the animations with the target
            animationGroup.Add(scaleKeyFrameAnimation);
            animationGroup.Add(rotationAnimation);
            
            return animationGroup;
        }

        private void EnableAnimations_Checked(object sender, RoutedEventArgs e)
        {
            if (_compositor != null)
            {
                UpdateAnimationState(true);
            }
        }

        private void EnableAnimations_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_compositor != null)
            {
                UpdateAnimationState(false);
            }
        }

        private void UpdateAnimationState(bool animate)
        {
            if (animate)
            {
                ImplicitAnimationCollection implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
                implicitAnimationCollection["Offset"] = CreateOffsetAnimation();
                implicitAnimationCollection["Scale"] = CreateScaleAnimation();
                foreach (var child in _root.Children)
                {
                    child.ImplicitAnimations = implicitAnimationCollection;
                }
            }
            else
            {
                foreach (var child in _root.Children)
                {
                    child.ImplicitAnimations = null;
                }
            }
        }

        private void ImplicitAnimationTransformer_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach(var surface in _imageList)
            {
                surface.Dispose();
            }
        }
    }
}
