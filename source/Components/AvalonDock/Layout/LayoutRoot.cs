/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>
	/// Implements the root of the layout viewmodel (see  <see cref="DockingManager.Layout"/> property).
	/// This root includes a  <see cref="RootPanel"/> property for binding content, side panel properties
	/// and many other layout related root items.
	///
	/// This class implements <see cref="LayoutElement.PropertyChanged"/> and
	/// <see cref="LayoutElement.PropertyChanging"/> to support direct UI binding scenarios
	/// with view updates supported.
	/// </summary>
	[ContentProperty(nameof(RootPanel))]
	[Serializable]
	public class LayoutRoot :LayoutElement, ILayoutContainer, ILayoutRoot, IXmlSerializable {
		#region fields

		private LayoutPanel _rootPanel;
		private LayoutActivityBar _activityBar = null;

		private ObservableCollection<LayoutFloatingWindow> _floatingWindows = null;
		private ObservableCollection<LayoutAnchorable> _hiddenAnchorables = null;

		[field: NonSerialized]
		private WeakReference _activeContent = null;

		private bool _activeContentSet = false;

		[field: NonSerialized]
		private WeakReference _lastFocusedDocument = null;

		[NonSerialized]
		private DockingManager _manager = null;

		#endregion fields

		public static LayoutPanel DefaultRootPanel => new LayoutPanel(new LayoutPanel(new LayoutDocumentPaneGroup(new LayoutDocumentPane()))) { Orientation = Orientation.Horizontal };

		#region Constructors

		/// <summary>Standard class constructor</summary>
		public LayoutRoot() {
			//RootPanel = DefaultRootPanel;
			//ActivityBar = new LayoutActivityBar();
		}
		#endregion Constructors

		#region Events

		/// <summary>
		/// Raised when the layout is updated. This event is raised via <see cref="FireLayoutUpdated()"/> method
		/// when a parent of a LayoutElement has changed.
		/// </summary>
		public event EventHandler Updated;

		/// <summary>Raised when an element is added to the layout.</summary>
		public event EventHandler<LayoutElementEventArgs> ElementAdded;

		/// <summary>Raised when an element is removed from the layout.</summary>
		public event EventHandler<LayoutElementEventArgs> ElementRemoved;

		#endregion Events

		#region Properties

		/// <summary>Gets/sets the root layout panel that contains the <see cref="LayoutDocumentPane"/>.</summary>
		public LayoutPanel RootPanel {
			get => _rootPanel;
			set {
				if(_rootPanel == value)
					return;
				RaisePropertyChanging(nameof(RootPanel));
				var activeContent = ActiveContent;
				var activeRoot = activeContent?.Root;
				if(_rootPanel != null && _rootPanel.Parent == this)
					_rootPanel.Parent = null;
				_rootPanel = value ?? DefaultRootPanel;
				_rootPanel.Parent = this;
				if(ActiveContent == null && activeRoot == this && activeContent != null) {
					ActiveContent = activeContent;
					if(ActiveContent != activeContent) {
						ActiveContent = activeContent;
					}
				}

				RaisePropertyChanged(nameof(RootPanel));
			}
		}

		public LayoutActivityBar ActivityBar {
			get => _activityBar;
			set {
				if (value == _activityBar)
					return;
				RaisePropertyChanging(nameof(ActivityBar));

				_activityBar = value;
				if (_activityBar != null) {
					_activityBar.Children.Clear();
					_activityBar.Parent = this;

					if (PrimarySideBar != null) {
						//sidePar.ReplaceChildrenNoCollectionChangedSubscribe(_activityBar.Children);
						_activityBar.ReplaceChildrenNoCollectionChangedSubscribe(_primarySideBar.Children);
					}
				}

				RaisePropertyChanged(nameof(ActivityBar));
				RaisePropertyChanged(nameof(Children));
			}
		}


		private LayoutAnchorableGroupPane _primarySideBar;

		public LayoutAnchorableGroupPane PrimarySideBar {
			get => _primarySideBar;
			set {
				if (_primarySideBar == value) return;

				_primarySideBar = value;
				if (ActivityBar != null) {
					//_primarySideBar.ReplaceChildrenNoCollectionChangedSubscribe(activityBar.Children);
					//activityBar.ReplaceChildrenNoCollectionChangedSubscribe(_primarySideBar.Children);
					//if (_primarySideBar.Parent == null)
					//	RootPanel.InsertChildAt(0, _primarySideBar);
					ActivityBar.ReplaceChildrenNoCollectionChangedSubscribe(_primarySideBar.Children);
				}
			}
		}

		//public static LayoutAnchorableGroupPane DefaultPrimarySideBar => new LayoutAnchorableGroupPane() {
		//	Name = DockingManager.PrimarySideBarKey,
		//	DockMinWidth = 56,
		//	DockWidth = new GridLength(168)
		//};

		/// <summary>Gets the floating windows that are part of this layout.</summary>
		public ObservableCollection<LayoutFloatingWindow> FloatingWindows {
			get {
				if(_floatingWindows == null) {
					_floatingWindows = new ObservableCollection<LayoutFloatingWindow>();
					_floatingWindows.CollectionChanged += _floatingWindows_CollectionChanged;
				}

				return _floatingWindows;
			}
		}

		/// <summary>Gets the hidden anchorables in the layout.</summary>
		public ObservableCollection<LayoutAnchorable> Hidden {
			get {
				if(_hiddenAnchorables == null) {
					_hiddenAnchorables = new ObservableCollection<LayoutAnchorable>();
					_hiddenAnchorables.CollectionChanged += _hiddenAnchorables_CollectionChanged;
				}

				return _hiddenAnchorables;
			}
		}

		#region Children

		/// <summary>Gets the child elements of the layout root.</summary>
		public IEnumerable<ILayoutElement> Children {
			get {
				if(RootPanel != null)
					yield return RootPanel;
				if(_floatingWindows != null) {
					foreach(var floatingWindow in _floatingWindows)
						yield return floatingWindow;
				}
				if(ActivityBar != null)
					yield return ActivityBar;
				if(_hiddenAnchorables != null) {
					foreach(var hiddenAnchorable in _hiddenAnchorables)
						yield return hiddenAnchorable;
				}
			}
		}

		/// <summary>Gets the number of child elements of the layout root.</summary>
		public int ChildrenCount => 5 + (_floatingWindows?.Count ?? 0) + (_hiddenAnchorables?.Count ?? 0);

		#endregion Children

		/// <summary>Gets the active LayoutContent-derived element.</summary>
		[XmlIgnore]
		public LayoutContent ActiveContent {
			get {
				return _activeContent.GetValueOrDefault<LayoutContent>();
			}
			set {
				var currentValue = ActiveContent;
				if(currentValue != value) {
					InternalSetActiveContent(currentValue, value);
				}
			}
		}

		[XmlIgnore]
		public LayoutContent LastFocusedDocument {
			get => _lastFocusedDocument.GetValueOrDefault<LayoutContent>();
			private set {
				var currentValue = LastFocusedDocument;
				if(currentValue != value) {
					RaisePropertyChanging(nameof(LastFocusedDocument));
					if(currentValue != null)
						currentValue.IsLastFocusedDocument = false;
					_lastFocusedDocument = new WeakReference(value);
					currentValue = LastFocusedDocument;
					if(currentValue != null)
						currentValue.IsLastFocusedDocument = true;
					RaisePropertyChanged(nameof(LastFocusedDocument));
				}
			}
		}

		/// <summary>Gets/sets the docking manager root control for this library.</summary>
		[XmlIgnore]
		public DockingManager Manager {
			get => _manager;
			internal set {
				if(value == _manager)
					return;
				RaisePropertyChanging(nameof(Manager));
				_manager = value;
				RaisePropertyChanged(nameof(Manager));
			}
		}

		#endregion Properties

		#region Overrides

#if TRACE
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("RootPanel()");

			RootPanel.ConsoleDump(tab + 1);

			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("FloatingWindows()");

			foreach(var fw in FloatingWindows)
				fw.ConsoleDump(tab + 1);

			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Hidden()");

			foreach(var hidden in Hidden)
				hidden.ConsoleDump(tab + 1);
		}
