using AvalonDock.Commands;
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

	[Serializable]
	public class LayoutActivityBar :LayoutGroup<LayoutAnchorableExpanderGroup> {
		#region fields

		private string _id;
		private LayoutAnchorableExpanderGroup _current;
		private LayoutAnchorableExpanderGroupBox _layoutAnchorableExpanderGroupBox;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutActivityBar() {
		}


		#endregion Constructors

		#region Properties

		public LayoutAnchorableExpanderGroupBox LayoutAnchorableExpanderGroupBox {
			get => _layoutAnchorableExpanderGroupBox;
			set {
				if(value != _layoutAnchorableExpanderGroupBox) {
					//var old = _layoutAnchorableExpanderGroupBox;
					//if(old != null) {
					//	Root.RootPanel.RemoveChild(old);
					//}
					_layoutAnchorableExpanderGroupBox = value;

					//if(_layoutAnchorableExpanderGroupBox != null) {
					//	Debug.WriteLine($"{Root.RootPanel == null}", "LayoutAnchorableExpanderGroupBox");
					//	Parent.Root.RootPanel.InsertChildAt(0, _layoutAnchorableExpanderGroupBox);
					//}
					RaisePropertyChanged(nameof(LayoutAnchorableExpanderGroupBox));
				}
			}
		}

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		//public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;
		public int Index => Children.IndexOf(Current);

		public LayoutAnchorableExpanderGroup Current {
			get => _current;
			set {
				if(value != _current) {
					if(_current != null) {
						_current.IsActive = false;
					}
					_current = value;
					RaisePropertyChanged(nameof(Current));
					RaisePropertyChanged(nameof(Index));
				}
			}
		}

		//public ICommand TestCommand => new RelayCommand<object>((p) => {
		// var v =	Root.Manager.LayoutAnchorableExpanderGroupBox;
		//	//MessageBox.Show($"{box?.IsVisible}");
		//	if(v != null) {
		//		v.SetVisible(!v.IsVisible);
		//	}
		//});

		//public ICommand TestCommand2 => new RelayCommand<object>((p) => {
		//	var box =  Root.Manager.LayoutAnchorableExpanderGroupBoxControl;
		//	//MessageBox.Show($"{box?.IsVisible}");
		//	Debug.WriteLine($"{box!= null}, {p.GetType()}", "TestCommand 2");
		//	var box2 =  Root.Manager.LayoutAnchorableExpanderGroupBox;
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

		protected void RemoveChild(LayoutAnchorableExpanderGroupBox item) {
			base.RemoveChild(item);
		}

		protected void InsertChild(int index, LayoutAnchorableExpanderGroupBox item) {
			base.InsertChildAt(index, item);
		}

		/// <inheritdoc />
		protected override void OnChildrenCollectionChanged() {
			base.OnChildrenCollectionChanged();

			foreach(var child in Children) {
				child.PropertyChanged -= Child_PropertyChanged;
				child.PropertyChanged += Child_PropertyChanged;
			}
		}

		private void Child_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
				case nameof(LayoutAnchorableExpanderGroupBox.IsActive):
					if(sender is LayoutAnchorableExpanderGroup gb && gb.IsActive) {
						//Debug.WriteLine($"{gb.Name}", "LayoutActivityBar_Child_PropertyChanged");
						//if(Current == null)
						Current = gb;
						//gb.SetVisible(!gb.IsVisible);
					}
					break;
				default:
					break;
			}
		}


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

			//foreach(ILayoutPositionableElement child in Children)
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
		//internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods


		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods

		public ObservableCollection<LayoutAnchorableExpanderGroup> OverflowItems {
			get {
				Debug.WriteLine($"{Parent.GetType().Name}, {Root == null}, {Root?.Manager == null}, {Root?.Manager?.LayoutAnchorableExpanderGroupBox == null}", "OverflowItems 1");
				//Debug.WriteLine($"{Parent.Root.Manager.LayoutAnchorableExpanderGroupBox == null}, {Parent.Root.Manager.LayoutAnchorableExpanderGroupBoxControl?.Model == null}", "OverflowItems 1");
				//Debug.WriteLine($"{Parent.Root.Manager.LayoutAnchorableExpanderGroupBox == null}, {Parent.Root.Manager.LayoutAnchorableExpanderGroupBoxControl?.Model == null}", "OverflowItems 1");
				var children = Root.Manager.LayoutAnchorableExpanderGroupBox?.Children;

				if(children != null)
					foreach(var child in children) {
						Debug.WriteLine($"{child.GetType()}, {child?.TabItem}, {child?.TabItem?.IsVisible}, ", "OverflowItems 2");
					}
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



		int count = 0;

		public string BindingTest {
			get {

				Debug.WriteLine($"{OverflowItems?.Count}", "BindingTest 1");

				//return "BindingTest";
				return count++.ToString();
			}
			//set {
			//	_bindingTest = value;
			//	RaisePropertyChanged(nameof(BindingTest));
			//}
		}




		public bool IsOverflowing => Children.OfType<LayoutAnchorableExpanderGroup>().Any(o => !o.TabItem.IsVisible);



	}

}
