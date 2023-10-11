///************************************************************************
//   AvalonDock

//   Copyright (C) 2007-2013 Xceed Software Inc.

//   This program is provided to you under the terms of the Microsoft Public
//   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
// ************************************************************************/

//using AvalonDock.Layout;
//using System.Diagnostics;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media;

//namespace AvalonDock.Controls {
//	/// <summary>
//	/// Implements a <see cref="LayoutAnchorableExpanderControl"/> drop target
//	/// on which other items (<see cref="LayoutAnchorableExpanderGroup"/>) can be dropped.
//	/// </summary>
//	internal class AnchorableExpanderGroupPaneDropTarget :DropTarget<LayoutAnchorableExpanderGroupPaneControl> {
//		#region fields

//		private LayoutAnchorableExpanderGroupPaneControl _targetPane;
//		private int _tabIndex = -1;

//		#endregion fields

//		#region Constructors

//		/// <summary>
//		/// Class constructor from parameters without a specific tabindex as dock position.
//		/// </summary>
//		/// <param name="paneControl"></param>
//		/// <param name="detectionRect"></param>
//		/// <param name="type"></param>
//		internal AnchorableExpanderGroupPaneDropTarget(LayoutAnchorableExpanderGroupPaneControl paneControl,
//																	 Rect detectionRect,
//																	 DropTargetType type)
//			: base(paneControl, detectionRect, type) {
//			_targetPane = paneControl;
//		}

//		/// <summary>
//		/// Class constructor from parameters with a specific tabindex as dock position.
//		/// This constructor can be used to drop an anchorable at a specific tab index.
//		/// </summary>
//		/// <param name="paneControl"></param>
//		/// <param name="detectionRect"></param>
//		/// <param name="type"></param>
//		/// <param name="tabIndex"></paramLayoutAnchorableExpanderGroupPaneControl
//		internal AnchorableExpanderGroupPaneDropTarget(LayoutAnchorableExpanderGroupPaneControl paneControl,
//											Rect detectionRect,
//											DropTargetType type,
//											int tabIndex)
//			: base(paneControl, detectionRect, type) {
//			_targetPane = paneControl;
//			_tabIndex = tabIndex;
//		}

//		#endregion Constructors

//		#region Overrides

//		/// <summary>
//		/// Method is invoked to complete a drag & drop operation with a (new) docking position
//		/// by docking of the <paramref name="floatingWindow"/> into this drop target.
//		/// </summary>
//		/// <param name="floatingWindow"></param>
//		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow) {
//			Debug.WriteLine($"{Type}, {_targetPane.Model.GetType()}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 10");

//			LayoutAnchorableExpanderGroupPane targetModel = _targetPane.Model as LayoutAnchorableExpanderGroupPane;
//			LayoutAnchorable anchorableActive = floatingWindow.Descendents().OfType<LayoutAnchorable>().FirstOrDefault();
//			switch(Type) {
//				case DropTargetType.AnchorableExpanderPaneDockBottom:
//					#region DropTargetType.AnchorableExpanderPaneDockBottom

//					{
//						var currentExpanderGroup = targetModel.Selected as ILayoutGroup;
//						int insertToIndex = currentExpanderGroup.IndexOfChild(targetModel);


//						LayoutAnchorableExpanderGroup layoutAnchorablePaneGroup = floatingWindow.RootPanel;
//						Debug.WriteLine($"{layoutAnchorablePaneGroup?.Children.Count}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 2");
//						if(layoutAnchorablePaneGroup != null &&
//							(layoutAnchorablePaneGroup.Children.Count == 1 ||
//								layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)) {

//							var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
//							for(int i = 0; i < anchorablesToMove.Length; i++) {
//								//Debug.WriteLine($"{anchorablesToMove[i].Children.FirstOrDefault()}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 3");
//								Debug.WriteLine($"{currentExpanderGroup.ChildrenCount}, {currentExpanderGroup.GetType()}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 3");

