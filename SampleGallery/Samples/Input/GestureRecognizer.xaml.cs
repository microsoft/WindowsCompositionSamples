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

using System.Collections.Generic;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.UI;
using Windows.Foundation;
using Microsoft.UI;

namespace CompositionSampleGallery
{
    public sealed partial class GestureRecognizer : SamplePage
    {
        public static string StaticSampleName => "Gesture Recognizer";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Demonstrates GestureRecognizer's Tap, Double Tap, Right Tap, Drag, and Hold " +
                                                        "events by changing the rectangle's color when a different gesture is detected on the rectangle. " +
                                                        "The rectangle's GestureRecognizer supports mouse, touch and pen for all events " +
                                                        "with the exception of Drag, which only supports mouse and pen.";
        public override string SampleDescription => StaticSampleDescription;

        private Microsoft.UI.Input.GestureRecognizer _gestureRecognizer;
        private Visual _rectangleVisual;
        private Point _relativePoint;

        public GestureRecognizer()
        {
            this.InitializeComponent();

            _rectangleVisual = ElementCompositionPreview.GetElementVisual(GestureRectangle);
            _gestureRecognizer = new Microsoft.UI.Input.GestureRecognizer
            {
                GestureSettings =
                    GestureSettings.Tap |
                    GestureSettings.DoubleTap |
                    GestureSettings.RightTap |
                    GestureSettings.Drag |
                    GestureSettings.Hold |
                    GestureSettings.HoldWithMouse
            };
            _gestureRecognizer.Tapped += OnTapped;
            _gestureRecognizer.RightTapped += OnRightTapped;
            _gestureRecognizer.Dragging += OnDrag;
            _gestureRecognizer.Holding += OnHold;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            GestureRectangle.CapturePointer(args.Pointer);

            PointerPoint pointerPoint = args.GetCurrentPoint(Grid);

            // Get coordinates relative to the Rectangle.
            GeneralTransform transform = Grid.TransformToVisual(GestureRectangle);
            _relativePoint = transform.TransformPoint(new Point(pointerPoint.Position.X, pointerPoint.Position.Y));

            _gestureRecognizer.ProcessDownEvent(pointerPoint);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint currentPoint = args.GetCurrentPoint(Grid);
            if (currentPoint.IsInContact)
            {
                IList<PointerPoint> points = args.GetIntermediatePoints(Grid);
                _gestureRecognizer.ProcessMoveEvents(points);
            }
            else
            {
                _gestureRecognizer.CompleteGesture();
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint(Grid);
            _gestureRecognizer.ProcessUpEvent(pointerPoint);
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            _gestureRecognizer.CompleteGesture();
        }

        private void OnTapped(object sender, TappedEventArgs args)
        {
            if (args.TapCount == 2)
            {
                GestureResultText.Text = "Double Tapped";
            }
            else
            {
                GestureResultText.Text = "Tapped";
            }

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
        }

        private void OnRightTapped(object sender, RightTappedEventArgs args)
        {
            GestureResultText.Text = "Right Tapped";

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
        }

        private void OnDrag(object sender, DraggingEventArgs args)
        {
            GestureResultText.Text = "Drag";

            var recOffset = _rectangleVisual.Offset;
            recOffset.X = (float)(args.Position.X - _relativePoint.X);
            recOffset.Y = (float)(args.Position.Y - _relativePoint.Y);
            _rectangleVisual.Offset = recOffset;
        }

        private void OnHold(object sender, HoldingEventArgs args)
        {
            GestureResultText.Text = "Holding";

            var color = (GestureRectangle.Fill as SolidColorBrush).Color;
            GestureRectangle.Fill = (color == Colors.Red) ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Red);
        }
    }
}
