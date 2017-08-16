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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MaterialCreator
{
    public sealed partial class ThreeButtonDialog : ContentDialog
    {
        public enum ThreeButtonDialogResult
        {
            FirstButton,
            SecondButton,
            TernaryButton
        }

        public ThreeButtonDialog()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public string Message { get; set; }
        public string FirstButtonText { get; set; }
        public string SecondButtonText { get; set; }
        public string ThirdButtonText { get; set; }

        public ThreeButtonDialogResult Result { get; private set; }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = ThreeButtonDialogResult.FirstButton;
            Hide();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = ThreeButtonDialogResult.SecondButton;
            Hide();
        }

        private void TernaryButton_Click(object sender, RoutedEventArgs e)
        {
            Result = ThreeButtonDialogResult.TernaryButton;
            Hide();
        }
    }
}
