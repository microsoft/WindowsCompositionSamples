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

using CompositionSampleGallery;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;

namespace CompositionSampleGallery
{
    public sealed partial class MainPage : Page
    {
        private ManagedSurface                  _splashSurface;
        private static CompositionCapabilities  _capabilities;

        private static bool                     _areEffectsSupported = true;
        private static bool                     _areEffectsFast = true;
        private static RuntimeSupportedSDKs     _runtimeCapabilities;
        private MainNavigationViewModel         _mainNavigation;        

        public MainPage()
        {
            _runtimeCapabilities = new RuntimeSupportedSDKs();

            // Get hardware capabilities and register changed event listener only when targeting the 
            // appropriate SDK version and the runtime supports this version
            if (_runtimeCapabilities.IsSdkVersionRuntimeSupported(RuntimeSupportedSDKs.SDKVERSION._15063))
            {
                _capabilities = new CompositionCapabilities();
                _capabilities.Changed += HandleCapabilitiesChangedAsync;
                _areEffectsSupported = _capabilities.AreEffectsSupported();
                _areEffectsFast = _capabilities.AreEffectsFast();
            }
            else
            {
                _areEffectsSupported = true;
                _areEffectsFast = true;
            }
            this.InitializeComponent();
            _mainNavigation = new MainNavigationViewModel(GalleryUI);

            // Initialize the image loader
            ImageLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);

            // Show the custome splash screen
            ShowCustomSplashScreen(MainWindow.CurrentWindow.Bounds);
        }

        public MainNavigationViewModel MainNavigation => _mainNavigation;

        public static bool AreEffectsSupported
        {
            get { return _areEffectsSupported; }
        }

        public static bool AreEffectsFast
        {
            get { return _areEffectsFast; }
        }

        public static RuntimeSupportedSDKs RuntimeCapabilities
        {
            get { return _runtimeCapabilities; }
        }

        private async void HandleCapabilitiesChangedAsync(CompositionCapabilities sender, object args)
        {
            _areEffectsSupported = _capabilities.AreEffectsSupported();
            _areEffectsFast = _capabilities.AreEffectsFast();

            GalleryUI.NotifyCompositionCapabilitiesChanged(_areEffectsSupported, _areEffectsFast);

            SampleDefinitions.RefreshSampleList();


            //
            // Let the user know that the display config has changed and some samples may or may
            // not be available
            //

            if (!_areEffectsSupported || !_areEffectsFast)
            {
                string message;

                if (!_areEffectsSupported)
                {
                    message = "Your display configuration may have changed.  Your current graphics hardware does not support effects.  Some samples will not be available";
                }
                else
                {
                    message = "Your display configuration may have changed. Your current graphics hardware does not support advanced effects.  Some samples will not be available";
                }

                var messageDialog = new MessageDialog(message);
                messageDialog.Commands.Add(new UICommand("Close"));

                // Show the message dialog
                await messageDialog.ShowAsync();
            }
        }

        private void ShowCustomSplashScreen(Rect imageBounds)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            Vector2 windowSize = new Vector2((float)MainWindow.CurrentWindow.Bounds.Width, (float)MainWindow.CurrentWindow.Bounds.Height);


            //
            // Create a container visual to hold the color fill background and image visuals.
            // Configure this visual to scale from the center.
            //

            ContainerVisual container = compositor.CreateContainerVisual();
            container.Size = windowSize;
            container.CenterPoint = new Vector3(windowSize.X, windowSize.Y, 0) * .5f;
            ElementCompositionPreview.SetElementChildVisual(this, container);


            //
            // Create the colorfill sprite for the background, set the color to the same as app theme
            //

            SpriteVisual backgroundSprite = compositor.CreateSpriteVisual();
            backgroundSprite.Size = windowSize;
            backgroundSprite.Brush = compositor.CreateColorBrush(Color.FromArgb(255, 0, 188, 242));
            container.Children.InsertAtBottom(backgroundSprite);


            //
            // Create the image sprite containing the splash screen image.  Size and position this to
            // exactly cover the Splash screen image so it will be a seamless transition between the two
            //

