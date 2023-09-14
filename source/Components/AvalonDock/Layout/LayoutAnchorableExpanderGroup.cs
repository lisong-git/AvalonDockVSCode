/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Controls;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>
	/// Implements an element in the layout model tree that can contain and arrange multiple
	/// <see cref="LayoutAnchorablePane"/> elements in x or y directions, which in turn contain
	/// <see cref="LayoutAnchorable"/> elements.
	/// </summary>
	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutAnchorableExpanderGroup :LayoutPositionableGroup<LayoutAnchorableExpander>, ILayoutAnchorablePane, ILayoutContentSelector, ILayoutOrientableGroup {
		#region fields

		private Orientation _orientation;
		private LayoutContent _current;
		private int _selectedIndex;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutAnchorableExpanderGroup() {
		}

		/// <summary>Class constructor <paramref name="firstChild"/> to be inserted into collection of children models.</summary>
		public LayoutAnchorableExpanderGroup(LayoutAnchorableExpander firstChild) {
			Children.Add(firstChild);
		}

		#endregion Constructors

		#region Properties

		//public LayoutContent SelectedContent {
		//	get => _current ?? Children.First(); 
		//	//set => _current = value;
		//}

		public int SelectedContentIndex {
			get => _selectedIndex;
			set {
				if(value < 0 || value >= Children.Count)
					value = -1;
				if(value == _selectedIndex)
					return;
				RaisePropertyChanging(nameof(SelectedContentIndex));
				RaisePropertyChanging(nameof(SelectedContent));
				if(_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = false;
				_selectedIndex = value;
				if(_selectedIndex >= 0 && _selectedIndex < Children.Count)
					Children[_selectedIndex].IsSelected = true;
				RaisePropertyChanged(nameof(SelectedContentIndex));
				RaisePropertyChanged(nameof(SelectedContent));
			}
		}

		/// <summary>Gets the selected content in the pane or null.</summary>
		public LayoutContent SelectedContent => _selectedIndex == -1 ? null : Children[_selectedIndex];
		/// <summary>
		/// Gets/sets the <see cref="System.Windows.Controls.Orientation"/> of this object.
		/// </summary>
		public Orientation Orientation {
			get => Orientation.Vertical;
			set {
				//if(value == _orientation)
				//	return;
				//RaisePropertyChanging(nameof(Orientation));
				//_orientation = value;
				//RaisePropertyChanged(nameof(Orientation));
			}
		}

		#endregion Properties

		#region Overrides

		/// <summary>
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		public int IndexOf(LayoutContent content) {
			if(!(content is LayoutAnchorableExpander anchorableChild))
				return -1;
			return Children.IndexOf(anchorableChild);
		}

		/// <inheritdoc />
		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

		/// <inheritdoc />
		protected override void OnIsVisibleChanged() {
			UpdateParentVisibility();
			base.OnIsVisibleChanged();
		}

		/// <inheritdoc />
		protected override void OnDockWidthChanged() {
			if(DockWidth.IsAbsolute && ChildrenCount == 1) {
				((ILayoutPositionableElement) Children[0]).DockWidth = DockWidth;
			}
			base.OnDockWidthChanged();
		}

		/// <inheritdoc />
		protected override void OnDockHeightChanged() {
			if(DockHeight.IsAbsolute && ChildrenCount == 1)
				((ILayoutPositionableElement) Children[0]).DockHeight = DockHeight;
			base.OnDockHeightChanged();
		}

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged() {
			if(DockWidth.IsAbsolute && ChildrenCount == 1)
				((ILayoutPositionableElement) Children[0]).DockWidth = DockWidth;
			if(DockHeight.IsAbsolute && ChildrenCount == 1)
				((ILayoutPositionableElement) Children[0]).DockHeight = DockHeight;
			base.OnChildrenCollectionChanged();
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			writer.WriteAttributeString(nameof(Orientation), Orientation.ToString());
			base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			if(reader.MoveToAttribute(nameof(Orientation)))
				Orientation = (Orientation) Enum.Parse(typeof(Orientation), reader.Value, true);
			base.ReadXml(reader);
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine(string.Format("AnchorableExpanderPane({0})", Orientation));

			foreach(LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Private Methods

		private void UpdateParentVisibility() {
			if(Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}

		#endregion Private Methods


		public bool CanHide => true;
		public bool CanClose => true;

		#region IsSelected

		private bool _isSelected = false;

		public bool IsSelected {
			get => _isSelected;
			set {
				if(value == _isSelected)
					return;
				var oldValue = _isSelected;
				RaisePropertyChanging(nameof(IsSelected));
				_isSelected = value;
				//if(Parent is ILayoutContentSelector parentSelector)
				//	parentSelector.SelectedContentIndex = _isSelected ? parentSelector.IndexOf(this) : -1;
				OnIsSelectedChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsSelected));
				LayoutAnchorableTabItem.CancelMouseLeave();
			}
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the <see cref="IsSelected"/> property.
		/// </summary>
		protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue) => IsSelectedChanged?.Invoke(this, EventArgs.Empty);

		public event EventHandler IsSelectedChanged;

		#endregion IsSelected

		#region IsActive

		[field: NonSerialized]
		private bool _isActive = false;

		[XmlIgnore]
		public bool IsActive {
			get => _isActive;
			set {
				if(value == _isActive)
					return;
				RaisePropertyChanging(nameof(IsActive));
				var oldValue = _isActive;
				_isActive = value;
				var root = Root;
				if(root != null) {
					if(root.ActiveContent != SelectedContent && value)
						Root.ActiveContent = SelectedContent;
					if(_isActive && root.ActiveContent != SelectedContent)
						root.ActiveContent = SelectedContent;
				}
				if(_isActive)
					IsSelected = true;
				OnIsActiveChanged(oldValue, value);
				RaisePropertyChanged(nameof(IsActive));
			}
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the <see cref="IsActive"/> property.
		/// </summary>
		protected virtual void OnIsActiveChanged(bool oldValue, bool newValue) {
			if(newValue)
				LastActivationTimeStamp = DateTime.Now;
			IsActiveChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler IsActiveChanged;

		#endregion IsActive

		public bool IsEnabled => true;

		private DateTime? _lastActivationTimeStamp = null;

		public DateTime? LastActivationTimeStamp {
			get => _lastActivationTimeStamp;
			set {
				if(value == _lastActivationTimeStamp)
					return;
				_lastActivationTimeStamp = value;
				RaisePropertyChanged(nameof(LastActivationTimeStamp));
			}
		}
	}
}