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

using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Microsoft.UI.Composition.Toolkit;
using Windows.Foundation;

namespace ContinuitySample
{
    /// <summary>
    /// Master page showing the list of Fruits. 
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Public variables used on the page. 
        Compositor _compositor;
        ContainerVisual _root;
        ItemCollection _sampleCollection;
        Item _incomingObject;
        public MainPage()
        {
            this.InitializeComponent();
            
            //Get the root Frame of Window. 
            Frame rootFrame = Window.Current.Content as Frame;
            _root = ElementCompositionPreview.GetElementVisual(rootFrame) as ContainerVisual;
            
            //Get compositor 
            _compositor = _root.Compositor;
            
            //Set up Sample Data to load the list.
            StoreData sampleData = new StoreData();
            _sampleCollection = sampleData.Collection;
        }
        /// <summary>
        /// Bind Compositon Images to the list which need to continue to the next page on navigation. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FruitsList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            //Get the clicked Item 
            var item = args.Item as Item;
            
            //Get hold of the UIElement to bind items. 
            var contentPanel = args.ItemContainer.ContentTemplateRoot as StackPanel;
            
            //Create Composition Image from data source
            var parentRectangle = _compositor.CreateContainerVisual();
            var rectangleColor = _compositor.CreateSpriteVisual();
            rectangleColor.Size = new System.Numerics.Vector2(40,40);
            
            //Use ImageFactory to load image from URI. 
            try
            {
                CompositionImageFactory imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);
                CompositionImageOptions options = new CompositionImageOptions()
                {
                    DecodeWidth = 160,
                    DecodeHeight = 160,
                };
                CompositionImage image = imageFactory.CreateImageFromUri(item.imageUri, options);
                rectangleColor.Brush = _compositor.CreateSurfaceBrush(image.Surface);

            }
            catch
            {
                rectangleColor.Brush = _compositor.CreateColorBrush(Windows.UI.Colors.Red);               
            }
            
            //Bind Composition Image to UIElement - Rectangle in this case. 
            Rectangle rectImage = contentPanel.FindName("imageItem") as Rectangle;
            parentRectangle.Children.InsertAtTop(rectangleColor);
            item.imageContainerVisual = parentRectangle;
            item.rectImage = rectImage;
            ElementCompositionPreview.SetElementChildVisual(rectImage, parentRectangle);
            
        }
        private void FruitsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Set up item before user naivate 
            //Hook up clicked item's composition visual to the rootframe and update data source with new contianervisual for further reference. 
            //Hook up containerVisual to UIElement so that you can easily query its children and manipulate SpriteVisual. 
            ContainerVisual tempContainer;
            try
            {
                ContinuityHelper.SetupContinuity((e.ClickedItem as Item).imageContainerVisual, (e.ClickedItem as Item).rectImage, out tempContainer);

                //Update datasourece with new containerVisual for future refernece 
                (e.ClickedItem as Item).imageContainerVisual = tempContainer;

                //keep track of object that was sent in case of back button click. 
                _incomingObject = e.ClickedItem as Item;
                //navigate to new page
                this.Frame.Navigate(typeof(DetailsPage), e.ClickedItem);
            }
            catch
            {
                throw;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Hide the back button on the list page as there is no where to go back to. 
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            if (string.IsNullOrEmpty(e.Parameter.ToString()))
                return;
            _incomingObject = e.Parameter as Item;
        }
        private void FruitsList_Loaded(object sender, RoutedEventArgs e)
        {
            if (null == _incomingObject)
                return;
            ContainerVisual tempContainer;
            
            //Execute animations and re-parenting when layout is available so that we know where to take the visual for final animation. 
            ContinuityHelper.InitiateContinuity(_incomingObject.rectImage, _incomingObject.imageContainerVisual, out tempContainer);
            _incomingObject.imageContainerVisual = tempContainer;
        }
    }
}
