using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TestApp {
	public class TestGrid :Grid {

		public TestGrid() {
			CreateLayouts();
		}


		private Expander expander1;
		private Expander expander2;
		private Expander expander3;
		private GridSplitter gs1;
		private GridSplitter gs2;
		private RowDefinition row1;
		private RowDefinition row2;
		private RowDefinition row3;
		private RowDefinition row4;
		private RowDefinition row5;

		private void CreateLayouts() {
			var defs = RowDefinitions;
			row1 = new RowDefinition() { Name = "row1", Height = new GridLength(1, GridUnitType.Auto) };
			row2 = new RowDefinition() { Name = "row2", Height = new GridLength(1, GridUnitType.Pixel) };
			row3 = new RowDefinition() { Name = "row3", Height = new GridLength(1, GridUnitType.Auto) };
			row4 = new RowDefinition() { Name = "row4", Height = new GridLength(1, GridUnitType.Pixel) };
			row5 = new RowDefinition() { Name = "row5", Height = new GridLength(1, GridUnitType.Star) };

			defs.Add(row1);
			defs.Add(row2);
			defs.Add(row3);
			defs.Add(row4);
			defs.Add(row5);

			expander1 = CreateExpander("expander1", Colors.Green);
			gs1 = new GridSplitter() { Name = "gs1", HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Colors.Red), };
			expander2 = CreateExpander("expander2", Colors.Blue);
			gs2 = new GridSplitter() { Name = "gs2", HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Colors.Red), };
			expander3 = CreateExpander("expander3", Colors.Green);
			expander3.IsExpanded = true;

			Grid.SetRow(expander1, 0);
			Grid.SetRow(gs1, 1);
			Grid.SetRow(expander2, 2);
			Grid.SetRow(gs2, 3);
			Grid.SetRow(expander3, 4);

			Children.Add(expander1);
			Children.Add(gs1);
			Children.Add(expander2);
			Children.Add(gs2);
			Children.Add(expander3);
		}

		private Expander CreateExpander(string name, Color background) {
			var content = new StackPanel();
			for(var i = 1; i <= 8; i++) {
				var tb = new TextBlock
								{
					Text = $"Content{i}"
				};
				content.Children.Add(tb);
			}

			var layout = new Expander
						{
				Name = name,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = new SolidColorBrush(background),
				Header="Expander Title 1",
			};
			layout.Collapsed += Expander_Collapsed;
			layout.Expanded += Expander_Expanded;
			layout.Content = content;
			return layout;
		}

		//private GridSplitter CreateGridSplitter()

		//private void Button_Click(object sender, RoutedEventArgs e) {
		//	Debug.WriteLine($"111", "TestExplanderWindow_LayoutUpdated");

		//	var rows = RowDefinitions.Select((col, i)=> $"{i}, {col.Height}, {col.ActualHeight}, {col.MinHeight}, {col.MaxHeight}, {col.Offset}");
		//	foreach(var col in rows) {
		//		Debug.WriteLine($"{col}", "TestExplanderWindow_LayoutUpdated");

		//	}
		//	columnListView.ItemsSource = rows;
		//}



		private void Expander_Expanded(object sender, RoutedEventArgs e) {
			var expland = sender as Expander;
			if(expland != null) {
				//var preDef = GetPreSplitter(expland);

				//Debug.WriteLine($"{preDef.ActualHeight}, {preDef.Height}, {preDef.MinHeight}, {preDef.MaxHeight}", "Expander_Expanded 1");
				//var nextDef = GetNextSplitter(expland);
				//Debug.WriteLine($"{nextDef.ActualHeight}, {nextDef.Height}, {nextDef.MinHeight}, {nextDef.MaxHeight}", "Expander_Expanded 1");
				var gridUnityType = GridUnitType.Star;
				switch(expland.Name) {
					case "expander1":
						row1.Height = new GridLength(1, gridUnityType);
						gs1.IsEnabled = expander3.IsExpanded || expander2.IsExpanded;
						break;
					case "expander2":
						row3.Height = new GridLength(1, gridUnityType);
						gs1.IsEnabled = expander1.IsExpanded;
						gs2.IsEnabled = true;
						break;
					case "expander3":
						row5.Height = new GridLength(1, gridUnityType);
						gs2.IsEnabled = expander1.IsExpanded || expander2.IsExpanded;
						break;
					default:
						break;
				}
				Debug.WriteLine($"{expland.ActualHeight}, {FindChildByName(expland, "ExpanderToggleButton")?.ActualHeight}, {((FrameworkElement) expland.Content)?.ActualHeight}", "Expander_Expanded 1");
				//PrintTest();
			}
		}

		private void Expander_Collapsed(object sender, RoutedEventArgs e) {
			var expland = sender as Expander;
			if(expland != null) {
				Debug.WriteLine($"{expland.ActualHeight}, {FindChildByName(expland, "ExpanderToggleButton")?.ActualHeight}, {FindChildByName(expland, "ExpanderToggleButton")?.ActualHeight}, {((FrameworkElement) expland.Content)?.ActualHeight}, {((FrameworkElement) expland.Content)?.Height}", "Expander_Collapsed 1");

				switch(expland.Name) {
					case "expander1":
						row1.Height = new GridLength(0, GridUnitType.Auto);
						gs1.IsEnabled = expander3.IsExpanded || expander2.IsExpanded;
						break;
					case "expander2":
						row3.Height = new GridLength(0, GridUnitType.Auto);
						gs1.IsEnabled = expander1.IsExpanded;
						gs2.IsEnabled = expander3.IsExpanded;
						break;
					case "expander3":
						row5.Height = new GridLength(0, GridUnitType.Auto);
						gs1.IsEnabled = false;
						gs2.IsEnabled = false;

						break;
					default:
						break;
				}
				//PrintTest();
			}

		}

		public FrameworkElement FindChildByName(DependencyObject reference, string name) {
			for(int i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++) {
				DependencyObject childObj = VisualTreeHelper.GetChild(reference, i);
				//if (childObj is FrameworkElement child1)
				//{
				//    Debug.WriteLine($"{child1.GetType()}, {child1.Name}", "FindChildByName 1");

				//}

				// 判断子元素的 xname 是否是要查找的名称
				if(childObj is FrameworkElement child && child.Name == name) {
					// Debug.WriteLine($"{nextDef.ActualHeight}, {nextDef.Height}, {nextDef.MinHeight}, {nextDef.MaxHeight}", "Expander_Expanded 1");
					// 转换为相应的控件类型
					// 找到后退出循环或递归
					return child;
				}
				return FindChildByName(childObj, name);
			}
			return null;
		}

	}

}