//								var layoutAnchorables = anchorablesToMove[i];
//								//currentExpanderGroup.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
//								currentExpanderGroup.InsertChildAt(insertToIndex + 1 + i, layoutAnchorables);
//								Debug.WriteLine($"{currentExpanderGroup.ChildrenCount}", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 31");

//							}
//						} else {
//							Debug.WriteLine($"", $"{nameof(AnchorableExpanderGroupPaneDropTarget)} Drop 4");
//							currentExpanderGroup.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
//						}
//					}
//					break;

//				#endregion DropTargetType.AnchorableExpanderPaneDockBottom

//				//case DropTargetType.AnchorableExpanderPaneDockTop:

//				//	#region DropTargetType.AnchorableExpanderPaneDockTop

//				//	{
//				//		var parentModel = targetModel.Parent as ILayoutGroup;
//				//		var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
//				//		int insertToIndex = parentModel.IndexOfChild(targetModel);

//				//		if(parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Vertical &&
//				//			parentModel.ChildrenCount == 1)
//				//			parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

//				//		if(parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Vertical) {
//				//			var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
//				//			if(layoutAnchorablePaneGroup != null &&
//				//				(layoutAnchorablePaneGroup.Children.Count == 1 ||
//				//					layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)) {
//				//				var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
//				//				for(int i = 0; i < anchorablesToMove.Length; i++)
//				//					parentModel.InsertChildAt(insertToIndex + i, anchorablesToMove[i]);
//				//			} else
//				//				parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
//				//		} else {
//				//			var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
//				//			var newOrientedPanel = new LayoutAnchorablePaneGroup()
//				//			{
//				//				Orientation = System.Windows.Controls.Orientation.Vertical,
//				//				DockWidth = targetModelAsPositionableElement.DockWidth,
//				//				DockHeight = targetModelAsPositionableElement.DockHeight,
//				//			};

//				//			parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
//				//			//the floating window must be added after the target modal as it could be raise a CollectGarbage call
//				//			newOrientedPanel.Children.Add(targetModel);
//				//			newOrientedPanel.Children.Insert(0, floatingWindow.RootPanel);
//				//		}
//				//	}
//				//	break;

//				//#endregion DropTargetType.AnchorablePaneDockTop

//				//case DropTargetType.AnchorablePaneDockLeft:

//				//	#region DropTargetType.AnchorablePaneDockLeft

//				//	{
//				//		var parentModel = targetModel.Parent as ILayoutGroup;
//				//		var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
//				//		int insertToIndex = parentModel.IndexOfChild(targetModel);

//				//		if(parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
//				//			parentModel.ChildrenCount == 1)
//				//			parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

//				//		if(parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal) {
//				//			var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
//				//			if(layoutAnchorablePaneGroup != null &&
//				//				(layoutAnchorablePaneGroup.Children.Count == 1 ||
//				//					layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)) {
//				//				var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
//				//				for(int i = 0; i < anchorablesToMove.Length; i++)
//				//					parentModel.InsertChildAt(insertToIndex + i, anchorablesToMove[i]);
//				//			} else
//				//				parentModel.InsertChildAt(insertToIndex, floatingWindow.RootPanel);
//				//		} else {
//				//			var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
//				//			var newOrientedPanel = new LayoutAnchorablePaneGroup()
//				//			{
//				//				Orientation = System.Windows.Controls.Orientation.Horizontal,
//				//				DockWidth = targetModelAsPositionableElement.DockWidth,
//				//				DockHeight = targetModelAsPositionableElement.DockHeight,
//				//			};

//				//			parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
//				//			//the floating window must be added after the target modal as it could be raise a CollectGarbage call
//				//			newOrientedPanel.Children.Add(targetModel);
//				//			newOrientedPanel.Children.Insert(0, floatingWindow.RootPanel);
//				//		}
//				//	}
//				//	break;

//				//#endregion DropTargetType.AnchorablePaneDockLeft

//				//case DropTargetType.AnchorablePaneDockRight:

//				//	#region DropTargetType.AnchorablePaneDockRight

