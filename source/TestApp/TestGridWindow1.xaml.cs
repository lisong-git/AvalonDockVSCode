using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestApp {
	/// <summary>
	/// TestGridWindow1.xaml 的交互逻辑
	/// </summary>
	public partial class TestGridWindow1 :Window {
		public TestGridWindow1() {
			InitializeComponent();

			var testGrid = new TestGrid();
			Grid.SetRowSpan(testGrid, 2);
			Grid.SetColumn(testGrid, 0);
			grid.Children.Add(testGrid);
			testGrid.UseLayoutRounding = false;
		}
	}
}