#endif

		#endregion Overrides

		#region Public Methods

		public void RemoveChild(ILayoutElement element) {
			if(element == RootPanel)
				RootPanel = null;
			else if(_floatingWindows != null && _floatingWindows.Contains(element))
				_floatingWindows.Remove(element as LayoutFloatingWindow);
			else if(_hiddenAnchorables != null && _hiddenAnchorables.Contains(element))
				_hiddenAnchorables.Remove(element as LayoutAnchorable);
			else if(element == ActivityBar)
				ActivityBar = null;
		}

		public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement) {
			if (oldElement == RootPanel)
				RootPanel = (LayoutPanel)newElement;
			else if (_floatingWindows != null && _floatingWindows.Contains(oldElement)) {
				var index = _floatingWindows.IndexOf(oldElement as LayoutFloatingWindow);
				_floatingWindows.Remove(oldElement as LayoutFloatingWindow);
				_floatingWindows.Insert(index, newElement as LayoutFloatingWindow);
			}
			else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(oldElement)) {
				var index = _hiddenAnchorables.IndexOf(oldElement as LayoutAnchorable);
				_hiddenAnchorables.Remove(oldElement as LayoutAnchorable);
				_hiddenAnchorables.Insert(index, newElement as LayoutAnchorable);
			}
			else if (oldElement == ActivityBar)
				ActivityBar = (LayoutActivityBar)newElement;
		}

		/// <summary>Removes any empty container not directly referenced by other layout items.</summary>
		public void CollectGarbage() {
			var exitFlag = true;

			#region collect empty panes

			do {
				exitFlag = true;

				//for each content that references via PreviousContainer a disconnected Pane set the property to null
				foreach(var content in this.Descendents().OfType<ILayoutPreviousContainer>().Where(c => c.PreviousContainer != null &&
					(c.PreviousContainer.Parent == null || c.PreviousContainer.Parent.Root != this))) {
					content.PreviousContainer = null;
				}

				//for each pane that is empty
				foreach(var emptyPane in this.Descendents().OfType<ILayoutPane>().Where(p => p.ChildrenCount == 0)) {
					//...set null any reference coming from contents not yet hosted in a floating window
					foreach(var contentReferencingEmptyPane in this.Descendents().OfType<LayoutContent>()
						.Where(c => ((ILayoutPreviousContainer) c).PreviousContainer == emptyPane && !c.IsFloating)) {
						if(contentReferencingEmptyPane is LayoutAnchorable anchorable &&
							!anchorable.IsVisible)
							continue;

						((ILayoutPreviousContainer) contentReferencingEmptyPane).PreviousContainer = null;
						contentReferencingEmptyPane.PreviousContainerIndex = -1;
					}

					//...if this pane is the only documentpane present in the layout of the main window (not floating) then skip it
					if(emptyPane is LayoutDocumentPane &&
						 emptyPane.FindParent<LayoutDocumentFloatingWindow>() == null &&
						 this.Descendents().OfType<LayoutDocumentPane>().Count(c => c != emptyPane && c.FindParent<LayoutDocumentFloatingWindow>() == null) == 0)
						continue;

					//...if this empty pane is not referenced by anyone, then remove it from its parent container
					//if(!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyPane)) {
					//	var parentGroup = emptyPane.Parent;
					//	parentGroup.RemoveChild(emptyPane);
					//	exitFlag = false;
					//	break;
					//}
				}

				if(!exitFlag) {
					//removes any empty anchorable pane group
					foreach(var emptyLayoutAnchorablePaneGroup in this.Descendents().OfType<LayoutAnchorableGroupPane>().Where(p => p.ChildrenCount == 0)) {
						var parentGroup = emptyLayoutAnchorablePaneGroup.Parent;
						parentGroup.RemoveChild(emptyLayoutAnchorablePaneGroup);
						exitFlag = false;
						break;
					}
				}


				if(!exitFlag) {
					//removes any empty layout panel
					foreach(var emptyLayoutPanel in this.Descendents().OfType<LayoutPanel>().Where(p => p.ChildrenCount == 0)) {
						var parentGroup = emptyLayoutPanel.Parent;
						parentGroup.RemoveChild(emptyLayoutPanel);
						exitFlag = false;
						break;
					}
					foreach(var emptyLayoutDocumentPane in this.Descendents().OfType<LayoutDocumentPane>().Where(p => p.ChildrenCount == 0)) {
						var parentGroup = emptyLayoutDocumentPane.Parent;
						if(!(parentGroup.Parent is LayoutDocumentFloatingWindow))
							continue;
						var index = RootPanel.IndexOfChild(this.Descendents().OfType<LayoutDocumentPaneGroup>().First());
						parentGroup.RemoveChild(emptyLayoutDocumentPane);
						if(!this.Descendents().OfType<LayoutDocumentPane>().Any()) {
							// Now the last Pane container is deleted, at least one is required for documents to be added.
							// We did not want to keep an empty window floating, but add a new one to the main window
							RootPanel.Children.Insert(index < 0 ? 0 : index, emptyLayoutDocumentPane);
						}
						exitFlag = false;
						break;
					}
				}

				if(!exitFlag) {
					//removes any empty floating window
					foreach(var emptyLayoutFloatingWindow in this.Descendents().OfType<LayoutFloatingWindow>().Where(p => p.ChildrenCount == 0)) {
						var parentGroup = emptyLayoutFloatingWindow.Parent;
						parentGroup.RemoveChild(emptyLayoutFloatingWindow);
						exitFlag = false;
						break;
					}
				}

				//if(!exitFlag) {
				//	//removes any empty anchor group
				//	foreach(var emptyLayoutAnchorGroup in this.Descendents().OfType<LayoutAnchorGroup>().Where(p => p.ChildrenCount == 0)) {
				//		if(!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyLayoutAnchorGroup)) {
				//			var parentGroup = emptyLayoutAnchorGroup.Parent;
				//			parentGroup.RemoveChild(emptyLayoutAnchorGroup);
				//			exitFlag = false;
				//			break;
				//		}
				//	}
				//}
			}
			while(!exitFlag);

			#endregion collect empty panes

			//#region collapse single child anchorable pane groups

			//do {
			//	exitFlag = true;

			//	Debug.WriteLine($"{this.Descendents().OfType<LayoutAnchorableGroupPane>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutAnchorableGroup).ToArray().Length}", "CollectGarbage 1");

			//	//for each pane that is empty
			//	foreach(LayoutAnchorableGroupPane paneGroupToCollapse in this.Descendents().OfType<LayoutAnchorableGroupPane>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutAnchorableGroup).ToArray()) {
			//		LayoutAnchorableGroup singleChild = paneGroupToCollapse.Children[0];
			//		paneGroupToCollapse.Orientation = singleChild.Orientation;
			//		Debug.WriteLine($"{singleChild.Title}, {singleChild.ChildrenCount}", "CollectGarbage 2");
			//		while(singleChild.ChildrenCount > 0) {
			//			paneGroupToCollapse.InsertChildAt(paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
			//		}
			//		paneGroupToCollapse.RemoveChild(singleChild);
			//		exitFlag = false;
			//		break;
			//	}
			//}
			//while(!exitFlag);

			//#endregion collapse single child anchorable pane groups
			#region collapse single child document pane groups

			do {
				exitFlag = true;
				//for each pane that is empty
				foreach(var paneGroupToCollapse in this.Descendents().OfType<LayoutDocumentPaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutDocumentPaneGroup).ToArray()) {
					var singleChild = paneGroupToCollapse.Children[0] as LayoutDocumentPaneGroup;
					paneGroupToCollapse.Orientation = singleChild.Orientation;
					while(singleChild.ChildrenCount > 0)
						paneGroupToCollapse.InsertChildAt(paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
					paneGroupToCollapse.RemoveChild(singleChild);
					exitFlag = false;
					break;
				}
			}
			while(!exitFlag);

			#endregion collapse single child document pane groups

			////do
			////{
			////  exitFlag = true;
			////  //for each panel that has only one child
			////  foreach( var panelToCollapse in this.Descendents().OfType<LayoutPanel>().Where( p => p.ChildrenCount == 1 && p.Children[ 0 ] is LayoutPanel ).ToArray() )
			////  {
			////    var singleChild = panelToCollapse.Children[ 0 ] as LayoutPanel;
			////    panelToCollapse.Orientation = singleChild.Orientation;
			////    panelToCollapse.RemoveChild( singleChild );
			////    ILayoutPanelElement[] singleChildChildren = new ILayoutPanelElement[ singleChild.ChildrenCount ];
			////    singleChild.Children.CopyTo( singleChildChildren, 0 );
			////    while( singleChild.ChildrenCount > 0 )
			////    {
			////      panelToCollapse.InsertChildAt(
			////          panelToCollapse.ChildrenCount, singleChildChildren[ panelToCollapse.ChildrenCount ] );
			////    }

			////    exitFlag = false;
			////    break;
			////  }
			////}
			////while( !exitFlag );

			// Update ActiveContent and LastFocusedDocument properties
			UpdateActiveContentProperty();

#if DEBUG
			Debug.Assert(!this.Descendents().OfType<LayoutAnchorableGroup>().Any(a => a.ChildrenCount == 0 && a.IsVisible));
			//DumpTree(true);
#if TRACE
			//RootPanel.ConsoleDump(4);
#endif
#endif
		}

		#region IXmlSerializable interface members

		/// <inheritdoc />
		XmlSchema IXmlSerializable.GetSchema() => null;

		/// <inheritdoc />
		void IXmlSerializable.ReadXml(XmlReader reader) {
			reader.MoveToContent();
			if(reader.IsEmptyElement) {
				reader.Read();
				return;
			}

			var layoutPanelElements = ReadRootPanel(reader, out var orientation, out var canDock);
			if(layoutPanelElements != null) {
				RootPanel = new LayoutPanel { Orientation = orientation, CanDock = canDock };
				//Add all children to RootPanel
				foreach(var panel in layoutPanelElements) RootPanel.Children.Add(panel);
			}

			ActivityBar = new LayoutActivityBar();
			//if (ReadElement(reader) != null) FillLayoutAnchorSide(reader, ActivityBar);
			_ = ReadElementList(reader, false);

			FloatingWindows.Clear();
			var floatingWindows = ReadElementList(reader, true);
			foreach(var floatingWindow in floatingWindows)
				FloatingWindows.Add((LayoutFloatingWindow) floatingWindow);

			Hidden.Clear();
			var hidden = ReadElementList(reader, false);
			foreach (var hiddenObject in hidden)
				Hidden.Add((LayoutAnchorable)hiddenObject);

			//Read the closing end element of LayoutRoot
			reader.ReadEndElement();
		}

		/// <inheritdoc />
		void IXmlSerializable.WriteXml(XmlWriter writer) {
			writer.WriteStartElement(nameof(RootPanel));
			RootPanel?.WriteXml(writer);
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(ActivityBar));
			ActivityBar?.WriteXml(writer);
			writer.WriteEndElement();

			// Write all floating windows (can be LayoutDocumentFloatingWindow or LayoutAnchorableFloatingWindow).
			// To prevent "can not create instance of abstract type", the type is retrieved with GetType().Name
			writer.WriteStartElement(nameof(FloatingWindows));
			foreach(var layoutFloatingWindow in FloatingWindows) {
				writer.WriteStartElement(layoutFloatingWindow.GetType().Name);
				layoutFloatingWindow.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement(nameof(Hidden));
			foreach(var layoutAnchorable in Hidden) {
				writer.WriteStartElement(layoutAnchorable.GetType().Name);
				layoutAnchorable.WriteXml(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion IXmlSerializable interface members

		#endregion Public Methods

		#region Internal Methods

		internal static Type FindType(string name) {
			var avalonAssembly = Assembly.GetAssembly(typeof(LayoutRoot));

			foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a != avalonAssembly))
				try {
					foreach(var type in assembly.GetTypes())
						if(type.Name.Equals(name))
							return type;
				} catch(ReflectionTypeLoadException) {
				}

			return null;
		}

		internal void FireLayoutUpdated() => Updated?.Invoke(this, EventArgs.Empty);

		internal void OnLayoutElementAdded(LayoutElement element) => ElementAdded?.Invoke(this, new LayoutElementEventArgs(element));

		internal void OnLayoutElementRemoved(LayoutElement element) {
			if(element.Descendents().OfType<LayoutContent>().Any(c => c == LastFocusedDocument))
				LastFocusedDocument = null;
			if(element.Descendents().OfType<LayoutContent>().Any(c => c == ActiveContent))
				ActiveContent = null;
			ElementRemoved?.Invoke(this, new LayoutElementEventArgs(element));
		}

		#endregion Internal Methods

		#region Private Methods

		private void _floatingWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			var bNotifyChildren = false;

			if(e.OldItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)) {
				foreach(LayoutFloatingWindow element in e.OldItems) {
					if(element.Parent != this)
						continue;
					element.Parent = null;
					bNotifyChildren = true;
				}
			}

			if(e.NewItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)) {
				foreach(LayoutFloatingWindow element in e.NewItems) {
					element.Parent = this;
					bNotifyChildren = true;
				}
			}

			// descendants of LayoutElement notify when their Children and ChildrenCount properties change
			// https://github.com/xceedsoftware/wpftoolkit/issues/1313
			//
			if(!bNotifyChildren)
				return;
			switch(e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					RaisePropertyChanged(nameof(Children));
					RaisePropertyChanged(nameof(ChildrenCount));
					break;

				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					RaisePropertyChanged(nameof(Children));
					break;
			}
		}

		private void _hiddenAnchorables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			var bNotifyChildren = false;

			if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace) {
				if(e.OldItems != null) {
					foreach(LayoutAnchorable element in e.OldItems) {
						if(element.Parent != this)
							continue;
						element.Parent = null;
						bNotifyChildren = true;
					}
				}
			}

			if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace) {
				if(e.NewItems != null) {
					foreach(LayoutAnchorable element in e.NewItems) {
						if(element.Parent == this)
							continue;
						element.Parent?.RemoveChild(element);
						element.Parent = this;
						bNotifyChildren = true;
					}
				}
			}

			// descendants of LayoutElement notify when their Children and ChildrenCount properties change
			// https://github.com/xceedsoftware/wpftoolkit/issues/1313
			//
			if(!bNotifyChildren)
				return;
			switch(e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					RaisePropertyChanged(nameof(Children));
					RaisePropertyChanged(nameof(ChildrenCount));
					break;

				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					RaisePropertyChanged(nameof(Children));
					break;
			}
		}

		private void InternalSetActiveContent(LayoutContent currentValue, LayoutContent newActiveContent) {
			RaisePropertyChanging(nameof(ActiveContent));
			if(currentValue != null && currentValue.IsActive)
				currentValue.IsActive = false;
			_activeContent = new WeakReference(newActiveContent);
			currentValue = ActiveContent;
			if(currentValue != null && !currentValue.IsActive)
				currentValue.IsActive = true;
			RaisePropertyChanged(nameof(ActiveContent));
			_activeContentSet = currentValue != null;
			if(currentValue != null) {
				if(currentValue.Parent is LayoutDocumentPane || currentValue is LayoutDocument)
					LastFocusedDocument = currentValue;
			} else
				LastFocusedDocument = null;
		}

		private void UpdateActiveContentProperty() {
			var activeContent = ActiveContent;
			if(_activeContentSet && (activeContent == null || activeContent.Root != this)) {
				_activeContentSet = false;
				InternalSetActiveContent(activeContent, null);
			}
		}

		/// <summary>
		/// Reads all properties of the <see cref="LayoutPanel"/> and returns them.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="orientation"></param>
		/// <param name="canDock"></param>
		/// <returns></returns>
		private List<ILayoutPanelElement> ReadRootPanel(XmlReader reader
			, out Orientation orientation
			, out bool canDock) {
			orientation = Orientation.Horizontal;
			canDock = true;

			var result = new List<ILayoutPanelElement>();
			var startElementName = reader.LocalName;
			reader.Read();
			if(reader.LocalName.Equals(startElementName) && reader.NodeType == XmlNodeType.EndElement)
				return null;

			while(reader.NodeType == XmlNodeType.Whitespace)
				reader.Read();

			if(reader.LocalName.Equals(nameof(RootPanel))) {
				orientation = (Orientation) Enum.Parse(typeof(Orientation), reader.GetAttribute(nameof(Orientation)), true);

				var canDockStr = reader.GetAttribute("CanDock");
				if(canDockStr != null)
					canDock = bool.Parse(canDockStr);

				reader.Read();
				while(true) {
					//Read all RootPanel children
					if(ReadElement(reader) is ILayoutPanelElement element)
						result.Add(element);
					else if(reader.NodeType == XmlNodeType.EndElement)
						break;
				}
			}

			reader.ReadEndElement();
			return result;
		}

		private List<object> ReadElementList(XmlReader reader, bool isFloatingWindow) {
			var resultList = new List<object>();
			while(reader.NodeType == XmlNodeType.Whitespace)
				reader.Read();
			if(reader.NodeType == XmlNodeType.EndElement)
				return resultList;

			if(reader.IsEmptyElement) {
				reader.Read();
				return resultList;
			}

			var startElementName = reader.LocalName;
			reader.Read();
			if(reader.LocalName.Equals(startElementName) && reader.NodeType == XmlNodeType.EndElement)
				return null;

			while(reader.NodeType == XmlNodeType.Whitespace)
				reader.Read();

			while(true) {
				if(isFloatingWindow) {
					if(!(ReadElement(reader) is LayoutFloatingWindow result))
						break;
					resultList.Add(result);
				} else {
					if(!(ReadElement(reader) is LayoutAnchorable result))
						break;
					resultList.Add(result);
				}
			}

			reader.ReadEndElement();
			return resultList;
		}

		private object ReadElement(XmlReader reader) {
			while(reader.NodeType == XmlNodeType.Whitespace)
				reader.Read();
			if(reader.NodeType == XmlNodeType.EndElement)
				return null;

			Type typeToSerialize;
			switch(reader.LocalName) {
				case nameof(LayoutAnchorableGroupPane):
					typeToSerialize = typeof(LayoutAnchorableGroupPane);
					break;

				case nameof(LayoutAnchorableGroup):
					typeToSerialize = typeof(LayoutAnchorableGroup);
					break;

				case nameof(LayoutAnchorable):
					typeToSerialize = typeof(LayoutAnchorable);
					break;

				case nameof(LayoutDocumentPaneGroup):
					typeToSerialize = typeof(LayoutDocumentPaneGroup);
					break;

				case nameof(LayoutDocumentPane):
					typeToSerialize = typeof(LayoutDocumentPane);
					break;

				case nameof(LayoutDocument):
					typeToSerialize = typeof(LayoutDocument);
					break;

				case nameof(LayoutPanel):
					typeToSerialize = typeof(LayoutPanel);
					break;

				case nameof(LayoutDocumentFloatingWindow):
					typeToSerialize = typeof(LayoutDocumentFloatingWindow);
					break;

				case nameof(LayoutAnchorableFloatingWindow):
					typeToSerialize = typeof(LayoutAnchorableFloatingWindow);
					break;

				default:
					typeToSerialize = FindType(reader.LocalName);
					if(typeToSerialize == null)
						throw new ArgumentException("AvalonDock.LayoutRoot doesn't know how to deserialize " + reader.LocalName);
					break;
			}
			XmlSerializer serializer = XmlSerializersCache.GetSerializer(typeToSerialize);
			return serializer.Deserialize(reader);
		}

		#endregion Private Methods

		#region Diagnostic tools

