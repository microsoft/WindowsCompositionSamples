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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CompositionSampleGallery.Commands;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI;
using SamplesCommon;

namespace CompositionSampleGallery
{
    /// <summary>
    /// This enum describes the different modes the view can be in in relation to lighting. None is 
    /// the default value, AddLight is used when in the 'Add Light' mode, and RemoveLight is used
    /// when removing a light.
    /// </summary>
    public enum LightMode
    {
        None,
        AddLight,
        RemoveLight
    }

    /// <summary>
    /// A container for the PointLight and its paired visual representation.
    /// </summary>
    class Light
    {
        public PointLight PointLight { get; set; }
        public SpriteVisual CircleRepresentation { get; set; }
    }


    /// <summary>
    /// Viewmodel for the VideoPlaygroundPage. Controls media playback and Composition related objects.
    /// </summary>
    public class VideoPlaygroundViewModel : INotifyPropertyChanged
    {
        #region Statics
        private static readonly int MaxNumberOfLights = 2;
        #endregion

        #region Private Members
        // Media
        private MediaPlayer _mediaPlayer;
        private MediaPlayerSurface _videoSurface;

        // Composition
        private Compositor _compositor;
        private ContainerVisual _videoRootVisual;
        private SpriteVisual _videoVisual;
        private CompositionSurfaceBrush _videoBrush;

        // Composition - Lighting
        private LightMode _lightMode;
        private List<Light> _lights;
        private ManagedSurface _circleSurface;

        // Page specific objects/variables.
        private ObservableCollection<double> _durationList;
        private ObservableCollection<EffectItem> _effectItems;
        private EffectItem _currentEffect;
        private float _effectValue1;
        private float _effectValue2;
        private int _effectIndex;
        private int _durationIndex;

        // Dispatcher used to change bound values when off the UI thread.
        private CoreDispatcher _dispatcher;
        #endregion

        #region Public Properties
        /// <summary>
        /// Contains the list of seconds used for the duration of animations.
        /// </summary>
        public ObservableCollection<double> Durations { get { return _durationList; } private set { _durationList = value; OnPropertyChanged(); } }
        /// <summary>
        /// Index of the duration ComboBox.
        /// </summary>
        public int DurationIndex { get { return _durationIndex; } set { _durationIndex = value;  OnPropertyChanged(); } }
        /// <summary>
        /// Set of effects that can be placed on the video.
        /// </summary>
        public ObservableCollection<EffectItem> EffectItems { get { return _effectItems; } private set { _effectItems = value; OnPropertyChanged(); } }
        /// <summary>
        /// The current effect being applied.
        /// </summary>
        public EffectItem CurrentEffect
        {
            get { return _currentEffect; }
            set
            {
                var brush = value != null ? value.GetEffectBrush() : null;

                if (brush == null)
                {
                    RemoveEffect();
                }
                else if (value != null)
                {
                    brush.SetSourceParameter("Video", _videoBrush);
                    SetEffect(brush);
                    EffectValue1 = value.ValueMin;
                    EffectValue2 = value.ValueMax;
                }

                _currentEffect = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// The current and starting value used for the current effect brush.
        /// </summary>
        public float EffectValue1 { get { return _effectValue1; } set { _effectValue1 = value; OnPropertyChanged(); CurrentEffect.ChangeValue(value); } }
        /// <summary>
        /// The ending/target value used for animating the current effect brush.
        /// </summary>
        public float EffectValue2 { get { return _effectValue2; } set { _effectValue2 = value; OnPropertyChanged(); } }
        /// <summary>
        /// The current index of the effect ComboBox.
        /// </summary>
        public int EffectIndex
        {
            get { return _effectIndex; }
            set
            {
                _effectIndex = value;
                OnPropertyChanged();
                if (value >= 0)
                {
                    CurrentEffect = EffectItems[value];
                }
                else
                {
                    CurrentEffect = null;
                }
            }
        }
        /// <summary>
        /// The current LightMode.
        /// </summary>
        public LightMode LightMode
        {
            get { return _lightMode; }
            set
            {
                _lightMode = value;
                ShowHideCircles(_lightMode != LightMode.None);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAddMode));
                OnPropertyChanged(nameof(IsRemoveMode));
            }
        }
        
