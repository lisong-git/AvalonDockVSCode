using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace AvalonDock.Layout {

	[ContentProperty(nameof(Children))]
	[Serializable]
	public class LayoutActivityBar :LayoutGroup<LayoutAnchorableExpanderGroupBox> {
		#region fields

		private string _id;
		private LayoutAnchorableExpanderGroupBox _current;

		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutActivityBar() {
		}

		#endregion Constructors

		#region Properties

		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
		//public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() != null;

		public LayoutAnchorableExpanderGroupBox Current {
			get => _current;
			set {
				if(value != _current) {
					if(_current != null) {
						_current.IsActive = false;
					}
					_current = value;
					RaisePropertyChanged(nameof(Current));
				}
			}
		}

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
					if(sender is LayoutAnchorableExpanderGroupBox gb && gb.IsActive) {
						//Debug.WriteLine($"{gb.Name}", "LayoutActivityBar_Child_PropertyChanged");
						Current = gb;
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
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("LayoutActivityBar()");

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
	}

}
