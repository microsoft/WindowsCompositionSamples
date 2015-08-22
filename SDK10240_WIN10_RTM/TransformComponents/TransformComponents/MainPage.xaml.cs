//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
//
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TransformComponents
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Create Container Visual from Grid to be used as root of Visual Tree
            _root = ElementCompositionPreview.GetContainerVisual(Placeholder) as ContainerVisual;
            _compositor = _root.Compositor;

            // Create Solid Color Visual and insert into Visual Tree
            _mainVisual = _compositor.CreateSolidColorVisual();
            _mainVisual.Size = new Vector2(RectangleWidth, RectangleHeight);
            _mainVisual.Color = Windows.UI.Colors.Blue;
            _mainVisual.CenterPoint = new Vector3(RectangleWidth / 2, RectangleHeight / 2, 0);

            float rectPositionX = (float)(-RectangleWidth / 2);
            float rectPositionY = (float)(-RectangleHeight / 2);
            _mainVisual.Offset = new Vector3(rectPositionX, rectPositionY, 0);

            _root.Children.InsertAtTop(_mainVisual);
        }

        private void OpacityToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visual between fully opaque and semi-transparent
            if(_mainVisual.Opacity == 1.0f)
            {
                _mainVisual.Opacity = 0.5f;
            }
            else
            {
                _mainVisual.Opacity = 1.0f;
            }
        }

        private void RotationToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visual between no rotation and 45 degree rotation
            _mainVisual.RotationAxis = new Vector3(0, 0, 1.0f);
            if(_mainVisual.RotationAngle == 0)
            {
                _mainVisual.RotationAngle = 45.0f;
            }
            else
            {
                _mainVisual.RotationAngle = 0;
            }
        }

        private void ScaleToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visual between no scaling (scale of 1) and 1.5 scaling of height and width
            if(_mainVisual.Scale == Vector3.One)
            {
                _mainVisual.Scale = new Vector3(1.5f, 1.5f, 1.0f);
            }
            else
            {
                _mainVisual.Scale = Vector3.One;
            }
        }

        private void InsetClipToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visual between having no clip and a clip that squares the visual
            if(_mainVisual.Clip == null)
            {
                InsetClip iClip = _compositor.CreateInsetClip();
                iClip.LeftInset = (RectangleWidth - RectangleHeight) / 2;
                iClip.RightInset = iClip.LeftInset;
                _mainVisual.Clip = iClip;
            }
            else
            {
                _mainVisual.Clip = null;
            }
        }

        const float RectangleWidth = 300.0f;
        const float RectangleHeight = 200.0f;

        SolidColorVisual _mainVisual;
        ContainerVisual _root;
        Compositor _compositor;
    }
}
