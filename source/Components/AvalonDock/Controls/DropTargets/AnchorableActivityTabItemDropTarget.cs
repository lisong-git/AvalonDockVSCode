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

namespace AvalonDock.Controls.DropTargets
{
	/// <summary>
	/// Implements a <see cref="LayoutAnchorableControl"/> drop target
	/// on which other items (<see cref="LayoutAnchorableGroup"/>) can be dropped.
	/// </summary>
	internal class AnchorableActivityTabItemDropTarget : DropTarget<LayoutActivityTabItem>
	{
		#region fields

		private LayoutActivityTabItem _targetPane;
		private int _index = -1;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor from parameters without a specific tabindex as dock position.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal AnchorableActivityTabItemDropTarget(LayoutActivityTabItem paneControl,
																	 Rect detectionRect,
																	 DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		/// <summary>
		/// Class constructor from parameters with a specific tabindex as dock position.
		/// This constructor can be used to drop an anchorable at a specific tab index.
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		/// <param name="index"></paramLayoutAnchorableExpanderGroupPaneControl
		internal AnchorableActivityTabItemDropTarget(LayoutActivityTabItem paneControl,
											Rect detectionRect,
											DropTargetType type,
											int index)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
			_index = index;
		}

		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			Debug.WriteLine($"{Type}", $"AnchorableActivityBarDropTarget Drop 1");

			LayoutAnchorableGroup targetModel = _targetPane.Model;
			switch (Type)
			{
				case DropTargetType.AnchorableExpanderDockTop:
					#region DropTargetType.Bottom
					{
						var expanderGroup = targetModel.Parent as ILayoutGroup;
						int insertToIndex = expanderGroup.IndexOfChild(targetModel);

						LayoutAnchorableGroupPane layoutAnchorableGroup = floatingWindow.RootPanel;
						layoutAnchorableGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
						Debug.WriteLine($"{expanderGroup}", $"AnchorableActivityBarDropTarget Drop 2");
						//layoutAnchorableGroup.Parent = null;
						expanderGroup.InsertChildAt(insertToIndex, layoutAnchorableGroup);
					}
					break;

				#endregion DropTargetType.Bottom
				case DropTargetType.AnchorableExpanderDockBottom:
					#region DropTargetType.Bottom
					{
						var expanderGroup = targetModel.Parent as ILayoutGroup;
						int insertToIndex = expanderGroup.IndexOfChild(targetModel);

						LayoutAnchorableGroupPane layoutAnchorableExpanderGroup = floatingWindow.RootPanel;
						layoutAnchorableExpanderGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
						Debug.WriteLine($"{expanderGroup}", $"AnchorableActivityBarDropTarget Drop 2");
						//layoutAnchorableGroup.Parent = null;
						expanderGroup.InsertChildAt(insertToIndex + 1, layoutAnchorableExpanderGroup);
					}
					break;

				#endregion DropTargetType.Bottom

				case DropTargetType.DockInside:
					#region DropTargetType.Inside

					//{
					//	var paneModel = targetModel as LayoutAnchorablePane;
					//	var layoutAnchorableGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;

					//	int i = _index == -1 ? 0 : _index;
					//	foreach(var anchorableToImport in
					//		layoutAnchorableGroup.Descendents().OfType<LayoutAnchorable>().ToArray()) {
					//		paneModel.Children.Insert(i, anchorableToImport);
					//		i++;
					//	}
					//}
					break;

					#endregion DropTargetType.Inside
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
			Debug.WriteLine($"{Type}", $"{nameof(AnchorableActivityTabItemDropTarget)} GetPreviewPath");
			//var targetModel = _targetPane.Model as LayoutAnchorableGroup;

			switch (Type) {
				case DropTargetType.AnchorableExpanderDockTop: {
						var targetScreenRect = TargetElement.GetScreenArea();

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						targetScreenRect.Height = 4.0;

						return new RectangleGeometry(targetScreenRect);
					}
				case DropTargetType.AnchorableExpanderDockBottom: {
						//var expanderGroup = targetModel.Parent as ILayoutGroup;
						var targetScreenRect = TargetElement.GetScreenArea();

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top + targetScreenRect.Height - 2);
						targetScreenRect.Height = 4.0;

						//Debug.WriteLine($"{targetScreenRect.Left}, {-overlayWindow.Left}", $"{nameof(AnchorableActivityTabItemDropTarget)} GetPreviewPath 2");
						return new RectangleGeometry(targetScreenRect);
					}
				default:
					return null;
			}
		}

		#endregion Overrides
	}
}