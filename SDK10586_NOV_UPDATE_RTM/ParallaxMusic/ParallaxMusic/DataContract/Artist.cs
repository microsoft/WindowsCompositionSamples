/*
Copyright (c) Microsoft Corporation 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ParallaxMusic.DataContract
{
    public class Artist
    {
        public Artist(JsonObject jsonObject)
        {
            _artist = jsonObject["Artists"].GetObject()["Items"].GetArray()[0].GetObject();
            Id = _artist["Id"].GetString();
            Name = _artist["Name"].GetString();
            Biography = _artist["Biography"].GetString();
            ImageUrl = _artist["ImageUrl"].GetString();
            
            // Covert the JsonArray of Albums to a List of AlbumContracts
            _albums = new List<Album>();

            JsonArray jsonAlbums = _artist["Albums"].GetObject()["Items"].GetArray();
            for(int i = 0; i < jsonAlbums.Count; i++)
            {
                _albums.Add(new Album(jsonAlbums[i].GetObject()));
            }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Biography { get; set; }
        public string ImageUrl { get; set; }

        public List<Album> Albums
        {
            get
            {
                return _albums;
            }
            set
            {
                if(value != _albums)
                {
                    _albums = value;
                }
            }
        }

        private JsonObject _artist;
        private List<Album> _albums;
    }
}
