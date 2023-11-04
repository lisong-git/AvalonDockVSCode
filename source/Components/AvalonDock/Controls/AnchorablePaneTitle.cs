/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using Microsoft.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AvalonDock.Controls {
	/// <summary>
	/// This control defines the Title area of a <see cref="LayoutAnchorableExpanderControl"/>.
	/// It is used to show a title bar with docking window buttons to let users interact
	/// with a <see cref="LayoutAnchorableExpander"/> via drop down menu click or drag & drop.
	/// </summary>
	public class AnchorablePaneTitle :ToggleButton {
		#region fields

		private bool _isMouseDown = false;

		private bool _isDragging = false;
		#endregion fields

		#region Constructors

		/// <summary>
		/// Static class constructor
		/// </summary>
		static AnchorablePaneTitle() {
			IsHitTestVisibleProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(true));
			FocusableProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(false));
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(AnchorablePaneTitle)));
		}

		//protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
		//	Debug.WriteLine($"{e.ButtonState}, {e.ClickCount}, {e.ButtonState}", $"AnchorablePaneTitle_OnPreviewMouseLeftButtonDown 1");
		//}

		#endregion Constructors

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorableExpander), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata(null, _OnModelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorableExpander"/> model attached of this view.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorableExpander model attached of this view."), Category("Anchorable")]
		public LayoutAnchorableExpander Model {
			get => (LayoutAnchorableExpander) GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		private static void _OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((AnchorablePaneTitle) sender).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			if(Model != null)
				SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			else
				SetLayoutItem(null);
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata((LayoutItem)null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="LayoutItem"/> (<see cref="LayoutAnchorableItem"/> or <see cref="LayoutDocumentItem"/>) attached to this view.</summary>
		[Bindable(true), Description("Gets the LayoutItem (LayoutAnchorableItem or LayoutDocumentItem) attached to this object."), Category("Layout")]
		public LayoutItem LayoutItem => (LayoutItem) GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.
		/// This dependency property indicates the <see cref="AvalonDock.Controls.LayoutItem"/> attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#region Overrides

		/// <inheritdoc />
		protected override void OnMouseMove(MouseEventArgs e) {
			if(e.LeftButton != MouseButtonState.Pressed)
				_isMouseDown = false;
			base.OnMouseMove(e);
		}

		/// <inheritdoc />
		protected override void OnMouseLeave(MouseEventArgs e) {
			//Debug.WriteLine($"", $"AnchorablePaneTitle OnMouseLeave 1");

			base.OnMouseLeave(e);
			if(_isMouseDown && e.LeftButton == MouseButtonState.Pressed) {
					Debug.WriteLine($"{Model?.GetType()}", $"AnchorablePaneTitle OnMouseLeave 2");
				var pane = this.FindVisualAncestor<LayoutAnchorablePaneControl>();

				if(pane != null) {

					var paneModel = pane.Model as LayoutAnchorablePane;
					var manager = paneModel.Root.Manager;
					manager.StartDraggingFloatingWindowForPane(paneModel);
				} else {
					// Start dragging a LayoutAnchorableExpander control that docked/reduced into a SidePanel and
					// 1) made visible by clicking on to its name in AutoHide mode
					// 2) user drags the top title bar of the LayoutAnchorableExpander control to drag it out of its current docking position
					Model?.Root?.Manager?.StartDraggingFloatingWindowForContent(Model);
				}
				_isDragging = true;
			}
			_isMouseDown = false;
		}

		//private MouseButtonEventArgs _mouseLeftButtonDownEventArgs;

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			//Debug.WriteLine($"{e.LeftButton}, {e.Handled}", $"AnchorablePaneTitle OnMouseLeftButtonDown 1");

			// Start a drag & drop action for a LayoutAnchorableExpander
			if(e.Handled || Model.CanMove == false)
				return;
			var attachFloatingWindow = false;
			var parentFloatingWindow = Model.FindParent<LayoutAnchorableFloatingWindow>();
			if(parentFloatingWindow != null)
				attachFloatingWindow = parentFloatingWindow.Descendents().OfType<LayoutAnchorablePane>().Count() == 1;

			if(attachFloatingWindow) {
				//the pane is hosted inside a floating window that contains only an anchorable pane so drag the floating window itself
				var floatingWndControl = Model.Root.Manager.FloatingWindows.Single(fwc => fwc.Model == parentFloatingWindow);
				floatingWndControl.AttachDrag(false);
			} else {
				_isMouseDown = true;//normal drag
				//_mouseLeftButtonDownEventArgs = e;
				_isDragging = false;
			}
				//e.Handled = true;

			//base.OnMouseLeftButtonDown(e);
		}



		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			//Debug.WriteLine($"{_isMouseDown}, {_isDragging}", $"AnchorablePaneTitle OnMouseLeftButtonUp 1");
			if(!_isDragging) {
				_isMouseDown = false;
				//base.OnMouseLeftButtonDown(_mouseLeftButtonDownEventArgs);
				//base.OnMouseLeftButtonUp(e);
				base.OnToggle();
				if(Model != null)
					Model.IsActive = true;//FocusElementManager.SetFocusOnLastElement(Model);
			}
		}

		//protected override void OnMouseUp(MouseButtonEventArgs e) {
		//	Debug.WriteLine($"{e.LeftButton}", $"AnchorablePaneTitle OnMouseUp 1");
		//}

		//protected override void OnMouseDown(MouseButtonEventArgs e) {
		//	Debug.WriteLine($"{e.LeftButton}", $"AnchorablePaneTitle OnMouseDown 1");
		//	base.OnMouseDown(e);
		//}

		//protected override void OnDrop(System.Windows.DragEventArgs e) {
		//	Debug.WriteLine($"{e}", $"AnchorablePaneTitle OnDrop 1");
		//}

		//protected override void OnDragEnter(System.Windows.DragEventArgs e) {
		//	Debug.WriteLine($"{e}", $"AnchorablePaneTitle OnDragEnter 1");
		//}
		//protected override void OnDragLeave(System.Windows.DragEventArgs e) {
		//	Debug.WriteLine($"{e}", $"AnchorablePaneTitle OnDragLeave 1");
		//}
		//protected override void OnDragOver(System.Windows.DragEventArgs e) {
		//	Debug.WriteLine($"{e}", $"AnchorablePaneTitle OnDragOver 1");
		//}


		#endregion Overrides
	}
}