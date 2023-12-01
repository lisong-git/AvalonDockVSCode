using AvalonDock.Layout;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls.DropTargets {

	/// <summary>
	/// Implements a <see cref="LayoutAnchorableControl"/> drop target
	/// on which other items (<see cref="LayoutAnchorableGroup"/>) can be dropped.
	/// </summary>
	internal class AnchorableGroupDropTarget :DropTarget<LayoutAnchorableGroupControl> {

		#region fields

		private LayoutAnchorableGroupControl _targetPane;
		private int _tabIndex = -1;

		#endregion fields



		#region Overrides

		/// <summary>
		/// Class constructor from parameters without a specific tabindex as dock position.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal AnchorableGroupDropTarget(LayoutAnchorableGroupControl paneControl,
																	 Rect detectionRect,
																	 DropTargetType type)
			: base(paneControl, detectionRect, type) {
			_targetPane = paneControl;
		}

		/// <summary>
		/// Class constructor from parameters with a specific tabindex as dock position.
		/// This constructor can be used to drop an anchorable at a specific tab index.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		/// <param name="tabIndex"></paramLayoutAnchorableExpanderGroupControl
		internal AnchorableGroupDropTarget(LayoutAnchorableGroupControl paneControl,
											Rect detectionRect,
											DropTargetType type,
											int tabIndex)
			: base(paneControl, detectionRect, type) {
			_targetPane = paneControl;
			_tabIndex = tabIndex;
		}

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow) {
			Debug.WriteLine($"{Type}, {_tabIndex}", $"{nameof(AnchorableGroupDropTarget)} Drop 1");

			LayoutAnchorable targetModel = _targetPane.Model as LayoutAnchorable;
			LayoutAnchorable anchorableActive = floatingWindow.Descendents().OfType<LayoutAnchorable>().FirstOrDefault();
			switch(Type) {
				case DropTargetType.AnchorableExpanderDockBottom:

					#region DropTargetType.AnchorableExpanderPaneDockBottom

					{
						var expanderGroup = targetModel.Parent as ILayoutGroup;
						int insertToIndex = expanderGroup.IndexOfChild(targetModel);

						LayoutAnchorableGroupPane paneModel = floatingWindow.RootPanel;
						Debug.WriteLine($"{paneModel?.Children.Count}", $"{nameof(AnchorableGroupDropTarget)} Drop 2");
						if(paneModel != null &&
							(paneModel.Children.Count == 1 ||
								paneModel.Orientation == System.Windows.Controls.Orientation.Vertical)) {
							var anchorablesToMove = paneModel.Children.ToArray();
							for(int i = 0; i < anchorablesToMove.Length; i++) {
								//Debug.WriteLine($"{anchorablesToMove[i].Children.FirstOrDefault()}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 3");
								Debug.WriteLine($"{expanderGroup.ChildrenCount}, {expanderGroup.GetType()}", $"{nameof(AnchorableGroupDropTarget)} Drop 3");

								var temp = anchorablesToMove[i];
								//expanderGroup.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
								expanderGroup.InsertChildAt(insertToIndex + 1 + i, temp);
								Debug.WriteLine($"{expanderGroup.ChildrenCount}", $"{nameof(AnchorableGroupDropTarget)} Drop 31");
							}
						} else {
							Debug.WriteLine($"", $"{nameof(AnchorableGroupDropTarget)} Drop 4");
							expanderGroup.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
						}
					}
					break;

					#endregion DropTargetType.AnchorableExpanderPaneDockBottom

					//case DropTargetType.AnchorablePaneDockInside:

					//	#region DropTargetType.AnchorablePaneDockInside

					//	//{
					//	//	var paneModel = targetModel as LayoutAnchorablePane;
					//	//	var paneModel = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

					//	//	int i = _index == -1 ? 0 : _index;
					//	//	foreach(var anchorableToImport in
					//	//		paneModel.Descendents().OfType<LayoutAnchorable>().ToArray()) {
					//	//		paneModel.Children.Insert(i, anchorableToImport);
					//	//		i++;
					//	//	}
					//	//}
					//	break;

					//	#endregion DropTargetType.AnchorablePaneDockInside
			}

			anchorableActive.IsActive = true;

			base.Drop(floatingWindow);
		}

		/// <summary>
		/// Gets a <see cref="Geometry"/> that is used to highlight/preview the docking position
		/// of this drop target for a <paramref name="floatingWindowModel"/> being docked inside an
		/// <paramref name="overlayWindow"/>.
		/// </summary>
		/// <param name="overlayWindow"></param>
		/// <param name="floatingWindowModel"></param>
		/// <retur ns>The geometry of the preview/highlighting WPF figure path.</returns>
		public override Geometry GetPreviewPath(OverlayWindow overlayWindow,
												LayoutFloatingWindow floatingWindowModel) {
			var targetModel = _targetPane.Model as LayoutAnchorableGroup;
			var orientableGroup = targetModel as ILayoutOrientableGroup;
			var orientable = orientableGroup.Orientation;
			Debug.WriteLine($"{Type}, {orientable}, {_targetPane.Model?.GetType().Name}", $"{nameof(AnchorableGroupDropTarget)} GetPreviewPath");
			if(orientable == System.Windows.Controls.Orientation.Vertical) {
				switch(Type) {
					case DropTargetType.AnchorableExpanderDockInside: {
							var targetScreenRect = DetectionRects.FirstOrDefault();
							//var targetScreenRect = TargetElement.GetScreenArea();
							targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
							//targetScreenRect.Offset(0, -10);
							//targetScreenRect.Offset(targetScreenRect.Width / 2.0, 0.0);

							//detectionRect
							//if(targetModel.IsExpanded) {
							//Debug.WriteLine($"{overlayWindow}; {targetScreenRect}; {DetectionRects.FirstOrDefault()}", $"{nameof(AnchorableExpanderGroupDropTarget)} GetPreviewPath 2");

							//targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top + targetScreenRect.Height/2);
							//targetScreenRect.Height /= 2.0;
							//} else {
							//	targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top + targetScreenRect.Height - 2);
							//	targetScreenRect.Height = 4.0;
							//}
							//Debug.WriteLine($"{targetScreenRect.Left}, {-overlayWindow.Left}", $"{nameof(AnchorableExpanderGroupDropTarget)} GetPreviewPath 2");

							return new RectangleGeometry(targetScreenRect);
						}
				}
			} else {
				switch(Type) {
					case DropTargetType.AnchorableExpanderDockInside: {
							var targetScreenRect = DetectionRects.FirstOrDefault();
							//if(targetModel.IsExpanded) {
							targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
							//targetScreenRect.Width /= 2.0;
							//} else {
							//	targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
							//	targetScreenRect.Width = 4.0;
							//}
							return new RectangleGeometry(targetScreenRect);
						}
				}
			}

			//return null;
			return new RectangleGeometry(DetectionRects.FirstOrDefault());
		}

		#endregion Overrides
	}
}