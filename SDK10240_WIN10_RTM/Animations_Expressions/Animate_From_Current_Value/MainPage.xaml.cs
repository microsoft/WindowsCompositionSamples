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
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Animate_From_Current_Value
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

        public void SetupVisual()
        {
            // Intialize the Compositor
            _compositor = new Compositor();
            _root = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(Container);
            _compositor = _root.Compositor;

            // Generate the Green Square
            var colorVisual = _compositor.CreateSolidColorVisual();
            colorVisual.Color = Colors.Green;
            colorVisual.Size = new System.Numerics.Vector2(50.0f, 50.0f);
            colorVisual.Offset = new Vector3(100.00f, 50.00f, 0.00f);

            // Add the Visual to the tree
            _root.Children.InsertAtTop(colorVisual);

            _target = colorVisual;
        }

        // Creates and defines the Keyframe animation using a current value of target Visual
        private void AnimateFromCurrentValue(Visual targetVisual, Vector3 finalOffset)
        {
            var animation = _compositor.CreateVector3KeyFrameAnimation();

            // Utilize a current value of the target visual in Expression KeyFrame
            // Note: If a keyframe at 0.0f not defined, we will auto define it using this.StartingValue.  
            animation.InsertExpressionKeyFrame(0.00f, "this.StartingValue");

            animation.InsertKeyFrame(1.00f, finalOffset);
            animation.Duration = TimeSpan.FromMilliseconds(3000);

            targetVisual.ConnectAnimation("Offset", animation).Start();

        }

        // Button click to initiate animation of visual to new Offset
        private void Animate_Click(object sender, RoutedEventArgs e)
        {
            var finalLocation = new Vector3(150.00f, 200.00f, 0.00f);
            AnimateFromCurrentValue(_target, finalLocation);
        }


        // Button click to initiate Visual setup
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            SetupVisual();
        }
    }
}
