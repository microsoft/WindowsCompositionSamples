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

using SamplesCommon.ImageLoader;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PropertySets : SamplePage
    {
        public PropertySets()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName    { get { return "Expressions & PropertySets"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Demonstrates how to use ExpressionAnimations and CompositionPropertySets to create a simple orbiting Visual."; } }
        public override string      SampleCodeUri       { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761172"; } }

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(MyGrid).Compositor;
            _imageLoader = ImageLoaderFactory.CreateImageLoader(compositor);
            ContainerVisual container = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(MyGrid, container);


            //
            // Create a couple of SurfaceBrushes for the orbiters and center
            //

            CompositionSurfaceBrush redBrush = compositor.CreateSurfaceBrush();
            _redBallSurface = _imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Samples/SDK 10586/PropertySets/RedBall.png"));
            redBrush.Surface = _redBallSurface.Surface;

            CompositionSurfaceBrush blueBrush = compositor.CreateSurfaceBrush();
            _blueBallSurface = _imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Samples/SDK 10586/PropertySets/BlueBall.png"));
            blueBrush.Surface = _blueBallSurface.Surface;


            //
            // Create the center and orbiting sprites
            //

            SpriteVisual redSprite = compositor.CreateSpriteVisual();
            redSprite.Brush = redBrush;
            redSprite.Size = new Vector2(100f, 100f);
            redSprite.Offset = new Vector3(200f, 200f, 0f);
            container.Children.InsertAtTop(redSprite);

            SpriteVisual blueSprite = compositor.CreateSpriteVisual();
            blueSprite.Brush = blueBrush;
            blueSprite.Size = new Vector2(25f, 25f);
            blueSprite.Offset = new Vector3(0f, 0f, 0f);
            container.Children.InsertAtTop(blueSprite);

            //
            // Create the expression.  This expression positions the orbiting sprite relative to the center of
            // of the red sprite's center.  As we animate the red sprite's position, the expression will read
            // the current value of it's offset and keep the blue sprite locked in orbit.
            //

            ExpressionAnimation expressionAnimation = compositor.CreateExpressionAnimation("visual.Offset + " +
                                                                                           "propertySet.CenterPointOffset + " +
                                                                                           "Vector3(cos(ToRadians(propertySet.Rotation)) * 150," +
                                                                                                   "sin(ToRadians(propertySet.Rotation)) * 75, 0)");

            //
            // Create the PropertySet.  This property bag contains all the value referenced in the expression.  We can
            // animation these property leading to the expression being re-evaluated per frame.
            //

            CompositionPropertySet propertySet = compositor.CreatePropertySet();
            propertySet.InsertScalar("Rotation", 0f);
            propertySet.InsertVector3("CenterPointOffset", new Vector3(redSprite.Size.X / 2 - blueSprite.Size.X / 2, 
                                                                       redSprite.Size.Y / 2 - blueSprite.Size.Y / 2, 0));

            // Set the parameters of the expression animation
            expressionAnimation.SetReferenceParameter("propertySet", propertySet);
            expressionAnimation.SetReferenceParameter("visual", redSprite);

            // Start the expression animation!
            blueSprite.StartAnimation("Offset", expressionAnimation);


            // Now animate the rotation property in the property bag, this generates the orbitting motion.
            var linear = compositor.CreateLinearEasingFunction();
            var rotAnimation = compositor.CreateScalarKeyFrameAnimation();
            rotAnimation.InsertKeyFrame(1.0f, 360f, linear);
            rotAnimation.Duration = TimeSpan.FromMilliseconds(4000);
            rotAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            propertySet.StartAnimation("Rotation", rotAnimation);

            // Lastly, animation the Offset of the red sprite to see the expression track appropriately
            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0f, new Vector3(125f, 50f, 0f));
            offsetAnimation.InsertKeyFrame(.5f, new Vector3(125f, 200f, 0f));
            offsetAnimation.InsertKeyFrame(1f, new Vector3(125f, 50f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(4000);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            redSprite.StartAnimation("Offset", offsetAnimation);
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _redBallSurface.Dispose();
            _blueBallSurface.Dispose();
            _imageLoader.Dispose();
        }

        private IImageLoader _imageLoader;
        private IManagedSurface _redBallSurface;
        private IManagedSurface _blueBallSurface;
    }
}
