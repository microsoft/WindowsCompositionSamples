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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery
{
    public sealed partial class FlipViewIndicator : UserControl
    {
        private FlipViewModel _model;
        private bool _ProgressFlipView = true;
        public FlipViewModel Model
        {
            get { return _model; }
            set { if (value != null) _model = value; }
        }

        public FlipViewIndicator()
        {
            this.InitializeComponent();

            _model = new FlipViewModel();
#if SDKVERSION_14393
            SampleDefinition item1 = SampleDefinitions.Definitions.Where(x => x.Type == typeof(Interactions3D)).FirstOrDefault();
            SampleDefinition item2 = SampleDefinitions.Definitions.Where(x => x.Type == typeof(PullToAnimate)).FirstOrDefault();
            _model.FlipViewItems.Add(new FeaturedFlipViewSample("Interaction Tracker 3D", "", "/Assets/BannerImages/IneractionTrackerBanner.png", item1));
            _model.FlipViewItems.Add(new FeaturedFlipViewSample("Create custom resting points with animation", "", "/Assets/BannerImages/PullToAnimateBanner.PNG", item2));
            this.DataContext = _model;
#endif

            // Automatically have the FlipView progress to the next item
            if (Model.FlipViewItems.Count() > 1)
            {
                Task t = ProgressFlipView();
            }
        }

        public async Task ProgressFlipView()
        {
            while (_ProgressFlipView)
            {
                await Task.Delay(3000);
                if (_ProgressFlipView)
                {
                    BannerFlipView.SelectedIndex = (BannerFlipView.SelectedIndex + 1) % Model.FlipViewItems.Count;
                }
            }
        }

        private void BannerFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Model.Selected = ((FlipView)sender).SelectedItem as FeaturedFlipViewSample;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FeaturedFlipViewSample SelectedSample = BannerFlipView.SelectedItem as FeaturedFlipViewSample;
            MainPage.Instance.NavigateToPage(typeof(SampleHost), SelectedSample.SampleDefinition);
        }

        private void IndicatorClick(object sender, RoutedEventArgs e)
        {
            // find the index of the clicked item and jump the flip view to that index
            FeaturedFlipViewSample clickedFeaturedSample = (sender as Button).DataContext as FeaturedFlipViewSample;
            int index = Model.FlipViewItems.IndexOf(clickedFeaturedSample);
            if (index >= 0 && index <= Model.FlipViewItems.Count())
            {
                _ProgressFlipView = false;
                BannerFlipView.SelectedIndex = index;
            }
        }
    }

    public class FlipViewModel
    {
        private FeaturedFlipViewSample _selected;
        private ObservableCollection<FeaturedFlipViewSample> _flipViewItems = new ObservableCollection<FeaturedFlipViewSample>();

        public FlipViewModel() { }

        public ObservableCollection<FeaturedFlipViewSample> FlipViewItems
        {
            get
            {
                return _flipViewItems;
            }
        }

        public FeaturedFlipViewSample Selected
        {
            get { return _selected; }
            set
            {
                if (value != null)
                {
                    foreach (var item in _flipViewItems.Where(x => x.Selected))
                    {
                        item.Selected = false;
                    }

                    _selected = value;
                    _selected.Selected = true;
                }
            }
        }
    }

    public class FeaturedFlipViewSample : INotifyPropertyChanged
    {
        private string _title;
        private string _Description;
        private string _navigationUrl;
        private string _backgroundImage;
        private SampleDefinition _sampleDefinition;
        private bool _selected = default(bool);
        public event PropertyChangedEventHandler PropertyChanged;

        public FeaturedFlipViewSample(string title, string description, string backgroundImageUrl, SampleDefinition sampleDefinition = null, string navigationUrl = null)
        {
            _title = title;
            _Description = description;
            _navigationUrl = navigationUrl;
            _backgroundImage = backgroundImageUrl;
            _sampleDefinition = sampleDefinition;
        }

        public string Title { get { return _title; } }
        public string Description { get { return _Description; } }
        public string NavigationUrl { get { return _navigationUrl; } }
        public string BackgroundImage { get { return _backgroundImage; } }
        public SampleDefinition SampleDefinition { get { return _sampleDefinition; } }
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                this.OnPropertyChanged("Selected");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    public class SelectedToSolidColorBrushValueConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String language)
        {
            bool selected = (bool)value;
            if (selected)
            {
                return new SolidColorBrush(Colors.White);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, String language)
        {
            if(value != null)
            {
                SolidColorBrush sbc = value as SolidColorBrush;
                if (sbc.Color == Colors.White)
                    return true;
            }
            return false;
        }
    }
}
