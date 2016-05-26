using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CompositionSampleGallery.Converters
{
    /// <summary>
    /// Inverts a given bool value.
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        /// <summary>
        /// Inverts a given bool value.
        /// </summary>
        /// <param name="value">Bool to be inverted.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>bool</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bvalue = (bool)value;
            return !bvalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
