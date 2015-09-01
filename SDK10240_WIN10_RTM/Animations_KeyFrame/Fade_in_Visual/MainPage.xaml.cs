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
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Fade_in_Visual
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private Visual _target;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Define and setup the Green square visual that will be later animated
        private void ShowVisual_Click(object sender, RoutedEventArgs e)
        {
            // Initialize the Compositor
            _compositor = new Compositor();
            _root = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(root);
            _compositor = _root.Compositor;

            // Generate the Green Square
            var colorVisual = _compositor.CreateSolidColorVisual();
            colorVisual.Color = Colors.Green;
            colorVisual.Size = new Vector2(150.0f, 150.0f);
            colorVisual.Offset = new Vector3(50.0f, 150.0f, 0.0f);

            // Add the image to the tree
            _root.Children.InsertAtBottom(colorVisual);

            _target = colorVisual;

        }

        private void Animate_Click(object sender, RoutedEventArgs e)
        {
            var animation = _compositor.CreateScalarKeyFrameAnimation();

            // Create two keyframes that define starting and ending value of the property
            animation.InsertKeyFrame(0.0f, 0.00f);
            animation.InsertKeyFrame(1.0f, 1.00f);

            animation.Duration = TimeSpan.FromMilliseconds(4000);

            var animator = _target.ConnectAnimation("Opacity", animation);

            animator.Start();
        }
    }
}
