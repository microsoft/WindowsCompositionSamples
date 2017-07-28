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
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.Graphics.Effects;
using System.Reflection;
using SamplesCommon;
using Windows.UI.ViewManagement;

namespace MaterialCreator
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Compositor _compositor;
        private SpriteVisual _sprite;
        private ContainerVisual _container;
        private bool _pendingBrushUpdate;
        private Material _material;
        private bool _inDialog;

        public MainPage()
        {
            this.InitializeComponent();

            Color bgColor = Color.FromArgb(255, 204, 204, 204);
            ApplicationView.GetForCurrentView().TitleBar.BackgroundColor = bgColor;
            ApplicationView.GetForCurrentView().TitleBar.InactiveBackgroundColor = bgColor;
            ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = bgColor;
            ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = bgColor;

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            ImageLoader.Initialize(_compositor);
        }
        
        public Material Material
        {
            get { return _material; }
            set
            {
                if (_material != value)
                {
                    if (_material != null)
                    {
                        _material.Dispose();
                    }

                    _material = value;

                    NotifyPropertyChanged(nameof(Material));
                }
            }
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridClip.Rect = new Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height);

            UpdatePreviewPanel();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _container = _compositor.CreateContainerVisual();
            _sprite = _compositor.CreateSpriteVisual();
            _container.Children.InsertAtTop(_sprite);
            ElementCompositionPreview.SetElementChildVisual(PreviewPanel, _container);

            CreateNewMaterial();

            UpdatePreviewPanel();
        }

        public UIElement LightingPanel
        {
            get { return PreviewPanel; }
        }

        public Vector2 GetLightOffset(PointerRoutedEventArgs e)
        {
            if (e == null)
            {
                return new Vector2(_sprite.Offset.X + (_sprite.Size.X / 2), _sprite.Offset.Y + (_sprite.Size.Y / 2));
            }
            else
            {
                Point position = e.GetCurrentPoint(LightingPanel).Position;

                return new Vector2((float)position.X, (float)position.Y);
            }
        }


        private void UpdatePreviewPanel()
        {
            if (_sprite != null)
            {
                string selected = (string)PreviewSizeCombo.SelectedValue;

                float percentInset = 0;
                if (selected == "Inset")
                {
                    percentInset = .2f;
                }

                _sprite.Size = new Vector2(((float)PreviewPanel.ActualWidth * (1 - percentInset)),
                                           ((float)PreviewPanel.ActualHeight * (1 - percentInset)));
                _sprite.Offset = new Vector3((float)PreviewPanel.ActualWidth * percentInset / 2,
                                             (float)PreviewPanel.ActualHeight * percentInset / 2, 0);
                _sprite.CenterPoint = new Vector3(_sprite.Size.X / 2, _sprite.Size.Y / 2, 0);

                ElementCompositionPreview.GetElementVisual(PreviewPanel).TransformMatrix = GetPerspectiveTransform(_sprite.Size);
            }

            PreviewClip.Rect = new Rect(0d, 0d, PreviewPanel.ActualWidth, PreviewPanel.ActualHeight);
        }

        private Matrix4x4 GetPerspectiveTransform(Vector2 size)
        {
            const double perspectiveAmount = 3;
            var perspective = Matrix4x4.Identity;

            // The perspective amount is a value from 0..infinity. Convert it to a value from 0..-1
            perspective.M34 = (float)(-((Math.Log(perspectiveAmount + 1)) / 1000));

            return
                Matrix4x4.CreateTranslation(new Vector3(-size / 2, 0)) *
                perspective *
                Matrix4x4.CreateTranslation(new Vector3(size / 2, 0));

        }

        private void OnBrushChanged(Material sender)
        {
            if (!_pendingBrushUpdate)
            {
                _pendingBrushUpdate = true;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += (s, e) =>
                {
                    _sprite.Brush = Material.Brush;
                    _pendingBrushUpdate = false;
                    timer.Stop();
                };
                timer.Interval = new TimeSpan(0, 0, 0, 0, 8);
                timer.Start();
            }
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            Material.AddLayer(new ColorLayer());
        }

        private async void BackgroundCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = (string)BackgroundCombo.SelectedValue;

            if (selected == "Grey")
            {
                PreviewPanel.Background = new SolidColorBrush(Colors.DarkGray);
            }
            else if (selected == "Dark")
            {
                PreviewPanel.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//Aurora.jpg")),
                    Stretch = Stretch.UniformToFill
                };
            }
            else if (selected == "Colorful")
            {
                PreviewPanel.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//Lake.jpg")),
                    Stretch = Stretch.UniformToFill
                };
            }
            else if (selected == "Light")
            {
                PreviewPanel.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//City.jpg")),
                    Stretch = Stretch.UniformToFill
                };
            }
            else if (selected == "Pattern")
            {
                PreviewPanel.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets//Ferns.jpg")),
                    Stretch = Stretch.UniformToFill
                };
            }
            else
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    bitmapImage.SetSource(stream);

                    PreviewPanel.Background = new ImageBrush()
                    {
                        ImageSource = bitmapImage,
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
        }

        private async void NewMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (!_inDialog)
            {
                _inDialog = true;

                ContentDialog newMaterialDialog = new ContentDialog()
                {
                    Title = "Create new material",
                    Content = "Create a new material and discard your current one?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    IsSecondaryButtonEnabled = true,
                };

                ContentDialogResult result = await newMaterialDialog.ShowAsync();
                switch (result)
                {
                    case ContentDialogResult.Primary:
                        CreateNewMaterial();
                        break;

                    case ContentDialogResult.Secondary:
                        // Cancel
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }

                _inDialog = false;
            }
        }

        private void CreateNewMaterial()
        {
            LightPanel.Children.Clear();
            NoLightText.Visibility = Visibility.Visible;

            // Create the new material
            Material = new Material();
            Material.Initialize(_compositor, OnBrushChanged);

            _sprite.Brush = Material.Brush;
        }

        private void AddLight_Click(object sender, RoutedEventArgs e)
        {
            Light light = new Light(_compositor, _container);
            light.Offset = new Vector3((float)PreviewPanel.ActualWidth / 2, (float)PreviewPanel.ActualHeight / 2, 100);

            Material.Lights.Add(light);

            LightControl control = new LightControl(light, this);
            LightPanel.Children.Add(control);

            NoLightText.Visibility = Visibility.Collapsed;

            control.ShowProperties();
        }

        public void RemoveLight(LightControl light)
        {
            // Dipose the light to remove the targets
            light.Dispose();

            Material.Lights.Remove(light.Light);

            LightPanel.Children.Remove(light);

            if (LightPanel.Children.Count == 0)
            {
                NoLightText.Visibility = Visibility.Visible;
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PreviewSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreviewPanel();
        }

        private void AnimateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_compositor == null)
            {
                return;
            }

            string selected = (string)AnimateCombo.SelectedValue;

            if (selected == "None")
            {
                _sprite.StopAnimation("RotationAngleInDegrees");
                _sprite.RotationAngleInDegrees = 0;
            }
            else if (selected == "X Rotation")
            {
                // Rotate the preview surface around the X axis
                var animation = _compositor.CreateScalarKeyFrameAnimation();
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                animation.InsertKeyFrame(0, 0);
                animation.InsertKeyFrame(.25f, 90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(.75f, -90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(1f, 0, _compositor.CreateLinearEasingFunction());
                animation.Duration = TimeSpan.FromSeconds(20);

                _sprite.RotationAxis = new Vector3(1, 0, 0);
                _sprite.StartAnimation("RotationAngleInDegrees", animation);
            }
            else if (selected == "Y Rotation")
            {
                // Rotate the preview surface around the Y axis
                var animation = _compositor.CreateScalarKeyFrameAnimation();
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                animation.InsertKeyFrame(0, 0);
                animation.InsertKeyFrame(.25f, 90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(.75f, -90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(1f, 0, _compositor.CreateLinearEasingFunction());
                animation.Duration = TimeSpan.FromSeconds(20);

                _sprite.RotationAxis = new Vector3(0, 1, 0);
                _sprite.StartAnimation("RotationAngleInDegrees", animation);
            }
            else if (selected == "Z Rotation")
            {
                // Rotate the preview surface around the Z axis
                var animation = _compositor.CreateScalarKeyFrameAnimation();
                animation.IterationBehavior = AnimationIterationBehavior.Forever;
                animation.InsertKeyFrame(0, 0);
                animation.InsertKeyFrame(.25f, 90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(.75f, -90, _compositor.CreateLinearEasingFunction());
                animation.InsertKeyFrame(1f, 0, _compositor.CreateLinearEasingFunction());
                animation.Duration = TimeSpan.FromSeconds(20);

                _sprite.RotationAxis = new Vector3(0, 0, 1);
                _sprite.StartAnimation("RotationAngleInDegrees", animation);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public static string Indent(int count)
        {
            return "".PadLeft(count);
        }

        private void BrushToString(StringBuilder sb, int indent, CompositionBrush brush)
        {
            const int indentIncrement = 4;
            Type type = brush.GetType();

            sb.AppendFormat("{0}{1}\r\n{0}{{\r\n", Indent(indent), type.Name);
            indent += indentIncrement;

            foreach (PropertyInfo info in brush.GetType().GetProperties())
            {
                string propertyName = info.Name.ToLower();
                if (propertyName == "implicitanimations" ||
                    propertyName == "compositor" ||
                    propertyName == "properties" ||
                    propertyName == "comment" ||
                    propertyName == "surface" ||
                    propertyName == "source" ||
                    propertyName == "stretch" ||
                    propertyName == "transformmatrix" ||
                    propertyName == "rotationangle" ||
                    propertyName == "centerpoint" ||
                    propertyName == "bitmapinterpolationmode" ||
                    propertyName == "comment" ||
                    propertyName == "dispatcher" ||
                    propertyName == "dispatcherqueue")
                {
                    continue;
                }

                object obj = info.GetValue(brush);
                sb.AppendFormat("{0}{1} : {2}\r\n", Indent(indent), info.Name, Helpers.ToString(obj));
            }

            indent -= indentIncrement;
            sb.AppendFormat("{0}}}\r\n", Indent(indent), type.Name);
        }

        private void EffectToString(StringBuilder sb, int indent, IGraphicsEffect effect, CompositionEffectBrush brush)
        {
            const int indentIncrement = 4;
            Type type = effect.GetType();
            sb.AppendFormat("{0}{1}\r\n{0}{{\r\n", Indent(indent), type.Name);

            Dictionary<string, object> expandedProperties = new Dictionary<string, object>();

            indent += indentIncrement;
            foreach (PropertyInfo info in effect.GetType().GetProperties())
            {
                string propertyName = info.Name.ToLower();
                if (propertyName == "cacheoutput" ||
                    propertyName == "bufferprecision" ||
                    propertyName == "colorhdr" ||
                    propertyName == "issupported" ||
                    propertyName == "clampoutput" ||
                    propertyName == "name" ||
                    propertyName == "alphamode")
                {
                    continue;
                }

                object obj = info.GetValue(effect);
                if (obj != null)
                {
                    if (obj is IGraphicsEffect || obj is IList<IGraphicsEffectSource>)
                    {
                        expandedProperties.Add(info.Name, obj);
                    }
                    else
                    {
                        if (obj is CompositionEffectSourceParameter)
                        {
                            CompositionEffectSourceParameter param = (CompositionEffectSourceParameter)obj;
                            CompositionBrush sourceBrush = brush.GetSourceParameter(param.Name);

                            string s = String.Format("{0}{1} :\r\n{0}{{\r\n", Indent(indent), info.Name);
                            sb.Append(s);
                            BrushToString(sb, indent + indentIncrement, sourceBrush);
                            sb.AppendFormat("{0}}}\r\n", Indent(indent));
                        }
                        else
                        {
                            sb.AppendFormat("{0}{1} : {2}\r\n", Indent(indent), info.Name, Helpers.ToString(obj));
                        }
                    }
                }
            }

            // Moved all of the nested source properties to the end of the list
            foreach (KeyValuePair<string, object> entry in expandedProperties)
            {
                string name = entry.Key;
                object obj = entry.Value;

                if (obj is IGraphicsEffect)
                {
                    string s = String.Format("{0}{1} :\r\n{0}{{\r\n", Indent(indent), name);
                    sb.Append(s);
                    EffectToString(sb, indent + indentIncrement, (IGraphicsEffect)obj, brush);
                    sb.AppendFormat("{0}}}\r\n", Indent(indent));
                }
                else if (obj is IList<IGraphicsEffectSource>)
                {
                    IList<IGraphicsEffectSource> list = (IList<IGraphicsEffectSource>)obj;

                    sb.AppendFormat("{0}{1} :\r\n{0}[\r\n", Indent(indent), name);
                    foreach (IGraphicsEffectSource source in list)
                    {
                        EffectToString(sb, indent + indentIncrement, (IGraphicsEffect)source, brush);
                    }
                    sb.AppendFormat("{0}]\r\n", Indent(indent));
                }
            }

            indent -= indentIncrement;
            sb.AppendFormat("{0}}}\r\n", Indent(indent));
        }

        private async void ViewEffectGraph_Click(object sender, RoutedEventArgs e)
        {
            if (!_inDialog)
            {
                _inDialog = true;

                IGraphicsEffect effect = Material.Effect;

                if (effect != null)
                {
                    StringBuilder sb = new StringBuilder(1024);

                    EffectToString(sb, 0, effect, (CompositionEffectBrush)Material.Brush);

                    ViewEffectDialog viewEffectDialog = new ViewEffectDialog()
                    {
                        Title = "View Effect Graph",
                        PrimaryButtonText = "OK",
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        IsSecondaryButtonEnabled = false,
                    };

                    viewEffectDialog.EffectText = sb.ToString();

                    await viewEffectDialog.ShowAsync();
                }

                _inDialog = false;
            }
        }
    }
}
