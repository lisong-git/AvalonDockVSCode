using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows;
using AvalonDock.Layout;
using System.Windows.Markup;
using System.Diagnostics;

namespace AvalonDock.Converters {

	// 实现 IValueConverter 接口的类
	public class ActivityBarActiveConverter :MarkupExtension,IValueConverter {
		// 将整数转换为布尔值，如果整数等于参数，则返回 true，否则返回 false
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Debug.WriteLine($"{value?.GetType()}, {targetType.Name}, {parameter?.GetType()}", "ActivityBarActiveConverter 1");
			if(parameter is LayoutPaneComposite model) {
			 var p =	model.Parent as LayoutPaneCompositePart; 
				if(p.SelectedItem == model) {
					p.SetVisible(!p.IsVisible);
				} else {
					var index = p.IndexOf(model);
					p.SelectedIndex = index;
					p.SetVisible(true);
				}
			}
			//Debug.WriteLine()
			return true;
		}

		// 将布尔值转换为整数，如果布尔值为 false，则返回 DependencyProperty.UnsetValue，否则返回参数
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			Debug.WriteLine($"{value?.GetType()}, {targetType.Name}, {parameter?.GetType()}", "ActivityBarActiveConverter 2");

			return value.Equals(false) ? DependencyProperty.UnsetValue : parameter;
		}

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return ConverterCreater.Get<ActivityBarActiveConverter>();
		}
	}
}
