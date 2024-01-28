namespace AvalonDock.MVVMTestApp
{
	using System.Diagnostics;
	using System.Linq;
	using System.Windows.Controls;
	using AvalonDock.Layout;

	class LayoutInitializer : ILayoutUpdateStrategy
	{

		//private static string _toolsPane = "ToolsPane";
		private static string _toolsPane = DockingManager.PanelKey;
		public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer) {
			//AD wants to add the anchorable into destinationContainer
			//just for test provide a new anchorablepane 
			//if the pane is floating let the manager go ahead
			LayoutAnchorableGroup destPane = destinationContainer as LayoutAnchorableGroup;
			if (destinationContainer != null &&
				destinationContainer.FindParent<LayoutFloatingWindow>() != null)
				return false;

			var toolsPane = layout.Descendents().OfType<LayoutAnchorableGroupPane>().FirstOrDefault(d => d.Name == _toolsPane);
			Debug.WriteLine($"{toolsPane != null}", "BeforeInsertAnchorable 1");

			if (toolsPane != null) {
				toolsPane.Children.Add(new LayoutAnchorableGroup(anchorableToShow) { Orientation = System.Windows.Controls.Orientation.Horizontal });
				//toolsPane.Children.Add(anchorableToShow);
				return true;
			}
			else {
				var documentPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
				Debug.WriteLine($"{documentPane != null}", "BeforeInsertAnchorable 2");
				if (documentPane != null) {
					var parentPane = documentPane.Parent as LayoutPanel;
					parentPane.Orientation = System.Windows.Controls.Orientation.Vertical;
					var group = new LayoutAnchorableGroup(anchorableToShow) { Orientation = Orientation.Horizontal };
					var groupPane = new LayoutAnchorableGroupPane(group) { Name = _toolsPane };
					parentPane.Children.Add(groupPane);
					return true;
				}
			}

			return false;
		}


		public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
		{
		}


		public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
		{
			return false;
		}

		public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
		{
		}
	}
}
