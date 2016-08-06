using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CompositionSampleGallery.Converters
{
    /// <summary>
    /// Picks between two strings depending on the input bool.
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        /// <summary>
        /// Takes a bool and returns a string depending on the value of the bool. This converter
        /// requires the parameter to be a string that is delimited by a ';'. The string is split
        /// on the ';' and returns the first token if the value is true. If the value is false, the
        /// second token is used.
        /// </summary>
        /// <param name="value">A bool to be evaluated/converted.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">A string that is delimited by a ';'.</param>
        /// <param name="language">Unused.</param>
        /// <returns>String</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bvalue = (bool)value;
            var parts = parameter.ToString().Split(';');
            return bvalue ? parts[0] : parts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
