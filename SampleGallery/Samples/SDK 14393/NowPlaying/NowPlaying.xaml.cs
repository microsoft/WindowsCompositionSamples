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

using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using SamplesCommon;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class NowPlaying : SamplePage
    {
        private Compositor          _compositor;
        private SpriteVisual        _textSprite;

        public NowPlaying()
        {
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Now Playing"; 
        public override string      SampleName => StaticSampleName; 
        public static string        StaticSampleDescription => "Demonstrates the use of image lighting with BackdropBrush with a HardLight blend to create an interesting dynamic visual effect."; 
        public override string      SampleDescription => StaticSampleDescription;
        public override string      SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=869002";

        private void Grid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {           
            // Get the current compositor
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Set the artist image
            ArtistImage.Source = new Uri("ms-appx:///Assets/Landscapes/Landscape-7.jpg");

            // Disable the placeholder image
            ArtistImage.PlaceholderDelay = TimeSpan.MinValue;

            // Bounds of the window, used for positioning lights
            Vector2 sizeWindowBounds = new Vector2((float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);

            // Setup the image and lighting effect
            CreateImageAndLights(sizeWindowBounds);

            // Setup text with the hard light blending over the drawn content
            CreateTextAndBlendEffect(sizeWindowBounds);
        }

        private void CreateTextAndBlendEffect(Vector2 sizeLightBounds)
        {
            //
            // Crete the effect graph, doing a hard light blend of the text over the 
            // content already drawn into the backbuffer
            //

            IGraphicsEffect graphicsEffect = new BlendEffect()
            {
                Mode = BlendEffectMode.HardLight,
                Foreground = new CompositionEffectSourceParameter("Text"),
                Background = new CompositionEffectSourceParameter("Destination"),
            };

            CompositionEffectFactory effectFactory = _compositor.CreateEffectFactory(graphicsEffect, null);
            CompositionEffectBrush brush = effectFactory.CreateBrush();

            // Bind the destination brush
            brush.SetSourceParameter("Destination", _compositor.CreateBackdropBrush());


            //
            // Create the text surface which we'll scroll over the image with the lighting effect
            //

            // Pick a nice size font depending on target size
            const float maxFontSize = 72;
            const float scaleFactor = 12;
            float fontSize = Math.Min(sizeLightBounds.X / scaleFactor, maxFontSize);

            // Create the text format description, then the surface
            CanvasTextFormat textFormat = new CanvasTextFormat
                    {
                        FontFamily = "Segoe UI",
                        FontSize = fontSize,
                        FontWeight = FontWeights.Bold,
                        WordWrapping = CanvasWordWrapping.WholeWord,
                        HorizontalAlignment = CanvasHorizontalAlignment.Center,
                        VerticalAlignment = CanvasVerticalAlignment.Center
                    };

            string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec efficitur, eros sit amet laoreet scelerisque, " +
                          "nunc odio ultricies metus, ut consectetur nulla massa eu nibh.Phasellus in lorem id nunc euismod tempus.Phasellus et nulla non turpis tempor blandit ut eget turpis." +
                          "Phasellus ac ornare elit, ut scelerisque dolor. Nam vel finibus lorem. Aenean malesuada pulvinar eros id ornare. Fusce blandit ante eget dolor efficitur suscipit." +
                          "Phasellus ac lacus nibh. Aenean eget blandit risus, in lacinia mi. Proin fermentum ante eros, non sollicitudin mi pretium eu. Curabitur suscipit lectus arcu, eget" +
                          "pretium quam sagittis non. Mauris purus mauris, condimentum nec laoreet sit amet, imperdiet sit amet nisi. Sed interdum, urna et aliquam porta, elit velit tincidunt orci," +
                          "vitae vestibulum risus lacus at nulla.Phasellus sapien ipsum, pellentesque varius enim nec, iaculis aliquet enim. Nulla id dapibus ante. Sed hendrerit sagittis leo, commodo" +
                          "fringilla ligula rutrum ut. Nullam sodales, ex ut pellentesque scelerisque, sapien nulla mattis lectus, vel ullamcorper leo enim ac mi.Sed consectetur vitae velit in consequat." +
                          "Pellentesque ac condimentum justo, at feugiat nulla. Sed ut congue neque. Nam gravida quam ac urna porttitor, ut bibendum ante mattis.Cras viverra cursus sapien, et sollicitudin" +
                          "risus fringilla eget. Nulla facilisi. Duis pellentesque scelerisque nisi, facilisis malesuada massa gravida et. Vestibulum ac leo sed orci tincidunt feugiat.Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Nunc id leo vestibulum, vulputate ipsum sit amet, scelerisque velit. Curabitur imperdiet justo et tortor dignissim, sit amet volutpat sem ullamcorper. Nam mollis ullamcorper tellus vitae convallis. Aliquam eleifend elit nec tincidunt pharetra. Aliquam turpis eros, mollis et nunc quis, porta molestie justo. Etiam ultrices sem non turpis imperdiet dictum.Aliquam molestie elit in urna sodales, nec luctus dui laoreet.Curabitur molestie risus vel ligula efficitur, non fringilla urna iaculis.Curabitur neque tortor, facilisis quis dictum facilisis, facilisis et ante. Sed nisl erat, semper vitae efficitur ut, congue vitae quam. Ut auctor lacus sit amet varius placerat.Sed ac tellus tempus, ultricies est quis, tempor felis.Nulla vel faucibus elit, eu tincidunt eros. Nulla blandit id nisl ut porta. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Etiam suscipit tellus a mattis pulvinar. Sed et libero vel ligula elementum suscipit.Ut elementum libero at sagittis pharetra. Fusce ultrices odio sapien, a posuere est consectetur ut.";

            // Make the surface twice the height to give us room to scroll
            Vector2 surfaceSize = new Vector2(sizeLightBounds.X, 2f * sizeLightBounds.Y);
            ManagedSurface textSurface = ImageLoader.Instance.LoadText(text, surfaceSize.ToSize(),
                                                                           textFormat, Colors.White, Colors.Transparent);
            brush.SetSourceParameter("Text", textSurface.Brush);

            // Create the sprite and parent it to the panel with the clip
            _textSprite = _compositor.CreateSpriteVisual();
            _textSprite.Size = surfaceSize;
            _textSprite.Brush = brush;

            ElementCompositionPreview.SetElementChildVisual(MyPanel, _textSprite);

            // Lastly, setup the slow scrolling animation of the text
            LinearEasingFunction linear = _compositor.CreateLinearEasingFunction();
            Vector3KeyFrameAnimation offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0f, new Vector3(0, 0, 0), linear);
            offsetAnimation.InsertKeyFrame(1f, new Vector3(0, -_textSprite.Size.Y * .5f, 0), linear);
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(30000);
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            _textSprite.StartAnimation("Offset", offsetAnimation);
        }

        private void CreateImageAndLights(Vector2 sizeLightBounds)
        {
            //
            // Image and effect setup
            //

            // Create the effect graph.  We will combine the desaturated image with two diffuse lights
            IGraphicsEffect graphicsEffect = new CompositeEffect()
            {
                Mode = Microsoft.Graphics.Canvas.CanvasComposite.Add,
                Sources =
                {
                    new SaturationEffect()
                    {
                        Saturation = 0,
                        Source = new CompositionEffectSourceParameter("ImageSource")
                    },

                    new PointDiffuseEffect()
                    {
                        Name = "Light1",
                        DiffuseAmount = 1f,
                    },

                    new PointDiffuseEffect()
                    {
                        Name = "Light2",
                        DiffuseAmount = 1f,
                    },
                }
            };

            // Create the effect factory, we're going to animate the light positions and colors
            CompositionEffectFactory effectFactory = _compositor.CreateEffectFactory(graphicsEffect, 
                                new[] { "Light1.LightPosition", "Light1.LightColor",
                                        "Light2.LightPosition", "Light2.LightColor", });

            // Create the effect brush and bind the normal map
            CompositionEffectBrush brush = effectFactory.CreateBrush();         

            // Update the CompositionImage to use the custom effect brush
            ArtistImage.Brush = brush;


            //
            //  Animation setup
            //
            
            // Setup the first light's position, top and to the left in general
            LinearEasingFunction linear = _compositor.CreateLinearEasingFunction();
            Vector3KeyFrameAnimation positionAnimation = _compositor.CreateVector3KeyFrameAnimation();
            positionAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 300f), linear);
            positionAnimation.InsertKeyFrame(.33f, new Vector3(sizeLightBounds.X * .5f, sizeLightBounds.Y * .5f, 100f), linear);
            positionAnimation.InsertKeyFrame(.66f, new Vector3(sizeLightBounds.X * .25f, sizeLightBounds.Y * .95f, 100f), linear);
            positionAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 300f), linear);
            positionAnimation.Duration = TimeSpan.FromMilliseconds(20000);
            positionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            brush.StartAnimation("Light1.LightPosition", positionAnimation);


            // Setup the first light's color animation, cycling through some brighter tones
            ColorKeyFrameAnimation colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, Colors.Orange);
            colorAnimation.InsertKeyFrame(.2f, Colors.Orange);
            colorAnimation.InsertKeyFrame(.3f, Colors.Red);
            colorAnimation.InsertKeyFrame(.5f, Colors.Red);
            colorAnimation.InsertKeyFrame(.6f, Colors.Yellow);
            colorAnimation.InsertKeyFrame(.8f, Colors.Yellow);
            colorAnimation.InsertKeyFrame(.9f, Colors.Orange);
            colorAnimation.InsertKeyFrame(1f, Colors.Orange);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
            colorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            brush.StartAnimation("Light1.LightColor", colorAnimation);


            // Setup the second light's position, down and to the right in general
            positionAnimation = _compositor.CreateVector3KeyFrameAnimation();
            positionAnimation.InsertKeyFrame(0f, new Vector3(sizeLightBounds.X, sizeLightBounds.Y, 200f), linear);
            positionAnimation.InsertKeyFrame(.33f, new Vector3(sizeLightBounds.X * .7f, sizeLightBounds.Y * .9f, 300f), linear);
            positionAnimation.InsertKeyFrame(.66f, new Vector3(sizeLightBounds.X * .95f, sizeLightBounds.Y * .95f, 100f), linear);
            positionAnimation.InsertKeyFrame(1f, new Vector3(sizeLightBounds.X, sizeLightBounds.Y, 200f), linear);
            positionAnimation.Duration = TimeSpan.FromMilliseconds(20000);
            positionAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            brush.StartAnimation("Light2.LightPosition", positionAnimation);
            
            // Setup the second light's color animation, cycling through some darker tones
            colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, Colors.Blue);
            colorAnimation.InsertKeyFrame(.2f, Colors.Blue);
            colorAnimation.InsertKeyFrame(.3f, Colors.DarkGreen);
            colorAnimation.InsertKeyFrame(.5f, Colors.DarkGreen);
            colorAnimation.InsertKeyFrame(.6f, Colors.DarkBlue);
            colorAnimation.InsertKeyFrame(.8f, Colors.DarkBlue);
            colorAnimation.InsertKeyFrame(.9f, Colors.Blue);
            colorAnimation.InsertKeyFrame(1f, Colors.Blue);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(20000);
            colorAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            brush.StartAnimation("Light2.LightColor", colorAnimation);
        }

        private void MyPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);

            // Resize the text layer if available
            if (_textSprite != null)
            {
                _textSprite.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height * 2f);
            }
        }
    }
}
