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

namespace Parallax_Expression
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor;
        private ContainerVisual _root;
        private Visual _foreground;
        private Visual _background;

        public MainPage()
        {
            this.InitializeComponent();
        }

        public void Parallax_Expression()
        {
            _compositor = new Compositor();
            _root = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(Container);
            _compositor = _root.Compositor;

            // Create the Blue Square
            var blueSquareVisual = _compositor.CreateSolidColorVisual();
            blueSquareVisual.Color = Colors.Blue;
            blueSquareVisual.Size = new System.Numerics.Vector2(100.0f, 100.0f);
            blueSquareVisual.Offset = new Vector3(100.00f, 50.00f, 0.00f);

            // Create the Green Square
            var greenSquareVisual = _compositor.CreateSolidColorVisual();
            greenSquareVisual.Color = Colors.Green;
            greenSquareVisual.Size = new System.Numerics.Vector2(50.0f, 50.0f);
            greenSquareVisual.Offset = new Vector3(100.00f, 50.00f, 0.00f);


            // Add the Blue and Green square visuals to the tree
            _root.Children.InsertAtTop(blueSquareVisual);
            _root.Children.InsertAtTop(greenSquareVisual);

            _foreground = greenSquareVisual;
            _background = blueSquareVisual;
        }

        // Creates and Defines Expression Animation for basic Parallax principle
        private void Parallax_Animation(Visual foreground, Visual background)
        {
            var animation = _compositor.CreateExpressionAnimation("background.Offset.x * (foreground.Size.X / background.Size.X)");

            animation.SetReferenceParameter("foreground", foreground);
            animation.SetReferenceParameter("background", background);

            background.ConnectAnimation("Offset.x", animation).Start();
        }

        // Button click to initiate Visual setup
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            Parallax_Expression();
        }

        // Button click to initiate animation of visual to new Offset
        private void Animate_Click(object sender, RoutedEventArgs e)
        {
            Parallax_Animation(_foreground, _background);
        }
    }
}
