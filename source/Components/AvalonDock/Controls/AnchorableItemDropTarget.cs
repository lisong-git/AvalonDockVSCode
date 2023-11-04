/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls {
	/// <summary>
	/// Implements a <see cref="LayoutAnchorableExpanderControl"/> drop target
	/// on which other items (<see cref="LayoutAnchorableExpanderGroup"/>) can be dropped.
	/// </summary>
	internal class AnchorableItemDropTarget :DropTarget<LayoutAnchorableTabItem> {
		#region fields

		private LayoutAnchorableTabItem _targetPane;
		private int _tabIndex = -1;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor from parameters without a specific tabindex as dock position.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal AnchorableItemDropTarget(LayoutAnchorableTabItem paneControl,
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
		/// <param name="tabIndex"></paramLayoutAnchorableExpanderGroupPaneControl
		internal AnchorableItemDropTarget(LayoutAnchorableTabItem paneControl,
											Rect detectionRect,
											DropTargetType type,
											int tabIndex)
			: base(paneControl, detectionRect, type) {
			_targetPane = paneControl;
			_tabIndex = tabIndex;
		}

		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow) {
			Debug.WriteLine($"{Type}", $"AnchorableItemDropTarget Drop 1");

			LayoutAnchorableExpanderGroup targetModel = _targetPane.Model as LayoutAnchorableExpanderGroup;
			//LayoutAnchorable anchorableActive = floatingWindow.Descendents().OfType<LayoutAnchorable>().FirstOrDefault();
			switch(Type) {
				case DropTargetType.AnchorablePaneDockLeft:
					#region DropTargetType.AnchorablePaneDockLeft

					{
						var targetGroup = targetModel.Parent as ILayoutGroup;
						int insertToIndex = targetGroup.IndexOfChild(targetModel);

						LayoutAnchorableExpanderGroup layoutAnchorablePaneGroup = floatingWindow.RootPanel;
						Debug.WriteLine($"{layoutAnchorablePaneGroup?.Children.Count}", $"AnchorableItemDropTarget Drop 2");
						//if(layoutAnchorablePaneGroup != null &&
						//	(layoutAnchorablePaneGroup.Children.Count == 1 ||
						//		layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)) {

						//	var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
						//	//for(int i = 0; i < anchorablesToMove.Length; i++) {
						//	//	//Debug.WriteLine($"{anchorablesToMove[i].Children.FirstOrDefault()}", $"AnchorableItemDropTarget Drop 3");
						//	//	Debug.WriteLine($"{targetGroup.ChildrenCount}, {targetGroup.GetType().Name}", $"AnchorableItemDropTarget Drop 3");

						//	//	var temp = anchorablesToMove[i];
						//	//	//targetGroup.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
						//	//	targetGroup.InsertChildAt(insertToIndex + 1 + i, temp);
						//	//	Debug.WriteLine($"{targetGroup.ChildrenCount}", $"AnchorableItemDropTarget Drop 31");
						//	//}
							
						//} else {
							Debug.WriteLine($"", $"AnchorableItemDropTarget Drop 4");
							targetGroup.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
						//}
					}
					break;

				#endregion DropTargetType.AnchorablePaneDockLeft

				case DropTargetType.AnchorablePaneDockRight:
					#region DropTargetType.AnchorablePaneDockRight
{
						var targetGroup = targetModel.Parent as ILayoutGroup;
						int insertToIndex = targetGroup.IndexOfChild(targetModel);


						LayoutAnchorableExpanderGroup layoutAnchorablePaneGroup = floatingWindow.RootPanel;
						Debug.WriteLine($"{layoutAnchorablePaneGroup?.Children.Count}", $"AnchorableItemDropTarget Drop 2");
						//if(layoutAnchorablePaneGroup != null &&
						//	(layoutAnchorablePaneGroup.Children.Count == 1 ||
						//		layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)) {

						//	var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
						//	//for(int i = 0; i < anchorablesToMove.Length; i++) {
						//	//	//Debug.WriteLine($"{anchorablesToMove[i].Children.FirstOrDefault()}", $"AnchorableItemDropTarget Drop 3");
						//	//	Debug.WriteLine($"{targetGroup.ChildrenCount}, {targetGroup.GetType().Name}", $"AnchorableItemDropTarget Drop 3");

						//	//	var temp = anchorablesToMove[i];
						//	//	//targetGroup.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
						//	//	targetGroup.InsertChildAt(insertToIndex + 1 + i, temp);
						//	//	Debug.WriteLine($"{targetGroup.ChildrenCount}", $"AnchorableItemDropTarget Drop 31");
						//	//}

						//} else {
						Debug.WriteLine($"", $"AnchorableItemDropTarget Drop 4");
						targetGroup.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
						//}
					}
					break;
				#endregion DropTargetType.AnchorablePaneDockRight

				case DropTargetType.AnchorablePaneDockInside:

					#region DropTargetType.AnchorablePaneDockInside

					//{
					//	var paneModel = targetModel as LayoutAnchorableExpanderGroup;
					//	var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorableExpanderGroup;

					//	//int i = _index == -1 ? 0 : _index;
					//	//foreach(var anchorableToImport in
					//	//	layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray()) {
					//	//	paneModel.Children.Insert(i, anchorableToImport);
					//	//	i++;
					//	//}
					//}
					break;

					#endregion DropTargetType.AnchorablePaneDockInside
			}

			//anchorableActive.IsActive = true;

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
			//Debug.WriteLine($"{Type}", $"{nameof(AnchorableItemDropTarget)} GetPreviewPath 1");

			switch(Type) {
				case DropTargetType.AnchorablePaneDockLeft: {
						var targetScreenRect = TargetElement.GetScreenArea();
						var left = -overlayWindow.Left;
						targetScreenRect.Offset(left, -overlayWindow.Top);

						//targetScreenRect.Width /= 2.0;
						targetScreenRect.Width = 2;
						Debug.WriteLine($"{targetScreenRect.Left}, {-overlayWindow.Left}", $"{nameof(AnchorableItemDropTarget)} GetPreviewPath 2");

						return new RectangleGeometry(targetScreenRect);
					}

				case DropTargetType.AnchorablePaneDockRight: {
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						targetScreenRect.Offset(targetScreenRect.Width - 1.0, 0.0);
						targetScreenRect.Width = 2.0;
						Debug.WriteLine($"{targetScreenRect.Left}, {-overlayWindow.Left}", $"{nameof(AnchorableItemDropTarget)} GetPreviewPath 3");
						return new RectangleGeometry(targetScreenRect);
					}
			}

			return null;
		}

		#endregion Overrides
	}
}