using AvalonDock.Commands;
using AvalonDock.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout {

	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutActivityBar :LayoutGroup<LayoutAnchorableGroup>, ILayoutPane, ILayoutSelector<LayoutAnchorableGroup> {
		#region fields

		//private string _id;
		private int _selectedIndex = -1;
		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		#endregion fields

		#region Constructors
		private int key;
		/// <summary>Class constructor</summary>
		public LayoutActivityBar() : base() {
			key = new Random().Next(0, 100);
		}

		#endregion Constructors

		#region Properties

		public ICommand TestCommand => new RelayCommand<object>((p) => {
			var model = Root?.PrimarySideBar;
			Debug.WriteLine($"{model?.IsVisible}, {key}", "TestCommand");
			model?.SetVisible(!model.IsVisible);
		});

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => true;

		/// <inheritdoc />
		protected override void ChildMoved(int oldIndex, int newIndex) {
			base.ChildMoved(oldIndex, newIndex);
		}

		protected void RemoveChild(LayoutAnchorableGroupPane item) {
			base.RemoveChild(item);
		}

		protected void InsertChild(int index, LayoutAnchorableGroupPane item) {
			base.InsertChildAt(index, item);
		}

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged() {
			AutoFixSelectedContent();

			for(var i = 0; i < Children.Count; i++) {
				if(!Children[i].IsSelected)
					continue;
				SelectedIndex = i;
				break;
			}

			foreach(var child in Children.OfType<LayoutAnchorableGroup>()) {
				child.IsActiveChanged -= Child_IsActiveChanged;
				child.IsActiveChanged += Child_IsActiveChanged;
			}

			base.OnChildrenCollectionChanged();
		}

		private void Child_IsActiveChanged(object sender, EventArgs e) {
			if (sender is LayoutAnchorableGroup model && model.IsActive) {
				Debug.WriteLine($"{Root}, {Root?.PrimarySideBar}, {Parent}", "Child_IsActiveChanged");
				if (Root.PrimarySideBar != null) {
					Root.PrimarySideBar.SetVisible(true);
				}
				//else {
				// var pane =	new LayoutAnchorableGroupPane() {
				//		Name = DockingManager.PrimarySideBarKey,
				//		DockMinWidth = 56,
				//		DockWidth = new GridLength(168)
				//	};
				//	Root.Manager.PrimarySideBar = pane;
				//	Root.Manager.PrimarySideBar.SetVisible(true);
				//}
			}
		}

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
			if(oldValue is ILayoutGroup oldGroup)
				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if(newValue is ILayoutGroup newGroup)
				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;

			//Init();

			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			//if(_id != null)
			//	writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);

			//base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			//if(reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id)))
			//	_id = reader.Value;
			//base.ReadXml(reader);
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			Trace.Write(new string(' ', tab * 4));
			Trace.WriteLine("LayoutActivityBar()");

			foreach(LayoutElement child in Children)
				child.ConsoleDump(tab + 1);
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

		public int SelectedIndex {
			get => _selectedIndex;
			set {
				if(_selectedIndex != value) {
					_selectedIndex = value;
					RaisePropertyChanged(nameof(SelectedIndex));
				}
			}
		}

		public LayoutAnchorableGroup SelectedItem {
			get => Children.Where((o, index) => index == SelectedIndex).FirstOrDefault();
			set {
				if(value != null && value != SelectedItem) {
					value.IsSelected = true;
				}
			}
		}

		/// <summary>
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		public int IndexOf(LayoutAnchorableGroup content) {
			return Children.IndexOf(content);
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Updates whether this object is hosted at the root level of a floating window control or not.
		/// </summary>
		//internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods

		public IEnumerable<LayoutAnchorableGroup> OverflowItems {
			get {
				var children = Children;
				var listSorted = Children.OfType<LayoutAnchorableGroup>().Where(o=> !(o.TabItem?.IsVisible == true));
				return listSorted.ToList();
			}
			set {
				//Debug.WriteLine($"", "OverflowItems 1");
				RaisePropertyChanged(nameof(OverflowItems));
				RaisePropertyChanged(nameof(HasOverflowItem));
			}
		}

		public bool HasOverflowItem => Children.OfType<LayoutAnchorableGroup>().Any(o => !(o.TabItem?.IsVisible == true));

		#region Private Metho
		private void AutoFixSelectedContent() {
			if(!_autoFixSelectedContent)
				return;
			if(SelectedIndex >= ChildrenCount)
				SelectedIndex = Children.Count - 1;
			if(SelectedIndex == -1 && ChildrenCount > 0)
				SetLastActivatedIndex();
		}

		/// <summary>Sets the current <see cref="SelectedContentIndex"/> to the last activated child with IsEnabled == true</summary>
		private void SetLastActivatedIndex() {
			var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
			SelectedIndex = Children.IndexOf(lastActivatedDocument);
		}
		#endregion

	}

}
