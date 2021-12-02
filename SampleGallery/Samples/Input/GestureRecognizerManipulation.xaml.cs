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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;

namespace CompositionSampleGallery
{
    public sealed partial class GestureRecognizerManipulation : SamplePage
    {
        public static string StaticSampleName => "Manipulation Gesture Recognizer";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Demonstrates the GestureRecognizer's Rotation, Scale and X/Y translate through pinching, " +
                                                        "two finger touch rotation, and dragging gestures on the image. " +
                                                        "Rotation and Scale only support touch, whereas X/Y translate supports mouse, touch, and pen";
        public override string SampleDescription => StaticSampleDescription;

        private Microsoft.UI.Input.GestureRecognizer _gestureRecognizer;

        private TransformGroup _cumulativeTransform;
        private MatrixTransform _previousTransform;
        private CompositeTransform _deltaTransform;

        public GestureRecognizerManipulation()
        {
            this.InitializeComponent();

            _cumulativeTransform = new TransformGroup();
            _previousTransform = new MatrixTransform();
            _deltaTransform = new CompositeTransform();

            _cumulativeTransform.Children.Add(_previousTransform);
            _cumulativeTransform.Children.Add(_deltaTransform);

            ImageVisual.RenderTransform = _cumulativeTransform;

            _gestureRecognizer = new Microsoft.UI.Input.GestureRecognizer
            {
                GestureSettings =
                    GestureSettings.ManipulationRotate |
                    GestureSettings.ManipulationScale |
                    GestureSettings.ManipulationTranslateX |
                    GestureSettings.ManipulationTranslateY
            };

            _gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            _gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            _gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            ImageVisual.CapturePointer(args.Pointer);

            PointerPoint currentPoint = args.GetCurrentPoint(grid);
            _gestureRecognizer.ProcessDownEvent(currentPoint);
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint currentPoint = args.GetCurrentPoint(grid);
            if (currentPoint.IsInContact)
            {
                IList<PointerPoint> points = args.GetIntermediatePoints(grid);
                _gestureRecognizer.ProcessMoveEvents(points);
            }
            else
            {
                _gestureRecognizer.CompleteGesture();
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint currentPoint = args.GetCurrentPoint(grid);
            _gestureRecognizer.ProcessUpEvent(currentPoint);
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            _gestureRecognizer.CompleteGesture();
            ImageVisual.ReleasePointerCapture(args.Pointer);
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs args)
        {
        }

        private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs args)
        {
            // TODO: Image gets distorted when "zooming in".
            _previousTransform.Matrix = _cumulativeTransform.Value;

            _deltaTransform.Rotation = args.Delta.Rotation;
            _deltaTransform.ScaleX = _deltaTransform.ScaleY = args.Delta.Scale;
            _deltaTransform.TranslateX = args.Delta.Translation.X;
            _deltaTransform.TranslateY = args.Delta.Translation.Y;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs args)
        {
        }
    }
}
