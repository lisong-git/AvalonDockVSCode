using AvalonDock.Layout;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls.DropTargets {

	/// <summary>
	/// Implements a <see cref="LayoutAnchorableControl"/> drop target
	/// on which other items (<see cref="LayoutPaneComposite"/>) can be dropped.
	/// </summary>
	internal class AnchorablePaneCompositeDropTarget :DropTarget<LayoutPaneCompositeControl> {

		#region fields

		private LayoutPaneCompositeControl _targetPane;
		private int _tabIndex = -1;

		#endregion fields

		#region Overrides

		/// <summary>
		/// Class constructor from parameters without a specific tabindex as dock position.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal AnchorablePaneCompositeDropTarget(LayoutPaneCompositeControl paneControl,
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
		internal AnchorablePaneCompositeDropTarget(LayoutPaneCompositeControl paneControl,
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
			Debug.WriteLine($"{Type}, {_tabIndex}", $"{nameof(AnchorablePaneCompositeDropTarget)} Drop 1");

			//LayoutAnchorable anchorableActive = floatingWindow.Descendents().OfType<LayoutAnchorable>().FirstOrDefault();
			switch (Type) {
				case DropTargetType.AnchorableExpanderDockInside:
					#region DropTargetType.AnchorablePaneDockInside
					{
						var targetModel = _targetPane.Model as ILayoutGroup;
						//var parent = targetModel.Parent as ILayoutGroup;
						var paneModel = floatingWindow.RootPanel;

						foreach (var anchorableToImport in
							paneModel.Descendents().OfType<LayoutAnchorable>().ToArray()) {
							targetModel.InsertChildAt(targetModel.ChildrenCount, anchorableToImport);
						}
					}
					//anchorableActive.IsActive = true;

					break;

					#endregion DropTargetType.AnchorablePaneDockInside
			}


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
			var targetModel = _targetPane.Model as LayoutPaneComposite;
			var orientableGroup = targetModel as ILayoutOrientableGroup;
			var orientable = orientableGroup.Orientation;
			Debug.WriteLine($"{Type}, {orientable}, {_targetPane.Model?.GetType().Name}", $"{nameof(AnchorablePaneCompositeDropTarget)} GetPreviewPath");
			if(orientable == System.Windows.Controls.Orientation.Vertical) {
				switch(Type) {
					case DropTargetType.AnchorableExpanderDockInside: {
							var targetScreenRect = DetectionRects.FirstOrDefault();
							targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
							return new RectangleGeometry(targetScreenRect);
						}
				}
			} else {
				switch(Type) {
					case DropTargetType.AnchorableExpanderDockInside: {
							var targetScreenRect = DetectionRects.FirstOrDefault();
							targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

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