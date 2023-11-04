//using AvalonDock.Layout;
//using System;
//using System.ComponentModel;
//using System.Windows;
//using System.Windows.Controls.Primitives;

//namespace AvalonDock.Controls {
//	public class LayoutToggleButtonControl :ToggleButton {
//		#region fields


//		#endregion fields

//		/// <summary>Class constructor from model and virtualization parameter.</summary>
//		/// <param name="model"></param>
//		/// <param name="IsVirtualizing">Whether tabbed items are virtualized or not.</param>
//		public LayoutToggleButtonControl()
//		//: base(IsVirtualizing)
//		{
//			//SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
//			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
//			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
//		}

//		#region Model

//		/// <summary><see cref="Model"/> dependency property.</summary>
//		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorableExpanderGroupPane), typeof(LayoutToggleButtonControl),
//				new FrameworkPropertyMetadata(null, OnModelChanged));

//		/// <summary>Gets/sets the model attached to this view.</summary>
//		[Bindable(true), Description("Gets/sets the model attached to this view."), Category("Other")]
//		public LayoutAnchorableExpanderGroupPane Model {
//			get => (LayoutAnchorableExpanderGroupPane) GetValue(ModelProperty);
//			set => SetValue(ModelProperty, value);
//		}

//		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
//		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutToggleButtonControl) d).OnModelChanged(e);

//		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
//		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
//			if(e.OldValue != null)
//				((LayoutContent) e.OldValue).PropertyChanged -= Model_PropertyChanged;
//			if(Model != null) {
//				Model.PropertyChanged += Model_PropertyChanged;
//			}
//		}

//		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
//			//if(e.PropertyName != nameof(LayoutAnchorable.IsEnabled))
//			//	return;
//			//if(Model == null)
//			//	return;
//			//IsEnabled = Model.IsEnabled;
//			//if(IsEnabled || !Model.IsActive)
//			//	return;
//			//switch(e.PropertyName) {
//			//	case nameof(Model.IsActive):
//			//		break;

//			//	default:
//			//		break;
//			//}
//		}

//		#endregion Model

//		#region Methods

//		/// <inheritdoc/>
//		//protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
//		//{
//		//	//if(Model != null)
//		//	//	Model.IsActive = true;
//		//	base.OnGotKeyboardFocus(e);
//		//}

//		/// <summary>
//		/// Executes when the element is removed from within an element tree of loaded elements.
//		/// </summary>
//		/// <param name="sender"></param>
//		/// <param name="e"></param>
//		private void LayoutAnchorableExplandControl_Unloaded(object sender, RoutedEventArgs e) {
//			// prevent memory leak via event handler
//			if(Model != null)
//				Model.PropertyChanged -= Model_PropertyChanged;

//			Unloaded -= LayoutAnchorableExplandControl_Unloaded;
//		}

//		#endregion Methods

//	}
//}
