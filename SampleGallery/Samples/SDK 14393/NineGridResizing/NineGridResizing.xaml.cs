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

using CompositionSampleGallery.Samples.SDK_14393.NineGridResizing;
using CompositionSampleGallery.Samples.SDK_14393.NineGridResizing.NineGridScenarios;
using SamplesCommon;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

namespace CompositionSampleGallery
{
    public sealed partial class NineGridResizing : SamplePage, INotifyPropertyChanged
    {
        private readonly Compositor _compositor;
        private readonly Visual _backgroundContainer;
        private readonly SpriteVisual _ninegridVisual;
        private readonly ManagedSurface _ninegridSurface;
        private readonly CompositionNineGridBrush _ninegridBrush;
        private readonly ObservableCollection<INineGridScenario> _nineGridBrushScenarios;
        private INineGridScenario _selectedBrushScenario;
        private Vector2 _defaultSize;
        private bool _isAnimatedInterpolation;
        private static readonly TimeSpan _duration = TimeSpan.FromSeconds(2);
        private ValueTimer<float> _valueTimerXSlider;
        private ValueTimer<float> _valueTimerYSlider;
        private ValueTimer<float> _valueTimerScaleSlider;

        public static string StaticSampleName => "Nine-Grid Resizing"; 
        public override string SampleName => StaticSampleName; 
        public static string StaticSampleDescription => "Resize and Scale a SpriteVisual painted with a NineGridBrush"; 
        public override string SampleDescription => StaticSampleDescription;
        public override string SampleCodeUri => "https://go.microsoft.com/fwlink/?linkid=869001";

        public NineGridResizing()
        {
            this.InitializeComponent();

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Add page loaded event listener
            this.Loaded += NineGridResizing_Loaded;

            // Set data context for data binding
            DataContext = this;

            // Sprite visual to be painted
            _ninegridVisual = _compositor.CreateSpriteVisual();

            // Create ninegridbrush and paint on visual;
            _ninegridBrush = _compositor.CreateNineGridBrush();
            _ninegridVisual.Brush = _ninegridBrush;
            _ninegridSurface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/Other/RoundedRect.png"));

            // Clip compgrid 
            var compGrid = ElementCompositionPreview.GetElementVisual(CompGrid);
            compGrid.Clip = _compositor.CreateInsetClip();

            // Scene container to be scaled
            _backgroundContainer = ElementCompositionPreview.GetElementVisual(bkgHost);

            // Insert Composition island
            ElementCompositionPreview.SetElementChildVisual(ngHost, _ninegridVisual);

            // Instatiate brush scenario list and fill with created brush scenarios
            _nineGridBrushScenarios = new ObservableCollection<INineGridScenario>(CreateBrushes(_compositor, _ninegridSurface, _ninegridVisual.Size));

            // Set default combo box selection to first item
            BrushScenarioSelected = _nineGridBrushScenarios.FirstOrDefault();

            // Value timer initialization for sliders
            _valueTimerXSlider = new ValueTimer<float>();
            _valueTimerXSlider.ValueChanged += OnXSliderValueChanged;

            _valueTimerYSlider = new ValueTimer<float>();
            _valueTimerYSlider.ValueChanged += OnYSliderValueChanged;

            _valueTimerScaleSlider = new ValueTimer<float>();
            _valueTimerScaleSlider.ValueChanged += OnScaleSliderValueChanged;
        }

