/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>
	/// Implements an element in the layout model tree that can contain and arrange multiple
	/// <see cref="LayoutPaneComposite"/> elements in x or y directions, which in turn contain
	/// <see cref="LayoutAnchorable"/> elements.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutPaneComposite : LayoutPositionableGroup<LayoutAnchorable>
		, ILayoutAnchorableGroup
		, ILayoutContentSelector
		, ILayoutOrientableGroup
		, ILayoutPaneSerializable {
		#region fields

		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		private string _id;
		private string _name;
		private Orientation _orientation = Orientation.Vertical;
		private int _selectedIndex;
		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutPaneComposite() {
		}

		/// <summary>Class constructor <paramref name="firstChild"/> to be inserted into collection of children models.</summary>
		public LayoutPaneComposite(LayoutAnchorable firstChild, Orientation orientation = Orientation.Vertical) {
			Children.Add(firstChild);
			_orientation = orientation;
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets/sets the unique id that is used for the serialization of this panel.</summary>
		string ILayoutPaneSerializable.Id {
			get => _id;
			set => _id = value;
		}

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
				RaisePropertyChanged(nameof(Orientation));
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		/// <summary>Gets the selected content in the pane or null.</summary>
		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];

		public int SelectedIndex {
			get => _selectedIndex;
			set {
				if (value < 0 || value >= Children.Count)
					value = -1;

				if (value == _selectedIndex) {
					return;
				}

				RaisePropertyChanging(nameof(SelectedIndex));
				RaisePropertyChanging(nameof(SelectedContent));
				SetChildSelected(_selectedIndex, false);
				_selectedIndex = value;
				SetChildSelected(_selectedIndex, true);
				RaisePropertyChanged(nameof(SelectedIndex));
				RaisePropertyChanged(nameof(SelectedContent));
			}
		}

		public string Name {
			get => _name;
			set {
				if (value == _name)
					return;
				_name = value;
				RaisePropertyChanged(nameof(Name));
			}
		}

		public string Title => Name ?? Children.FirstOrDefault()?.Title ?? "默认";

		private void SetChildSelected(int index, bool selected) {
			if (index >= 0 && index < Children.Count)
				Children[index].IsSelected = selected;
		}
		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine($"AnchorableGroup({Name}, {Orientation})");

			foreach (LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}

		/// <summary>
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		public int IndexOf(LayoutContent content) {
			if (!(content is LayoutAnchorable anchorableChild))
				return -1;
			return Children.IndexOf(anchorableChild);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			writer.WriteAttributeString(nameof(Orientation), Orientation.ToString());
			if (_id != null) writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
			if (_name != null) writer.WriteAttributeString(nameof(Name), _name);
			base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			if (reader.MoveToAttribute(nameof(Orientation)))
				_orientation = (Orientation)Enum.Parse(typeof(Orientation), reader.Value, true);
			if (reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id))) _id = reader.Value;
			if (reader.MoveToAttribute(nameof(Name))) _name = reader.Value;
			//Debug.WriteLine($"{Name}, {Title}, {Orientation}, {_id}", "LayoutPaneComposite ReadXml");
			base.ReadXml(reader);
		}

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged() {
			AutoFixSelectedContent();
			for (var i = 0; i < Children.Count; i++) {
				if (!Children[i].IsSelected)
					continue;
				SelectedIndex = i;
				break;
			}

			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			base.OnChildrenCollectionChanged();
		}

		/// <inheritdoc />
		protected override void OnDockHeightChanged() {
			if (DockHeight.IsAbsolute && ChildrenCount == 1)
				((ILayoutPositionableElement)Children[0]).DockHeight = DockHeight;
			base.OnDockHeightChanged();
		}

		/// <inheritdoc />
		protected override void OnDockWidthChanged() {
			if (DockWidth.IsAbsolute && ChildrenCount == 1) {
				((ILayoutPositionableElement)Children[0]).DockWidth = DockWidth;
			}
			base.OnDockWidthChanged();
		}

		/// <inheritdoc />
		protected override void OnIsVisibleChanged() {
			UpdateParentVisibility();
			base.OnIsVisibleChanged();
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
#if TRACE
#endif

		#endregion Overrides

		#region Private Methods

		private void UpdateParentVisibility() {
			if (Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}

		#endregion Private Methods


		//public bool CanHide => true;
		//public bool CanClose => true;

		#region IsSelected

		private bool _isSelected = false;

		public event EventHandler IsSelectedChanged;

		public bool IsSelected {
			get => _isSelected;
			set {
				if (value == _isSelected)
					return;
				var oldValue = _isSelected;
				RaisePropertyChanging(nameof(IsSelected));
				_isSelected = value;
				if (_isSelected && Parent is ILayoutSelector<LayoutPaneComposite> parentSelector) {
					parentSelector.SelectedIndex = parentSelector.IndexOf(this);
				}
				if (!_isSelected) {
					IsActive = false;
				}
				OnIsSelectedChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsSelected));
				LayoutPaneCompositeTabItem.CancelMouseLeave();
			}
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the <see cref="IsSelected"/> property.
		/// </summary>
		protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue) => IsSelectedChanged?.Invoke(this, EventArgs.Empty);
		#endregion IsSelected

		#region ToolTip
		public string ToolTip => Title ?? string.Empty;
		#endregion ToolTip

		#region IsActive

		[field: NonSerialized]
		private bool _isActive = false;

		public event EventHandler IsActiveChanged;

		[XmlIgnore]
		public bool IsActive {
			get => _isActive;
			set {
				//Debug.WriteLine($"{_isActive}, {value}", $"LayoutPaneComposite IsActive 1");
				if (value == _isActive)
					return;
				RaisePropertyChanging(nameof(IsActive));
				var oldValue = _isActive;
				_isActive = value;
				var root = Root;
				if (root != null) {
					if (root.ActiveContent != SelectedContent && value)
						Root.ActiveContent = SelectedContent;
					if (_isActive && root.ActiveContent != SelectedContent)
						root.ActiveContent = SelectedContent;
				}
				if (_isActive) {
					IsSelected = true;
					IsVisible = true;
				}
				OnIsActiveChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsActive));
			}
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the <see cref="IsActive"/> property.
		/// </summary>
		protected virtual void OnIsActiveChanged(bool oldValue, bool newValue) {
			if (newValue)
				LastActivationTimeStamp = DateTime.Now;
			IsActiveChanged?.Invoke(this, EventArgs.Empty);
		}
		#endregion IsActive

		private DateTime? _lastActivationTimeStamp = null;
		private LayoutActivityTabItem _tabItem;
		public bool IsEnabled => true;
		public DateTime? LastActivationTimeStamp {
			get => _lastActivationTimeStamp;
			set {
				if (value == _lastActivationTimeStamp)
					return;
				_lastActivationTimeStamp = value;
				RaisePropertyChanged(nameof(LastActivationTimeStamp));
			}
		}
		public LayoutActivityTabItem TabItem {
			get => _tabItem;
			set {
				_tabItem = value;
				RaisePropertyChanged(nameof(TabItem));
				if (Parent is LayoutActivityBar activityBar) {
					activityBar.OverflowItems = null;
				}
			}
		}

		public override string ToString() {
			return Title;
		}

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

		/// <summary>Invalidates the current <see cref="SelectedContentIndex"/> and sets the index for the next avialable child with IsEnabled == true.</summary>
		internal void SetNextSelectedIndex() {
			SelectedIndex = -1;
			for (var i = 0; i < Children.Count; ++i) {
				if (!Children[i].IsEnabled)
					continue;
				SelectedIndex = i;
				return;
			}
		}

		/// <summary>
		/// Updates whether this object is hosted at the root level of a floating window control or not.
		/// </summary>
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Metho
		private void AutoFixSelectedContent() {
			if (!_autoFixSelectedContent)
				return;
			if (SelectedIndex >= ChildrenCount)
				SelectedIndex = Children.Count - 1;
			if (SelectedIndex == -1 && ChildrenCount > 0)
				SetLastActivatedIndex();
		}

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		/// <summary>Sets the current <see cref="SelectedContentIndex"/> to the last activated child with IsEnabled == true</summary>
		private void SetLastActivatedIndex() {
			var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
			SelectedIndex = Children.IndexOf(lastActivatedDocument);
		}
		#endregion
	}
}