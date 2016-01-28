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
    public class Album
    {
        public Album(JsonObject jsonObject)
        {
            _album = jsonObject;

            Id = _album["Id"].GetString();
            Name = _album["Name"].GetString();
            ImageUrl = _album["ImageUrl"].GetString();

            // If the Album contains tracks, deserialize 
            if (_album.ContainsKey("Tracks") && _album["Tracks"].GetObject() != null)
            {
                _tracks = new List<Track>();
                JsonArray jsonTracks = _album["Tracks"].GetObject()["Items"].GetArray();
                for(int i = 0; i < jsonTracks.Count; i++)
                {
                    _tracks.Add(new Track(jsonTracks[i].GetObject()));
                }
            }

        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public List<Track> Tracks
        {
            get
            {
                return _tracks;
            }
            set
            {
                if(value != _tracks)
                {
                    _tracks = value;
                }
            }
        }

        private JsonObject _album;
        private List<Track> _tracks;
    }
}