        // Page specific properties that help with bindings and commands.
        public bool IsAddMode { get { return LightMode == LightMode.AddLight; } }
        public bool IsRemoveMode { get { return LightMode == LightMode.RemoveLight; } }
        public IDelegateCommand AddLightCommand { get; private set; } 
        public IDelegateCommand RemoveLightCommand { get; private set; }
        public IDelegateCommand OpenFileCommand { get; private set; }
        public IDelegateCommand OpenLinkDialogCommand { get; private set; }
        public IDelegateCommand OpenLinkCommand { get; private set; }
        public IDelegateCommand AnimateCommand { get; private set; }
        #endregion

        #region Constructors
        public VideoPlaygroundViewModel(Compositor compositor, Grid videoContentGrid)
        {
            // Stash the dispatcher so that we can make calls off the UI thread (or rather, request
            // the UI thread to make calls on our behalf) that influence the UI.
            _dispatcher = videoContentGrid.Dispatcher;
            _compositor = compositor;
            // Set up our composition objects.
            EnsureComposition(videoContentGrid);

            EffectIndex = -1;
            _lights = new List<Light>();

            EffectItems = new ObservableCollection<EffectItem>();

            // No effect.
            EffectItems.Add(EffectItem.None);
            // Saturation effect with an adjustable Sturation property.
            EffectItems.Add(new EffectItem() { EffectName = "Saturation", AnimatablePropertyName = "Saturation", ValueMin = 0, ValueMax = 1, SmallChange = 0.1f, LargeChange = 0.3f, CreateEffectFactory = CreateSaturationEffectFactory });
            // Blur effect with an adjustable BlurAmount property.
            EffectItems.Add(new EffectItem() { EffectName = "Blur", AnimatablePropertyName = "BlurAmount", ValueMin = 0, ValueMax = 20, SmallChange = 1.0f, LargeChange = 3.0f, CreateEffectFactory = CreateBlurEffectFactory });
            // Invert effect with no adjustable properties.
            EffectItems.Add(new EffectItem() { EffectName = "Invert", AnimatablePropertyName = null, CreateEffectFactory = CreateInvertEffectFactory });
            // HueRotation effect with an adjustable Angle property.
            EffectItems.Add(new EffectItem() { EffectName = "HueRotation", AnimatablePropertyName = "Angle", ValueMin = 0, ValueMax = (float)(2.0 * Math.PI), SmallChange = 0.1f, LargeChange = 1.0f, CreateEffectFactory = CreateHueRotationEffectFactory });

            Durations = new ObservableCollection<double>();

            // Setup our duration values, from 0.5 seconds to 5 seconds.
            for (int i = 1; i <= 10; i++)
            {
                Durations.Add(i * 0.5);
            }

            AddLightCommand = new DelegateCommand(AddLightButton);
            RemoveLightCommand = new DelegateCommand(RemoveLightButton);
            AnimateCommand = new DelegateCommand(Animate);
            OpenFileCommand = new DelegateCommand(OpenFile);
            OpenLinkDialogCommand = new DelegateCommand(OpenLinkDialog);
            OpenLinkCommand = new DelegateCommand(OpenLink);

            DurationIndex = 0;
        }
        #endregion

        #region MediaPlayer
        /// <summary>
        /// Command that shows the file picker UI to select a video.
        /// </summary>
        /// <param name="obj">Unused.</param>
        private void OpenFile(object obj)
        {
            LoadFile();
        }

