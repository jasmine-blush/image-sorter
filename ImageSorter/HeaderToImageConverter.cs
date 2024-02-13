using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ImageSorter
{
    #region HeaderToImageConverter

    [ValueConversion(typeof(string), typeof(bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri uri = new Uri(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Images/folder.png");
            BitmapImage source = new BitmapImage(uri);
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    #endregion // DoubleToIntegerConverter


}