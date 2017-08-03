//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System;
using System.Diagnostics;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace MaterialCreator
{
    public delegate void VectorChangedEventHandler(object sender, object args);

    public sealed partial class VectorControl : UserControl
    {
        Vector4 _vector;
        int _components;

        public VectorControl(int components)
        {
            this.InitializeComponent();
            _components = components;

            switch (components)
            {
                case 2:
                    x.Visibility = Visibility.Visible;
                    XColumn.Width = new GridLength(1, GridUnitType.Star);
                    YColumn.Width = new GridLength(1, GridUnitType.Star);
                    ZColumn.Width = new GridLength(0);
                    WColumn.Width = new GridLength(0);
                    break;
                case 3:
                    XColumn.Width = new GridLength(1, GridUnitType.Star);
                    YColumn.Width = new GridLength(1, GridUnitType.Star);
                    ZColumn.Width = new GridLength(1, GridUnitType.Star);
                    WColumn.Width = new GridLength(0);
                    break;
                case 4:
                    XColumn.Width = new GridLength(1, GridUnitType.Star);
                    YColumn.Width = new GridLength(1, GridUnitType.Star);
                    ZColumn.Width = new GridLength(1, GridUnitType.Star);
                    WColumn.Width = new GridLength(1, GridUnitType.Star);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            UpdateUI();
        }

        public event VectorChangedEventHandler VectorChanged;

        public int Components
        {
            get { return _components; }
        }
        public Vector2 Vector2
        {
            get { return new Vector2(_vector.X, _vector.Y); }
            set
            {
                _vector = new Vector4(value.X, value.Y, 0, 0);
                UpdateUI();
            }
        }

        public Vector3 Vector3
        {
            get { return new Vector3(_vector.X, _vector.Y, _vector.Z); }
            set
            {
                _vector = new Vector4(value.X, value.Y, value.Z, 0);
                UpdateUI();
            }
        }

        public Vector4 Vector4
        {
            get { return _vector; }
            set
            {
                _vector = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            x.Text = _vector.X.ToString("0.00");
            y.Text = _vector.Y.ToString("0.00");
            z.Text = _vector.Z.ToString("0.00");
            w.Text = _vector.W.ToString("0.00");
        }

        private void Vector_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _vector.X = Convert.ToSingle(x.Text);
                _vector.Y = Convert.ToSingle(y.Text);
                _vector.Z = Convert.ToSingle(z.Text);
                _vector.W = Convert.ToSingle(w.Text);

                if (VectorChanged != null)
                {
                    VectorChanged(this, null);
                }
            }
            catch
            {
                // Error parsing
            }
        }
    }
}
