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

using ParallaxMusic.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace ParallaxMusic.Data
{
    class RemoteDataSource : IDataSource
    {
        private static readonly string CLIENT_ID = "[CLIENT_ID]";
        private static readonly string CLIENT_SECRET = "[CLIENT_SECRET]";

        public string Token
        {
            get; set;
        }

        public RemoteDataSource()
        {

        }

        public async Task<bool> Initialize()
        {
            _client = new HttpClient();

            // Define the data needed to request an authorization token.
            var service = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            var scope = "http://music.xboxlive.com";
            var grantType = "client_credentials";

            // Create the request data.
            var requestData = new Dictionary<string, string>();
            requestData["client_id"] = CLIENT_ID;
            requestData["client_secret"] = CLIENT_SECRET;
            requestData["scope"] = scope;
            requestData["grant_type"] = grantType;

            // Post the request and retrieve the response.
            string token = null;
            var response = await _client.PostAsync(new Uri(service), new HttpFormUrlEncodedContent(requestData));
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                token = Regex.Match(responseString, ".*\"access_token\":\"(.*?)\".*", RegexOptions.IgnoreCase).Groups[1].Value;
            }
            else
            {
                await new MessageDialog("Authentication failed. Please provide a valid client.").ShowAsync();
                App.Current.Exit();
            }
            
            Token = token;
            return (token != null) ? true : false;
        }

        public async Task<string> GetArtistStringAsync(string id)
        {
            return await LookupItem(id);
        }

        public async Task<Artist> GetArtistAsync(string id)
        {
            Artist retVal = null;
            var content = await GetArtistStringAsync(id);
            if(content != null)
            {
                JsonObject jsonObject = JsonObject.Parse(content);
                retVal = new Artist(jsonObject);
            }

            return retVal;
        }

        public async Task<string> GetAlbumStringAsync(string id)
        {
            return await LookupItem(id);
        }

        public async Task<Album> GetAlbumAsync(string id)
        {
            Album retVal = null;
            var content = await GetAlbumStringAsync(id);
            if (content != null)
            {
                JsonObject jsonObject = JsonObject.Parse(content);

                // Select the Album out of results
                jsonObject = jsonObject["Albums"].GetObject()["Items"].GetArray()[0].GetObject();

                retVal = new Album(jsonObject);
            }

            return retVal;
        }

        private async Task<string> LookupItem(string id)
        {
            var service = SERVICE + id + "/" + LOOKUP + "?extras=Albums+TopTracks+ArtistDetails+Tracks+AlbumDetails" + "&accessToken=Bearer+" + WebUtility.UrlEncode(Token);
            var response = await _client.GetAsync(new Uri(service));
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private static readonly string SERVICE = "https://music.xboxlive.com/1/content/";
        private static readonly string LOOKUP = "lookup";

        private HttpClient _client;
    }
}
