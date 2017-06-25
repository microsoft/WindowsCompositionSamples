/*
Copyright (c) Microsoft Corporation 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE
*/

using System;
using System.Diagnostics;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Animation_Batches
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _mainContainer;
        private Visual _root;
        private Visual _target;
        private Visual _target2;
        private CompositionScopedBatch _batch;
        private LinearEasingFunction _linear;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Define and setup the Green Square Visual that will be animated 
        public void SetupVisual()
        {
            // Intialize the Compositor
            _compositor = new Compositor();
            _root = ElementCompositionPreview.GetElementVisual(Container);
            _compositor = _root.Compositor;

            _linear = _compositor.CreateLinearEasingFunction();

            // Create Green Square
            var colorVisual = _compositor.CreateSpriteVisual();
            colorVisual.Brush = _compositor.CreateColorBrush(Colors.Green);
            colorVisual.Size = new Vector2(150.0f, 150.0f);
            colorVisual.Offset = new Vector3(250.0f, 50.0f, 0.0f);
            colorVisual.CenterPoint = new Vector3(75.0f, 75.0f, 0.0f);
            _target = colorVisual;

            // Create Blue Square
            var colorVisual2 = _compositor.CreateSpriteVisual();
            colorVisual2.Brush = _compositor.CreateColorBrush(Colors.Aqua);
            colorVisual2.Size = new Vector2(200.0f, 150.0f);
            colorVisual2.Offset = new Vector3(25.0f, 50.0f, 0.0f);
            colorVisual2.IsVisible = false; 
            _target2 = colorVisual2;

            // Add the Blue and Green square visuals to the tree
            _mainContainer = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(Container, _mainContainer);
            _mainContainer.Children.InsertAtTop(_target);
            _mainContainer.Children.InsertAtTop(_target2);

            // Create Scoped batch for animations
            _batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            // Add Animation1 to the batch
            Animation1(_target);

            // Suspend the batch to exclude an animation
            _batch.Suspend();

            // Exluding Animation2 from batch
            Animation2(_target);

            // Resuming the batch to collect additional animations
            _batch.Resume();

            // Add Animation3 to the batch
            Animation3(_target);

            // Batch is ended an no objects can be added
            _batch.End();

            // Method triggered when batch completion event fires
            _batch.Completed += OnBatchCompleted;
        }

        // Defines what happens when a batch is completed
        public void OnBatchCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            _target2.IsVisible = true;
        }
        
    
        // Defines a KeyFrame animation to animate opacity
        public void Animation3(Visual _target)
        {
            var animation3 = _compositor.CreateScalarKeyFrameAnimation();

            animation3.InsertKeyFrame(0.00f, 1.00f);
            animation3.InsertKeyFrame(0.50f, 0.50f);
            animation3.InsertKeyFrame(1.00f, 1.00f);

            animation3.Duration = TimeSpan.FromMilliseconds(2000);
            _target.StartAnimation("Opacity", animation3);
        }

        // Defines a KeyFrame animation to animate rotation
        public void Animation2(Visual _target)
        {
            var animation2 = _compositor.CreateScalarKeyFrameAnimation();

            animation2.InsertKeyFrame(0.00f, 0.00f, _linear);
            animation2.InsertKeyFrame(0.50f, 180.0f, _linear);
            animation2.InsertKeyFrame(1.00f, 360.0f, _linear);

            animation2.Duration = TimeSpan.FromMilliseconds(2000);
            animation2.IterationCount = 5;

            _target.StartAnimation("RotationAngleinDegrees", animation2);
        }

        // Defines a KeyFrame animation to animate offset
        public void Animation1(Visual _target)
        {
            var animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, new Vector3(175.0f, 250.0f, 0.0f));
            animation.Duration = TimeSpan.FromMilliseconds(2000);

            _target.StartAnimation("Offset", animation);
        }

        // Button Click handler to setup Visual
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            SetupVisual();
        }

    }
}