            _splashSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/StoreAssets/Wide.png"));
            SpriteVisual imageSprite = compositor.CreateSpriteVisual();
            imageSprite.Brush = compositor.CreateSurfaceBrush(_splashSurface.Surface);
            imageSprite.Offset = new Vector3((float)imageBounds.X,(float)imageBounds.Y, 0f);
            imageSprite.Size = new Vector2((float)imageBounds.Width, (float)imageBounds.Height);
            container.Children.InsertAtTop(imageSprite);
        }

        private void HideCustomSplashScreen()
        {
            ContainerVisual container = (ContainerVisual)ElementCompositionPreview.GetElementChildVisual(this);
            Compositor compositor = container.Compositor;

            // Setup some constants for scaling and animating
            const float ScaleFactor = 20f;
            TimeSpan duration = TimeSpan.FromMilliseconds(1200);
            LinearEasingFunction linearEase = compositor.CreateLinearEasingFunction();
            CubicBezierEasingFunction easeInOut = compositor.CreateCubicBezierEasingFunction(new Vector2(.38f, 0f), new Vector2(.45f, 1f));

            // Create the fade animation which will target the opacity of the outgoing splash screen
            ScalarKeyFrameAnimation fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(1, 0);
            fadeOutAnimation.Duration = duration;

            // Create the scale up animation for the grid
            Vector2KeyFrameAnimation scaleUpGridAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpGridAnimation.InsertKeyFrame(0.1f, new Vector2(1 / ScaleFactor, 1 / ScaleFactor));
            scaleUpGridAnimation.InsertKeyFrame(1, new Vector2(1, 1));
            scaleUpGridAnimation.Duration = duration;

            // Create the scale up animation for the Splash screen visuals
            Vector2KeyFrameAnimation scaleUpSplashAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpSplashAnimation.InsertKeyFrame(0, new Vector2(1, 1));
            scaleUpSplashAnimation.InsertKeyFrame(1, new Vector2(ScaleFactor, ScaleFactor));
            scaleUpSplashAnimation.Duration = duration;

            // Configure the grid visual to scale from the center
            Visual gridVisual = ElementCompositionPreview.GetElementVisual(GalleryUI);
            gridVisual.Size = new Vector2((float)GalleryUI.ActualWidth, (float)GalleryUI.ActualHeight);
            gridVisual.CenterPoint = new Vector3(gridVisual.Size.X, gridVisual.Size.Y, 0) * .5f;


            //
            // Create a scoped batch for the animations.  When the batch completes, we can dispose of the
            // splash screen visuals which will no longer be visible.
            //

            CompositionScopedBatch batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            container.StartAnimation("Opacity", fadeOutAnimation);
            container.StartAnimation("Scale.XY", scaleUpSplashAnimation);
            gridVisual.StartAnimation("Scale.XY", scaleUpGridAnimation);

            batch.Completed += Batch_Completed;
            batch.End();
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            // Now that the animations are complete, dispose of the custom Splash Screen visuals
            ElementCompositionPreview.SetElementChildVisual(this, null);

            if (_splashSurface != null)
            {
                _splashSurface.Dispose();
                _splashSurface = null;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Now that loading is complete, dismiss the custom splash screen
            HideCustomSplashScreen();
        }
    }

    // This class caches and provides information about the supported 
    // Windows.Foundation.UniversalApiContract of the runtime
    public class RuntimeSupportedSDKs
    {
        List<SDKVERSION> _supportedSDKs;

        public enum SDKVERSION
        {
            _10586 = 2,   // November Update (1511)
            _14393,       // Anniversary Update (1607)
            _15063,       // Creators Update (1703)
            _16299,       // Fall Creators Update
            _17134,       // Version 1803
            _17763        // Version 1810
        };

        public RuntimeSupportedSDKs()
        {
            _supportedSDKs = new List<SDKVERSION>();

            // Determine which versions of the SDK are supported on the runtime
            foreach(SDKVERSION v in Enum.GetValues(typeof(SDKVERSION)))
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", (ushort)Convert.ToInt32(v)))
                {
                    _supportedSDKs.Add(v);
                }
            }
        }

        public bool IsSdkVersionRuntimeSupported(SDKVERSION sdkVersion)
        {
            if(_supportedSDKs.Contains(sdkVersion))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<SDKVERSION> AllSupportedSdkVersions
        {
            get
            {
                return _supportedSDKs;
            }
        }
    }

    public class IsPaneOpenToVisibilityConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
              string language)
        {
            bool IsOpen = (bool)value;

            if (IsOpen)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            return null;
        }
    }
}
