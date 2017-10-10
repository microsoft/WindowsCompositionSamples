//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CompositionSampleGallery.Converters
{
    /// <summary>
    /// Appends a string to a given value.
    /// </summary>
    public class StringAppenderConverter : IValueConverter
    {
        /// <summary>
        /// Returns value.ToString() + " " + parameter.ToString().
        /// </summary>
        /// <param name="value">The value to be appended.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">The string to append.</param>
        /// <param name="language">Unused.</param>
        /// <returns>String</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = "";
            if (value != null)
            {
                result = value.ToString();
                if (parameter != null)
                {
                    result += " " + parameter.ToString();
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
