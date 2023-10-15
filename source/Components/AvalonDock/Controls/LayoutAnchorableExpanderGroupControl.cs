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

namespace AvalonDock.Controls {
	/// <summary>
	/// Implements a group control that hosts a <see cref="LayoutAnchorablePaneGroup"/> model.
	///
	/// This Grid based control can host multiple other controls in its Children collection
	/// (<see cref="LayoutAnchorableControl"/>).
	/// </summary>
	public class LayoutAnchorableExpanderGroupControl :ExpanderGridControl<LayoutAnchorableExpander> {
		#region fields


		#endregion fields

		#region Constructors


		public LayoutAnchorableExpanderGroupControl() {
			//Debug.WriteLine($"1", "LayoutAnchorableExpanderGroupControl");
		}


		#endregion Constructors

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorableExpanderGroup), typeof(LayoutAnchorableExpanderGroupControl),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>Gets/sets the model attached to this view.</summary>
		[Bindable(true), Description("Gets/sets the model attached to this view."), Category("Other")]
		public override ILayoutElement Model {
			get => (LayoutAnchorableExpanderGroup) GetValue(ModelProperty);
			set {
				Debug.WriteLine($"{value.GetType()}", "LayoutAnchorableExpanderGroupControl Model");
				SetValue(ModelProperty, value);
				base.Model = value;
			}
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableExpanderGroupControl) d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e) {
			//if(e.OldValue != null)
			//	((LayoutContent) e.OldValue).PropertyChanged -= Model_PropertyChanged;
			//if(Model != null) {
			//	Model.PropertyChanged += Model_PropertyChanged;
			//SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			//} else
			//SetLayoutItem(null);
			//Debug.WriteLine($"{e.NewValue?.GetType()}", "LayoutAnchorableExpanderGroupControl OnOverflowOpenChanged");
			if(e.NewValue is LayoutAnchorableExpanderGroup model) {
				base.Model = model;
			}
		}

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName != nameof(LayoutAnchorable.IsEnabled))
				return;
			if(Model == null)
				return;
			//IsEnabled = Model.IsEnabled;
			//iIsEnabled || !Model.IsActive)
			//	return;f(
			if(Model.Parent != null && Model.Parent is LayoutAnchorablePane layoutAnchorablePane)
				layoutAnchorablePane.SetNextSelectedIndex();
		}

		#endregion Model

		public double EmptyLength() {
		 return ActualHeight - Children.OfType<FrameworkElement>().Sum(o => o.ActualHeight);
		}

		#region Overrides

		protected override void OnFixChildrenDockLengths() {
			var model = Model as LayoutAnchorableExpanderGroup;
			if(model == null)
				return;

			if(model.Orientation == Orientation.Horizontal) {
				// Setup DockWidth for children
				for(int i = 0; i < model.Children.Count; i++) {
					var childModel = model.Children[i] as ILayoutPositionableElement;
					if(!childModel.DockWidth.IsStar) {
						childModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
					}
				}
			} else {
				// Setup DockHeight for children
				for(int i = 0; i < model.Children.Count; i++) {
					var childModel = model.Children[i] as ILayoutPositionableElement;
					if(!childModel.DockHeight.IsStar) {
						childModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
					}
				}
			}
		}

		#endregion Overrides
	}
}