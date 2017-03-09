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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq;

namespace CompositionSampleGallery
{
    public class FeaturedSample
    {
        string _name;
        string _description;
        string _imageUrl;

        public FeaturedSample(string name, string description, string imageUrl)
        {
            _name = name;
            _description = description;
            _imageUrl = imageUrl;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
        }
    }

    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();

            // Specify "New Samples"
            var result = from sampleDef in SampleDefinitions.Definitions
                         orderby sampleDef.DateAdded descending
                         select sampleDef;

            NewSamples.ItemsSource = result.Take(5);

#if SDKVERSION_14393

            var featuredSamples = from sampleDef in SampleDefinitions.Definitions
                                  where (sampleDef.Type == typeof(ThumbnailLighting))
                                  || (sampleDef.Type == typeof(PullToAnimate))
                                  || (sampleDef.Type == typeof(ShadowPlayground))
                                  || (sampleDef.Type == typeof(ConnectedAnimationShell))
                                  select sampleDef;

#else
            var featuredSamples = from sampleDef in SampleDefinitions.Definitions
                                  where (sampleDef.Type == typeof(ColorBloomTransition))
                                  || (sampleDef.Type == typeof(ConnectedAnimationShell))
                                  || (sampleDef.Type == typeof(ZoomWithPerspective))
                                  select sampleDef;            
#endif
            FeaturedSampleList.ItemsSource = featuredSamples;
        }
    }
}
