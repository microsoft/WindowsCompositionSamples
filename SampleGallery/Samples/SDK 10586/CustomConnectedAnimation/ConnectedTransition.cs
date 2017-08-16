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

using ExpressionBuilder;
using SamplesCommon;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

using EF = ExpressionBuilder.ExpressionFunctions;

namespace CompositionSampleGallery
{
    class ConnectedTransition
    {
        private UIElement               _host;
        private UIElement               _parent;
        private object                  _payload;
        private SpriteVisual            _sprite;
        private bool                    _imageLoaded;
        private bool                    _animationCompleted;
        private CompositionScopedBatch  _scopeBatch;
        private CompositionImage        _targetImage;

        public object Payload { get { return _payload; } }
        public object Host { get { return _host; } }

        public ConnectedTransition()
        {
        }

        public void Initialize(UIElement host, CompositionImage sourceElement, object payload)
        {
            _host = host;
            _parent = host;
            _payload = payload;

            // Make a copy of the sourceElement's sprite so we can hand it off to the next page
            SpriteVisual sourceSprite = sourceElement.SpriteVisual;
            Compositor compositor = sourceSprite.Compositor;
            _sprite = compositor.CreateSpriteVisual();
            _sprite.Size = sourceSprite.Size;
            _sprite.Brush = sourceElement.SurfaceBrush;

            // We're going to use the backing surface, make sure it doesn't get released
            sourceElement.SharedSurface = true;

            // Determine the offset from the host to the source element used in the transition
            GeneralTransform coordinate = sourceElement.TransformToVisual(_parent);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            // Set the sprite to that offset relative to the host
            _sprite.Offset = new Vector3((float)position.X, (float)position.Y, 0);

            // Set the sprite as the content under the host
            ElementCompositionPreview.SetElementChildVisual(_parent, _sprite);
        }

        public void Start(UIElement newParent, CompositionImage targetImage, ScrollViewer scrollViewer, UIElement animationTarget)
        {
            Visual transitionVisual = ElementCompositionPreview.GetElementChildVisual(_parent);
            ElementCompositionPreview.SetElementChildVisual(_parent, null);


            //
            // We need to reparent the transition visual under the new parent.  This is important to ensure
            // it's propertly clipped, etc.
            //

            GeneralTransform coordinate = newParent.TransformToVisual(_parent);
            Point position = coordinate.TransformPoint(new Point(0, 0));

            Vector3 currentOffset = transitionVisual.Offset;
            currentOffset.X -= (float)position.X;
            currentOffset.Y -= (float)position.Y;
            transitionVisual.Offset = currentOffset;

            _parent = newParent;
            _targetImage = targetImage;

            // Move the transition visual to it's new parent
            ElementCompositionPreview.SetElementChildVisual(_parent, transitionVisual);

            // Hide the target Image now since the handoff visual is still transitioning
            targetImage.Opacity = 0f;

            // Load image if necessary
            _imageLoaded = targetImage.IsContentLoaded;
            if (!_imageLoaded)
            {
                targetImage.ImageOpened += CompositionImage_ImageOpened;
            }

            //
            // Create a scoped batch around the animations.  When the batch completes, we know the animations
            // have finished and we can cleanup the transition related objects.
            //

            Compositor compositor = transitionVisual.Compositor;
            _scopeBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            //
            // Determine the offset between the parent and the target UIElement.  This will be used to calculate the
            // target position we are animating to.
            //

            coordinate = targetImage.TransformToVisual(_parent);
            position = coordinate.TransformPoint(new Point(0, 0));

            TimeSpan totalDuration = TimeSpan.FromMilliseconds(1000);
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();

            if (scrollViewer != null)
            {
                CompositionPropertySet scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

                // Include the scroller offset as that is a factor
                position.X += scrollViewer.HorizontalOffset;
                position.Y += scrollViewer.VerticalOffset;


                //
                // Since the target position is relative to the target UIElement which can move, we need to construct
                // an expression to bind the target's position to the end position of our animation.
                //

                var scrollPropSet = scrollProperties.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
                var itemOffset = new Vector3((float)position.X, (float)position.Y, 0);
                var expression = EF.Vector3(scrollPropSet.Translation.X, scrollPropSet.Translation.Y, 0) + itemOffset ;
                offsetAnimation.InsertExpressionKeyFrame(1f, expression);
                offsetAnimation.Duration = totalDuration;
            }
            else
            {
                offsetAnimation.InsertKeyFrame(1, new Vector3((float)position.X, (float)position.Y, 0));
                offsetAnimation.Duration = totalDuration;
            }

            // Create size animation to change size of the visual
            Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
            sizeAnimation.InsertKeyFrame(1f, new Vector2((float)targetImage.ActualWidth, (float)targetImage.ActualHeight));
            sizeAnimation.Duration = totalDuration;

            // Create the fade in animation for the other page content
            if (animationTarget != null)
            {
                Visual fadeVisual = ElementCompositionPreview.GetElementVisual(animationTarget);
                ScalarKeyFrameAnimation fadeIn = compositor.CreateScalarKeyFrameAnimation();
                fadeIn.InsertKeyFrame(0f, 0.0f);
                fadeIn.InsertKeyFrame(1f, 1.0f);
                fadeIn.Duration = totalDuration;
                fadeVisual.StartAnimation("Opacity", fadeIn);
            }

            //Start Animations 
            _sprite.StartAnimation("Size", sizeAnimation);
            _sprite.StartAnimation("Offset", offsetAnimation);

            //Scoped batch completed event
            _scopeBatch.Completed += ScopeBatch_Completed;
            _scopeBatch.End();

            // Clear the flag
            _animationCompleted = false;
        }

        public void Cancel()
        {
            if (!Completed)
            {
                Complete(true);
            }
        }

        public bool Completed
        {
            get
            {
                // Either we aren't actively transitioning or the image and animation have completed
                return (_sprite == null) || (_imageLoaded && _animationCompleted);
            }
        }

        private void Complete(bool forceComplete)
        {
            // If we're forcing completion, make sure the scope batch is cleaned up
            if (forceComplete && (_scopeBatch != null))
            {
                CleanupScopeBatch();
            }

            // If we've completed the transition or we're forcing completion, cleanup
            if (forceComplete || (_imageLoaded && _animationCompleted))
            {
                _sprite = null;

                // Clear the sprite from the UIElement
                ElementCompositionPreview.SetElementChildVisual(_parent, null);

                // Clean up the image and show it
                if (_targetImage != null)
                {
                    _targetImage.ImageOpened -= CompositionImage_ImageOpened;

                    _targetImage.Opacity = 1f;

                    _targetImage = null;
                }
            }
        }

        private void CompositionImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            _imageLoaded = true;
            Complete(false);
        }

        private void ScopeBatch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            _animationCompleted = true;
            Complete(false);

            CleanupScopeBatch();
        }

        private void CleanupScopeBatch()
        {
            if (_scopeBatch != null)
            {
                _scopeBatch.Completed -= ScopeBatch_Completed;
                _scopeBatch.Dispose();
                _scopeBatch = null;
            }
        }
    }
}