//				//	{
//				//		var parentModel = targetModel.Parent as ILayoutGroup;
//				//		var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
//				//		int insertToIndex = parentModel.IndexOfChild(targetModel);

//				//		if(parentModelOrientable.Orientation != System.Windows.Controls.Orientation.Horizontal &&
//				//			parentModel.ChildrenCount == 1)
//				//			parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Horizontal;

//				//		if(parentModelOrientable.Orientation == System.Windows.Controls.Orientation.Horizontal) {
//				//			var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
//				//			if(layoutAnchorablePaneGroup != null &&
//				//				(layoutAnchorablePaneGroup.Children.Count == 1 ||
//				//					layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)) {
//				//				var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
//				//				for(int i = 0; i < anchorablesToMove.Length; i++)
//				//					parentModel.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
//				//			} else
//				//				parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
//				//		} else {
//				//			var targetModelAsPositionableElement = targetModel as ILayoutPositionableElement;
//				//			var newOrientedPanel = new LayoutAnchorablePaneGroup()
//				//			{
//				//				Orientation = System.Windows.Controls.Orientation.Horizontal,
//				//				DockWidth = targetModelAsPositionableElement.DockWidth,
//				//				DockHeight = targetModelAsPositionableElement.DockHeight,
//				//			};

//				//			parentModel.InsertChildAt(insertToIndex, newOrientedPanel);
//				//			newOrientedPanel.Children.Add(targetModel);
//				//			newOrientedPanel.Children.Add(floatingWindow.RootPanel);
//				//		}
//				//	}
//				//	break;

//				//#endregion DropTargetType.AnchorablePaneDockRight

//				//case DropTargetType.AnchorablePaneDockInside:

//				//	#region DropTargetType.AnchorablePaneDockInside

//				//	//{
//				//	//	var paneModel = targetModel as LayoutAnchorablePane;
//				//	//	var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

//				//	//	int i = _index == -1 ? 0 : _index;
//				//	//	foreach(var anchorableToImport in
//				//	//		layoutAnchorablePaneGroup.Descendents().OfType<LayoutAnchorable>().ToArray()) {
//				//	//		paneModel.Children.Insert(i, anchorableToImport);
//				//	//		i++;
//				//	//	}
//				//	//}
//				//	break;

//				//	#endregion DropTargetType.AnchorablePaneDockInside
//			}

//			anchorableActive.IsActive = true;

//			base.Drop(floatingWindow);
//		}

//		/// <summary>
//		/// Gets a <see cref="Geometry"/> that is used to highlight/preview the docking position
//		/// of this drop target for a <paramref name="floatingWindowModel"/> being docked inside an
//		/// <paramref name="overlayWindow"/>.
//		/// </summary>
//		/// <param name="overlayWindow"></param>
//		/// <param name="floatingWindowModel"></param>
//		/// <returns>The geometry of the preview/highlighting WPF figure path.</returns>
//		public override Geometry GetPreviewPath(OverlayWindow overlayWindow,
//												LayoutFloatingWindow floatingWindowModel) {
//			switch(Type) {
//				case DropTargetType.AnchorableExpanderPaneDockBottom: {
//						var targetScreenRect = TargetElement.GetScreenArea();
//						targetScreenRect.Offset(-overlayWindow.DockLeft, -overlayWindow.DockTop);

//						targetScreenRect.Offset(0.0, targetScreenRect.Height / 2.0);
//						targetScreenRect.Height /= 2.0;

//						return new RectangleGeometry(targetScreenRect);
//					}

//				case DropTargetType.AnchorableExpanderPaneDockTop: {
//						var targetScreenRect = TargetElement.GetScreenArea();
//						targetScreenRect.Offset(-overlayWindow.DockLeft, -overlayWindow.DockTop);

//						targetScreenRect.Height /= 2.0;

//						return new RectangleGeometry(targetScreenRect);
//					}
//			}

//			return null;
//		}

//		#endregion Overrides
//	}
//}