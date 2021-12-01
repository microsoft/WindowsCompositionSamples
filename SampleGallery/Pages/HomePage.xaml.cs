﻿//*********************************************************
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

using System.Diagnostics;
using System.Linq;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            var featuredSamples = from sampleDef in SampleDefinitions.Definitions
                                  where (sampleDef.Type == typeof(ColorBloomTransition))
                                  || (sampleDef.Type == typeof(ConnectedAnimationShell))
                                  || (sampleDef.Type == typeof(ZoomWithPerspective))
                                  || (sampleDef.Type == typeof(LayerVisualAnd3DTransform))
                                  select sampleDef;

            FeaturedSampleList.ItemsSource = featuredSamples;
        }
    }
}
