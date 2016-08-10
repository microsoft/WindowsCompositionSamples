using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CompositionSampleGallery.Converters
{
    /// <summary>
    /// Converts a LightMode to a Brush. When the LightMode is set to None, the brush is transparent.
    /// Otherwise the brush is red.
    /// </summary>
    public class LightModeToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Returns a transparent SolidColorBrush if the LightMode is set to None. Otherwise returns
        /// a red SolidColorBrush.
        /// </summary>
        /// <param name="value">The LightMode to be converted.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>SolidColorBrush</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            LightMode lightMode = (LightMode)value;
            Color color = lightMode == LightMode.None ? Colors.Transparent : Colors.Red;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
