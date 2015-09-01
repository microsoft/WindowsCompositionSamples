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

namespace Animation_State_Changed
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private Visual _target;
        private CompositionPropertyAnimator _animator;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Define and setup the Green Square Visual that will be animated 
        public void SetupVisual()
        {
            // Intialize the Compositor
            _compositor = new Compositor();
            _root = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(Container);
            _compositor = _root.Compositor;

            // Generate the Green Square
            var colorVisual = _compositor.CreateSolidColorVisual();
            colorVisual.Color = Colors.Green;
            colorVisual.Size = new Vector2(150.0f, 150.0f);
            colorVisual.Offset = new Vector3(25.0f, 50.0f, 0.0f);

            // Add the image to the tree
            _root.Children.InsertAtTop(colorVisual);

            _target = colorVisual;
        }

        // Defines a Keyframe animation to animate offset
        public void AnimateVisual()
        {
            var animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1.0f, new Vector3(175.0f, 250.0f, 0.0f));
            animation.Duration = TimeSpan.FromMilliseconds(2000);


            _animator = _target.ConnectAnimation("Offset", animation);
            // Register for the state changed event (end)
            _animator.AnimationEnded += OnAnimationEnded;
            _animator.Start();
        }

        // Defines the event functionality when an animation complete.
        private void OnAnimationEnded(CompositionPropertyAnimator sender, AnimationEndedEventArgs eventArgs)
        {
            // For sample sake, explicitly looking for only one animation.
            if (sender == _animator)
            {
                ((ContainerVisual)_target.Parent).Children.Remove(_target);
                Debug.WriteLine("Animation Complete. Remove Visual from Tree");

                // Remove the registration for state event change
                sender.AnimationEnded -= OnAnimationEnded;
            }
        }

        // Button Click handler to setup Visual
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            SetupVisual();
        }

        // Button Click handler to begin the animation of the Visual
        private void Animate_Click(object sender, RoutedEventArgs e)
        {
            AnimateVisual();

        }
    }
}
