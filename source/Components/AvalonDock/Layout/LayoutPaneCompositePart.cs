/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Implements the model for a layout anchorable pane control (a pane in a tool window environment).
	/// A layout anchorable pane control can have multiple LayoutAnchorable controls  as its children.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutPaneCompositePart : LayoutPositionableGroup<LayoutPaneComposite>
		, IPaneCompositePart
		, ILayoutPanelElement
		, ILayoutPositionableElement
		, ILayoutSelector<LayoutPaneComposite>
		, ILayoutPaneSerializable {
		#region fields

		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		private string _name = null;
		private string _title = null;
		private string _id;
		private int _selectedIndex = -1;

		private bool _isActive = false;
		private Orientation _orientation = Orientation.Vertical;

		#endregion fields
		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutPaneCompositePart() : base() {
		}

		public LayoutPaneCompositePart(LayoutPaneComposite anchorableExpanderGroup) : this() {
			Children.Add(anchorableExpanderGroup);
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets/sets the <see cref="System.Windows.Controls.Orientation"/> of this object.
		/// </summary>
		public Orientation Orientation {
			get => _orientation;
			set {
				if (value == _orientation)
					return;
				RaisePropertyChanging(nameof(Orientation));
				_orientation = value;
				UpdateChildrenOrientation();
				RaisePropertyChanged(nameof(Orientation));
			}
		}

		public string Title {
			get => _title ?? Children.FirstOrDefault()?.Title ?? "Empty Title";
			set {
				if (value == _title)
					return;
				_title = value;
				RaisePropertyChanged(nameof(Title));
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		public string Name {
			get => _name;
			set {
				if (value == _name)
					return;
				_name = value;
				RaisePropertyChanged(nameof(Name));
			}
		}

		public bool IsActive {
			get => _isActive;
			set {
				if (value == _isActive)
					return;

				RaisePropertyChanging(nameof(IsActive));
				_isActive = value;

				if (_isActive) {
					IsVisible = true;
				}
				RaisePropertyChanged(nameof(IsActive));
			}
		}

		public void SetVisible(bool isVisible) {
			IsVisible = isVisible;
		}

		/// <summary>Gets or sets the index of the selected content in the pane.</summary>
		public int SelectedContentIndex {
			get => _selectedIndex;
			set {
				if (value < 0 || value >= Children.Count)
					value = -1;
				if (value == _selectedIndex)
					return;
				RaisePropertyChanging(nameof(SelectedContentIndex));
				//RaisePropertyChanging(nameof(SelectedContent));
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = false;


				_selectedIndex = value;
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedContentIndex));
				//RaisePropertyChanged(nameof(SelectedContent));
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
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		/// <inheritdoc />
		protected override void ChildMoved(int oldIndex, int newIndex) {
			if (_selectedIndex == oldIndex) {
				RaisePropertyChanging(nameof(SelectedContentIndex));
				_selectedIndex = newIndex;
				RaisePropertyChanged(nameof(SelectedContentIndex));
			}
			base.ChildMoved(oldIndex, newIndex);
		}

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged() {
			AutoFixSelectedContent();
			for (var i = 0; i < Children.Count; i++) {
				if (!Children[i].IsSelected)
					continue;
				SelectedContentIndex = i;
				break;
			}

			UpdateChildrenOrientation();

			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			RaisePropertyChanged(nameof(Title));
			base.OnChildrenCollectionChanged();
		}

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
			if (oldValue is ILayoutGroup oldGroup)
				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup)
				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			if (_id != null)
				writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (_name != null)
				writer.WriteAttributeString(nameof(Name), _name);
			base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id)))
				_id = reader.Value;
			if (reader.MoveToAttribute(nameof(Name)))
				_name = reader.Value;
			_autoFixSelectedContent = false;
			base.ReadXml(reader);
			_autoFixSelectedContent = true;
			AutoFixSelectedContent();
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine($"AnchorableGroupPane({Name})");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Public Methods

		public LayoutPaneComposite GetPaneComposite(string id) {
			return Children.SingleOrDefault(o => o.Name == id);
		}


		/// <summary>
		/// Gets whether the model hosts only 1 <see cref="LayoutAnchorable"/> (True)
		/// or whether there are more than one <see cref="LayoutAnchorable"/>s below
		/// this model pane.
		/// </summary>
		public bool IsDirectlyHostedInFloatingWindow {
			get {
				var parentFloatingWindow = this.FindParent<LayoutAnchorableFloatingWindow>();
				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
			}
		}


		//public int SelectedIndex {
		//	get => _selectedIndex;
		//	set {
		//		if(_selectedIndex != value) {
		//			_selectedIndex = value;
		//			RaisePropertyChanged(nameof(SelectedIndex));
		//			RaisePropertyChanged(nameof(SelectedItem));
		//		}
		//	}
		//}

		public int SelectedIndex {
			get => _selectedIndex;
			set {
				if (value < 0 || value >= Children.Count) value = -1;
				if (value == _selectedIndex) return;
				RaisePropertyChanging(nameof(SelectedIndex));
				RaisePropertyChanging(nameof(SelectedItem));
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = false;
				_selectedIndex = value;
				if (_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedIndex));
				RaisePropertyChanged(nameof(SelectedItem));
			}
		}

		public LayoutPaneComposite SelectedItem => _selectedIndex == -1 ? null : Children[_selectedIndex];

		/// <summary>
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		public int IndexOf(LayoutPaneComposite content) {
			return Children.IndexOf(content);
		}
		#endregion Public Methods

		#region Internal Methods

		#endregion Internal Methods

		#region Private Methods

		protected void AutoFixSelectedContent() {
			if (!_autoFixSelectedContent)
				return;
			if (SelectedContentIndex >= ChildrenCount)
				SelectedContentIndex = Children.Count - 1;
			if (SelectedContentIndex == -1 && ChildrenCount > 0)
				SetLastActivatedIndex();
		}

		/// <summary>Sets the current <see cref="SelectedContentIndex"/> to the last activated child with IsEnabled == true</summary>
		private void SetLastActivatedIndex() {
			var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
			SelectedContentIndex = Children.IndexOf(lastActivatedDocument);
		}

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		private void UpdateChildrenOrientation() {
			foreach (var child in Children) {
				child.Orientation = _orientation;
			}
		}


		/// <summary>
		/// Updates whether this object is hosted at the root level of a floating window control or not.
		/// </summary>
		//internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods
	}
}