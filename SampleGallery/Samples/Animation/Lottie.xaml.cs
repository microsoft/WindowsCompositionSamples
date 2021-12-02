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
using AnimatedVisuals;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using CompositionSampleGallery.Shared;
using Windows.UI;

namespace CompositionSampleGallery
{
    public sealed partial class Lottie : SamplePage
    {
        // Some interesting colors for the LottieLogo Lottie animation.
        private static readonly LottieLogoColors[] s_palette = new[]
        {
            new LottieLogoColors(
                background: Color.FromArgb(0xFF, 0x00, 0xD1, 0xC1),
                foreground: Colors.White,
                accent: Color.FromArgb(0xFF, 0x00, 0x7A, 0x87)),
            new LottieLogoColors(
                background: Colors.DarkOliveGreen,
                foreground: Colors.Red,
                accent: Colors.DarkRed),
            new LottieLogoColors(
                background: Colors.DarkRed,
                foreground: Colors.Red,
                accent: Colors.DarkOliveGreen),
            new LottieLogoColors(
                background: Colors.Blue,
                foreground: Colors.Red,
                accent: Colors.Yellow),
            new LottieLogoColors(
                background: Colors.White,
                foreground: Colors.Gray,
                accent: Colors.DarkGray),
            new LottieLogoColors(
                background: Colors.Black,
                foreground: Colors.Purple,
                accent: Colors.MediumPurple),
        };

        private readonly LottieLogo1 _lottieSource = new LottieLogo1();
        private readonly ContainerVisual _gridVisual;
        private IAnimatedVisual _lottieInstance;
        private int _paletteIndex;

        public Lottie()
        {
            this.InitializeComponent();

            // Insert a ContainerVisual under the Grid. This will be used to scale the size
            // of the Lottie to fit the size of the Grid.
            _gridVisual = CompositionTarget.GetCompositorForCurrentThread().CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(_lottieGrid, _gridVisual);
        }

        public static string StaticSampleName => "Lottie";
        public override string SampleName => StaticSampleName;
        public static string StaticSampleDescription => "Demonstrates the use of a Lottie file that has been converted to Microsoft.UI.Composition code with LottieGen, and binding to colors in the Lottie. Click on the animation to change its colors.";
        public override string SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://aka.ms/lottie";

        public LocalDataSource Model { set; get; }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var c = CompositionTarget.GetCompositorForCurrentThread();

            // Instantiate the Lottie.
            _lottieInstance = _lottieSource.TryCreateAnimatedVisual(c, out _);

            // Hook it into the Grid.
            var rootVisual = _lottieInstance.RootVisual;
            var gridVisualChildren = _gridVisual.Children;
            gridVisualChildren.RemoveAll();
            gridVisualChildren.InsertAtTop(rootVisual);

            // Set its size.
            SizeLottieToFitGrid();

            // Animate it in a loop.
            using (var kfa = c.CreateScalarKeyFrameAnimation())
            using (var easing = c.CreateLinearEasingFunction())
            {
                kfa.Duration = _lottieInstance.Duration;
                kfa.IterationBehavior = AnimationIterationBehavior.Forever;
                kfa.InsertKeyFrame(1, 1, easing);
                rootVisual.Properties.StartAnimation("Progress", kfa);
            }

            // Set its colors.
            SetLottieColors(0);
        }

        private void SetLottieColors(int paletteIndex)
        {
            _paletteIndex = paletteIndex % s_palette.Length;
            var colors = s_palette[_paletteIndex];
            _lottieSource.Color_FFFFFF = colors.Foreground;
            _lottieSource.Color_00D1C1 = colors.Background;
            _lottieSource.Color_007A87 = colors.Accent;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Remove the Lottie instance from the tree and dispose it.
            _gridVisual.Children.RemoveAll();
            _lottieInstance.Dispose();
            _lottieInstance = null;
        }

        // Called when the Grid is resized. The Lottie must be resized
        // to fit the new Grid size.
        private void _lottieGrid_SizeChanged(object sender, SizeChangedEventArgs e) => SizeLottieToFitGrid();

        private void SizeLottieToFitGrid()
        {
            if (_lottieInstance is null)
            {
                // The Lottie instance hasn't been created yet.
                return;
            }

            // The size the Lottie should fit into.
            var targetSize = _lottieGrid.ActualSize;

            // The size of the Lottie with no scaling.
            var lottieNaturalSize = _lottieInstance.Size;

            // Calculate the scale that needs to be applied in order to make the
            // Lottie fit inside the target size.
            var scale = targetSize / lottieNaturalSize;

            // Adjust the scale to make it uniform (same scaling for width and height).
            var smallestScaleDimension = MathF.Min(scale.X, scale.Y);

            // Calculate the size of the Lottie animation after scaling.
            var scaledLottieSize = lottieNaturalSize * smallestScaleDimension;

            // Apply the scale so the Lottie will fit exactly.
            _gridVisual.Scale = new Vector3(smallestScaleDimension, smallestScaleDimension, 0);

            // Center the Lottie within the available space.
            _gridVisual.Offset = new Vector3((targetSize - scaledLottieSize) / 2, 0);
        }

        // When the pointer is pressed on the Lottie, give it new colors.
        private void _lottieGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) =>
            SetLottieColors(_paletteIndex + 1);

        // Describes the colors for the LottieLogo Lottie animation.
        readonly struct LottieLogoColors
        {
            internal LottieLogoColors(Color background, Color foreground, Color accent) =>
                (Background, Foreground, Accent) = (background, foreground, accent);

            public Color Background { get; }

            public Color Foreground { get; }

            public Color Accent { get; }
        }
    }
}
