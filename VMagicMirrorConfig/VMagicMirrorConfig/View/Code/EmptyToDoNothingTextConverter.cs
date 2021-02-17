﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Baku.VMagicMirrorConfig
{
    public class EmptyToDoNothingTextConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value switch
            {
                string s when string.IsNullOrEmpty(s) => LocalizedString.GetString("CommonUi_DoNothing"),
                string s => s,
                _ => Binding.DoNothing,
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)  =>  Binding.DoNothing;
    }
}
