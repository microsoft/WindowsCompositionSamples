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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;


namespace CompositionSampleGallery
{
    public sealed partial class PullToRefresh : SamplePage
    {
        private Visual _image;
        private Visual _root;
        private Compositor _compositor;
        private VisualInteractionSource _interactionSource;
        private InteractionTracker _tracker;
        private Windows.UI.Core.CoreWindow _Window;

        public PullToRefresh()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
            _Window = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow;
            this.PointerPressed += new PointerEventHandler(Window_PointerPressed);

        }

        

        public static string       StaticSampleName     { get { return "PullToRefresh ListView Items"; } }
        public override string     SampleName           { get { return StaticSampleName; } }
        public override string     SampleDescription    { get { return "Demonstrates how to apply a parallax effect to each item in a ListView control. As you scroll the ListView control watch as each ListView item translates at a different rate in comparison to the ListView's scroll position."; } }
        public override string     SampleCodeUri        { get { return "http://go.microsoft.com/fwlink/p/?LinkID=761169"; } }

        public LocalDataSource Model { set; get; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

         
            
            ThumbnailList.ItemsSource = Model.Items;
            _image = ElementCompositionPreview.GetElementVisual(ContentPanel);
            _root = ElementCompositionPreview.GetElementVisual(Root);
            _compositor = _image.Compositor;

            ConfigureInteractionTracker();
        }



        private void ConfigureInteractionTracker()
        {
            _tracker = InteractionTracker.Create(_compositor);
            
            _interactionSource = VisualInteractionSource.Create(_root);

            _interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;


            //_interactionSource.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;

            _tracker.InteractionSources.Add(_interactionSource);


            _tracker.MaxPosition = new Vector3((float)Root.ActualWidth, (float)Root.ActualHeight, 0);
            _tracker.MinPosition = new Vector3(-(float)Root.ActualWidth, -(float)Root.ActualHeight, 0);

            //
            // Use the Tacker's Position (negated) to apply to the Offset of the Image.
            //

            var positionExpression = _compositor.CreateExpressionAnimation("-tracker.Position");
            positionExpression.SetReferenceParameter("tracker", _tracker);

            _image.StartAnimation("Offset", positionExpression);
        }

        private void Window_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)

            {

                // Tell the system to use the gestures from this pointer point (if it can).

                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(ThumbnailList));

            }

        }
        

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)

        {

            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)

            {

                // Tell the system to use the gestures from this pointer point (if it can).

                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Root));
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(ThumbnailList));

            }

        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);
            //System.Diagnostics.Debug.WriteLine("GridClip.Rect" + GridClip.Rect);
        }

    }
}
