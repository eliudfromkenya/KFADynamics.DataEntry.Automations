using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data.Converters;

namespace Avalonia.Themes.KFADynamics.Converters {
    internal class WrapContentIntoContentPresenterConverter : IValueConverter {
        public static WrapContentIntoContentPresenterConverter Instance { get; } = new WrapContentIntoContentPresenterConverter();
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is Control ? value : new ContentPresenter() { Content = value };

        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}