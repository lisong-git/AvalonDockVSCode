using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using AvalonDock.Controls;
using System.Linq;

namespace AvalonDock {
	/// <summary>
	/// 为ScrollView内的元素支持鼠标滚轮横向滚动
	/// </summary>
	public static class ScrollViewerHelper {

		public static readonly DependencyProperty WheelScrollsHorizontallyProperty
				= DependencyProperty.RegisterAttached("WheelScrollsHorizontally",
						typeof(bool),
						typeof(ScrollViewerHelper),
						new PropertyMetadata(false, UseHorizontalScrollingChangedCallback));

		private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var element = d as UIElement ?? throw new Exception("Attached property must be used with UIElement.");

			if((bool) e.NewValue)
				element.PreviewMouseWheel += OnPreviewMouseWheel;
			else
				element.PreviewMouseWheel -= OnPreviewMouseWheel;
		}

		private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args) {
			var scrollViewer = ((UIElement)sender).FindVisualChildren<ScrollViewer>().SingleOrDefault();

			if(scrollViewer == null || !scrollViewer.IsMouseOver)
				return;

			if(args.Delta < 0)
				scrollViewer.LineRight();
			else
				scrollViewer.LineLeft();

			args.Handled = true;
		}

		public static void SetWheelScrollsHorizontally(UIElement element, bool value) => element.SetValue(WheelScrollsHorizontallyProperty, value);

		public static bool GetWheelScrollsHorizontally(UIElement element) => (bool) element.GetValue(WheelScrollsHorizontallyProperty);
				
	}
}

