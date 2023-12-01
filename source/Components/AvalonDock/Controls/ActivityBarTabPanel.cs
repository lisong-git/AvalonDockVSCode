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


		#region HasOverflowItems

		/// <summary>
		///     The key needed set a read-only property.
		/// </summary>
		internal static readonly DependencyPropertyKey HasOverflowItemsPropertyKey =
								DependencyProperty.RegisterReadOnly(
												"HasOverflowItems",
												typeof(bool),
												typeof(ActivityBarTabPanel),
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
			get { return (bool) GetValue(HasOverflowItemsProperty); }
		}
		#endregion HasOverflowItems

		#region IsOverflowItem
		/// <summary>
		///     The key needed set a read-only property.
		/// Attached property to indicate if the item is placed in the overflow panel
		/// </summary>
		internal static readonly DependencyPropertyKey IsOverflowItemPropertyKey =
								DependencyProperty.RegisterAttachedReadOnly(
												"IsOverflowItem",
												typeof(bool),
												typeof(ActivityBarTabPanel),
												new FrameworkPropertyMetadata(false));

		/// <summary>
		///     The DependencyProperty for the IsOverflowItem property.
		///     Flags:              None
		///     Default Value:      false
		/// </summary>
		public static readonly DependencyProperty IsOverflowItemProperty =
								IsOverflowItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Writes the attached property IsOverflowItem to the given element.
		/// </summary>
		/// <param name="element">The element to which to write the attached property.</param>
		/// <param name="value">The property value to set</param>
		internal static void SetIsOverflowItem(DependencyObject element, object value) {
			element.SetValue(IsOverflowItemPropertyKey, value);
		}

		/// <summary>
		/// Reads the attached property IsOverflowItem from the given element.
		/// </summary>
		/// <param name="element">The element from which to read the attached property.</param>
		/// <returns>The property's value.</returns>
		public static bool GetIsOverflowItem(DependencyObject element) {
			if(element is null) {
				throw new ArgumentNullException(nameof(element));
			}

			return (bool) element.GetValue(IsOverflowItemProperty);
		}

		#endregion

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
			var visibleChildren = Children.Cast<TabItem>().Where(ch => ch.Visibility != Visibility.Collapsed);
			var offset = 0.0;
			var skipAllOthers = false;

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
					offset += child.ActualHeight + child.Margin.Top + child.Margin.Bottom;
				}

			}
				if(visibleChildren.FirstOrDefault()?.DataContext is LayoutAnchorableGroup group && group.Parent is LayoutActivityBar activityBar) {
					activityBar.OverflowItems = null;
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
				Debug.WriteLine($"{contentModel?.Title}, {contentModel.Root == null}, {contentModel?.Root.Manager == null}", "ActivityBarTabPanel_OnMouseLeave");

				manager.StartDraggingFloatingWindowForPane(contentModel);
			}

			base.OnMouseLeave(e);
		}

		#endregion Overrides
	}
}