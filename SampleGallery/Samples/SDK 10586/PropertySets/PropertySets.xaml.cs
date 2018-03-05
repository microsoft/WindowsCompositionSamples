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

using ExpressionBuilder;
using SamplesCommon;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

using EF = ExpressionBuilder.ExpressionFunctions;

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

        public static string        StaticSampleName => "Expressions & PropertySets"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how to use ExpressionAnimations and CompositionPropertySets to create a simple orbiting Visual."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761172"; 

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(MyGrid).Compositor;
            ContainerVisual container = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(MyGrid, container);


            //
            // Create a couple of SurfaceBrushes for the orbiters and center
            //

            _redBallSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/RedBall.png"));
            _blueBallSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/BlueBall.png"));


            //
            // Create the center and orbiting sprites
            //

            SpriteVisual redSprite = compositor.CreateSpriteVisual();
            redSprite.Brush = _redBallSurface.Brush;
            redSprite.Size = new Vector2(100f, 100f);
            redSprite.Offset = new Vector3((float)Window.Current.Bounds.Width / 2 - redSprite.Size.X/2, 150f, 0f);
            container.Children.InsertAtTop(redSprite);

            SpriteVisual blueSprite = compositor.CreateSpriteVisual();
            blueSprite.Brush = _blueBallSurface.Brush;
            blueSprite.Size = new Vector2(25f, 25f);
            blueSprite.Offset = new Vector3((float)Window.Current.Bounds.Width / 2 - redSprite.Size.X / 2, 50f, 0f);
            container.Children.InsertAtTop(blueSprite);

            //
            // Create the PropertySet that contains all the value referenced in the expression. We can also
            // animate these properties, leading to the expression being re-evaluated per frame.
            //

            _propertySet = compositor.CreatePropertySet();
            _propertySet.InsertScalar("Rotation", 0f);
            _propertySet.InsertVector3("CenterPointOffset", new Vector3(redSprite.Size.X / 2 - blueSprite.Size.X / 2,
                                                                        redSprite.Size.Y / 2 - blueSprite.Size.Y / 2, 
                                                                        0));

            //
            // Create the expression.  This expression positions the orbiting sprite relative to the center of
            // of the red sprite's center.  As we animate the red sprite's position, the expression will read
            // the current value of it's offset and keep the blue sprite locked in orbit.
            //

            var propSetCenterPoint = _propertySet.GetReference().GetVector3Property("CenterPointOffset");
            var propSetRotation = _propertySet.GetReference().GetScalarProperty("Rotation");
            var orbitExpression = redSprite.GetReference().Offset + propSetCenterPoint + 
                EF.Vector3(
                    EF.Cos(EF.ToRadians(propSetRotation)) * 150,
                    EF.Sin(EF.ToRadians(propSetRotation)) * 75, 
                    0);

            // Start the expression animation!
            blueSprite.StartAnimation("Offset", orbitExpression);

            // Now animate the rotation property in the property bag, this generates the orbitting motion.
            var linear = compositor.CreateLinearEasingFunction();
            var rotAnimation = compositor.CreateScalarKeyFrameAnimation();
            rotAnimation.InsertKeyFrame(1.0f, 360f, linear);
            rotAnimation.Duration = TimeSpan.FromMilliseconds(4000);
            rotAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            _propertySet.StartAnimation("Rotation", rotAnimation);

            // Lastly, animation the Offset of the red sprite to see the expression track appropriately
            var offsetAnimation = compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0f, 50f);
            offsetAnimation.InsertKeyFrame(.5f, 150f);
            offsetAnimation.InsertKeyFrame(1f, 50f);
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(4000);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            redSprite.StartAnimation("Offset.Y", offsetAnimation);
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _redBallSurface.Dispose();
            _blueBallSurface.Dispose();
        }

        private ManagedSurface _redBallSurface;
        private ManagedSurface _blueBallSurface;
        private CompositionPropertySet _propertySet;
    }
}
