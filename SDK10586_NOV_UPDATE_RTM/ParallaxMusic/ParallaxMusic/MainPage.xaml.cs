/*
Copyright (c) Microsoft Corporation 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE
*/

using ParallaxMusic.Data;
using ParallaxMusic.DataContract;
using System;
using System.Numerics;
using Windows.Data.Json;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ParallaxMusic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        ///  Helper function to return a ScrollViewer given a tree of DependencyObjects
        /// </summary>
        /// <param name="o">A DependencyObject that contains a ScrollViewer</param>
        /// <returns>A ScrollViewer, or null if none is found</returns>
        private static ScrollViewer GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o as ScrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Contructor for the MainPage
        /// Initializes the data source, CompositionObjects, and wires up event handlers
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.InitializeComposition();

            this.InitializeData();

            this.Loaded += MainPage_Loaded;
            this.SizeChanged += MainPage_SizeChanged;

            albumsList.Loaded += AlbumsList_Loaded;
        }

        /// <summary>
        /// Artist data model used for Databinding
        /// </summary>
        public Artist Artist
        {
            get; set;
        }

        /// <summary>
        /// Initializes the IDataSource and requests the Arist information.
        /// When the data returns, it will update any OneWay data bindings.
        /// </summary>
        private async void InitializeData()
        {
            _dataSource = new RemoteDataSource();
            var success = await _dataSource.Initialize();
            if (success)
            {
                Artist artist = await _dataSource.GetArtistAsync(DEFAULT_ARTIST_ID);
                if (artist != null)
                {
                    Artist = artist;

                    // Bind our data to our UI
                    Bindings.Update();

                    var width = Window.Current.Bounds.Width;
                    var height = Window.Current.Bounds.Height;

                    artistImage.Source = new BitmapImage(new Uri(Artist.ImageUrl + "&h=768&w=1024"));

                    progressRing.IsActive = false;
                    progressRing.Visibility = Visibility.Collapsed;

                    ElementCompositionPreview.GetElementVisual(this).StartAnimation("Opacity", _pageFadeIn);
                }
            }
        }

        /// <summary>
        /// Creates the necessary ExpressionAnimations objects that will be
        /// used when connecting Visuals and a ScrollViewer
        /// </summary>
        private void InitializeComposition()
        {
            // Obtain an instance of the Compositor that will be used to create Composition Objects 
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Create a CompositionPropertySet to store initial values used in Expression Animations
            _initialProperties = _compositor.CreatePropertySet();
            _initialProperties.InsertScalar("OffsetY", 0.0f);
            _initialProperties.InsertScalar("WindowWidth", (float)Window.Current.Bounds.Width);

            // Create an ExpressionAnimation that performs single item parallax
            // based on the ScrollViewer position and the item's Offset.X
            _itemParallax = _compositor.CreateExpressionAnimation();
            _itemParallax.SetScalarParameter("ParallaxValue", 0.2f);
            _itemParallax.Expression = "(ScrollManipulation.Translation.X * ParallaxValue) - "
                                      + "ScrollManipulation.Translation.X";

            // Create an ExpressionAnimation that performs per item parallax of ListViewItems
            // based on the ScrollViewer position and the item's Offset.Y
            _perItemParallax = _compositor.CreateExpressionAnimation();
            _perItemParallax.SetScalarParameter("StartOffset", 0.0f);
            _perItemParallax.SetScalarParameter("ParallaxValue", 0.3f);
            _perItemParallax.SetScalarParameter("ItemHeight", 0.0f);
            _perItemParallax.SetReferenceParameter("InitialProperties", _initialProperties);
            _perItemParallax.Expression = "(InitialProperties.OffsetY + StartOffset - (0.5 * ItemHeight)) * ParallaxValue - "
                                         +"(InitialProperties.OffsetY + StartOffset - (0.5 * ItemHeight))";

            // Create an ExpressionAnimation that performs a fade animation
            // based on the ScrollViewer position and the Width of the current Window
            _artistImageFadeOut = _compositor.CreateExpressionAnimation();
            _artistImageFadeOut.SetReferenceParameter("FadeOutDistance", _initialProperties);
            _artistImageFadeOut.Expression = "1 + ScrollManipulation.Translation.X / FadeOutDistance.WindowWidth";

            // Create an ExpressionAnimation that performs a fade animation
            // based on the ScrollViewer position and the Height of Artist Name
            _artistNameFadeOut = _compositor.CreateExpressionAnimation();
            _artistNameFadeOut.SetScalarParameter("Height", 1.0f);
            _artistNameFadeOut.Expression = "1 + ScrollManipulation.Translation.Y / Height";

            // Create an ExpressionAnimation that performs a scale down animation
            // based on the ScrollViewer position and the Height of Artist Name
            _artistNameShrink = _compositor.CreateExpressionAnimation();
            _artistNameShrink.SetScalarParameter("Height", 1.0f);
            _artistNameShrink.Expression = "Vector3("
                                            + "1 + ScrollManipulation.Translation.Y / Height,"
                                            + "1 + ScrollManipulation.Translation.Y / Height,"
                                            + "0)";

            // Create a KeyframeAnimation that performs a basic fade in of the
            // Page once data has been loaded
            _pageFadeIn = _compositor.CreateScalarKeyFrameAnimation();
            _pageFadeIn.InsertKeyFrame(0f, 0f);
            _pageFadeIn.InsertKeyFrame(1f, 1f);
            _pageFadeIn.Duration = TimeSpan.FromMilliseconds(500);
        }

        /// <summary>
        /// Updates the size of the two main Panels of the sample.
        /// </summary>
        /// <param name="sender">The Page object</param>
        /// <param name="e">Contains the new/old size of the Page</param>
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Get the current Window Width;
            var width = Window.Current.Bounds.Width;
            
            // Update the size of the two main panels
            bioPanel.Width = width;
            artistPanel.Width = width;

            // Update the value used in the fade out animation to 
            _initialProperties.InsertScalar("WindowWidth", (float)width);
        }

        /// <summary>
        /// Once the AlbumsList is loaded, a ScrollViewer is located within
        /// the ListView control and the ScrollViewerManipulationPropertSet
        /// is assigned to the appropriate ExpressionAnimations that are
        /// driven by the AlbumList ScrollViewer.
        /// </summary>
        /// <param name="sender">ListView control</param>
        /// <param name="e">RoutedEventArgs</param>
        private void AlbumsList_Loaded(object sender, RoutedEventArgs e)
        {
            // Find the ScrollViewer associated with the ListView
            ScrollViewer scrollViewer = GetScrollViewer(albumsList);

            // Obtain the ScrollViewerManipulationPropertySet that contains the Translastion value of
            // the ScrollViewer
            _scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            // Create an ExpressionAnimation that "animates" or drives the property "OffsetY" in the intial CompositionPropertySet
            var expression = _compositor.CreateExpressionAnimation("ScrollManipulation.Translation.Y");
            expression.SetReferenceParameter("ScrollManipulation", _scrollProperties);

            // Start the animating the OffsetY property, which will drive the animation of the per item parallax expression
            _initialProperties.StartAnimation("OffsetY", expression);
        }

        /// <summary>
        /// Associates the respective ScorllViewerManipulationPropertySets to
        /// the ExpressionAnimations that will drive the parallax effect for
        /// the Artist Name, and Artist Image.
        /// </summary>
        /// <param name="sender">The Page</param>
        /// <param name="e">RoutedEventArgs</param>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Obtain the ScollViewerManipulationPropertySets from the two ScrollViewers since they are now loaded
            CompositionPropertySet bioScrollViewProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(bioScroller);
            CompositionPropertySet musicPivotProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(musicPivot);

            // Obtain the handoff Visual for UIElement to start ExpresssionAnimations
            Visual artistImageVisual = ElementCompositionPreview.GetElementVisual(artistImage);
           
            // Update the referenced ScrollManipulation parameter for the fade out animation on the artist image
            _artistImageFadeOut.SetReferenceParameter("ScrollManipulation", musicPivotProperties);
            
            // Update the referenced ScrollManipulation parameter for the translate animation on the artist image
            _itemParallax.SetReferenceParameter("ScrollManipulation", musicPivotProperties);

            // Start the animations on the Opacity and Offset of the handoff Visual
            artistImageVisual.StartAnimation("Opacity", _artistImageFadeOut);
            artistImageVisual.StartAnimation("Offset.X", _itemParallax);

            // Obtain the handoff Visual for UIElement to start ExpresssionAnimations
            Visual artistNameVisual = ElementCompositionPreview.GetElementVisual(artistName);
            artistNameVisual.CenterPoint = new Vector3(0, 0, 0);

            // Update the parameters used for the fade out ExpressionAnimation on the artist name
            _artistNameFadeOut.SetReferenceParameter("ScrollManipulation", bioScrollViewProperties);
            _artistNameFadeOut.SetScalarParameter("Height", (float)artistName.RenderSize.Height);

            // Update the parameters for the scale ExpressionAnimation on the artist name
            _artistNameShrink.SetReferenceParameter("ScrollManipulation", bioScrollViewProperties);
            _artistNameShrink.SetScalarParameter("Height", (float)artistName.RenderSize.Height);

            // Reusing the _itemParallax animation and just updating the ParallaxValue to be a different value
            _itemParallax.SetScalarParameter("ParallaxValue", 0.5f);

            // Start the animations for Opacity, Scale, and Offset.X on the artist name Visual
            artistNameVisual.StartAnimation("Opacity", _artistNameFadeOut);
            artistNameVisual.StartAnimation("Scale", _artistNameShrink);
            artistNameVisual.StartAnimation("Offset.X", _itemParallax);

        }

        /// <summary>
        /// Used to render each ListView item, obtain the handoff Visual,
        /// and wire up the per item parallax ExpressionAnimations.
        /// </summary>
        /// <param name="sender">The ListView</param>
        /// <param name="args">ContainerContentChangingEventArgs</param>
        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            // Obtain the backing data item
            Album album = args.Item as Album;

            // Find the UIElements in the DataTemplate and update their respective values
            RelativePanel panel = args.ItemContainer.ContentTemplateRoot as RelativePanel;
            // Since we defined the template in markup we know that
            // the Canvas is the first child of the RelativePanel
            // and the Image is the first child of the Canvas so we
            // can directly index to them
            Canvas canvas = panel.Children[0] as Canvas;
            Image image = canvas.Children[0] as Image;
            image.Source = new BitmapImage(new Uri(album.ImageUrl + "&w=960&h=960"));

            // Get the handoff visual of UIElement
            // Initialize values for the Size and Offset
            Visual sprite = ElementCompositionPreview.GetElementVisual(image);
            sprite.Size = new Vector2(960f, 960f);
            sprite.Offset = new Vector3(0f, 0f, 0f);

            // Update the scalar parameter of the Visual starting offset
            _perItemParallax.SetScalarParameter("StartOffset", (float)args.ItemIndex * sprite.Size.Y / 4.0f);
            sprite.StartAnimation("Offset.Y", _perItemParallax);
            
            args.Handled = true;
        }
        
        // Windows.UI.Composition
        private Compositor _compositor;

        // Expression Animations
        private ExpressionAnimation _itemParallax;
        private ExpressionAnimation _perItemParallax;
        private ExpressionAnimation _artistImageFadeOut;
        private ExpressionAnimation _artistNameFadeOut;
        private ExpressionAnimation _artistNameShrink;
   
        // ScrollViewerManipulationPropertySets
        private CompositionPropertySet _scrollProperties;
        private CompositionPropertySet _initialProperties;

        // Data source used
        private IDataSource _dataSource;

        // Default values for the data source
        private static readonly string DEFAULT_ARTIST_ID = "music.51A80000-0200-11DB-89CA-0019B92A3933";
        private ScalarKeyFrameAnimation _pageFadeIn;
    }
}
