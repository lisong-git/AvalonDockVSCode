using AvalonDock.Layout;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AvalonDock.Controls {
	/// <summary>
	/// This control defines the Title area of a <see cref="LayoutAnchorableControl"/>.
	/// It is used to show a title bar with docking window buttons to let users interact
	/// with a <see cref="LayoutAnchorable"/> via drop down menu click or drag & drop.
	/// </summary>
	public class AnchorableTitle :ToggleButton {
		#region fields

		private bool _isMouseDown = false;

		private bool _isDragging = false;
		#endregion fields

		#region Constructors

		/// <summary>
		/// Static class constructor
		/// </summary>
		static AnchorableTitle() {
			IsHitTestVisibleProperty.OverrideMetadata(typeof(AnchorableTitle), new FrameworkPropertyMetadata(true));
			FocusableProperty.OverrideMetadata(typeof(AnchorableTitle), new FrameworkPropertyMetadata(false));
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorableTitle), new FrameworkPropertyMetadata(typeof(AnchorableTitle)));
		}

		#endregion Constructors

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(AnchorableTitle),
				new FrameworkPropertyMetadata(null, _OnModelChanged));

		/// <summary>Gets/sets the <see cref="LayoutAnchorable"/> model attached of this view.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorable model attached of this view."), Category("Anchorable")]
		public LayoutAnchorable Model {
			get => (LayoutAnchorable) GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		private static void _OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((AnchorableTitle) sender).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			if(Model != null)
				SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			else
				SetLayoutItem(null);
		}

		#endregion Model

		//#region Orientation

		///// <summary><see cref="Model"/> dependency property.</summary>
		//public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(AnchorableTitle),
		//		new FrameworkPropertyMetadata(null, _OnModelChanged));

		///// <summary>Gets/sets the <see cref="LayoutAnchorable"/> model attached of this view.</summary>
		//[Bindable(true), Description("Gets/sets the LayoutAnchorable model attached of this view."), Category("Anchorable")]
		//public Orientation Orientation {
		//	get => (Orientation) GetValue(OrientationProperty);
		//	set => SetValue(OrientationProperty, value);
		//}

		//#endregion Orientation

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(AnchorableTitle),
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
			//Debug.WriteLine($"", $"AnchorableExpanderTitle_OnMouseLeave 1");

			base.OnMouseLeave(e);
			if(_isMouseDown && e.LeftButton == MouseButtonState.Pressed) {
					//Debug.WriteLine($"{Model?.GetType().Name}, {Model.Root == null}, {Model?.Root?.Manager == null}", $"AnchorableExpanderTitle_OnMouseLeave 2");
				//var pane = this.FindVisualAncestor<LayoutAnchorableGroupControl>();

				//if(pane != null) {

				//	var paneModel = pane.Model as LayoutPaneComposite;
				//	var manager = paneModel.Root.Manager;
				//	manager.StartDraggingFloatingWindowForPane(paneModel);
				//} else {
				//	// Start dragging a LayoutAnchorable control that docked/reduced into a SidePanel and
				//	// 1) made visible by clicking on to its name in AutoHide mode
				//	// 2) user drags the top title bar of the LayoutAnchorable control to drag it out of its current docking position
				//	Model?.Root?.Manager?.StartDraggingFloatingWindowForContent(Model);
				//}
				Model?.Root?.Manager?.StartDraggingFloatingWindowForContent(Model);

				_isDragging = true;
			}
			_isMouseDown = false;
		}

		//private MouseButtonEventArgs _mouseLeftButtonDownEventArgs;

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			// Start a drag & drop action for a LayoutAnchorable
			if(e.Handled || Model.CanMove == false)
				return;
			var attachFloatingWindow = false;
			var parentFloatingWindow = Model.FindParent<LayoutAnchorableFloatingWindow>();
			if(parentFloatingWindow != null)
				attachFloatingWindow = parentFloatingWindow.Descendents().OfType<LayoutPaneComposite>().Count() == 1;

			if(attachFloatingWindow) {
				//the pane is hosted inside a floating window that contains only an anchorable pane so drag the floating window itself
				//var floatingWndControl = Model.Root.Manager.FloatingWindows.Single(fwc => fwc.Model == parentFloatingWindow);
				//floatingWndControl.AttachDrag(false);
			} else {
				_isMouseDown = true;//normal drag
				_isDragging = false;
			}
			//e.Handled = true;
			//Debug.WriteLine($"", $"{nameof(AnchorableTitle)} OnMouseLeftButtonDown");
			//base.OnMouseLeftButtonDown(e);
		}



		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
			//Debug.WriteLine($"{_isMouseDown}, {_isDragging}", $"{nameof(AnchorableTitle)} OnMouseLeftButtonUp 1");
			if(!_isDragging) {
				_isMouseDown = false;
				//base.OnMouseLeftButtonDown(_mouseLeftButtonDownEventArgs);
				//base.OnMouseLeftButtonUp(e);
				base.OnToggle();
				if(Model != null)
					Model.IsActive = true;//FocusElementManager.SetFocusOnLastElement(Model);
			}
		}

		#endregion Overrides
	}
}