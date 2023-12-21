using AvalonDock.Layout;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvalonDock.Controls {
	public class AnchorableGroupPaneTitle :Control {

		#region Constructors

		/// <summary>
		/// Static class constructor
		/// </summary>
		static AnchorableGroupPaneTitle() {
			IsHitTestVisibleProperty.OverrideMetadata(typeof(AnchorableGroupPaneTitle), new FrameworkPropertyMetadata(true));
			FocusableProperty.OverrideMetadata(typeof(AnchorableGroupPaneTitle), new FrameworkPropertyMetadata(false));
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorableGroupPaneTitle), new FrameworkPropertyMetadata(typeof(AnchorableGroupPaneTitle)));
		}

		#endregion Constructors

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutPaneComposite), typeof(AnchorableGroupPaneTitle),
				new FrameworkPropertyMetadata(null, _OnModelChanged));

		/// <summary>Gets/sets the <see cref="LayoutPaneComposite"/> model attached of this view.</summary>
		[Bindable(true), Description("Gets/sets the LayoutAnchorable model attached of this view."), Category("Anchorable")]
		public LayoutPaneComposite Model {
			get => (LayoutPaneComposite) GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		private static void _OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((AnchorableGroupPaneTitle) sender).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			//if(Model != null)
			//	SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			//else
			//	SetLayoutItem(null);
		}

		#endregion Model
	}
}
