///************************************************************************
//   AvalonDock

//   Copyright (C) 2007-2013 Xceed Software Inc.

//   This program is provided to you under the terms of the Microsoft Public
//   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
// ************************************************************************/

//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Windows.Markup;
//using System.Xml.Serialization;

//namespace AvalonDock.Layout {

//	[ContentProperty(nameof(Children))]
//	[Serializable]
//	public class LayoutAnchorableExpanderGroupPane :LayoutPositionableGroup<LayoutAnchorableExpanderGroup>
//				//, ILayoutPanelElement
//				//, ILayoutPane
//		, ILayoutAnchorablePane
//		, ILayoutPositionableElement
//		, ILayoutSelector<LayoutAnchorableExpanderGroup>
//		, ILayoutPaneSerializable {
//		#region fields

//		[XmlIgnore]
//		private bool _autoFixSelectedContent = true;

//		private string _name = null;
//		private string _id;
//		private int _selectedIndex = -1;

//		private bool _isActive = false;

//		#endregion fields

//		#region Constructors

//		/// <summary>Class constructor</summary>
//		public LayoutAnchorableExpanderGroupPane() {
//		}

//		#endregion Constructors

//		#region Properties

//		/// <summary>Gets whether the pane is hosted in a floating window.</summary>
//		public string Name {
//			get => _name;
//			set {
//				if(value != _name) {
//					_name = value;
//					RaisePropertyChanged(nameof(Name));
//				}
//			}
//		}

//		public bool IsActive {
//			get => _isActive;
//			set {
//				//var watch = Stopwatch.StartNew();
//				if(value != _isActive) {
//					_isActive = value;

//					RaisePropertyChanged(nameof(IsActive));
//					//watch.Stop();
//					//Debug.WriteLine($"程序耗时: {watch.ElapsedMilliseconds}, {_isActive}", "IsActive");
//					//MessageBox.Show($"{watch.ElapsedMilliseconds}", "IsActive");
//				}
//			}
//		}

//		//public LayoutAnchorableExpanderGroup Content {
//		//	get => _content;
//		//	set {
//		//		if(value == _content)
//		//			return;
//		//		RaisePropertyChanging(nameof(Content));
//		//		_content = value;
//		//		Children.Clear();
//		//		Children.Add(_content);
//		//		RaisePropertyChanged(nameof(Content));
//		//	}
//		//}

//		public void SetVisible(bool isVisible) {
//			IsVisible = isVisible;
//		}

//		/// <summary>Gets/sets the unique id that is used for the serialization of this panel.</summary>
//		string ILayoutPaneSerializable.Id {
//			get => _id;
//			set => _id = value;
//		}

//		#endregion Properties

//		#region Overrides

//		/// <inheritdoc />
//		protected override bool GetVisibility() => Children.Count > 0 && Children.Any(c => c.IsVisible);

//		/// <inheritdoc />
//		protected override void ChildMoved(int oldIndex, int newIndex) {
//			base.ChildMoved(oldIndex, newIndex);
//		}

//		/// <inheritdoc />
//		protected override void OnChildrenCollectionChanged() {
//			base.OnChildrenCollectionChanged();
//		}

//		/// <inheritdoc />
//		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
//			if(oldValue is ILayoutGroup oldGroup)
//				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
//			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
//			if(newValue is ILayoutGroup newGroup)
//				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;
//			base.OnParentChanged(oldValue, newValue);
//		}

//		/// <inheritdoc />
//		public override void WriteXml(System.Xml.XmlWriter writer) {
//			if(_id != null)
//				writer.WriteAttributeString(nameof(ILayoutPaneSerializable.Id), _id);
//			if(_name != null)
//				writer.WriteAttributeString(nameof(Name), _name);
//			base.WriteXml(writer);
//		}

//		/// <inheritdoc />
//		public override void ReadXml(System.Xml.XmlReader reader) {
//			if(reader.MoveToAttribute(nameof(ILayoutPaneSerializable.Id)))
//				_id = reader.Value;
//			if(reader.MoveToAttribute(nameof(Name)))
//				_name = reader.Value;
//			_autoFixSelectedContent = false;
//			base.ReadXml(reader);
//			_autoFixSelectedContent = true;
//			AutoFixSelectedContent();
//		}

//#if TRACE
//		/// <inheritdoc />
//		public override void ConsoleDump(int tab) {
//			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
//			System.Diagnostics.Trace.WriteLine("AnchorablePane()");

//			//foreach(ILayoutPositionableElement child in Children)
//			//	child.ConsoleDump(tab + 1);
//		}
//#endif

//		#endregion Overrides

//		#region Public Methods

//		/// <summary>
//		/// Gets whether the model hosts only 1 <see cref="LayoutAnchorable"/> (True)
//		/// or whether there are more than one <see cref="LayoutAnchorable"/>s below
//		/// this model pane.
//		/// </summary>
//		public bool IsDirectlyHostedInFloatingWindow {
//			get {
//				var parentFloatingWindow = this.FindParent<LayoutAnchorableFloatingWindow>();
//				return parentFloatingWindow != null && parentFloatingWindow.IsSinglePane;
//				//return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
//			}
//		}

//		public int SelectedIndex {
//			get => _selectedIndex;
//			set {
//				Debug.WriteLine($"{_selectedIndex}, {value}", $"SelectedIndex");

//				if(_selectedIndex != value) {
//					_selectedIndex = value;
//					RaisePropertyChanged(nameof(SelectedIndex));
//				}
//			}
//		}

//		public LayoutAnchorableExpanderGroup SelectedItem {
//			get => Children.Where((o, index) => index == SelectedIndex).FirstOrDefault();
//			set {
//				//if(value != SelectedItem) { 
					
//				//}
//			}
//		}

//		#endregion Public Methods

//		#region Internal Methods



//		/// <summary>
//		/// Updates whether this object is hosted at the root level of a floating window control or not.
//		/// </summary>
//		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

//		#endregion Internal Methods

//		#region Private Methods

//		private void AutoFixSelectedContent() {
//			if(!_autoFixSelectedContent)
//				return;
//			SetLastActivatedIndex();
//		}

//		/// <summary>Sets the current <see cref="SelectedIndex"/> to the last activated child with IsEnabled == true</summary>
//		private void SetLastActivatedIndex() {
//			//var lastActivatedDocument = Children.Where(c => c.IsEnabled).OrderByDescending(c => c.LastActivationTimeStamp.GetValueOrDefault()).FirstOrDefault();
//			//SelectedIndex = Children.IndexOf(lastActivatedDocument);
//		}

//		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));



//		#endregion Private Methods

//		#region Public Methods

//		/// <summary>
//		/// Gets the index of the layout content (which is required to be a <see cref="LayoutAnchorable"/>)
//		/// or -1 if the layout content is not a <see cref="LayoutAnchorable"/> or is not part of the childrens collection.
//		/// </summary>
//		/// <param name="content"></param>
//		public int IndexOf(LayoutAnchorableExpanderGroup content) {
//			return Children.IndexOf(content);
//		}

//		#endregion Public Methods
//	}

//}