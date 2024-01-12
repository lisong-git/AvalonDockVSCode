namespace AvalonDock.MVVMTestApp
{
	using System.Windows.Controls;
	using System.Windows;
	using System.Diagnostics;

	class PanesStyleSelector : StyleSelector
	{
		public Style ToolStyle
		{
			get;
			set;
		}

		public Style FileStyle
		{
			get;
			set;
		}

		public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
		{
<<<<<<< HEAD
			Debug.WriteLine($"{item?.GetType()?.Name}", "SelectStyle");
=======
			Debug.WriteLine($"{item.GetType().Name}", "SelectStyle");
>>>>>>> 4e44adb17b85797821902ce92cc3d7ef9d9cb1cc
			if (item is ToolViewModel)
				return ToolStyle;

			if (item is FileViewModel)
				return FileStyle;

			return base.SelectStyle(item, container);
		}
	}
}
