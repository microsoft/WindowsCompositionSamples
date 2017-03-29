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

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace ImplicitAnimations
{
    public static class UIElementExtensionMethods
    {
        public static void EnableLayoutImplicitAnimations(this UIElement element)
        {
            Compositor compositor;
            var result = ElementCompositionPreview.GetElementVisual(element);
            compositor = result.Compositor;
#if SDKVERSION_14393
            var elementImplicitAnimation = compositor.CreateImplicitAnimationCollection();
            elementImplicitAnimation["Offset"] = createOffsetAnimation(compositor);
            result.ImplicitAnimations = elementImplicitAnimation;
#endif
        }

        private static KeyFrameAnimation createOffsetAnimation(Compositor compositor)
        {
            Vector3KeyFrameAnimation kf = compositor.CreateVector3KeyFrameAnimation();
            kf.InsertExpressionKeyFrame(1.0f, "FinalValue");
            kf.Duration = TimeSpan.FromSeconds(0.9);
            return kf;
        }
    }
}
