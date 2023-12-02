/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Controls {
	/// <summary>
	/// provides a <see cref="Panel"/> that contains the TabItem Headers of the <see cref="LayoutAnchorablePaneControl"/>.
	/// </summary>
	public class AnchorablePaneTabPanel :Panel {
		#region Constructors

		public AnchorablePaneTabPanel() {
			this.FlowDirection = System.Windows.FlowDirection.LeftToRight;
		}

		#endregion Constructors

		#region Property

		#region HasOverflowItems

		/// <summary>
		///     The key needed set a read-only property.
		/// </summary>
		internal static readonly DependencyPropertyKey HasOverflowItemsPropertyKey =
								DependencyProperty.RegisterReadOnly(
												"HasOverflowItems",
												typeof(bool),
												typeof(AnchorablePaneTabPanel),
												new FrameworkPropertyMetadata(false));

		/// <summary>
		///     The DependencyProperty for the HasOverflowItems property.
		///     Flags:              None
		///     Default Value:      false
		/// </summary>
		public static readonly DependencyProperty HasOverflowItemsProperty =
								HasOverflowItemsPropertyKey.DependencyProperty;

		/// <summary>
		/// Whether we have overflow items
		/// </summary>
		public bool HasOverflowItems {
			get => (bool) GetValue(HasOverflowItemsProperty);
		}
		#endregion HasOverflowItems

		#endregion Property

		#region Overrides

		protected override Size MeasureOverride(Size availableSize) {
			Size desideredSize = new Size();
			foreach(FrameworkElement child in InternalChildren) {
				child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				var childWidth = child.DesiredSize.Width;

				if(desideredSize.Width + childWidth > availableSize.Width) {
					//Debug.WriteLine($"{availableSize.Height}, {desideredSize.Height}", "MeasureOverride");
					return desideredSize;
				}
				desideredSize.Width += childWidth;
				desideredSize.Height = Math.Max(desideredSize.Height, child.DesiredSize.Height);
			}

			return new Size(Math.Min( desideredSize.Width, availableSize.Width), desideredSize.Height);
		}

		protected override Size ArrangeOverride(Size finalSize) {
			var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != Visibility.Collapsed);
			var offset = 0.0;
			var skipAllOthers = false;

			foreach (TabItem child in visibleChildren) {
				if (skipAllOthers || offset + child.DesiredSize.Width > finalSize.Width) {
					bool isLayoutContentSelected = false;
					var layoutContent = child.Content as LayoutAnchorableGroup;

					if (layoutContent != null)
						isLayoutContentSelected = layoutContent.IsSelected;

					if (isLayoutContentSelected && !child.IsVisible) {
						var parentContainer = layoutContent.Parent as ILayoutContainer;
						var parentSelector = layoutContent.Parent as ILayoutSelector<LayoutAnchorableGroup>;
						var parentPane = layoutContent.Parent as ILayoutPane;
						int contentIndex = parentSelector.IndexOf(layoutContent);
						if (contentIndex > 0 &&
							parentContainer.ChildrenCount > 1) {
							parentPane.MoveChild(contentIndex, 0);
							parentSelector.SelectedIndex = 0;
							return ArrangeOverride(finalSize);

						}
					}
					child.Visibility = Visibility.Hidden;
					skipAllOthers = true;
				}
				else {
					child.Visibility = Visibility.Visible;
					child.Arrange(new Rect(offset, 0.0, child.DesiredSize.Width, finalSize.Height));
					offset += child.ActualWidth + child.Margin.Left + child.Margin.Right;
				}
			}
			SetValue(HasOverflowItemsPropertyKey, skipAllOthers);
			return new Size(offset, finalSize.Height);
		}


		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e) {
			if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
				LayoutAnchorableGroupTabItem.IsDraggingItem()) {
				var contentModel = LayoutAnchorableGroupTabItem.GetDraggingItem().Model as LayoutAnchorableGroup;
				Debug.WriteLine($"{contentModel.Title}", "AnchorablePaneTabPanel");
				var manager = contentModel.Root.Manager;
				LayoutAnchorableGroupTabItem.ResetDraggingItem();

				//manager.StartDraggingFloatingWindowForContent(contentModel);
				manager.StartDraggingFloatingWindowForPane(contentModel);
			}

			base.OnMouseLeave(e);
		}

		#endregion Overrides
	}
}