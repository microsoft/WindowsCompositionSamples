//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class PointerRotate : SamplePage
    {
        public PointerRotate()
        {
            this.InitializeComponent();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _container = _compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(root, _container);

            TiltUIElement();
        }

        public static string        StaticSampleName    { get { return "Pointer Rotate"; } }
        public override string      SampleName          { get { return StaticSampleName; } }
        public override string      SampleDescription   { get { return "Animate the Rotation Angle and Axis of an object based on the hover position of the pointer."; } }

        private void TiltUIElement()
        {

            // Grab the backing Visual for the UIElement image
            _tiltVisual = ElementCompositionPreview.GetElementVisual(tiltImage);

            // Set the CenterPoint of the backing Visual, so the rotation axis will defined from the middle
            _tiltVisual.CenterPoint = new Vector3((float)tiltImage.Width / 2, (float)tiltImage.Height / 2, 0f);

            // Grab the PropertySet containing the hover pointer data that will be used to drive the rotations
            // Note: We have a second UIElement we will grab the pointer data against and the other we will animate
            var hoverPosition = ElementCompositionPreview.GetPointerPositionPropertySet(HitTestRect);

            // Calculate distance from corner of quadrant to Center
            var center = new Vector3((float)tiltImage.Width / 2, (float)tiltImage.Height / 2, 0);
            var xSquared = Math.Pow(tiltImage.Width / 2, 2);
            var ySquared = Math.Pow(tiltImage.Height / 2, 2);
            var distanceToCenter = (float)Math.Sqrt(xSquared + ySquared);

            // || DEFINE THE EXPRESSION FOR THE ROTATION ANGLE ||             
            // The Visual will be rotated between -35 and +35 degrees depending on which "quadrant" the hover pointer is in.
            // Conceptually, divide the image into the 4 Cartesian Quadrants. Starting from the top left going clockwise: 2, 1, 4, 3
            // Quadrants 1 and 2 will have a positive rotation, while 3 and 4 will have negative rotations.
            // The amount of rotation is defined by the distance from the hover point location to the center.            
            var positiveRotateCheck = "(hover.Position.Y  < center.Y) || (hover.Position.X > center.X && hover.Position.Y == center.Y) ? ";
            var positiveRotateValue = "35 * ((Clamp(Distance(center, hover.Position), 0, distanceToCenter) % distanceToCenter)/distanceToCenter) : ";
            var negativeRotateCheck = "(hover.Position.Y > center.Y) || (hover.Position.X < center.X && hover.Position.Y == center.Y) ? ";
            var negativeRotateValue = "-35 * ((Clamp(Distance(center, hover.Position), 0, distanceToCenter) % distanceToCenter)/distanceToCenter) : this.CurrentValue";
            
            var angleExpression = _compositor.CreateExpressionAnimation();
            angleExpression.Expression = positiveRotateCheck + positiveRotateValue + negativeRotateCheck + negativeRotateValue;
            angleExpression.SetReferenceParameter("hover", hoverPosition);
            angleExpression.SetVector3Parameter("center", center);
            angleExpression.SetScalarParameter("distanceToCenter", distanceToCenter);
            _tiltVisual.StartAnimation("RotationAngleInDegrees", angleExpression);

            // || DEFINE THE EXPRESSION FOR THE ROTATION AXIS ||             
            // The RotationAxis will be defined as the axis perpendicular to vector position of the hover pointer.
            // The axis is calculated by first converting the pointer position into the coordinate space where the center point (0, 0) is in the middle.
            // The perpendicular axis is then calculated by transposing the cartesian x, y components and taking the minus sign of one.
            // The axis is dependent on which quadrant or axis the pointer position is on.
            var quad2Check = "(hover.Position.Y < center.Y && hover.Position.X < center.X) ? Vector3(-(-center.Y + hover.Position.Y), -center.X + hover.Position.X, 0) : ";
            var quad1Check = "(hover.Position.Y < center.Y && hover.Position.X > center.X) ? Vector3(-(-center.Y + hover.Position.Y), hover.Position.X - center.X, 0) : ";
            var quad4Check = "(hover.Position.Y > center.Y && hover.Position.X < center.X) ? Vector3((hover.Position.Y - center.Y), center.X - hover.Position.X, 0) : ";
            var quad3Check = "(hover.Position.Y > center.Y && hover.Position.X > center.X) ? Vector3((hover.Position.Y - center.Y), -(hover.Position.X - center.X), 0) : ";
            var xAxisCheck = "(hover.Position.Y == center.Y && hover.Position.X != center.X) ? Vector3(0, center.X, 0) : ";
            var yAxisCheck = "(hover.Position.Y != center.Y && hover.Position.X == center.X) ? Vector3(center.Y, 0, 0) : this.CurrentValue";

            var axisAngleExpression = _compositor.CreateExpressionAnimation();
            axisAngleExpression.Expression = quad1Check + quad2Check + quad3Check + quad4Check + xAxisCheck + yAxisCheck;
            axisAngleExpression.SetReferenceParameter("hover", hoverPosition);
            axisAngleExpression.SetVector3Parameter("center", center);
            _tiltVisual.StartAnimation("RotationAxis", axisAngleExpression);

        }

        // Define a perspective for the image so a perceived z-distance will be shown when the image rotates
        private void UpdatePerspective()
        {
            var visual = ElementCompositionPreview.GetElementVisual(root);

            var size = new Vector2((float)root.ActualWidth, (float)root.ActualHeight);

            Matrix4x4 perspective = new Matrix4x4(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, -1.0f / (size.X / 2),
                0.0f, 0.0f, 0.0f, 1.0f);

            // Matrix translations are to make sure the perspective is "centered"
            visual.TransformMatrix =
                Matrix4x4.CreateTranslation(-size.X / 2, -size.Y / 2, 0f) *
                perspective *
                Matrix4x4.CreateTranslation(size.X / 2, size.Y / 2, 0f);
        }

        private void Root_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            UpdatePerspective();
        }

        private ContainerVisual _container;
        private Compositor _compositor;
        private Visual _tiltVisual;
    }
}
