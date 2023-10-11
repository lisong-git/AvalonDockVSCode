/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AvalonDock.Controls {
	/// <inheritdoc cref="TabControl"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Provides a control to display multible (or just one) LayoutAnchorable(s).
	/// See also <seealso cref="AnchorablePaneTabPanel"/>.
	/// </summary>
	/// <seealso cref="TabControlEx"/>
	/// <seealso cref="ILayoutControl"/>
	public class LayoutActivityBarControl :TabControlEx, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields

		private readonly LayoutActivityBar _model;

		#endregion fields

		#region Constructors

		/// <summary>Static class constructor to register WPF style keys.</summary>
		static LayoutActivityBarControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutActivityBarControl), new FrameworkPropertyMetadata(typeof(LayoutActivityBarControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutActivityBarControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>Class constructor from model and virtualization parameter.</summary>
		/// <param name="model"></param>
		/// <param name="IsVirtualizing">Whether tabbed items are virtualized or not.</param>
		internal LayoutActivityBarControl(LayoutActivityBar model, bool IsVirtualizing = false) {
			_model = model ?? throw new ArgumentNullException(nameof(model));
			SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			//SetBinding(SelectedIndexProperty, new Binding("Model.SelectedIndex") { Source = this });
			//SetBinding(ContentProperty, new Binding("Model") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			//SizeChanged += OnSizeChanged;
		}

		#endregion Constructors



		#region Properties

		/// <summary>Gets the layout model of this control.</summary>
		[Bindable(false), Description("Gets the layout model of this control."), Category("Other")]
		public ILayoutElement Model => _model;


		#region HasOverflowItems

		/// <summary><see cref="HasOverflowItems"/> dependency property.</summary>
		public static readonly DependencyProperty HasOverflowItemsProperty = DependencyProperty.Register(nameof(HasOverflowItems), typeof(bool), typeof(LayoutActivityBarControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>Gets/sets the model attached to the anchorable tab item.</summary>
		[Bindable(true), Description("Gets/sets the model attached to the anchorable tab item."), Category("Other")]
		public bool HasOverflowItems {
			get => (bool) GetValue(HasOverflowItemsProperty);
		  private set => SetValue(HasOverflowItemsProperty, value);
		}

		#endregion HasOverflowItems

		#region IsOverflowOpen

		/// <summary><see cref="IsOverflowOpen"/> dependency property.</summary>
		public static readonly DependencyProperty IsOverflowOpenProperty = DependencyProperty.Register(nameof(IsOverflowOpen), typeof(bool), typeof(LayoutActivityBarControl),
				new FrameworkPropertyMetadata(false));

		/// <summary>Gets/sets the model attached to the anchorable tab item.</summary>
		[Bindable(true), Description("Gets/sets the model attached to the anchorable tab item."), Category("Other")]
		public bool IsOverflowOpen {
			get => (bool) GetValue(IsOverflowOpenProperty);
			set => SetValue(IsOverflowOpenProperty, value);
		}

		#endregion IsOverflowOpen

		#endregion Properties

		#region Overrides

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.Input.Keyboard.GotKeyboardFocus"/> attached
		/// event reaches an element in its route that is derived from this class.
		/// Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> that contains the event data.</param>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e) {
			base.OnGotKeyboardFocus(e);
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
		/// event is raised on this element. Implement this method to add class handling
		/// for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
		/// The event data reports that the left mouse button was pressed.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseRightButtonDown"/> routed
		/// event reaches an element in its route that is derived from this class. Implement
		/// this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The
		/// event data reports that the right mouse button was pressed.</param>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e) {
			base.OnMouseRightButtonDown(e);
		}

		#endregion Overrides

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			var modelWithActualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithActualSize.ActualWidth = ActualWidth;
			modelWithActualSize.ActualHeight = ActualHeight;
		}

		#endregion Private Methods

		//public IEnumerable GetOverflowItems() {

		//}
	}
}