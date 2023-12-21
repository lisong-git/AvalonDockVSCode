/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AvalonDock.Controls.DropTargets
{
	/// <summary>
	/// Implements a <see cref="LayoutDocumentControl"/> drop target
	/// on which other items (<see cref="LayoutDocument"/>) can be dropped.
	/// </summary>
	internal class DocumentDropTarget : DropTarget<LayoutDocumentControl>
	{
		#region fields

		private LayoutDocumentControl _targetPane;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class contructor
		/// </summary>
		/// <param name="paneControl"></param>
		/// <param name="detectionRect"></param>
		/// <param name="type"></param>
		internal DocumentDropTarget(LayoutDocumentControl paneControl,
											 Rect detectionRect,
											 DropTargetType type)
			: base(paneControl, detectionRect, type)
		{
			_targetPane = paneControl;
		}

		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the LayoutDocument <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutDocumentFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			//switch (Type)
			//{
			//	case DropTargetType.DocumentPaneGroupDockInside:

			//		#region DropTargetType.DocumentPaneGroupDockInside

			//		{
			//			var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
			//			var paneModel = paneGroupModel as LayoutDocumentPaneGroup;
			//			var sourceModel = floatingWindow.RootPanel as LayoutDocumentPaneGroup;

			//			paneModel.Children.Insert(0, sourceModel);
			//		}
			//		break;

			//		#endregion DropTargetType.DocumentPaneGroupDockInside
			//}

			base.Drop(floatingWindow);
		}

		/// <summary>
		/// Method is invoked to complete a drag & drop operation with a (new) docking position
		/// by docking of the LayoutAnchorable <paramref name="floatingWindow"/> into this drop target.
		/// </summary>
		/// <param name="floatingWindow"></param>
		protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
		{
			ILayoutPane targetModel = _targetPane.Model as ILayoutPane;

			//switch (Type)
			//{
			//	case DropTargetType.DocumentPaneGroupDockInside:

			//		#region DropTargetType.DocumentPaneGroupDockInside

			//		{
			//			var paneGroupModel = targetModel as LayoutDocumentPaneGroup;
			//			var paneModel = paneGroupModel.Children[0] as LayoutDocumentPane;
			//			LayoutPaneCompositePart layoutAnchorableGroupPane = floatingWindow.RootPanel;

			//			int i = 0;
			//			foreach (var anchorableToImport in layoutAnchorableGroupPane.Descendents().OfType<LayoutAnchorable>().ToArray())
			//			{
			//				// BD: 18.07.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
			//				//anchorableToImport.SetCanCloseInternal(true);

			//				paneModel.Children.Insert(i, anchorableToImport);
			//				i++;
			//			}
			//		}
			//		break;

			//		#endregion DropTargetType.DocumentPaneGroupDockInside
			//}

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
												LayoutFloatingWindow floatingWindowModel) {
			Debug.WriteLine($"{Type}", $"{nameof(DocumentDropTarget)} GetPreviewPath");
			switch(Type) {
				case DropTargetType.DocumentPaneDockInside:

					#region DropTargetType.DocumentPaneGroupDockInside

					{
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}

				#endregion DropTargetType.DocumentPaneGroupDockInside

				case DropTargetType.DocumentPaneDockLeft: {

						//var targetScreenRect = DetectionRects.FirstOrDefault();
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Width /= 2.0;

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}

				case DropTargetType.DocumentPaneDockTop: {

						//var targetScreenRect = DetectionRects.FirstOrDefault();
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Height /= 2.0;

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}

				case DropTargetType.DocumentPaneDockRight: {
						//var targetScreenRect = DetectionRects.FirstOrDefault();

						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(targetScreenRect.Width / 2.0, 0.0);
						targetScreenRect.Width /= 2.0;

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
						return new RectangleGeometry(targetScreenRect);
					}
				case DropTargetType.DocumentPaneDockBottom: {

						//var targetScreenRect = DetectionRects.FirstOrDefault();
						var targetScreenRect = TargetElement.GetScreenArea();
						targetScreenRect.Offset(0.0, targetScreenRect.Height / 2.0);
						targetScreenRect.Height /= 2.0;

						targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);

						return new RectangleGeometry(targetScreenRect);
					}
			}


			//var targetScreenRect2 = TargetElement.GetScreenArea();
			//targetScreenRect2.Offset(-overlayWindow.Left, -overlayWindow.Top);

			//return new RectangleGeometry(targetScreenRect2);
			return null;
		}

		#endregion Overrides
	}
}
