/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Controls {
	/// <summary>
	/// Provides a panel that contains the TabItem Headers of the <see cref="LayoutDocumentPaneControl"/>.
	/// </summary>
	public class ActivityBarTabPanel :Panel {
		#region Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		public ActivityBarTabPanel() {
			this.FlowDirection = FlowDirection.LeftToRight;
		}

		#endregion Constructors

		#region Overrides

		protected override Size MeasureOverride(Size availableSize) {
			Size desideredSize = new Size();
			foreach(FrameworkElement child in InternalChildren) {
				child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				var childHeight = child.DesiredSize.Height;

				if(desideredSize.Height + childHeight > availableSize.Height) {
					//Debug.WriteLine($"{availableSize.Height}, {desideredSize.Height}", "MeasureOverride");
					return desideredSize;
				}
				desideredSize.Height += childHeight;
				desideredSize.Width = Math.Max(desideredSize.Width, child.DesiredSize.Width);
			}
			return new Size(desideredSize.Width, Math.Min(desideredSize.Height, availableSize.Height));
		}

		protected override Size ArrangeOverride(Size finalSize) {
			var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != Visibility.Collapsed);
			var offset = 0.0;
			var skipAllOthers = false;

			//var itemFullHeight = 0.0;

			foreach(TabItem child in visibleChildren) {
				if(skipAllOthers || offset + child.DesiredSize.Height > finalSize.Height) {
					bool isLayoutContentSelected = false;
					var layoutContent = child.Content as LayoutContent;

					if(layoutContent != null)
						isLayoutContentSelected = layoutContent.IsSelected;

					if(isLayoutContentSelected && !child.IsVisible) {
						var parentContainer = layoutContent.Parent as ILayoutContainer;
						var parentSelector = layoutContent.Parent as ILayoutContentSelector;
						var parentPane = layoutContent.Parent as ILayoutPane;
						int contentIndex = parentSelector.IndexOf(layoutContent);
						if(contentIndex > 0 &&
							parentContainer.ChildrenCount > 1) {
							parentPane.MoveChild(contentIndex, 0);
							parentSelector.SelectedIndex = 0;
							return ArrangeOverride(finalSize);
						}
					}
					child.Visibility = Visibility.Hidden;
					skipAllOthers = true;
				} else {
					child.Visibility = Visibility.Visible;
					child.Arrange(new Rect(0.0, offset, finalSize.Width, child.DesiredSize.Height));
					//if(itemFullHeight == 0.0) {
					//	itemFullHeight = child.ActualHeight + child.Margin.DockTop + child.Margin.DockBottom;
					//}
					offset += child.ActualHeight + child.Margin.Top + child.Margin.Bottom;
					//offset += itemFullHeight;
				}
			}

			//Debug.WriteLine($"{finalSize.Height}, {offset}, ", "ArrangeOverride");
			return new Size(finalSize.Width, offset);
		}

		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e) {
			if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
					LayoutActivityTabItem.IsDraggingItem()) {
				var contentModel = LayoutActivityTabItem.GetDraggingItem().Model;
				var manager = contentModel.Root.Manager;
				LayoutActivityTabItem.ResetDraggingItem();
				//Trace.WriteLine("OnMouseLeave()");

				manager.StartDraggingFloatingWindowForPane(contentModel);
			}

			base.OnMouseLeave(e);
		}

		#endregion Overrides
	}
}