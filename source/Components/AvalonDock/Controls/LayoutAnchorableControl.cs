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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AvalonDock.Controls {
	/// <inheritdoc cref="Expander"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Provides a control to display multible (or just one) LayoutAnchorable(s).
	/// See also <seealso cref="LayoutAnchorableControl"/>.
	/// </summary>
	/// <seealso cref="Expander"/>
	/// <seealso cref="ILayoutControl"/>
	public class LayoutAnchorableControl :Expander, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields

		//private readonly LayoutAnchorable _model;

		#endregion fields

		#region Constructors

		/// <summary>Static class constructor to register WPF style keys.</summary>
		static LayoutAnchorableControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorableControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(false));
		}

		public LayoutAnchorableControl() {
			Unloaded += LayoutAnchorableExplandControl_Unloaded;
			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
			SizeChanged += OnSizeChanged;
		}

		/// <summary>Class constructor from model and virtualization parameter.</summary>
		/// <param name="model"></param>
		/// <param name="IsVirtualizing">Whether tabbed items are virtualized or not.</param>
		internal LayoutAnchorableControl(LayoutAnchorable model, bool IsVirtualizing)
		: this() {
			//_model = model ?? throw new ArgumentNullException(nameof(model));
			Model = model;
			//SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			SetBinding(ExpandDirectionProperty, new Binding("Model.ExpandDirection") { Source = this, Mode = BindingMode.OneWay });
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
		}

		#endregion Constructors

		#region Properties

		///// <summary>Gets the layout model of this control.</summary>
		//[Bindable(true), Description("Gets the layout model of this control."), Category("Other")]
		//public ILayoutElement Model => _model;
		ILayoutElement ILayoutControl.Model => Model;

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(LayoutAnchorableControl),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>Gets/sets the model attached to this view.</summary>
		[Bindable(true), Description("Gets/sets the model attached to this view."), Category("Other")]
		public LayoutAnchorable Model {
			get => (LayoutAnchorable) GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableControl) d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			if(e.OldValue != null)
				((LayoutContent) e.OldValue).PropertyChanged -= Model_PropertyChanged;
			if(Model != null) {
				Model.PropertyChanged += Model_PropertyChanged;
				SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			} else
				SetLayoutItem(null);
		}

		private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(e.PropertyName != nameof(LayoutAnchorable.IsEnabled))
				return;
			if(Model == null)
				return;
			IsEnabled = Model.IsEnabled;
			if(IsEnabled || !Model.IsActive)
				return;
			if(Model.Parent != null && Model.Parent is LayoutPaneComposite group)
				group.SetNextSelectedIndex();
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableControl),
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

		#region Overrides

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.Input.Keyboard.GotKeyboardFocus"/> attached
		/// event reaches an element in its route that is derived from this class.
		/// Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> that contains the event data.</param>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e) {
			//if (_model?.SelectedItem != null) _model.SelectedItem.IsActive = true;
			if(Model != null)
				Model.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		#endregion Overrides

		#region Private Methods
		/// <summary>
		/// Executes when the element is removed from within an element tree of loaded elements.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LayoutAnchorableExplandControl_Unloaded(object sender, RoutedEventArgs e) {
			// prevent memory leak via event handler
			if(Model != null)
				Model.PropertyChanged -= Model_PropertyChanged;

			Unloaded -= LayoutAnchorableExplandControl_Unloaded;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			//var modelWithActualSize = _model as ILayoutPositionableElementWithActualSize;
			var modelWithActualSize = Model as ILayoutPositionableElementWithActualSize;
			//Debug.WriteLine($"{Model.Title}, {modelWithActualSize.DockWidth}, {ActualWidth}", "OnSizeChanged");
			modelWithActualSize.ActualWidth = ActualWidth;
			modelWithActualSize.ActualHeight = ActualHeight;
		}

		#endregion Private Methods
	}
}