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
            Landscapes = new ObservableCollection<Thumbnail>();
            Landscapes.Add(new Thumbnail("Rocky Mountains",     PREFIX_URL_LANDSCAPE + "Landscape-1.jpg",   "Landscape shot of mountains in rocky terrain"));
            Landscapes.Add(new Thumbnail("Sunny Landscape",     PREFIX_URL_LANDSCAPE + "Landscape-2.jpg",   "Picturesque scene with the sun high in the sky"));
            Landscapes.Add(new Thumbnail("Mountain Road",       PREFIX_URL_LANDSCAPE + "Landscape-3.jpg",   "Winding road through a mountain pass"));
            Landscapes.Add(new Thumbnail("Harvest",             PREFIX_URL_LANDSCAPE + "Landscape-4.jpg",   "Corn stalks on a clear day"));
            Landscapes.Add(new Thumbnail("Rock Formation",      PREFIX_URL_LANDSCAPE + "Landscape-5.jpg",   "Unique rock formation off of a mountain"));
            Landscapes.Add(new Thumbnail("At Sea",              PREFIX_URL_LANDSCAPE + "Landscape-6.jpg",   "Sunset over the water"));
            Landscapes.Add(new Thumbnail("Snowy Mountain",      PREFIX_URL_LANDSCAPE + "Landscape-7.jpg",   "A snowy mountain framed by pine trees"));
            Landscapes.Add(new Thumbnail("Sea to Sky",          PREFIX_URL_LANDSCAPE + "Landscape-8.jpg",   "A lake framed by mountains and pine trees"));
            Landscapes.Add(new Thumbnail("On the Beach",        PREFIX_URL_LANDSCAPE + "Landscape-9.jpg",   "Shot of the beach with greenery"));
            Landscapes.Add(new Thumbnail("Lush Mountains",      PREFIX_URL_LANDSCAPE + "Landscape-10.jpg",  "Landscape shot of mountains in the forrest"));
            Landscapes.Add(new Thumbnail("White Dunes",         PREFIX_URL_LANDSCAPE + "Landscape-11.jpg",  "White sand dunes and a clear sky"));
            Landscapes.Add(new Thumbnail("Dunes with Tracks",   PREFIX_URL_LANDSCAPE + "Landscape-12.jpg",  "Sand dunes after driving on an ATV"));
            Landscapes.Add(new Thumbnail("Shadowed Dunes",      PREFIX_URL_LANDSCAPE + "Landscape-13.jpg",  "Sand dunes casting a shadow"));

            Abstract = new ObservableCollection<Thumbnail>();
            Abstract.Add(new Thumbnail("Pink Bubbles",          PREFIX_URL_ABSTRACT + "Abstract-1.jpg",     "A macro shot of bubbles with a pink background"));
            Abstract.Add(new Thumbnail("Blue Bubbles",          PREFIX_URL_ABSTRACT + "Abstract-2.jpg",     "A macro shot of bubbles with a blue background"));
            Abstract.Add(new Thumbnail("Orange Bubbles",        PREFIX_URL_ABSTRACT + "Abstract-3.jpg",     "A portrait macro shot orange bubbles"));
            Abstract.Add(new Thumbnail("Green Bubbles",         PREFIX_URL_ABSTRACT + "Abstract-4.jpg",     "A macro shot of green oil bubbles"));
            Abstract.Add(new Thumbnail("Drop",                  PREFIX_URL_ABSTRACT + "Abstract-5.jpg",     "A macro shot of a droplet of water against nature"));
            Abstract.Add(new Thumbnail("Petals",                PREFIX_URL_ABSTRACT + "Abstract-6.jpg",     "A close up shot of flower petals"));
            Abstract.Add(new Thumbnail("Up Close",              PREFIX_URL_ABSTRACT + "Abstract-7.jpg",     "A zoomed in shot of the center of a flower"));

            Nature = new ObservableCollection<Thumbnail>();
            Nature.Add(new Thumbnail("Cardoon",                 PREFIX_URL_NATURE + "Nature-1.jpg",         "Close up shot of a purple cardoon"));
            Nature.Add(new Thumbnail("Meadow",                  PREFIX_URL_NATURE + "Nature-2.jpg",         "Purple flowers in a meadow"));
            Nature.Add(new Thumbnail("Pink Flower",             PREFIX_URL_NATURE + "Nature-3.jpg",         "A close up shot of a unique pink and yellow flower"));
            Nature.Add(new Thumbnail("Red Flowers",             PREFIX_URL_NATURE + "Nature-4.jpg",         "A close up shot of a red flower amid a flower patch"));
            Nature.Add(new Thumbnail("Dahlia",                  PREFIX_URL_NATURE + "Nature-5.jpg",         "A pink dahlia on a window sill"));
            Nature.Add(new Thumbnail("Petals",                  PREFIX_URL_NATURE + "Nature-6.jpg",         "A shot focused on the petals of a pink flower"));
            Nature.Add(new Thumbnail("Cynthia",                 PREFIX_URL_NATURE + "Nature-7.jpg",         "Cynthia butterfly landing on a flower"));
            Nature.Add(new Thumbnail("Painted Lady",            PREFIX_URL_NATURE + "Nature-8.jpg",         "Cynthia butterfly showing its painted lady wings"));
            Nature.Add(new Thumbnail("Macro Snail",             PREFIX_URL_NATURE + "Nature-9.jpg",         "A macro shot of a snail in the grass"));
            Nature.Add(new Thumbnail("Snail",                   PREFIX_URL_NATURE + "Nature-10.jpg",        "A curious snail raising his head to take a look around"));
            Nature.Add(new Thumbnail("Mushroom",                PREFIX_URL_NATURE + "Nature-11.jpg",        "A small mushroom coming out for spring"));
            Nature.Add(new Thumbnail("Japanese Macaques",       PREFIX_URL_NATURE + "Nature-12.jpg",        "Two japanese macaque monkeys take care of each other"));
            Nature.Add(new Thumbnail("Bird Calls",              PREFIX_URL_NATURE + "Nature-13.jpg",        "A bird calls out looking for its family"));
        }

        public ObservableCollection<Thumbnail> Landscapes
        {
            get; set;
        }

        public ObservableCollection<Thumbnail> Abstract
        {
            get; set;
        }
        public ObservableCollection<Thumbnail> Nature
        {
            get; set;
        }

        public ObservableCollection<Thumbnail> AggregateDataSources(ObservableCollection<Thumbnail>[] sources)
        {
            ObservableCollection<Thumbnail> items = new ObservableCollection<Thumbnail>();
            foreach(ObservableCollection<Thumbnail> list in sources)
            {
                foreach(Thumbnail thumbnail in list)
                {
                    items.Add(thumbnail);
                }
            }

            return RandomizeDataSource(items);
        }

        public static ObservableCollection<Thumbnail> RandomizeDataSource(ObservableCollection<Thumbnail> list)
        {
            Random rng = new Random();
            for (int i = list.Count-1; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                Thumbnail tmp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = tmp;
            }

            return list;
        }

        private static readonly string PREFIX_URL_LANDSCAPE = "ms-appx:///Assets/Landscapes/";
        private static readonly string PREFIX_URL_NATURE = "ms-appx:///Assets/Nature/";
        private static readonly string PREFIX_URL_ABSTRACT = "ms-appx:///Assets/Abstract/";
    }

}
