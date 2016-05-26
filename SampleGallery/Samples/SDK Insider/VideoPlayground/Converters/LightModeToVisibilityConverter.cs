using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CompositionSampleGallery.Converters
{
    /// <summary>
    /// Converts a LightMode to a Visibility enum.
    /// </summary>
    public class LightModeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Returns Visibility.Visible if the value matches the parameter. Returns Visibility.Collapsed
        /// otherwise.
        /// </summary>
        /// <param name="value">The LightMode to convert.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">The string value of the LightMode value checked against.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Visibility</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            LightMode lightMode = (LightMode)value;
            String refMode = parameter.ToString().Trim();
            bool match = refMode.Equals(lightMode.ToString());
            return match ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
