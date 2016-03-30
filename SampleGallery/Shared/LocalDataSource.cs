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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompositionSampleGallery.Shared
{
    public class Thumbnail
    {
        public Thumbnail()
        {

        }

        public Thumbnail(string name, string url, string description)
        {
            Name = name;
            ImageUrl = url;
            Description = description;
        }

        public string Name
        {
            get; set;
        }

        public string ImageUrl
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }
    }

    public class LocalDataSource
    {
        public LocalDataSource()
        {
            Items = new ObservableCollection<Thumbnail>();
            Items.Add(new Thumbnail("Landscape 1", PREFIX_URL + "Landscape-1.jpg", "24mm f/4.0 1/2500 ISO 200"));
            Items.Add(new Thumbnail("Landscape 2", PREFIX_URL + "Landscape-2.jpg", "24mm f/8.0 1/2000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 3", PREFIX_URL + "Landscape-3.jpg", "24mm f/8.0 1/640 ISO 100"));
            Items.Add(new Thumbnail("Landscape 4", PREFIX_URL + "Landscape-4.jpg", "70mm f/2.8 1/8000 ISO 400"));
            Items.Add(new Thumbnail("Landscape 5", PREFIX_URL + "Landscape-5.jpg", "70mm f/2.8 1/5000 ISO 400"));
            Items.Add(new Thumbnail("Landscape 6", PREFIX_URL + "Landscape-6.jpg", "70mm f/2.8 1/1250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 7", PREFIX_URL + "Landscape-7.jpg", "70mm f/4.0 1/250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 8", PREFIX_URL + "Landscape-8.jpg", "70mm f/2.8 1/1250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 9", PREFIX_URL + "Landscape-9.jpg", "70mm f/2.8 1/2000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 10", PREFIX_URL + "Landscape-10.jpg", "24mm f/4.0 1/1000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 11", PREFIX_URL + "Landscape-11.jpg", "50mm f/16 1/125 ISO 100"));
            Items.Add(new Thumbnail("Landscape 12", PREFIX_URL + "Landscape-12.jpg", "24mm f/4.0 1/250 ISO 100"));
        }

        public ObservableCollection<Thumbnail> Items
        {
            get; set;
        }

        private static readonly string PREFIX_URL = "ms-appx:///Assets/Landscapes/";
    }

}