#if DEBUG

		public void DumpTree(bool shortPropertyNames = false) {
			void DumpElement(ILayoutElement element, StringBuilder indent, int childID, bool isLastChild) {
				Debug.Write($"{indent}{(indent.Length > 0 ? isLastChild ? " └─ " : " ├─ " : "")}{childID:D2} 0x{element.GetHashCode():X8} " +
								$"{element.GetType().Name} {(shortPropertyNames ? "P" : "Parent")}:0x{element.Parent?.GetHashCode() ?? 0:X8} " +
								$"{(shortPropertyNames ? "R" : "Root")}:0x{element.Root?.GetHashCode() ?? 0:X8}");
				if(!(element is ILayoutContainer containerElement)) {
					Debug.WriteLine("");
					return;
				}
				Debug.WriteLine($" {(shortPropertyNames ? "C" : "Children")}:{containerElement.ChildrenCount}");
				var nrChild = 0;
				indent.Append(isLastChild ? "   " : " │ ");
				foreach(var child in containerElement.Children) {
					var lastChild = nrChild == containerElement.ChildrenCount - 1;
					DumpElement(child, indent, nrChild++, lastChild);
				}
				indent.Remove(indent.Length - 3, 3);
			}

			DumpElement(this, new StringBuilder(), 0, true);
		}

#endif

		#endregion Diagnostic tools
	}
}
