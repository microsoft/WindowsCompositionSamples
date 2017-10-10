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

using SamplesCommon;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class ShadowPlayground : SamplePage
    {
        private Visual _shadowContainer;
        private Compositor _compositor;
        private SpriteVisual _imageVisual;
        private CompositionImage _image;
        private ManagedSurface _imageMaskSurface;
        private CompositionMaskBrush _maskBrush;
        private bool _isMaskEnabled;

        public ShadowPlayground()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Shadow Playground"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Experiment with the available properties on the DropShadow object to create interesting shadows."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761177"; 

        private void SamplePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get backing visual from shadow container and interop compositor
            _shadowContainer = ElementCompositionPreview.GetElementVisual(ShadowContainer);
            _compositor = _shadowContainer.Compositor;

            // Get CompositionImage, its sprite visual
            _image = VisualTreeHelperExtensions.GetFirstDescendantOfType<CompositionImage>(ShadowContainer);
            _imageVisual = _image.SpriteVisual;

            // Load mask asset onto surface using helpers in SamplesCommon
            _imageMaskSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
            
            // Get surface brush from composition image
            CompositionSurfaceBrush source = _image.SurfaceBrush as CompositionSurfaceBrush;

            // Create mask brush for toggle mask functionality
            _maskBrush = _compositor.CreateMaskBrush();
            _maskBrush.Mask = _imageMaskSurface.Brush;
            _maskBrush.Source = source;

            // Initialize toggle mask
            _isMaskEnabled = false;
        }

        private void SamplePage_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_imageMaskSurface != null)
            {
                _imageMaskSurface.Dispose();
            }
        }

        private void MaskButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_isMaskEnabled) //then remove mask
            {
                _image.Brush = _maskBrush.Source; //set set composition image's brush to (the initial) surfacebrush (source) 
                Shadow.Mask = null; //remove mask from shadow
            }
            else //add mask
            {
                _image.Brush = _maskBrush; //set composition image's brush to maskbrush
                Shadow.Mask = _maskBrush.Mask; //add mask to shadow
            }

            // Update bool
            _isMaskEnabled = !_isMaskEnabled;
        }
    }
}
