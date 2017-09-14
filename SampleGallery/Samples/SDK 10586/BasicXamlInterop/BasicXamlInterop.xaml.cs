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

using System;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class BasicXamlInterop : SamplePage
    {
        private ContainerVisual _container2;

        public BasicXamlInterop()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Basic Xaml Interop"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates how obtain a Windows.UI.Composition Compositor instance using Windows.UI.Xaml.Hosting to create CompositionObjects in a Windows.UI.Xaml based application."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761160"; 

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //
            // Example 1 - Animate a tree of XAML content.  ElementCompositionPreview.GetElementVisual()
            //             returns the Visual which contains all Visual children under the target
            //             UIElement including that UIELement's Visuals.  The returned Visual
            //             can be used to manipulate the XAML tree, but you cannot add to or modify
            //             the Visual tree.
            //

            Visual visual = ElementCompositionPreview.GetElementVisual(TextBlock1);
            Compositor compositor = visual.Compositor;

            // Apply a simple animation to the XAML content
            var animation = compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(0.5f, new Vector3(100, 0, 0));
            animation.InsertKeyFrame(1.0f, new Vector3(0, 0, 0));
            animation.Duration = TimeSpan.FromMilliseconds(4000);
            animation.IterationBehavior = AnimationIterationBehavior.Forever;
            visual.StartAnimation("Offset", animation);


            //
            // Example 2 - Add some Windows.UI.Composition content to the XAML tree.
            //             ElementCompositionPreview.SetElementChildVisual() sets a Windows.UI.Composition
            //             Visual as the child of the target UIElement.  You can use this Visual as the
            //             basis for creating a tree of Windows.UI.Composition content under the target
            //             UIElement in the tree.
            //

            ContainerVisual container = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(TextBlock2, container);

            // Add some solid color sprites under the container.
            SpriteVisual redSprite = compositor.CreateSpriteVisual();
            redSprite.Brush = compositor.CreateColorBrush(Colors.Red);
            redSprite.Size = new Vector2(100f, 100f);
            redSprite.Offset = new Vector3(0f, (float)TextBlock2.RenderSize.Height, 0f);
            container.Children.InsertAtTop(redSprite);

            SpriteVisual blueSprite = compositor.CreateSpriteVisual();
            blueSprite.Brush = compositor.CreateColorBrush(Colors.Blue);
            blueSprite.Size = new Vector2(100f, 100f);
            blueSprite.Offset = new Vector3(100f, (float)TextBlock2.RenderSize.Height, 0f);
            container.Children.InsertAtTop(blueSprite);

            // Start the same animation
            container.StartAnimation("Offset", animation);


            //
            // Example 3 - Add some Windows.UI.Composition content to the XAML tree and modify their tree order.
            //

            _container2 = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(TextBlock3, _container2);
            _container2.Offset = new Vector3(0, 50f, 0f);

            // Add some solid color sprites under the container.
            SpriteVisual orangeSprite = compositor.CreateSpriteVisual();
            orangeSprite.Brush = compositor.CreateColorBrush(Colors.Orange);
            orangeSprite.Size = new Vector2(100f, 100f);
            orangeSprite.Offset = new Vector3(0f, 0f, 0f);
            _container2.Children.InsertAtTop(orangeSprite);

            SpriteVisual greenSprite = compositor.CreateSpriteVisual();
            greenSprite.Brush = compositor.CreateColorBrush(Colors.Green);
            greenSprite.Size = new Vector2(100f, 100f);
            greenSprite.Offset = new Vector3(50f, 0f, 0f);
            _container2.Children.InsertAtTop(greenSprite);

            SpriteVisual purpleSprite = compositor.CreateSpriteVisual();
            purpleSprite.Brush = compositor.CreateColorBrush(Colors.Purple);
            purpleSprite.Size = new Vector2(100f, 100f);
            purpleSprite.Offset = new Vector3(100f, 0f, 0f);
            _container2.Children.InsertAtTop(purpleSprite);

            // Start a timer, when it fires rearrange the sprites in the tree.
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            var child = _container2.Children.FirstOrDefault();
            _container2.Children.Remove(child);
            _container2.Children.InsertAtTop(child);
        }
    }
}
