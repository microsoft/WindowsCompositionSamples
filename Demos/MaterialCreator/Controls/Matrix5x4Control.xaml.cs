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

using Microsoft.Graphics.Canvas.Effects;
using System;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{
    public delegate void MatrixChangedEventHandler(object sender, object args);

    public sealed partial class Matrix5x4Control : UserControl
    {
        private Matrix5x4 _mat;
        public event MatrixChangedEventHandler MatrixChanged;

        public Matrix5x4Control()
        {
            this.InitializeComponent();

            UpdateUI();
        }

        public Matrix5x4 Matrix
        {
            get { return _mat; }
            set
            {
                _mat = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            m11.Text = _mat.M11.ToString("0.00");
            m12.Text = _mat.M12.ToString("0.00");
            m13.Text = _mat.M13.ToString("0.00");
            m14.Text = _mat.M14.ToString("0.00");

            m21.Text = _mat.M21.ToString("0.00");
            m22.Text = _mat.M22.ToString("0.00");
            m23.Text = _mat.M23.ToString("0.00");
            m24.Text = _mat.M24.ToString("0.00");

            m31.Text = _mat.M31.ToString("0.00");
            m32.Text = _mat.M32.ToString("0.00");
            m33.Text = _mat.M33.ToString("0.00");
            m34.Text = _mat.M34.ToString("0.00");

            m41.Text = _mat.M41.ToString("0.00");
            m42.Text = _mat.M42.ToString("0.00");
            m43.Text = _mat.M43.ToString("0.00");
            m44.Text = _mat.M44.ToString("0.00");

            m51.Text = _mat.M51.ToString("0.00");
            m52.Text = _mat.M52.ToString("0.00");
            m53.Text = _mat.M53.ToString("0.00");
            m54.Text = _mat.M54.ToString("0.00");
        }

        private void Matrix_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _mat.M11 = Convert.ToSingle(m11.Text);
                _mat.M12 = Convert.ToSingle(m12.Text);
                _mat.M13 = Convert.ToSingle(m13.Text);
                _mat.M14 = Convert.ToSingle(m14.Text);

                _mat.M21 = Convert.ToSingle(m21.Text);
                _mat.M22 = Convert.ToSingle(m22.Text);
                _mat.M23 = Convert.ToSingle(m23.Text);
                _mat.M24 = Convert.ToSingle(m24.Text);

                _mat.M31 = Convert.ToSingle(m31.Text);
                _mat.M32 = Convert.ToSingle(m32.Text);
                _mat.M33 = Convert.ToSingle(m33.Text);
                _mat.M34 = Convert.ToSingle(m34.Text);

                _mat.M41 = Convert.ToSingle(m41.Text);
                _mat.M42 = Convert.ToSingle(m42.Text);
                _mat.M43 = Convert.ToSingle(m43.Text);
                _mat.M44 = Convert.ToSingle(m44.Text);

                _mat.M51 = Convert.ToSingle(m51.Text);
                _mat.M52 = Convert.ToSingle(m52.Text);
                _mat.M53 = Convert.ToSingle(m53.Text);
                _mat.M54 = Convert.ToSingle(m54.Text);
                
                if (MatrixChanged != null)
                {
                    MatrixChanged(this, null);
                }
            }
            catch
            {
                // Error parsing
            }
        }
    }
}
