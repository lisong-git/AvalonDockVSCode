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
	public class LayoutActivityBar :LayoutGroup<LayoutAnchorableExpanderGroup>, ILayoutSelector<LayoutAnchorableExpanderGroup> {
		#region fields

		private string _id;
		private int _selectedIndex = -1;
		[XmlIgnore]
		private bool _autoFixSelectedContent = true;

		public static readonly string PrimarySideBarKey = "PART_PrimarySideBar";
		public static readonly string SecondarySideBarKey = "PART_SecondarySideBar";
		public static readonly string PanelKey = "PART_Panel";

		private LayoutAnchorableExpanderGroupPane _layoutAnchorableExpanderGroupBox;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutActivityBar() {
		}

		internal void Init() {
			LayoutAnchorableExpanderGroupPane = new LayoutAnchorableExpanderGroupPane()
			{
				Name = PrimarySideBarKey,
				DockMinWidth = 56,
				DockWidth = new GridLength(168)
			};
		}


		#endregion Constructors

		#region Properties

		public LayoutAnchorableExpanderGroupPane LayoutAnchorableExpanderGroupPane {
			get => _layoutAnchorableExpanderGroupBox;

			set {
				if(value != _layoutAnchorableExpanderGroupBox) {
					//var old = _layoutAnchorableExpanderGroupBox;
					//if(old != null) {
					//	Root.RootPanel.RemoveChild(old);
					//}
					RaisePropertyChanging(nameof(LayoutAnchorableExpanderGroupPane));

					_layoutAnchorableExpanderGroupBox = value;
					_layoutAnchorableExpanderGroupBox.ReplaceChildrenNoCollectionChangedSubscribe(Children);
				 var primarySideBar = 	Root.RootPanel.Children.OfType<LayoutAnchorableExpanderGroupPane>()
						.Where(o=> PrimarySideBarKey == o.Name)
						.FirstOrDefault();

					var rootPanel =Root.RootPanel; ;
					if(primarySideBar != null) {
						rootPanel.ReplaceChild(primarySideBar, _layoutAnchorableExpanderGroupBox);
					} else {
						rootPanel.InsertChildAt(0, _layoutAnchorableExpanderGroupBox);
					}
					RaisePropertyChanged(nameof(LayoutAnchorableExpanderGroupPane));
				}
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		//public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		public ICommand TestCommand => new RelayCommand<object>((p) => {
			var v =  Root.Manager.PrimarySideBar;
			//MessageBox.Show($"{box?.IsVisible}");
			if(v != null) {
				v.SetVisible(!v.IsVisible);
			}
		});

		//public ICommand TestCommand2 => new RelayCommand<object>((p) => {
		//	var box =  Root.Manager.LayoutAnchorableExpanderGroupBoxControl;
		//	//MessageBox.Show($"{box?.IsVisible}");
		//	Debug.WriteLine($"{box != null}, {p.GetType()}", "TestCommand 2");
		//	var box2 =  Root.Manager.LayoutAnchorableExpanderGroupPane;
		//	//MessageBox.Show($"{box?.IsVisible}");
		//	if(box != null) {
		//		//box.Se
		//		var index = Index;
		//		Debug.WriteLine($"{index}, {box.SelectedIndex}", "TestCommand2 1");
		//		if(box.SelectedIndex == index) {
		//			box2.SetVisible(!box2.IsVisible);
		//		} else {
		//			box.SelectedIndex = index;
		//			box2.SetVisible(true);
		//		}
		//	}
		//});

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override bool GetVisibility() => true;

		/// <inheritdoc />
		protected override void ChildMoved(int oldIndex, int newIndex) {
			base.ChildMoved(oldIndex, newIndex);
		}

		protected void RemoveChild(LayoutAnchorableExpanderGroupPane item) {
			base.RemoveChild(item);
		}

		protected void InsertChild(int index, LayoutAnchorableExpanderGroupPane item) {
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

			foreach(var child in Children.OfType<LayoutAnchorableExpanderGroup>()) {
				child.IsActiveChanged -= Child_IsActiveChanged;
				child.IsActiveChanged += Child_IsActiveChanged;
			}

			base.OnChildrenCollectionChanged();
		}

		private void Child_IsActiveChanged(object sender, EventArgs e) {
			if(sender is LayoutAnchorableExpanderGroup model) {
				//Debug.WriteLine($"{model.IsActive}", "Child_IsActiveChanged 1");
				if(model.IsActive) {
					LayoutAnchorableExpanderGroupPane.SetVisible(true);
				}
			}
		}

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
			if(oldValue is ILayoutGroup oldGroup)
				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if(newValue is ILayoutGroup newGroup)
				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;

			Init();

			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			if(_id != null)
				writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);

			base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			if(reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id)))
				_id = reader.Value;
			base.ReadXml(reader);
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
				Debug.WriteLine($"{_selectedIndex}, {value}", $"LayoutActivityBar SelectedIndex");

				if(_selectedIndex != value) {
					_selectedIndex = value;
					RaisePropertyChanged(nameof(SelectedIndex));
				}
			}
		}

		public LayoutAnchorableExpanderGroup SelectedItem {
			get => Children.Where((o, index) => index == SelectedIndex).FirstOrDefault();
			set {
				if(value != SelectedItem) {
					value.IsSelected = true;
				}
			}
		}

		/// <summary>
		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
		/// </summary>
		/// <param name="content"></param>
		public int IndexOf(LayoutAnchorableExpanderGroup content) {
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

		public ObservableCollection<LayoutAnchorableExpanderGroup> OverflowItems {
			get {
				//Debug.WriteLine($"{Parent.GetType().Name}, {Root == null}, {Root?.Manager == null}, {Root?.Manager?.PrimarySideBar == null}", "OverflowItems 1");
				//Debug.WriteLine($"{Parent.Root.Manager.LayoutAnchorableExpanderGroupPane == null}, {Parent.Root.Manager.LayoutAnchorableExpanderGroupBoxControl?.Model == null}", "OverflowItems 1");
				//Debug.WriteLine($"{Parent.Root.Manager.LayoutAnchorableExpanderGroupPane == null}, {Parent.Root.Manager.LayoutAnchorableExpanderGroupBoxControl?.Model == null}", "OverflowItems 1");
				var children = Children;

				//if(children != null)
				//	foreach(var child in children) {
				//		Debug.WriteLine($"{child.GetType()}, {child?.TabItem}, {child?.TabItem?.IsVisible}, ", "OverflowItems 2");
				//	}
				////var listSorted = Children.OfType<LayoutAnchorableExpanderGroup>().Where(o=> !(o.TabItem?.IsVisible == true)).ToList();
				//var listSorted = Children.OfType<LayoutAnchorableExpanderGroup>()
				//	//.Where(o=> !(o.TabItem?.IsVisible == true))
				//	;
				////listSorted.Sort();
				//return listSorted.ToList();
				return children;
			}
			set {
				Debug.WriteLine($"", "OverflowItems 1");
			}
		}

		public bool IsOverflowing => Children.OfType<LayoutAnchorableExpanderGroup>().Any(o => !o.TabItem.IsVisible);

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
