/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Implements the model for a layout anchorable pane control (a pane in a tool window environment).
	/// A layout anchorable pane control can have multiple LayoutAnchorable controls  as its children.
	/// </summary>
	//[ContentProperty(nameof(Content))]
	[Serializable]
	public class LayoutAnchorableExpander :LayoutPositionable,  ILayoutPositionableElement, ILayoutPaneSerializable {
		#region fields

		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		private string _name = null;
		private string _id;
		private bool _isExpanded;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutAnchorableExpander() {
			//UpdateHeight();
		}

		/// <summary>Class constructor from <see cref="LayoutAnchorable"/> which will be added into its children collection.</summary>
		public LayoutAnchorableExpander(LayoutAnchorable anchorable) : this() {
			//Children.Add(anchorable);
			//_isExpanded = anchorable.IsExpanded;
			Content = anchorable;
		}

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

		/// <summary>Gets whether the pane can be hidden.</summary>
		//public bool CanHide => Children.All(a => a.CanHide);

		///// <summary>Gets whether the pane can be closed.</summary>
		//public bool CanClose => Children.All(a => a.CanClose);

		//private LayoutAnchorable LayoutAnchorable => Children.FirstOrDefault();
		//private LayoutAnchorable LayoutAnchorable => this;

		//public LayoutAnchorable Content {
		//	get => Children.FirstOrDefault();
		//	set {
		//		if(value == Children.FirstOrDefault())
		//			return;

		//		RaisePropertyChanging(nameof(Content));
		//		Children.Add(value);
		//		RaisePropertyChanged(nameof(Content));
		//	}
		//}

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
			Debug.WriteLine($"{DockHeight}", "LayoutAnchorableExpanderPane UpdateHeight 1");

			if(IsExpanded) {
				var t = new GridLength(1.0, GridUnitType.Star);
				if(DockHeight == t) {
					DockHeight = new GridLength(1.0, GridUnitType.Pixel);
				}
				Debug.WriteLine($"{DockHeight}, {t}", "LayoutAnchorableExpanderPane UpdateHeight 2");
				DockHeight = t;
				// DockMinHeight = double.MaxValue;
			} else {
				var  t = new GridLength(25, GridUnitType.Pixel);
				Debug.WriteLine($"{DockHeight}, {t}", "LayoutAnchorableExpanderPane UpdateHeight 3");
				DockHeight = t;

				// Dock
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		//public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

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

		///// <summary>Gets or sets the index of the selected content in the pane.</summary>
		//public int SelectedContentIndex
		//{
		//	get => _selectedIndex;
		//	set
		//	{
		//		if (value < 0 || value >= Children.Count) value = -1;
		//		if (value == _selectedIndex) return;
		//		RaisePropertyChanging(nameof(SelectedContentIndex));
		//		RaisePropertyChanging(nameof(SelectedContent));
		//		if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
		//			Children[_selectedIndex].IsSelected = false;
		//		_selectedIndex = value;
		//		if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
		//			Children[_selectedIndex].IsSelected = true;
		//		RaisePropertyChanged(nameof(SelectedContentIndex));
		//		RaisePropertyChanged(nameof(SelectedContent));
		//	}
		//}

		/// <summary>Gets the selected content in the pane or null.</summary>
		//public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		//public LayoutContent SelectedContent => Children?.Count == 0 ? null : Children[0];
		/// <summary>Gets/sets the unique id that is used for the serialization of this panel.</summary>
		string ILayoutPaneSerializable.Id {
			get => _id;
			set => _id = value;
		}

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		//protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		/// <inheritdoc />
		//protected override void ChildMoved(int oldIndex, int newIndex)
		//{
		//	if (_selectedIndex == oldIndex)
		//	{
		//		RaisePropertyChanging(nameof(SelectedContentIndex));
		//		_selectedIndex = newIndex;
		//		RaisePropertyChanged(nameof(SelectedContentIndex));
		//	}
		//	base.ChildMoved(oldIndex, newIndex);
		//}

		/// <inheritdoc />
		//protected override void OnChildrenCollectionChanged() {
		//	AutoFixSelectedContent();
		//	for(var i = 0; i < Children.Count; i++) {
		//		if(!Children[i].IsSelected)
		//			continue;
		//		//SelectedContentIndex = i;
		//		break;
		//	}
		//	RaisePropertyChanged(nameof(CanClose));
		//	RaisePropertyChanged(nameof(CanHide));
		//	RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
		//	base.OnChildrenCollectionChanged();
		//}

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
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		//public int IndexOf(LayoutContent content) {
		//	if(!(content is LayoutAnchorable anchorableChild))
		//		return -1;
		//	return Children.IndexOf(anchorableChild);
		//}

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

		/// <summary>Invalidates the current <see cref="SelectedContentIndex"/> and sets the index for the next avialable child with IsEnabled == true.</summary>
		//internal void SetNextSelectedIndex()
		//{
		//	SelectedContentIndex = -1;
		//	for (var i = 0; i < Children.Count; ++i)
		//	{
		//		if (!Children[i].IsEnabled) continue;
		//		SelectedContentIndex = i;
		//		return;
		//	}
		//}

		/// <summary>
		/// Updates whether this object is hosted at the root level of a floating window control or not.
		/// </summary>
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods

		private void AutoFixSelectedContent() {
			if(!_autoFixSelectedContent)
				return;
			//if (SelectedContentIndex >= ChildrenCount) SelectedContentIndex = Children.Count - 1;
			//if (SelectedContentIndex == -1 && ChildrenCount > 0) SetLastActivatedIndex();
		}

		/// <summary>Sets the current <see cref="SelectedContentIndex"/> to the last activated child with IsEnabled == true</summary>
		//private void SetLastActivatedIndex()
		//{
		//	var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
		//	SelectedContentIndex = Children.IndexOf(lastActivatedDocument);
		//}

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods
	}
}