        /// <summary>
        /// Handles property changes for data binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Data binding for animated interpolation toggle button
        /// </summary>
        public bool IsAnimatedInterpolation
        {
            get { return _isAnimatedInterpolation; }
            set
            {
                _isAnimatedInterpolation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAnimatedInterpolation)));
                if (_isAnimatedInterpolation)
                {
                    _valueTimerXSlider.IntervalMilliseconds = _valueTimerYSlider.IntervalMilliseconds = _valueTimerScaleSlider.IntervalMilliseconds = 250;
                }
                else
                {
                    _valueTimerXSlider.IntervalMilliseconds = _valueTimerYSlider.IntervalMilliseconds = _valueTimerScaleSlider.IntervalMilliseconds = 0;
                }
            }
        }

        /// <summary>
        /// Data binding for selected brush combobox scenario.
        /// </summary>
        public INineGridScenario BrushScenarioSelected
        {
            get { return _selectedBrushScenario; }
            set
            {
                _selectedBrushScenario = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BrushScenarioSelected)));
                ComboBoxSelectedItemChanged();
            }
        }

        /// <summary>
        /// Instantiate brush scenarios to use on the visual; used in combobox BrushSelection changed event.
        /// </summary>
        private static INineGridScenario[] CreateBrushes(Compositor compositor, ManagedSurface ninegridSurface, Vector2 visualSize)
        {
            ninegridSurface.Brush.Stretch = CompositionStretch.Fill;

            // Create INineGridScenario array to return. Surface scenario is special because it's used as input to another scenario
            var surfaceNineGridScenario = new SurfaceNineGridScenario(compositor, ninegridSurface.Brush, "Source: SurfaceBrush");
            return new INineGridScenario[]
            {
                new ColorNineGridScenario(compositor, "Source: ColorBrush(hollow)"),
                new BorderNineGridScenario(compositor, ninegridSurface.Brush, visualSize, "Source: ColorBrush(w/ content)"),
                surfaceNineGridScenario,
                new EffectNineGridScenario(compositor, (CompositionNineGridBrush)surfaceNineGridScenario.Brush, "Input to: EffectBrush"),
                new MaskNineGridScenario(compositor, ninegridSurface.Brush, "Input to: MaskBrush")
            };
        }

        /// <summary>
        /// Handles selection change event from the XAML BrushSelection ComboBox.
        /// Uses the appropriate scenario to update the visual with the correct brush.
        /// </summary>
        private void ComboBoxSelectedItemChanged()
        {
            // Remove content from the border case when switching brushes
            var children = _ninegridVisual.Children;
            if (children != null)
            {
                children.RemoveAll();
            }

            BrushScenarioSelected.Selected(_ninegridVisual);
        }

        /// <summary>
        /// Helper for slider value changes; updates ninegrid visual to correct x/y size with animation.
        /// </summary>
        private void AnimateXYSliderChangeHelper(ValueTimer<float> sender, string direction)
        {
            // For animated case, animate from the current to the released values using a keyframe animation
            if (IsAnimatedInterpolation)
            {
                var percentSliderValue = (float)sender.Value / 100.0f;

                float defaultSizeValue;
                switch (direction)
                {
                    case "x":
                        defaultSizeValue = _defaultSize.X;
                        break;
                    case "y":
                        defaultSizeValue = _defaultSize.Y;
                        break;
                    default:
                        throw new ArgumentException("Parameter must be 'x' or 'y'", direction);
                }

                // Define keyframe animation
                var animation = _compositor.CreateScalarKeyFrameAnimation();
                animation.InsertExpressionKeyFrame(1, direction + " * p");
                animation.SetScalarParameter(direction, defaultSizeValue);
                animation.SetScalarParameter("p", percentSliderValue);
                animation.Duration = _duration;

                // Start animation
                _ninegridVisual.StartAnimation("Size." + direction.ToUpper(), animation);
            }
        }

        /// <summary>
        /// Called on x slider value changed; calls value timer to start attempt at value change.
        /// </summary>
        private void SizeXSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_valueTimerXSlider != null)
            {
                _valueTimerXSlider.Restart((float)((Slider)sender).Value);
            }
        }

        /// <summary>
        /// Callback for value timer to execute value changed.
        /// </summary>
        private void OnXSliderValueChanged(ValueTimer<float> sender, ValueChangedEventArgs<float> args)
        {
            if (_valueTimerXSlider.IntervalMilliseconds != 0 && IsAnimatedInterpolation)
            {
                this.AnimateXYSliderChangeHelper(sender, "x");
            }
            else if (!IsAnimatedInterpolation)
            {
                // For non-animated case, change Size.X based on the percentage value from the slider
                var p = (float)args.Value / 100.0f;
                var x = _defaultSize.X;
                var y = _ninegridVisual.Size.Y;
                _ninegridVisual.Size = new Vector2(x * p, y);
            }
        }

        /// <summary>
        /// Called on y slider value changed; calls value timer to start attempt at value change.
        /// </summary>
        private void SizeYSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_valueTimerYSlider != null)
            {
                _valueTimerYSlider.Restart((float)((Slider)sender).Value);
            }
        }

        /// <summary>
        /// Callback for value timer to execute value changed.
        /// </summary>
        private void OnYSliderValueChanged(ValueTimer<float> sender, ValueChangedEventArgs<float> args)
        {
            if (_valueTimerYSlider.IntervalMilliseconds != 0 && IsAnimatedInterpolation)
            {
                this.AnimateXYSliderChangeHelper(sender, "y");
            }
            else if(!IsAnimatedInterpolation)
            {
                // For non-animated case, change Size.Y based on the percentage value from the slider
                var x = _ninegridVisual.Size.X;
                var p = (float)args.Value / 100.0f;
                var y = _defaultSize.Y;
                _ninegridVisual.Size = new Vector2(x, y * p);
            }
        }

        /// <summary>
        /// Called on scale slider value changed; calls value timer to start attempt at value change.
        /// </summary>
        private void ScaleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_valueTimerScaleSlider != null)
            {
                _valueTimerScaleSlider.Restart((float)((Slider)sender).Value);
            }
        }

        /// <summary>
        /// Callback for value timer to execute value changed.
        /// </summary>
        private void OnScaleSliderValueChanged(ValueTimer<float> sender, ValueChangedEventArgs<float> args)
        {
            if (_valueTimerScaleSlider.IntervalMilliseconds != 0 && IsAnimatedInterpolation)
            {
                // For animated case, animate from the current to the released values using a keyframe animation
                var scaleValue = (float)args.Value;

                // Define keyframe animation
                var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
                scaleAnimation.InsertKeyFrame(1, new Vector3(scaleValue / 100.0f, scaleValue / 100.0f, 1));
                scaleAnimation.Duration = _duration;

                // Start animations
                _ninegridVisual.StartAnimation("Scale", scaleAnimation);
            }
            else if(!IsAnimatedInterpolation)
            {
                // For non-animated case, change Scale based on the percentage value from the slider
                var s = (float)args.Value;
                _ninegridVisual.Scale = new Vector3(s / 100.0f, s / 100.0f, 1);
            }
        }

        /// <summary>
        /// Called on reset button click; reses sliders and visual to original values.
        /// </summary>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset values on controls and restore default transforms
            _ninegridVisual.Size = _defaultSize;
            _ninegridVisual.Scale = new Vector3(1, 1, 0);
            _ninegridBrush.SetInsetScales(1.0f);
            _backgroundContainer.Scale = new Vector3(1, 1, 0);
            SizeXSlider.Value = 100;
            SizeYSlider.Value = 100;
            ScaleSlider.Value = 100;
            IsAnimatedInterpolation = false;
        }

        /// <summary>
        /// Called on page load to do initial setup.
        /// </summary>
        private void NineGridResizing_Loaded(object sender, RoutedEventArgs e)
        {
            // Set properties for ninegridVisual and backgroundContainer
            SetDefaultVisualProperties();
        }

        /// <summary>
        /// Set/update properties for the visual and container.
        /// </summary>
        private void SetDefaultVisualProperties()
        {
            // Compute size and transforms 
            _defaultSize = new Vector2((float)(Math.Min(ngHost.ActualWidth, ngHost.ActualHeight)) * 0.35f);

            // Specify centerpoint for scale transforms
            _backgroundContainer.CenterPoint = new Vector3(bkgHost.RenderSize.ToVector2() / 2, 0);

            _ninegridVisual.Size = new Vector2((float)SizeXSlider.Value / 100.0f * _defaultSize.X, (float)SizeYSlider.Value / 100.0f * _defaultSize.Y);
            _ninegridVisual.Offset = new Vector3(ngHost.RenderSize.ToVector2() / 2, 0);
            _ninegridVisual.AnchorPoint = new Vector2(0.5f);
        }

        /// <summary>
        /// Recomputes transforms on visual based on updated UIElement size.
        /// </summary>
        private void CompGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDefaultVisualProperties();
        }

        private void SamplePage_Unloaded(object sender, RoutedEventArgs e)
        {
            _valueTimerXSlider.Dispose();
            _valueTimerYSlider.Dispose();
            _valueTimerScaleSlider.Dispose();
        }
    }
}
