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
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MaterialCreator
{
    public sealed partial class EffectControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Effect _effect;

        public EffectControl()
        {
            this.InitializeComponent();
            
            this.DataContextChanged += OnDataContextChanged;
        }

        public Effect Effect
        {
            get { return _effect; }
            set
            {
                if (_effect != value)
                {
                    _effect = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Effect)));
                }
            }
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (e.NewValue is Effect newEffect)
            {
                if (newEffect != null && Effect != newEffect)
                {
                    Effect = newEffect;
                    Eyeball.Opacity = Effect.Enabled ? 1 : .2f;

                    if (!Effect.FirstLoadCompleted)
                    {
                        Effect.FirstLoadCompleted = true;
                        ShowEffectPropertyFlyout();
                    }
                }
            }
        }

        private void EffectVisible_Clicked(object sender, RoutedEventArgs e)
        {
            if (Effect.Enabled)
            {
                Effect.Enabled = false;
                Eyeball.Opacity = .2f;
            }
            else
            {
                Effect.Enabled = true;
                Eyeball.Opacity = 1;
            }
        }

        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button ancestor = VisualTreeHelperExtensions.GetFirstAncestorOfType<Button>((DependencyObject)e.OriginalSource);

            if (ancestor == null)
            {
                ShowEffectPropertyFlyout();
            }
        }

        public void ShowEffectPropertyFlyout()
        {
            EditEffectControl editControl = new EditEffectControl();
            editControl.Initialize(Effect);

            Flyout fly = new Flyout();
            fly.Content = editControl;
            fly.Opened += Fly_Opened;
            fly.Placement = FlyoutPlacementMode.Left;

            fly.ShowAt(this);
        }
        private void Fly_Opened(object sender, object e)
        {
            Flyout fly = (Flyout)sender;
            EditEffectControl control = (EditEffectControl)fly.Content;
            control.Width = ActualWidth - 2;
        }

        private void DeleteEffect_Click(object sender, RoutedEventArgs e)
        {
            Effect.Layer.Effects.Remove(Effect);
            _effect.Layer.InvalidateLayer("Effect");
        }

        private void EffectProperties_Click(object sender, RoutedEventArgs e)
        {
            ShowEffectPropertyFlyout();
        }

        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            EffectButtons.Visibility = Visibility.Visible;
            RootGrid.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemListLowColor"]);
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            EffectButtons.Visibility = Visibility.Collapsed;
            RootGrid.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}