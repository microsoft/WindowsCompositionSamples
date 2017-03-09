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

        public Thumbnail(string name, string url, string description) : this(name, url, description, null)
        {
        }

        public Thumbnail(string name, string url, string description, string details)
        {
            Name = name;
            ImageUrl = url;
            Description = description;
            Details = details;
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

        public string Details
        {
            get; set;
        }
    }

    public class LocalDataSource
    {
        public LocalDataSource()
        {
            Items = new ObservableCollection<Thumbnail>();
            Items.Add(new Thumbnail("Landscape 1",  PREFIX_URL_LANDSCAPE + "Landscape-1.jpg",   "24mm f/4.0 1/2500 ISO 200"));
            Items.Add(new Thumbnail("Landscape 2",  PREFIX_URL_LANDSCAPE + "Landscape-2.jpg",   "24mm f/8.0 1/2000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 3",  PREFIX_URL_LANDSCAPE + "Landscape-3.jpg",   "24mm f/8.0 1/640 ISO 100"));
            Items.Add(new Thumbnail("Landscape 4",  PREFIX_URL_LANDSCAPE + "Landscape-4.jpg",   "70mm f/2.8 1/8000 ISO 400"));
            Items.Add(new Thumbnail("Landscape 5",  PREFIX_URL_LANDSCAPE + "Landscape-5.jpg",   "70mm f/2.8 1/5000 ISO 400"));
            Items.Add(new Thumbnail("Landscape 6",  PREFIX_URL_LANDSCAPE + "Landscape-6.jpg",   "70mm f/2.8 1/1250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 7",  PREFIX_URL_LANDSCAPE + "Landscape-7.jpg",   "70mm f/4.0 1/250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 8",  PREFIX_URL_LANDSCAPE + "Landscape-8.jpg",   "70mm f/2.8 1/1250 ISO 100"));
            Items.Add(new Thumbnail("Landscape 9",  PREFIX_URL_LANDSCAPE + "Landscape-9.jpg",   "70mm f/2.8 1/2000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 10", PREFIX_URL_LANDSCAPE + "Landscape-10.jpg",  "24mm f/4.0 1/1000 ISO 100"));
            Items.Add(new Thumbnail("Landscape 11", PREFIX_URL_LANDSCAPE + "Landscape-11.jpg",  "50mm f/16 1/125 ISO 100"));
            Items.Add(new Thumbnail("Landscape 12", PREFIX_URL_LANDSCAPE + "Landscape-12.jpg",  "24mm f/4.0 1/250 ISO 100"));

            Cities = new ObservableCollection<Thumbnail>();
            Cities.Add(new Thumbnail("Athens",      PREFIX_URL_CITY + "athens.jpg",     "Acropolis",            "The Acropolis is an ancient citadel containing the remains of ancient buildings, such as the Parthenon."));
            Cities.Add(new Thumbnail("Barcelona",   PREFIX_URL_CITY + "barcelona.jpg",  "Sagrada Familia",      "Sagrada Familia is a large church in Barcelona designed by Antoni Gaudi."));
            Cities.Add(new Thumbnail("Berlin",      PREFIX_URL_CITY + "berlin.jpg",     "Brandenburg Gate",     "This neoclassical monument in Berlin stands as a symbol of unity and peace."));
            Cities.Add(new Thumbnail("Brussels",    PREFIX_URL_CITY + "brussels.jpg",   "Atomium",              "This structure in Brussels connects spheres so that it matches the shape of an iron crystal."));
            Cities.Add(new Thumbnail("Copenhagen",  PREFIX_URL_CITY + "copenhagen.jpg", "Nyhavn",               "Nyhavn is a 17th century waterfront canal and entertainment district."));
            Cities.Add(new Thumbnail("Dubrovnik",   PREFIX_URL_CITY + "dubrovnik.jpg",  "Old Town",             "Dubrovnik is a Croatian city on the Adriatic Sea in Dalmatia."));
            Cities.Add(new Thumbnail("London",      PREFIX_URL_CITY + "london.jpg",     "Big Ben",              "Big Ben is the nickname of the clock in London's parliament building."));
            Cities.Add(new Thumbnail("Lucerne",     PREFIX_URL_CITY + "lucerne.jpg",    "Chapel Bridge",        "Chapel Bridge is a covered wooden footbridge spanning across Reuss River."));
            Cities.Add(new Thumbnail("Paris",       PREFIX_URL_CITY + "paris.jpg",      "Eiffel Tower",         "The Eiffel Tower is a wrought iron lattice tower on the Champ de Mars in Paris."));
            Cities.Add(new Thumbnail("Prague",      PREFIX_URL_CITY + "prague.jpg",     "Astronomical Clock",   "This clock in Prague Old Town Hall was installed in 1410."));
            Cities.Add(new Thumbnail("Rome",        PREFIX_URL_CITY + "rome.jpg",       "Colosseum",            "The Colosseum is an oval amphitheatre in the center of Rome."));

        }

        public ObservableCollection<Thumbnail> Items
        {
            get; set;
        }

        public ObservableCollection<Thumbnail> Cities
        {
            get; set;
        }

        private static readonly string PREFIX_URL_LANDSCAPE = "ms-appx:///Assets/Landscapes/";
        private static readonly string PREFIX_URL_CITY = "ms-appx:///Assets/Cities/";
    }

}
