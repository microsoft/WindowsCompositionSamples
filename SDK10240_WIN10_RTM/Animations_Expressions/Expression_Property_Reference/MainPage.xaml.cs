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

namespace Expression_Property_Reference
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private Visual _source;
        private Visual _target;

        public MainPage()
        {
            this.InitializeComponent();
        }

        public void SetupVisuals()
        {
            // Intialize the Compositor
            _compositor = new Compositor();
            _root = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(Container);
            _compositor = _root.Compositor;

            // Create the Blue Square
            var blueSquareVisual = _compositor.CreateSolidColorVisual();
            blueSquareVisual.Color = Colors.Blue;
            blueSquareVisual.Size = new System.Numerics.Vector2(50.0f, 50.0f);
            blueSquareVisual.Offset = new Vector3(100.00f, 50.00f, 0.00f);

            // Create the Green Square with 20% opacity
            var greenSquareVisual = _compositor.CreateSolidColorVisual();
            greenSquareVisual.Color = Colors.Green;
            greenSquareVisual.Size = new System.Numerics.Vector2(50.0f, 50.0f);
            greenSquareVisual.Offset = new Vector3(150.00f, 100.00f, 0.00f);
            greenSquareVisual.Opacity = 0.20f;

            // Add the Visuals to the tree
            _root.Children.InsertAtTop(greenSquareVisual);
            _root.Children.InsertAtTop(blueSquareVisual);

            _source = greenSquareVisual;
            _target = blueSquareVisual;
        }

        // Creates and defines Expression Animation to modify target object's property by referencing another Visual's property
        private void AnimateOpacity(Visual sourceVisual, Visual targetVisual)
        {
            // Reference the Opacity property of another Composition Visual in the expression
            var expression = _compositor.CreateExpressionAnimation("source.Opacity");

            expression.SetReferenceParameter("source", sourceVisual);

            targetVisual.ConnectAnimation("Opacity", expression).Start();
        }

        // Button click to initiate setup of Visuals
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            SetupVisuals();
        }

        // Button click to initiate animation of visual to new Opacity
        private void Animate_Click(object sender, RoutedEventArgs e)
        {
            AnimateOpacity(_source, _target);
        }
    }
}