        /// <summary>
        /// Command that shows the given ContentDialog.
        /// </summary>
        /// <param name="param">The ContentDialog to show.</param>
        private async void OpenLinkDialog(object param)
        {
            var dialog = param as ContentDialog;

            if (dialog != null)
            {
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Command that opens the given link as a MediaSource.
        /// </summary>
        /// <param name="param">String that is a valid Uri.</param>
        private void OpenLink(object param)
        {
            try
            {
                var link = param as String;

                var source = MediaSource.CreateFromUri(new Uri(link));

                LoadSource(source);
            }
            catch(Exception ex)
            {
                var ignored = ErrorMessage("Video Error", ex.Message);
            }
        }

        /// <summary>
        /// Show the file picker UI and returns the given video.
        /// </summary>
        /// <returns>StorageFile that contains the selected video.</returns>
        private async Task<StorageFile> OpenFile()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            // We could technically support more formats, all we would have to do is add them
            // to the FileTypeFilter colleciton. For now we'll just use mp4 files.
            picker.FileTypeFilter.Add(".mp4");

            return await picker.PickSingleFileAsync();
        }

        /// <summary>
        /// Requests the user to choose a video from their machine. If chosen, it is created into a
        /// MediaSource to play.
        /// </summary>
        public async void LoadFile()
        {
            var file = await OpenFile();

            if (file != null)
            {
                var source = MediaSource.CreateFromStorageFile(file);
                LoadSource(source);
            }
        }

        /// <summary>
        /// Starts playing a given MediaSource. Creates the MediaPlayer if not alreay created, as well
        /// as the CompositionSurfaceBrush that contains the video.
        /// </summary>
        /// <param name="source">The MediaSoruce intended to play on the visual.</param>
        private void LoadSource(MediaSource source)
        {
            EnsureMediaPlayer();

            var item = new MediaPlaybackItem(source);

            _mediaPlayer.Source = item;
            _mediaPlayer.IsLoopingEnabled = true;

            EnsureVideoBrush();

            _mediaPlayer.Play();

            // Now that we have a video selected and playing, its safe to choose an effect.
            if (EffectIndex < 0)
            {
                EffectIndex = 0;
            }
        }

        /// <summary>
        /// Creates a MediaPlayer if one hasn't already been created. Also listens to the player's
        /// MediaFailed event.
        /// </summary>
        private void EnsureMediaPlayer()
        {
            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            }
        }

