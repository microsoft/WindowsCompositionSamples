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

using CompositionSampleGallery.Shared;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace CompositionSampleGallery
{
    public sealed partial class PointerEnterEffects : SamplePage
    {
        private Compositor _compositor;
        private PointerEffectTechniques.EffectTechniques _currentTechnique;

        public PointerEnterEffects()
        {
            Model = new LocalDataSource();
            this.InitializeComponent();
        }

        public static string        StaticSampleName => "Pointer Enter/Exit Effects"; 
        public override string      SampleName => StaticSampleName;
        public static string        StaticSampleDescription => "Demonstrates how to apply effects to ListView items that respond to mouse enter, mouse leave, and mouse position. Hover your mouse cursor over a ListView item to trigger the selected effect."; 
        public override string      SampleDescription => StaticSampleDescription; 
        public override string      SampleCodeUri => "http://go.microsoft.com/fwlink/p/?LinkID=761167"; 

        public LocalDataSource Model
        {
            get; set;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            // Populate the Effect combobox
            IList<ComboBoxItem> effectList = new List<ComboBoxItem>();
            foreach (PointerEffectTechniques.EffectTechniques.EffectTypes type in 
                     Enum.GetValues(typeof(PointerEffectTechniques.EffectTechniques.EffectTypes)))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = type;
                item.Content = type.ToString();
                effectList.Add(item);
            }

            EffectSelection.ItemsSource = effectList;
            EffectSelection.SelectedIndex = 0;

            ThumbnailList.ItemsSource = ThumbnailList.ItemsSource = Model.AggregateDataSources(new ObservableCollection<Thumbnail>[] { Model.Landscapes, Model.Nature });
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_currentTechnique != null)
            {
                _currentTechnique.ReleaseResources();
                _currentTechnique = null;
            }
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            CompositionImage image = args.ItemContainer.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();
            Thumbnail thumbnail = args.Item as Thumbnail;
            Uri uri = new Uri(thumbnail.ImageUrl);

            // Update the image URI
            image.Source = uri;

            // Update the image with the current effect
            ApplyEffect(image);
        }

        private async void EffectSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PointerEffectTechniques.EffectTechniques newTechnique = null;

            ComboBoxItem item = EffectSelection.SelectedValue as ComboBoxItem;
            switch ((PointerEffectTechniques.EffectTechniques.EffectTypes)item.Tag)
            {
                case PointerEffectTechniques.EffectTechniques.EffectTypes.Exposure:
                    newTechnique = new PointerEffectTechniques.ExposureTechnique(_compositor);
                    break;
                case PointerEffectTechniques.EffectTechniques.EffectTypes.Desaturation:
                    newTechnique = new PointerEffectTechniques.DesaturateTechnique(_compositor);
                    break;
                case PointerEffectTechniques.EffectTechniques.EffectTypes.Blur:
                    newTechnique = new PointerEffectTechniques.BlurTechnique(_compositor);
                    break;
                case PointerEffectTechniques.EffectTechniques.EffectTypes.SpotLight:
                    newTechnique = new PointerEffectTechniques.SpotLightTechnique(_compositor);
                    break;
                case PointerEffectTechniques.EffectTechniques.EffectTypes.PointLightFollow:
                    newTechnique = new PointerEffectTechniques.PointLightFollowTechnique(_compositor);
                    break;
                default:
                    break;
            }

            // Load the resources async
            await newTechnique.LoadResources();

            // Everything is ready to go, release the current technique
            if (_currentTechnique != null)
            {
                _currentTechnique.ReleaseResources();
                _currentTechnique = null;
            }

            // Set the new technique
            _currentTechnique = newTechnique;

            // Update the list to use the new technique
            RefreshListViewContent();
        }

        private async void ApplyEffect(CompositionImage image)
        {
            ManagedSurface effectSurface = null;

            // If we've requested a load time effect input, kick it off now
            if (_currentTechnique.LoadTimeEffectHandler != null)
            {
                effectSurface = await ImageLoader.Instance.LoadFromUriAsync(image.Source, Size.Empty, _currentTechnique.LoadTimeEffectHandler);
            }

            // Create the new brush, set the inputs and set it on the image
            CompositionEffectBrush brush = _currentTechnique.CreateBrush();
            brush.SetSourceParameter("ImageSource", image.SurfaceBrush);
            image.Brush = brush;

            // Set the effect surface as input
            if (effectSurface != null)
            {
                // Set to UniformToFill to match the stretch mode of the original image
                effectSurface.Brush.Stretch = CompositionStretch.UniformToFill;
                brush.SetSourceParameter("EffectSource", effectSurface.Brush);
            }
        }

        private void RefreshListViewContent()
        {
            if (ThumbnailList.ItemsPanelRoot != null)
            {
                foreach (ListViewItem item in ThumbnailList.ItemsPanelRoot.Children)
                {
                    CompositionImage image = item.ContentTemplateRoot.GetFirstDescendantOfType<CompositionImage>();

                    ApplyEffect(image);
                }
            }
        }

        private void Canvas_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            CompositionImage image = ((DependencyObject)sender).GetFirstDescendantOfType<CompositionImage>();
            _currentTechnique.OnPointerEnter(e.GetCurrentPoint(image).Position.ToVector2(), image);
        }

        private void Canvas_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            CompositionImage image = ((DependencyObject)sender).GetFirstDescendantOfType<CompositionImage>();
            _currentTechnique.OnPointerExit(e.GetCurrentPoint(image).Position.ToVector2(), image);
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            CompositionImage image = ((DependencyObject)sender).GetFirstDescendantOfType<CompositionImage>();
            _currentTechnique.OnPointerMoved(e.GetCurrentPoint(image).Position.ToVector2(), image);
        }
    }
}
