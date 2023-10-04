/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="Expander"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Provides a control to display multible (or just one) LayoutAnchorable(s).
	/// See also <seealso cref="AnchorablePaneTabPanel"/>.
	/// </summary>
	/// <seealso cref="Expander"/>
	/// <seealso cref="ILayoutControl"/>
	public class LayoutAnchorableExpanderControl : Expander, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields

		private readonly LayoutAnchorableExpander _model;

		#endregion fields

		#region Constructors

		/// <summary>Static class constructor to register WPF style keys.</summary>
		static LayoutAnchorableExpanderControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorableExpanderControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>Class constructor from model and virtualization parameter.</summary>
		/// <param name="model"></param>
		/// <param name="IsVirtualizing">Whether tabbed items are virtualized or not.</param>
		internal LayoutAnchorableExpanderControl(LayoutAnchorableExpander model, bool IsVirtualizing)
			//: base(IsVirtualizing)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			//SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			SizeChanged += OnSizeChanged;
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets the layout model of this control.</summary>
		[Bindable(true), Description("Gets the layout model of this control."), Category("Other")]
		public ILayoutElement Model => _model;

		#endregion Properties

		#region Overrides

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.Input.Keyboard.GotKeyboardFocus"/> attached
		/// event reaches an element in its route that is derived from this class.
		/// Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> that contains the event data.</param>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			//if (_model?.SelectedItem != null) _model.SelectedItem.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
		/// event is raised on this element. Implement this method to add class handling
		/// for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
		/// The event data reports that the left mouse button was pressed.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			//if (!e.Handled && _model?.SelectedItem != null) _model.SelectedItem.IsActive = true;
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseRightButtonDown"/> routed
		/// event reaches an element in its route that is derived from this class. Implement
		/// this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The
		/// event data reports that the right mouse button was pressed.</param>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			//if (!e.Handled && _model?.SelectedItem != null) _model.SelectedItem.IsActive = true;
		}

		#endregion Overrides

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithActualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithActualSize.ActualWidth = ActualWidth;
			modelWithActualSize.ActualHeight = ActualHeight;
		}

		#endregion Private Methods
	}
}