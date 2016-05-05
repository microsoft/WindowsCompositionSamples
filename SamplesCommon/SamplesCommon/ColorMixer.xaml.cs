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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SamplesCommon
{
    public delegate void ColorChangedEventHandler(object sender, ColorEventArgs args);

    public class ColorEventArgs : EventArgs
    {
        internal ColorEventArgs(Color newColor)
        {
            NewColor = newColor;
        }

        public Color NewColor { get; private set; }
    }

    public sealed partial class ColorMixer : UserControl
    {
        /// <summary>
        /// Raised when the color changes.
        /// </summary>
        public event ColorChangedEventHandler ColorChanged;

        #region DependencyProperties
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorMixer), new PropertyMetadata(Colors.Black, OnColorChanged));

        public static readonly DependencyProperty RedProperty =
            DependencyProperty.Register("Red", typeof(int), typeof(ColorMixer), new PropertyMetadata(0, OnRedChanged));

        public static readonly DependencyProperty GreenProperty =
            DependencyProperty.Register("Green", typeof(int), typeof(ColorMixer), new PropertyMetadata(0, OnGreenChanged));

        public static readonly DependencyProperty BlueProperty =
            DependencyProperty.Register("Blue", typeof(int), typeof(ColorMixer), new PropertyMetadata(0, OnBlueChanged));
        #endregion  DependencyProperties

        public ColorMixer()
        {
            InitializeComponent();
        }

        public int Red
        {
            get { return (int)GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }

        public int Green
        {
            get { return (int)GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }

        public int Blue
        {
            get { return (int)GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        static void OnRedChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var self = (ColorMixer)d;
            var color = self.Color;
            color.R = ClampIntToByte((int)args.NewValue);
            self.Color = color;
        }

        static void OnGreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var self = (ColorMixer)d;
            var color = self.Color;
            color.G = ClampIntToByte((int)args.NewValue);
            self.Color = color;
        }

        static void OnBlueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var self = (ColorMixer)d;
            var color = self.Color;
            color.B = ClampIntToByte((int)args.NewValue);
            self.Color = color;
        }

        static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((ColorMixer)d).OnColorChanged((Color)args.NewValue);
        }

        void OnColorChanged(Color newColor)
        {
            // Force colors to be opaque. This is to work around a bug
            // where the Color gets initialized to a transparent color.
            newColor.A = 255;

            // Update the RGB values
            Red = newColor.R;
            Green = newColor.G;
            Blue = newColor.B;

            ((SolidColorBrush)Swatch.Background).Color = newColor;

            ColorChanged?.Invoke(this, new ColorEventArgs(Color));

            if (newColor != Color)
            {
                Color = newColor;
            }
        }

        static byte ClampIntToByte(int value)
        {
            return (byte)((value < 0)
                ? 0
                : ((value > 255) ? 255 : value));
        }

    }

}
