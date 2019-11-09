using System;
using System.Globalization;
using System.Windows.Data;

namespace Baku.VMagicMirrorConfig
{
    public class WhiteSpaceStringToNullConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
        {
            //""をnullにするのがポイント
            string s when string.IsNullOrEmpty(s) => null,
            string s => s,
            _ => Binding.DoNothing,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is string s) ? s : Binding.DoNothing;
        }
    }
}
