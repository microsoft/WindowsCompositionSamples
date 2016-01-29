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

using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace ContinuitySample
{
    static class ContinuityHelper
    {

        /// <summary>
        /// Set up Continuity by hooking continuing Visuals to root Frame and getting global co-ordinates so that Visual seems to remain on the same location even though pages change and local coordinate space changes as well. 
        /// </summary>
        /// <param name="containerVisual">Container Visual which contains visual that needs to continue between pages</param>
        /// <param name="hostElement">UIElement which the Visual is hosted in</param>
        /// <param name="newContainerVisual">new ContainerVisual after re-parent of sprite Visual to root.</param>
        public static void SetupContinuity(ContainerVisual containerVisual,UIElement HostElement, out ContainerVisual newContainerVisual)
        {
            if (null == containerVisual || null == HostElement)
            {
                newContainerVisual = null;
                return;
            }
            Frame rootFrame = Window.Current.Content as Frame;
            Visual rootVisual = ElementCompositionPreview.GetElementVisual(rootFrame);
            Compositor compositor = rootVisual.Compositor;
            //remove element from current tree 
            var visualChild = containerVisual.Children.FirstOrDefault();
            containerVisual.Children.Remove(visualChild);

            //create temp container to add the visual to the root 
            ContainerVisual tempContainer = compositor.CreateContainerVisual();
            tempContainer.Children.InsertAtTop(visualChild);
            //Get location of visual as compared to root frame. 
            var coordinate = HostElement.TransformToVisual(rootFrame);
            var position = coordinate.TransformPoint(new Point(0, 0));
            //set the location of container visual to same visual location but now root as the parent.
            tempContainer.Offset = new System.Numerics.Vector3((float)position.X, (float)position.Y, 0);
            visualChild.Offset = new System.Numerics.Vector3(0, 0, 0);

            //add container with sprite to the window of app
            ElementCompositionPreview.SetElementChildVisual(rootFrame, tempContainer);
            containerVisual = null;
            newContainerVisual = tempContainer;
        }
        /// <summary>
        /// Start animations and re-parent to destination UI Elmenent to finish the operations
        /// </summary>
        /// <param name="destinationElement">Destination UIElement where Visual should show up after page has loaded</param>
        /// <param name="containerVisual">ContainerVisual that contains Visual which needs to show in UIElement</param>
        /// <param name="newContainerVisual">ContainerVisual after visual is parented to UIElement</param>
        public static void InitiateContinuity(FrameworkElement destinationElement, ContainerVisual containerVisual, out ContainerVisual newContainerVisual)
        {
            if (null == containerVisual || null == destinationElement)
            {
                newContainerVisual = null;
                return;
            }
            //Get the frame of Window
            Frame rootFrame = Window.Current.Content as Frame;
            Visual rootVisual = ElementCompositionPreview.GetElementVisual(rootFrame);
            Compositor compositor = rootVisual.Compositor;
            //Create Temporary Container. this will be added to final UIElement 
            ContainerVisual _TempContainer = compositor.CreateContainerVisual();
            // Get Sprite Visual from incoming container
            var spriteHeroImage = containerVisual.Children.FirstOrDefault();

            //Create animation scoped batch to track animation completion and to complete re-parenting
            CompositionScopedBatch scopeBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            //Get coordinates of UIElement in reference to root so that it can be used for animations final value
            var coordinate = destinationElement.TransformToVisual(rootFrame);
            var position = coordinate.TransformPoint(new Point(0, 0));
            
            //Create offset animation to make visual move on screen
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3((float)position.X, (float)position.Y, 0));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(600);

            //Create size animation to change size of the visuals 
            Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
            sizeAnimation.InsertKeyFrame(1f, new System.Numerics.Vector2((float)destinationElement.ActualWidth, (float)destinationElement.ActualHeight));
            sizeAnimation.Duration = TimeSpan.FromMilliseconds(600);

            //Start Animations 
            spriteHeroImage.StartAnimation("size", sizeAnimation);
            containerVisual.StartAnimation("offset", offsetAnimation);
            //Scoped batch completed event. 
            scopeBatch.Completed += (o, e) =>
            {
                //Re-parent SpriteVisual to temp container and add temp container to UIElement as animations are finished.
                spriteHeroImage.Offset = new System.Numerics.Vector3(0, 0, 0);
                containerVisual.Children.Remove(spriteHeroImage);
                _TempContainer.Children.InsertAtTop(spriteHeroImage);
                ElementCompositionPreview.SetElementChildVisual(destinationElement, _TempContainer);
                containerVisual = null;

            };
            newContainerVisual = _TempContainer;

            scopeBatch.End();
        }
    }
}