        /// <summary>
        /// Handles the MediaFailed event from our MediaPlayer.
        /// </summary>
        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            if (_dispatcher != null)
            {
                await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    EffectIndex = -1;
                    await ErrorMessage("Video Error", args.ErrorMessage);
                });
            }
        }

        /// <summary>
        /// Show a given error message on a MessageDialog.
        /// </summary>
        /// <param name="message">Desired message for the MessageDialog.</param>
        /// <returns>Awaitable Task</returns>
        private async Task ErrorMessage(String title, String message)
        {
            var dialog = new MessageDialog(message);
            dialog.Title = title;

            await dialog.ShowAsync();
        }
        #endregion

        #region Video Visual
        /// <summary>
        /// Creates the visual tree that will house the video.
        /// </summary>
        /// <param name="videoContentGrid"></param>
        private void EnsureComposition(Grid videoContentGrid)
        {
            if (_videoVisual == null)
            {
                // Create a ContainerVisual that we'll attach lights to, as well as our video.
                _videoRootVisual = _compositor.CreateContainerVisual();

                // Create a SpriteVisual to host the video content.
                _videoVisual = _compositor.CreateSpriteVisual();
                _videoVisual.Size = new Vector2((float)videoContentGrid.ActualWidth, (float)videoContentGrid.ActualHeight);

                // Make sure our visual stays in sync with our host.
                videoContentGrid.SizeChanged += (s, a) =>
                {
                    _videoVisual.Size = new Vector2((float)videoContentGrid.ActualWidth, (float)videoContentGrid.ActualHeight);
                };

                // Attach our visuals to the tree.
                _videoRootVisual.Children.InsertAtTop(_videoVisual);
                ElementCompositionPreview.SetElementChildVisual(videoContentGrid, _videoRootVisual);
            }
        }

        /// <summary>
        /// Creates the CompositionSurfaceBrush if one hasn't already been created. Also assigns the
        /// brush to the video SpriteVisual.
        /// </summary>
        private void EnsureVideoBrush()
        {
            if (_videoSurface == null)
            {
                // Getting the surface that represents the MediaPlayer's swapchain for video content
                // is easy, just pass a Compositor to the MediaPlayer!
                _videoSurface = _mediaPlayer.GetSurface(_compositor);

                // Create a brush for our new surface and attach it to our visual.
                _videoBrush = _compositor.CreateSurfaceBrush(_videoSurface.CompositionSurface);
                _videoVisual.Brush = _videoBrush;
            }
        }
        #endregion

        #region Effects
        /// <summary>
        /// Command that requests the current effect to aniamte. 
        /// </summary>
        /// <param name="obj">Unused.</param>
        private void Animate(object obj)
        {
            if (CurrentEffect != null)
            {
                CurrentEffect.Animate(EffectValue1, EffectValue2, Durations[DurationIndex]);
            }
        }

        /// <summary>
        /// Remove the effect brush from the video by reassigning the plain brush containing 
        /// our video.
        /// </summary>
        private void RemoveEffect()
        {
            if (_videoVisual != null)
            {
                _videoVisual.Brush = _videoBrush;
            }
        }

        /// <summary>
        /// Assign a given CompositionEffectBrush to our video visual. The caller is in charge
        /// of assigning the video content to the brush.
        /// </summary>
        /// <param name="brush">The effect brush to apply to the video visual.</param>
        private void SetEffect(CompositionEffectBrush brush)
        {
            if (_videoVisual != null)
            {
                _videoVisual.Brush = brush;
            }
        }

        /// <summary>
        /// Creates a CompositionEffectFactory that creates SaturationEffects.
        /// </summary>
        /// <returns>CompositionEffectFactory</returns>
        private CompositionEffectFactory CreateSaturationEffectFactory()
        {
            var effectDefinition = new SaturationEffect()
            {
                Name = "Saturation",
                Saturation = 0.0f,
                Source = new CompositionEffectSourceParameter("Video")
            };

            return _compositor.CreateEffectFactory(effectDefinition, new string[] { "Saturation.Saturation" });
        }

        /// <summary>
        /// Creates a CompositionEffectFactory that creates GaussianBlurEffects.
        /// </summary>
        /// <returns>CompositionEffectFactory</returns>
        private CompositionEffectFactory CreateBlurEffectFactory()
        {
            var effectDefinition = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("Video")
            };

            return _compositor.CreateEffectFactory(effectDefinition, new string[] { "Blur.BlurAmount" });
        }

        /// <summary>
        /// Creates a CompositionEffectFactory that creates InvertEffects.
        /// </summary>
        /// <returns>CompositionEffectFactory</returns>
        private CompositionEffectFactory CreateInvertEffectFactory()
        {
            var effectDefinition = new InvertEffect()
            {
                Source = new CompositionEffectSourceParameter("Video")
            };

            return _compositor.CreateEffectFactory(effectDefinition);
        }

        /// <summary>
        /// Creates a CompositionEffectFactory that creates HueRotationEffects.
        /// </summary>
        /// <returns>CompositionEffectFactory</returns>
        private CompositionEffectFactory CreateHueRotationEffectFactory()
        {
            var effectDefinition = new HueRotationEffect()
            {
                Name = "HueRotation",
                Angle = 0.0f,
                Source = new CompositionEffectSourceParameter("Video")
            };

            return _compositor.CreateEffectFactory(effectDefinition, new string[] { "HueRotation.Angle" });
        }
        #endregion

        #region Lights
        /// <summary>
        /// Command that changes the page into AddLight mode, or into normal/none mode if already 
        /// in AddLight mode.
        /// </summary>
        /// <param name="obj">Unused.</param>
        public void AddLightButton(object obj)
        {
            if (LightMode != LightMode.AddLight)
            {
                LightMode = LightMode.AddLight;
            }
            else
            {
                LightMode = LightMode.None;
            }
        }

        /// <summary>
        /// Command that changes the page into RemoveLight mode, or into normal/none mode if already 
        /// in RemoveLight mode.
        /// </summary>
        /// <param name="obj">Unused.</param>
        public void RemoveLightButton(object obj)
        {
            if (LightMode != LightMode.RemoveLight)
            {
                LightMode = LightMode.RemoveLight;
            }
            else
            {
                LightMode = LightMode.None;
            }
        }

        /// <summary>
        /// Add a light at a given x and y, which should be in the root's coordinate space.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void AddLight(float x, float y)
        {
            if (_lights.Count < MaxNumberOfLights)
            {
                var lightVisual = _compositor.CreatePointLight();

                lightVisual.CoordinateSpace = _videoRootVisual;
                lightVisual.Targets.Add(_videoVisual);

                lightVisual.Offset = new Vector3(x, y, 75.0f);

                var circleVisual = CreateCircleVisual();

                circleVisual.Offset = new Vector3(x, y, 0.0f);
                _videoRootVisual.Children.InsertAtTop(circleVisual);

                var light = new Light() { PointLight = lightVisual, CircleRepresentation = circleVisual };
                _lights.Add(light);
            }
            else
            {
                var ignored = ErrorMessage(
                    "Maximum Allowed Lights",
                    "You have reached the maximum number of lights. Please remove some lights before continuing.");
            }

            // Exit AddLight mode.
            LightMode = LightMode.None;
        }

        /// <summary>
        /// Do a "hit-test" for a light and remove it if found at the given x and y in the root's
        /// coordinate space.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public void TryRemoveLight(float x, float y)
        {
            Light lightToRemove = null;

            foreach (var light in _lights)
            {
                var pointLight = light.PointLight;
                if ((x >= pointLight.Offset.X - 25.0f && x <= pointLight.Offset.X + 25.0f) &&
                    (y >= pointLight.Offset.Y - 25.0f && y <= pointLight.Offset.Y + 25.0f))
                {
                    lightToRemove = light;
                    break;
                }
            }

            // Did we find a light?
            if (lightToRemove != null)
            {
                // Stop tracking this light and dispose of it.
                _lights.Remove(lightToRemove);
                lightToRemove.PointLight.Targets.RemoveAll();
                lightToRemove.PointLight.CoordinateSpace = null;
                lightToRemove.PointLight.Dispose();

                // Remove the circle
                _videoRootVisual.Children.Remove(lightToRemove.CircleRepresentation);

                // Exit RemoveLight mode.
                LightMode = LightMode.None;
            }
        }

        /// <summary>
        /// Creates a circle that represents the light.
        /// </summary>
        /// <returns>Cirlce Visual</returns>
        private SpriteVisual CreateCircleVisual()
        {
            var visual = _compositor.CreateSpriteVisual();

            visual.AnchorPoint = new Vector2(0.5f, 0.5f);
            visual.Size = new Vector2(50.0f, 50.0f);

            EnsureCircleBrush();

            visual.Brush = _circleSurface.Brush;

            return visual;
        }

        /// <summary>
        /// Creates the circle used in the circle brush if not already created.
        /// </summary>
        private void EnsureCircleBrush()
        {
            if (_circleSurface == null)
            {
                _circleSurface = ImageLoader.Instance.LoadCircle(200, Colors.White);
            }
        }

        /// <summary>
        /// Shows or hides the representations of the lights.
        /// </summary>
        /// <param name="isVisible">True if visible, false if hidden.</param>
        private void ShowHideCircles(bool isVisible)
        {
            foreach(var light in _lights)
            {
                light.CircleRepresentation.IsVisible = isVisible;
            }
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// Cleanup our resources and stop video playback.
        /// </summary>
        public void Cleanup()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
            }

            if (_videoSurface != null)
            {
                _videoSurface.Dispose();
            }

            if (_circleSurface != null)
            {
                _circleSurface.Dispose();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
