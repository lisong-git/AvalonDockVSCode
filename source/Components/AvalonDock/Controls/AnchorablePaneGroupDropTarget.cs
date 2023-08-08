/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a <see cref="DockingManager"/> drop target
	/// on which other items (<see cref="LayoutDocument"/> or <see cref="LayoutAnchorable"/>) can be dropped.
	///
	/// The resulting drop targets are usually the 4 outer drop target buttons
	/// re-presenting a <see cref="LayoutAnchorSideControl"/> shown as overlay
	/// on the <see cref="DockingManager"/> when the user drags an item over it.
	/// </summary>
	internal class AnchorablePaneGroupDropTarget : DropTarget<LayoutAnchorableExpanderGroupControl>
	{
		#region fields

		private LayoutAnchorableExpanderGroupControl _targetPane;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal AnchorablePaneGroupDropTarget(LayoutAnchorableExpanderGroupControl manager,
										  Rect detectionRect,
										  DropTargetType type)
			: base(manager, detectionRect, type)
		{
			_targetPane = manager;
		}

		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the LayoutAnchorable <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			Debug.WriteLine($"{Type}", "Drop");
			LayoutAnchorableExpanderGroup targetModel = _targetPane.Model as LayoutAnchorableExpanderGroup;
			LayoutAnchorable anchorableActive = floatingWindow.Descendents().OfType<LayoutAnchorable>().FirstOrDefault();

			switch(Type)
			{
				
				case DropTargetType.AnchorablePaneGroupDockBottom:
					#region DropTargetType.AnchorablePaneGroupDockBottom

					{
						var parentModel = targetModel.Parent as ILayoutGroup;
						var parentModelOrientable = targetModel.Parent as ILayoutOrientableGroup;
						int insertToIndex = parentModel.IndexOfChild(targetModel);

							parentModelOrientable.Orientation = System.Windows.Controls.Orientation.Vertical;

							var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
							Debug.WriteLine($"{layoutAnchorablePaneGroup?.Children.Count}", "AnchorablePaneDropTarget Drop 2");
							if(layoutAnchorablePaneGroup != null &&
								(layoutAnchorablePaneGroup.Children.Count == 1 ||
									layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)) {

								var anchorablesToMove = layoutAnchorablePaneGroup.Children.ToArray();
								for(int i = 0; i < anchorablesToMove.Length; i++) {
									Debug.WriteLine($"{anchorablesToMove[i].Children.FirstOrDefault()}", "AnchorablePaneDropTarget Drop 3");
									var la = anchorablesToMove[i].Children.First() as LayoutAnchorable;
									var temp = new LayoutAnchorableExpander(la);
									//parentModel.InsertChildAt(insertToIndex + 1 + i, anchorablesToMove[i]);
									parentModel.InsertChildAt(insertToIndex + 1 + i, temp);
								}
							} else {
								Debug.WriteLine($"", "AnchorablePaneDropTarget Drop 4");
								parentModel.InsertChildAt(insertToIndex + 1, floatingWindow.RootPanel);
							}
						
					}
					break;

					#endregion DropTargetType.DockingManagerDockBottom
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
		/// <returns>The geometry of the preview/highlighting WPF figure path.</returns>
		public override Geometry GetPreviewPath(OverlayWindow overlayWindow,
												LayoutFloatingWindow floatingWindowModel)
		{
			var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
			var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
			var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

			var targetScreenRect = TargetElement.GetScreenArea();
			Debug.WriteLine($"{Type}", "GetPreviewPath");

			switch(Type)
			{

				//case DropTargetType.DockingManagerDockTop:
				//	{
				//		var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
				//		var previewBoxRect = new Rect(
				//			targetScreenRect.Left - overlayWindow.Left,
				//			targetScreenRect.Top - overlayWindow.Top,
				//			targetScreenRect.Width,
				//			Math.Min(desideredHeight, targetScreenRect.Height / 2.0));

				//		return new RectangleGeometry(previewBoxRect);
				//	}

				case DropTargetType.AnchorablePaneGroupDockBottom:
					{
						//var desideredHeight = layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight;
						var desideredHeight = _targetPane.EmptyLength() - 1;
						var previewBoxRect = new Rect(
							targetScreenRect.Left - overlayWindow.Left,
							targetScreenRect.Bottom - overlayWindow.Top - desideredHeight,
							targetScreenRect.Width,
							desideredHeight);
						return new RectangleGeometry(previewBoxRect);
					}
			}

			throw new InvalidOperationException();
		}

		#endregion Overrides
	}
}