using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace AvalonDock.Converters {

	public class IntToBoolConverter :IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return value.Equals(false) ? DependencyProperty.UnsetValue : parameter;
		}
	}
}
