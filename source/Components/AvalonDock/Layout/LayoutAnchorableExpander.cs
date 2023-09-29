/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Diagnostics;
using System.Windows;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Implements the model for a layout anchorable pane control (a pane in a tool window environment).
	/// A layout anchorable pane control can have multiple LayoutAnchorable controls  as its children.
	/// </summary>
	[Serializable]
	public class LayoutAnchorableExpander :LayoutPositionable,  ILayoutPositionableElement, ILayoutPaneSerializable {
		#region fields
		private string _name = null;
		private string _id;
		private bool _isExpanded;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutAnchorableExpander() {
			UpdateHeight();
		}

		/// <summary>Class constructor from <see cref="LayoutAnchorable"/> which will be added into its children collection.</summary>
		//public LayoutAnchorableExpander(LayoutAnchorable anchorable) : this() {
		//	//Children.Add(anchorable);
		//	Content = anchorable;
		//	Title = anchorable.Title;
		//	_isExpanded = true;
			
		//}

		#endregion Constructors

		//#region Title

		///// <summary><see cref="Title"/> dependency property.</summary>
		//public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(LayoutItem),
		//		new FrameworkPropertyMetadata(null, OnTitleChanged));

		///// <summary>Gets/sets the the title of the element.</summary>
		//[Bindable(true), Description("Gets/sets the the title of the element."), Category("Other")]
		//public string Title {
		//	get => (string) GetValue(TitleProperty);
		//	set => SetValue(TitleProperty, value);
		//}

		///// <summary>Handles changes to the <see cref="Title"/> property.</summary>
		//private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutItem) d).OnTitleChanged(e);

		///// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Title"/> property.</summary>
		//protected virtual void OnTitleChanged(DependencyPropertyChangedEventArgs e) {
		//	if(LayoutElement != null)
		//		LayoutElement.Title = (string) e.NewValue;
		//}

		//#endregion Title

		#region Properties

		public bool IsExpanded {
			get => _isExpanded;
			set {
				if(_isExpanded == value)
					return;

				RaisePropertyChanging(nameof(IsExpanded));
				_isExpanded = value;
				//Debug.WriteLine($"==========================================", "LayoutAnchorableExpanderPane IsExpanded");
				//Debug.WriteLine($"{_isExpanded}, {DockHeight}", "LayoutAnchorableExpanderPane IsExpanded 1");
				UpdateHeight();
				//Debug.WriteLine($"{_isExpanded}, {DockHeight}", "LayoutAnchorableExpanderPane IsExpanded 2");
				RaisePropertyChanged(nameof(IsExpanded));
			}
		}

		private void UpdateHeight() {
			//Debug.WriteLine($"{LayoutAnchorable?.Title}", "LayoutAnchorableExpanderPane UpdateHeight 0");
			//Debug.WriteLine($"{DockHeight}", "LayoutAnchorableExpanderPane UpdateHeight 1");

			if(IsExpanded) {
				var t = new GridLength(1.0, GridUnitType.Star);
				if(DockHeight == t) {
					DockHeight = new GridLength(1.0, GridUnitType.Pixel);
				}
				//Debug.WriteLine($"{DockHeight}, {t}", "LayoutAnchorableExpanderPane UpdateHeight 2");
				DockHeight = t;
				// DockMinHeight = double.MaxValue;
			} else {
				var  t = new GridLength(25, GridUnitType.Pixel);
				//Debug.WriteLine($"{DockHeight}, {t}", "LayoutAnchorableExpanderPane UpdateHeight 3");
				DockHeight = t;

				// Dock
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