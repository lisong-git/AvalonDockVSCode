/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvalonDock.Controls {
	/// <inheritdoc />
	/// <summary>
	/// Implements the TabItem Header that is displayed when the <see cref="LayoutAnchorablePaneControl"/>
	/// shows more than 1 <see cref="LayoutAnchorableControl"/>. This TabItem is displayed at the bottom
	/// of a <see cref="LayoutAnchorablePaneControl"/>.
	/// </summary>
	/// <seealso cref="Control"/>
	public class LayoutActivityTabItem :Control {
		#region fields

		private bool _isMouseDown = false;
		private static LayoutActivityTabItem _draggingItem = null;
		private static bool _cancelMouseLeave = false;

		#endregion fields

		#region Constructors

		static LayoutActivityTabItem() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutActivityTabItem), new FrameworkPropertyMetadata(typeof(LayoutActivityTabItem)));
		}

		#endregion Constructors

		#region Properties

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorableExpanderGroup), typeof(LayoutActivityTabItem),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>Gets/sets the model attached to the anchorable tab item.</summary>
		[Bindable(true), Description("Gets/sets the model attached to the anchorable tab item."), Category("Other")]
		public LayoutAnchorableExpanderGroup Model {
			get => (LayoutAnchorableExpanderGroup) GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutActivityTabItem) d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			//SetLayoutItem(Model?.Root.Manager.GetLayoutItemFromModel(Model));
			//Model.TabItem2 = this;
			//UpdateLogicalParent();
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutActivityTabItem),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>Gets the the LayoutItem attached to this tag item.</summary>
		[Bindable(true), Description("Gets the the LayoutItem attached to this tag item."), Category("Other")]
		public LayoutItem LayoutItem => (LayoutItem) GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.
		/// This dependency property indicates the LayoutItem attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#endregion Properties

		#region Internal Methods

		internal static bool IsDraggingItem() => _draggingItem != null;

		internal static LayoutActivityTabItem GetDraggingItem() => _draggingItem;

		internal static void ResetDraggingItem() => _draggingItem = null;

		internal static void CancelMouseLeave() => _cancelMouseLeave = true;

		#endregion Internal Methods

		#region Overrides

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);

			// Start a drag & drop action for a LayoutAnchorable
			//var anchorAble = this.Model as LayoutAnchorable;
			//if(anchorAble != null) {
			//	if(anchorAble.CanMove == false)
			//		return;
			//}

			_isMouseDown = true;
			_draggingItem = this;

			if(Model != null) {
				var activityBar = Model.Parent as LayoutActivityBar;
				_lastSelectedIndex = activityBar.SelectedIndex;				
				Model.IsActive = true;
			}
		}

		private int _lastSelectedIndex = -1;
		/// <inheritdoc />
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if(e.LeftButton != MouseButtonState.Pressed) {
				_isMouseDown = false;
				_draggingItem = null;
			} else
				_cancelMouseLeave = false;
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			_isMouseDown = false;
			var model = Model;
			if(model != null) {
				var activityBar = model.Parent as LayoutActivityBar;
				var paneModel = activityBar.LayoutAnchorableExpanderGroupPane;
				//Debug.WriteLine($"{_lastSelectedIndex}, {activityBar.SelectedIndex}", $"OnMouseLeftButtonUp 1");

				if(_lastSelectedIndex!= -1 && _lastSelectedIndex == activityBar.SelectedIndex) {
					paneModel.SetVisible(!paneModel.IsVisible);
				}
			}
			_lastSelectedIndex = -1;
			base.OnMouseLeftButtonUp(e);
		}


		/// <inheritdoc />
		protected override void OnMouseLeave(MouseEventArgs e) {
			base.OnMouseLeave(e);
			if(_isMouseDown && e.LeftButton == MouseButtonState.Pressed) {
				// drag the item if the mouse leave is not canceled.
				// Mouse leave should be canceled when selecting a new tab to prevent automatic undock when Panel size is Auto.
				_draggingItem = !_cancelMouseLeave ? this : null;
			}
			_isMouseDown = false;
			_cancelMouseLeave = false;
		}

		/// <inheritdoc />
		protected override void OnMouseEnter(MouseEventArgs e) {
			base.OnMouseEnter(e);
			if(_draggingItem == null || _draggingItem == this || e.LeftButton != MouseButtonState.Pressed)
				return;

			var model = Model;
			var container = model.Parent;
			var containerPane = model.Parent as ILayoutPane;
			Debug.WriteLine($"{model?.GetType().Name}, {container.GetType().Name}, {containerPane?.GetType().Name}, {container is ILayoutPane}", "LayoutActivityTabItem_OnMouseEnter 1");
			if(containerPane is LayoutAnchorableExpanderGroupPane expGroupBox && !expGroupBox.CanRepositionItems)
				return;
			//if (containerPane.Parent is LayoutAnchorablePaneGroup layoutAnchorablePaneGroup && !layoutAnchorablePaneGroup.CanRepositionItems) return;
			var childrenList = container.Children.ToList();

			// Hotfix to avoid crash caused by a likely threading issue Back in the containerPane.
			var oldIndex = childrenList.IndexOf(_draggingItem.Model);
			var newIndex = childrenList.IndexOf(model);
			Debug.WriteLine($"{oldIndex}, {newIndex}, {containerPane?.ChildrenCount}, {childrenList?.Count}", "LayoutActivityTabItem_OnMouseEnter 2");

			if(newIndex < containerPane.ChildrenCount && oldIndex > -1)
				containerPane.MoveChild(oldIndex, newIndex);
		}

		#endregion Overrides
	}
}