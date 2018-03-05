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
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class TextShimmer : SamplePage
    {
        public TextShimmer()
        {
            this.InitializeComponent();
            this.Loaded += TextShimmer_Loaded;
        }

        public static string        StaticSampleName => "Text Shimmer";
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Target a XAML UIElement with a Composition Light"; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=869005";

        private void TextShimmer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //get interop compositor
            _compositor = ElementCompositionPreview.GetElementVisual(TextBlock).Compositor;

            //get interop visual for XAML TextBlock
            var text = ElementCompositionPreview.GetElementVisual(TextBlock);

            _pointLight = _compositor.CreatePointLight();
            _pointLight.Color = Colors.White;
            _pointLight.CoordinateSpace = text; //set up co-ordinate space for offset
            _pointLight.Targets.Add(text); //target XAML TextBlock

            //starts out to the left; vertically centered; light's z-offset is related to fontsize
            _pointLight.Offset = new Vector3(-(float)TextBlock.ActualWidth, (float)TextBlock.ActualHeight / 2, (float)TextBlock.FontSize);
            
            //simple offset.X animation that runs forever
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1, 2 * (float)TextBlock.ActualWidth);
            animation.Duration = TimeSpan.FromSeconds(3.3f);
            animation.IterationBehavior = AnimationIterationBehavior.Forever;

            _pointLight.StartAnimation("Offset.X", animation);
        }

        private Compositor _compositor;
        private PointLight _pointLight;
    }
}
