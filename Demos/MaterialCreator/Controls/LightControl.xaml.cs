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

using SamplesCommon;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MaterialCreator
{
    public sealed partial class LightControl : UserControl
    {
        Light _light;
        MainPage _editor;
        bool _lightFollowEnabled;

        public LightControl(Light light, MainPage editor, bool enableLightFollow = true)
        {
            this.InitializeComponent();
            
            _light = light;
            _editor = editor;
            Eyeball.Opacity = _light.Enabled ? 1 : .2f;

            if (enableLightFollow)
            {
                ToggleLightFollow();
            }
            else
            {
                // We're disabling by default, so dim the button
                UpdateLightFollow();
            }
        }

        public void Dispose()
        {
            _light.Dispose();
            
            // Disable follow 
            if (_lightFollowEnabled)
            {
                ToggleLightFollow();
            }
        }

        private void LightProperties_Click(object sender, RoutedEventArgs e)
        {
            ShowProperties();
        }

        public Light Light
        {
            get
            {
                return _light;
            }
        }

        public void ShowProperties()
        {
            EditLightControl editControl = new EditLightControl(this);

            Flyout fly = new Flyout();
            fly.Content = editControl;
            fly.Opened += Fly_Opened;
            fly.Placement = FlyoutPlacementMode.Left;

            fly.ShowAt(this);
        }

        private void Fly_Opened(object sender, object e)
        {
            Flyout fly = (Flyout)sender;
            EditLightControl control = (EditLightControl)fly.Content;
            control.Width = ActualWidth - 2;
        }

        private void DeleteLight_Click(object sender, RoutedEventArgs e)
        {
            _light.Enabled = false;

            _editor.RemoveLight(this);
        }

        private void LightVisible_Clicked(object sender, RoutedEventArgs e)
        {
            if (_light.Enabled)
            {
                _light.Enabled = false;
                Eyeball.Opacity = .2f;
            }
            else
            {
                _light.Enabled = true;
                Eyeball.Opacity = 1f;
            }
        }

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button ancestor = VisualTreeHelperExtensions.GetFirstAncestorOfType<Button>((DependencyObject)e.OriginalSource);

            if (ancestor == null)
            {
                ShowProperties();
            }
        }

        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            RootGrid.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemListLowColor"]);
            LightButtons.Visibility = Visibility.Visible;
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RootGrid.Background = new SolidColorBrush(Colors.Transparent);
            LightButtons.Visibility = Visibility.Collapsed;
        }
        

        private void Editor_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_light != null && (_light.Type == LightTypes.Point || _light.Type == LightTypes.Spot))
            {
                Vector2 position = _editor.GetLightOffset(e);

                Vector3 offset = (Vector3)_light.GetProperty("Offset");
                _light.SetProperty("Offset", new Vector3(position.X, position.Y, offset.Z));
            }
        }

        private void LightFollow_Click(object sender, RoutedEventArgs e)
        {
            ToggleLightFollow();
        }

        private void ToggleLightFollow()
        {
            if (!_lightFollowEnabled)
            {
                _lightFollowEnabled = true;
                _editor.LightingPanel.PointerMoved += Editor_PointerMoved;
            }
            else
            {
                _lightFollowEnabled = false;
                _editor.LightingPanel.PointerMoved -= Editor_PointerMoved;

                if (_light.Type == LightTypes.Point || _light.Type == LightTypes.Spot)
                {
                    // Reset to value set by sliders
                    _light.SetProperty("Offset", _light.Offset);
                }
            }

            UpdateLightFollow();
        }

        private void UpdateLightFollow()
        {
            if (!_lightFollowEnabled)
            {
                LightFollow.Opacity = .2f;
            }
            else
            {
                LightFollow.Opacity = 1f;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_lightFollowEnabled)
            {
                // Ensure we turn off mouse tracking to remove event handler
                ToggleLightFollow();
            }
        }
    }
}
