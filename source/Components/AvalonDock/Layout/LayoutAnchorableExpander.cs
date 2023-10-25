/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Implements the model for a layout anchorable pane control (a pane in a tool window environment).
	/// A layout anchorable pane control can have multiple LayoutAnchorable controls  as its children.
	/// </summary>
	[Serializable]
	public class LayoutAnchorableExpander :LayoutPositionable,  ILayoutPositionableElement, IExpander, ILayoutPaneSerializable {
		#region fields
		private string _name = null;
		private string _id;
		private bool _isExpanded;
		//private ExpandDirection _expandDirection = ExpandDirection.Down;
		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutAnchorableExpander() {
			UpdateSize();
		}

		#endregion Constructors

		#region Properties

		public ExpandDirection ExpandDirection {
			get {
				if(Parent is ILayoutOrientableGroup orientableGroup && Orientation.Horizontal == orientableGroup.Orientation) {
					return ExpandDirection.Right;
				} else {
					return ExpandDirection.Down;
				}
			}


			//set {
			//	if(_expandDirection == value)
			//		return;

			//	RaisePropertyChanging(nameof(ExpandDirection));
			//	_expandDirection = value;
			//	UpdateSize();
			//	RaisePropertyChanged(nameof(ExpandDirection));
			//}
		}

		public bool IsExpanded {
			get => _isExpanded;
			set {
				if(_isExpanded == value)
					return;

				RaisePropertyChanging(nameof(IsExpanded));
				_isExpanded = value;
				//Debug.WriteLine($"==========================================", "LayoutAnchorableExpanderPane IsExpanded");
				//Debug.WriteLine($"{_isExpanded}, {DockHeight}", "LayoutAnchorableExpanderPane IsExpanded 1");
				UpdateSize();
				//Debug.WriteLine($"{_isExpanded}, {DockHeight}", "LayoutAnchorableExpanderPane IsExpanded 2");
				RaisePropertyChanged(nameof(IsExpanded));
			}
		}

		private void UpdateSize() {
			//Debug.WriteLine($"{LayoutAnchorable?.Title}", "LayoutAnchorableExpanderPane UpdateSize 0");
			if(IsExpanded) {
				if(ExpandDirection == ExpandDirection.Right) {
					//ForceWidth(new GridLength(1.0, GridUnitType.Star));
					ForceWidth(new GridLength(0, GridUnitType.Auto));
				} else {
					//ForceHeight(new GridLength(1.0, GridUnitType.Star));
					ForceHeight(new GridLength(0, GridUnitType.Auto));
				}
			} else {
				if(ExpandDirection == ExpandDirection.Right) {
					//DockWidth = new GridLength(25, GridUnitType.Pixel);
					DockWidth = new GridLength(0, GridUnitType.Auto);
				} else {
					//DockHeight = new GridLength(25, GridUnitType.Pixel);
					DockHeight = new GridLength(0, GridUnitType.Auto);
				}
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		public string Name {
			get => _name;
			set {
				if(value == _name)
					return;
				_name = value;
				RaisePropertyChanged(nameof(Name));
			}
		}

		/// <summary>Gets/sets the unique id that is used for the serialization of this panel.</summary>
		string ILayoutPaneSerializable.Id {
			get => _id;
			set => _id = value;
		}

		#endregion Properties

		#region Overrides


		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
			if(oldValue is ILayoutGroup oldGroup)
				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if(newValue is ILayoutGroup newGroup)
				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;

			RaisePropertyChanged(nameof(ExpandDirection));
			UpdateSize();
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		//public override void WriteXml(System.Xml.XmlWriter writer) {
		//	if(_id != null)
		//		writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
		//	if(_name != null)
		//		writer.WriteAttributeString(nameof(Name), _name);
		//	base.WriteXml(writer);
		//}

		///// <inheritdoc />
		//public override void ReadXml(System.Xml.XmlReader reader) {
		//	if(reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id)))
		//		_id = reader.Value;
		//	if(reader.MoveToAttribute(nameof(Name)))
		//		_name = reader.Value;
		//	_autoFixSelectedContent = false;
		//	base.ReadXml(reader);
		//	_autoFixSelectedContent = true;
		//	AutoFixSelectedContent();
		//}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("AnchorablePane()");

			//foreach(LayoutElement child in Children)
			//	child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Public Methods

		/// <summary>
		/// Gets whether the model hosts only 1 <see cref="LayoutAnchorable"/> (True)
		/// or whether there are more than one <see cref="LayoutAnchorable"/>s below
		/// this model pane.
		/// </summary>
		public bool IsDirectlyHostedInFloatingWindow {
			get {
				var parentFloatingWindow = this.FindParent<LayoutAnchorableFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
			}
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Updates whether this object is hosted at the root level of a floating window control or not.
		/// </summary>
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods
	}
}