/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Implements the model for a layout anchorable control (tool window).
	/// A <see cref="LayoutAnchorable"/> can be anchored to the left, right, top, or bottom side of
	/// the Layout property of the <see cref="DockingManager"/>. It can contain
	/// custom application content (WPF controls) and other children elements.
	/// </summary>
	[Serializable]
	public class LayoutAnchorable : LayoutContent, IXmlSerializable, ILayoutPositionableElementWithActualSize, IExpander, ILayoutPaneSerializable {
		#region fields

		private double _autohideWidth = 0.0;
		private double _autohideMinWidth = 100.0;
		private double _autohideHeight = 0.0;
		private double _autohideMinHeight = 100.0;
		private bool _canHide = true;
		private bool _canAutoHide = true;
		private bool _canDockAsTabbedDocument = true;
		// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
		//private bool _canCloseValueBeforeInternalSet;
		private bool _canMove = true;
		private Visibility _contentVisibility = Visibility.Visible;

		// DockWidth fields
		private GridLength _dockWidth = new GridLength(1.0, GridUnitType.Star);


		private double? _resizableAbsoluteDockWidth;

		// DockHeight fields
		private GridLength _dockHeight = new GridLength(1.0, GridUnitType.Star);

		private double? _resizableAbsoluteDockHeight;

		private bool _allowDuplicateContent = true;
		//private bool _canRepositionItems = true;

		private double _dockMinWidth = 21.0;
		private double _dockMinHeight = 21.0;

		[NonSerialized]
		private double _actualWidth;

		[NonSerialized]
		private double _actualHeight;
		private string _name = null;
		private string _id;
		private bool _isExpanded;
		#endregion fields

		#region Constructors

		/// <summary>Class constructor</summary>
		public LayoutAnchorable() {
			// LayoutAnchorable will hide by default, not close.
			// BD: 14.08.2020 Inverting both _canClose and _canCloseDefault to false as anchorables are only hidden but not closed
			//     That would allow CanClose to be properly serialized if set to true for an instance of LayoutAnchorable
			_canClose = _canCloseDefault = false;
			UpdateSize();

		}

		#endregion Constructors

		#region Events

		/// <summary>Event is invoked when the _contentVisibility of this object has changed.</summary>
		public event EventHandler IsVisibleChanged;

		public event EventHandler<CancelEventArgs> Hiding;
		public event EventHandler<CancelEventArgs> Collapsing;

		#endregion Events

		#region Properties

		/// <summary>Gets/sets the width for this anchorable in AutoHide mode.</summary>
		public double AutoHideWidth {
			get => _autohideWidth;
			set {
				if (value == _autohideWidth)
					return;
				RaisePropertyChanging(nameof(AutoHideWidth));
				value = Math.Max(value, _autohideMinWidth);
				_autohideWidth = value;
				RaisePropertyChanged(nameof(AutoHideWidth));
			}
		}

		/// <summary>Gets/sets the minimum width for this anchorable in AutoHide mode.</summary>
		public double AutoHideMinWidth {
			get => _autohideMinWidth;
			set {
				if (value == _autohideMinWidth)
					return;
				RaisePropertyChanging(nameof(AutoHideMinWidth));
				if (value < 0)
					throw new ArgumentOutOfRangeException("Negative value is not allowed.", nameof(value));
				_autohideMinWidth = value;
				RaisePropertyChanged(nameof(AutoHideMinWidth));
			}
		}

		/// <summary>Gets/sets the height for this anchorable in AutoHide mode.</summary>
		public double AutoHideHeight {
			get => _autohideHeight;
			set {
				if (value == _autohideHeight)
					return;
				RaisePropertyChanging(nameof(AutoHideHeight));
				value = Math.Max(value, _autohideMinHeight);
				_autohideHeight = value;
				RaisePropertyChanged(nameof(AutoHideHeight));
			}
		}

		/// <summary>Gets/sets the minimum height for this anchorable in AutoHide mode.</summary>
		public double AutoHideMinHeight {
			get => _autohideMinHeight;
			set {
				if (value == _autohideMinHeight)
					return;
				RaisePropertyChanging(nameof(AutoHideMinHeight));
				if (value < 0)
					throw new ArgumentOutOfRangeException("Negative value is not allowed.", nameof(value));

				_autohideMinHeight = value;
				RaisePropertyChanged(nameof(AutoHideMinHeight));
			}
		}

		/// <summary>Gets/sets whether the anchorable can hide (be invisible in the UI) or not.</summary>
		public bool CanHide {
			get => _canHide;
			set {
				if (value == _canHide)
					return;
				_canHide = value;
				RaisePropertyChanged(nameof(CanHide));
			}
		}

		/// <summary>Gets/sets whether the anchorable can be anchored to an achor side in an AutoHide status or not.</summary>
		public bool CanAutoHide {
			get => _canAutoHide;
			set {
				if (value == _canAutoHide)
					return;
				_canAutoHide = value;
				RaisePropertyChanged(nameof(CanAutoHide));
			}
		}

		/// <summary>Gets/sets whether the anchorable can be docked as a tabbed document or not.</summary>
		public bool CanDockAsTabbedDocument {
			get => _canDockAsTabbedDocument;
			set {
				if (_canDockAsTabbedDocument == value)
					return;
				_canDockAsTabbedDocument = value;
				RaisePropertyChanged(nameof(CanDockAsTabbedDocument));
			}
		}

		/// <summary>Gets/sets whether a document can be dragged (to be dropped in a different location) or not.
		/// Use this property in conjunction with <see cref="CanMove"/> and <see cref="CanClose"/> and <see cref="LayoutPanel.CanDock"/>
		/// to lock an anchorable in its layout position.</summary>
		public bool CanMove {
			get => _canMove;
			set {
				if (value == _canMove)
					return;
				_canMove = value;
				RaisePropertyChanged(nameof(CanMove));
			}
		}

		/// <summary>Get a value indicating if the anchorable is anchored to an achor side in an AutoHide status or not.</summary>
		//public bool IsAutoHidden => Parent is LayoutAnchorGroup;

		/// <summary>Gets whether this object is in a state where it is not visible in the UI or not.</summary>
		[XmlIgnore]
		public bool IsHidden => Parent is LayoutRoot;

		/// <summary>Gets/sets whether this object is in a state where it is visible in the UI or not.</summary>
		[XmlIgnore]
		public bool IsVisible {
			get => Parent != null && !(Parent is LayoutRoot);
			set { if (value) Show(); else Hide(); }
		}

		public Visibility ContentVisibility {
			get => _contentVisibility;
			set {
				if (_contentVisibility == value)
					return;
				_contentVisibility = value;
				//if(Parent is La)
				//Debug.WriteLine($"{_contentVisibility}", "ContentVisibility");
				if (Parent is LayoutAnchorableGroup lap) {
					if (lap.Parent is LayoutAnchorableGroupPane pg) {
						//Debug.WriteLine($"LayoutAnchorablePane,{lap.Parent.GetType()},  {lap.FixedDockHeight}, {lap.DockHeight}", "ContentVisibility");

						//if(ContentVisibility == Visibility.Collapsed) {
						//	lap.DockHeight = new GridLength(lap.FixedDockHeight, lap.DockHeight.GridUnitType);
						//} else {
						//	//lap.DockHeight = new GridLength(0, lap.DockHeight.GridUnitType);
						//}
						lap.ResizableAbsoluteDockHeight = lap.FixedDockHeight;
					}
				}
				RaisePropertyChanged(nameof(ContentVisibility));
			}
		}



		public GridLength DockHeight {
			//get => _dockHeight.IsAbsolute && _resizableAbsoluteDockHeight < _dockHeight.Value && _resizableAbsoluteDockHeight.HasValue ?
			//			new GridLength(_resizableAbsoluteDockHeight.Value) : _dockHeight;
			get {
				var h = _dockHeight.IsAbsolute && _resizableAbsoluteDockHeight < _dockHeight.Value && _resizableAbsoluteDockHeight.HasValue ?
			new GridLength(_resizableAbsoluteDockHeight.Value) : _dockHeight;
				return h;
			}
			set {
				//Debug.WriteLine($"{_dockHeight}", "LayoutPositionable DockHeight 2");

				if (_dockHeight == value || !(value.Value > 0))
					return;
				if (value.IsAbsolute)
					_resizableAbsoluteDockHeight = value.Value;
				RaisePropertyChanging(nameof(DockHeight));
				_dockHeight = value;
				//Debug.WriteLine($"{_dockHeight}", "LayoutPositionable DockHeight 2");
				RaisePropertyChanged(nameof(DockHeight));
				OnDockHeightChanged();
			}
		}

		public double FixedDockHeight => _dockHeight.IsAbsolute && _dockHeight.Value >= _dockMinHeight ? _dockHeight.Value : _dockMinHeight;

		public double ResizableAbsoluteDockHeight {
			get => _resizableAbsoluteDockHeight ?? 0;
			set {
				if (!_dockHeight.IsAbsolute)
					return;
				if (value < _dockHeight.Value && value > 0) {
					RaisePropertyChanging(nameof(DockHeight));
					_resizableAbsoluteDockHeight = value;
					RaisePropertyChanged(nameof(DockHeight));
					OnDockHeightChanged();
				}
				else if (value > _dockHeight.Value && _resizableAbsoluteDockHeight < _dockHeight.Value)
					_resizableAbsoluteDockHeight = _dockHeight.Value;
				else if (value == 0)
					_resizableAbsoluteDockHeight = DockMinHeight;
			}
		}


		/// <summary>
		/// Gets or sets the AllowDuplicateContent property.
		/// When this property is true, then the LayoutDocumentPane or LayoutAnchorablePane allows dropping
		/// duplicate content (according to its Title and ContentId). When this dependency property is false,
		/// then the LayoutDocumentPane or LayoutAnchorablePane hides the OverlayWindow.DropInto button to prevent dropping of duplicate content.
		/// </summary>
		public bool AllowDuplicateContent {
			get => _allowDuplicateContent;
			set {
				if (value == _allowDuplicateContent)
					return;
				RaisePropertyChanging(nameof(AllowDuplicateContent));
				_allowDuplicateContent = value;
				RaisePropertyChanged(nameof(AllowDuplicateContent));
			}
		}

		/// <summary>
		/// Defines the smallest available width that can be applied to a deriving element.
		///
		/// The system ensures the minimum width by blocking/limiting <see cref="GridSplitter"/>
		/// movement when the user resizes a deriving element or resizes the main window.
		/// </summary>
		public double DockMinWidth {
			get => _dockMinWidth;
			set {
				if (value == _dockMinWidth)
					return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinWidth));
				_dockMinWidth = value;
				RaisePropertyChanged(nameof(DockMinWidth));
			}
		}

		/// <summary>
		/// Defines the smallest available height that can be applied to a deriving element.
		///
		/// The system ensures the minimum height by blocking/limiting <see cref="GridSplitter"/>
		/// movement when the user resizes a deriving element or resizes the main window.
		/// </summary>
		public double DockMinHeight {
			get => _dockMinHeight;
			set {
				if (value == _dockMinHeight)
					return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinHeight));
				_dockMinHeight = value;
				RaisePropertyChanged(nameof(DockMinHeight));
			}
		}

		double ILayoutPositionableElementWithActualSize.ActualWidth {
			get => _actualWidth;
			set => _actualWidth = value;
		}

		double ILayoutPositionableElementWithActualSize.ActualHeight {
			get => _actualHeight;
			set => _actualHeight = value;
		}

		/// <summary>
		/// Event fired when floating properties were updated.
		/// </summary>
		//public event EventHandler FloatingPropertiesUpdated;

		public GridLength DockWidth {
			//get => _dockWidth.IsAbsolute && _resizableAbsoluteDockWidth < _dockWidth.Value && _resizableAbsoluteDockWidth.HasValue ?
			//			new GridLength(_resizableAbsoluteDockWidth.Value) : _dockWidth;
			get {
				var width = _dockWidth.IsAbsolute && _resizableAbsoluteDockWidth < _dockWidth.Value && _resizableAbsoluteDockWidth.HasValue ?
			new GridLength(_resizableAbsoluteDockWidth.Value) : _dockWidth;
				//Debug.WriteLine($"{width}, {_dockWidth}, {_dockWidth.IsAbsolute}, {_dockWidth.Value}, {_dockMinWidth}, {_resizableAbsoluteDockWidth}", "LayoutPositionable 2");
				return width;
			}
			set {
				if (value == _dockWidth || !(value.Value > 0))
					return;

				//Debug.WriteLine($"{_dockWidth}, {value}, {_resizableAbsoluteDockHeight}", "LayoutPositionable 3");

				if (value.IsAbsolute)
					_resizableAbsoluteDockWidth = value.Value;
				RaisePropertyChanging(nameof(DockWidth));
				_dockWidth = value;
				RaisePropertyChanged(nameof(DockWidth));
				OnDockWidthChanged();
			}
		}

		public double FixedDockWidth => _dockWidth.IsAbsolute && _dockWidth.Value >= _dockMinWidth ? _dockWidth.Value : _dockMinWidth;

		public double ResizableAbsoluteDockWidth {
			get => _resizableAbsoluteDockWidth ?? 0;
			set {
				if (!_dockWidth.IsAbsolute)
					return;
				if (value <= _dockWidth.Value && value > 0) {
					RaisePropertyChanging(nameof(DockWidth));
					_resizableAbsoluteDockWidth = value;
					RaisePropertyChanged(nameof(DockWidth));
					OnDockWidthChanged();
				}
				else if (value > _dockWidth.Value && _resizableAbsoluteDockWidth < _dockWidth.Value)
					_resizableAbsoluteDockWidth = _dockWidth.Value;
			}
		}

		public ExpandDirection ExpandDirection {
			get {
				if (Parent is ILayoutOrientableGroup orientableGroup && Orientation.Horizontal == orientableGroup.Orientation) {
					return ExpandDirection.Right;
				}
				else {
					return ExpandDirection.Down;
				}
			}
		}

		public bool IsExpanded {
			get => _isExpanded;
			set {
				if (_isExpanded == value)
					return;

				RaisePropertyChanging(nameof(IsExpanded));
				_isExpanded = value;
				RaisePropertyChanged(nameof(IsExpanded));
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

		/// <summary>Gets/sets the unique id that is used for the serialization of this panel.</summary>
		string ILayoutPaneSerializable.Id {
			get => _id;
			set => _id = value;
		}

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue) {
			if (oldValue is ILayoutGroup oldGroup)
				oldGroup.ChildrenCollectionChanged -= OnParentChildrenCollectionChanged;
			RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
			if (newValue is ILayoutGroup newGroup)
				newGroup.ChildrenCollectionChanged += OnParentChildrenCollectionChanged;

			RaisePropertyChanged(nameof(ExpandDirection));
			UpdateSize();

			UpdateParentVisibility();
			RaisePropertyChanged(nameof(IsVisible));
			NotifyIsVisibleChanged();
			//RaisePropertyChanged(nameof(IsHidden));
			//RaisePropertyChanged(nameof(IsAutoHidden));
			base.OnParentChanged(oldValue, newValue);
		}

		/// <inheritdoc />
		protected override void InternalDock() {
			var root = Root as LayoutRoot;
			LayoutAnchorableGroup anchorableGroup = null;

			//look for active content parent pane
			if (root.ActiveContent != null && root.ActiveContent != this)
				anchorableGroup = root.ActiveContent.Parent as LayoutAnchorableGroup;
			//look for a pane on the right side
			//if (anchorableGroup == null)
			//	anchorableGroup = root.Descendents().OfType<LayoutAnchorableGroup>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow && pane.GetSide() == AnchorSide.Right);
			if (anchorableGroup == null)
				anchorableGroup = root.Descendents().OfType<LayoutAnchorableGroup>().FirstOrDefault(pane => !pane.IsHostedInFloatingWindow);
			//look for an available pane
			if (anchorableGroup == null)
				anchorableGroup = root.Descendents().OfType<LayoutAnchorableGroup>().FirstOrDefault();
			var added = false;
			if (root.Manager.LayoutUpdateStrategy != null)
				added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(root, this, anchorableGroup);

			if (!added) {
				if (anchorableGroup == null) {
					var mainLayoutPanel = new LayoutPanel { Orientation = Orientation.Horizontal };
					if (root.RootPanel != null)
						mainLayoutPanel.Children.Add(root.RootPanel);

					root.RootPanel = mainLayoutPanel;
					anchorableGroup = new LayoutAnchorableGroup { DockWidth = new GridLength(200.0, GridUnitType.Pixel) };
					mainLayoutPanel.Children.Add(anchorableGroup);
				}
				anchorableGroup.Children.Add(this);
			}
			root.Manager.LayoutUpdateStrategy?.AfterInsertAnchorable(root, this);
			base.InternalDock();
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader) {
			if (reader.MoveToAttribute(nameof(IsExpanded)))
				IsExpanded = bool.Parse(reader.Value);
			if (reader.MoveToAttribute(nameof(CanHide)))
				CanHide = bool.Parse(reader.Value);
			if (reader.MoveToAttribute(nameof(CanAutoHide)))
				CanAutoHide = bool.Parse(reader.Value);
			if (reader.MoveToAttribute(nameof(AutoHideWidth)))
				AutoHideWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute(nameof(AutoHideHeight)))
				AutoHideHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute(nameof(AutoHideMinWidth)))
				AutoHideMinWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute(nameof(AutoHideMinHeight)))
				AutoHideMinHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
			if (reader.MoveToAttribute(nameof(CanDockAsTabbedDocument)))
				CanDockAsTabbedDocument = bool.Parse(reader.Value);
			if (reader.MoveToAttribute(nameof(CanMove)))
				CanMove = bool.Parse(reader.Value);
			base.ReadXml(reader);
		}

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer) {
			if (IsExpanded)
				writer.WriteAttributeString(nameof(IsExpanded), IsExpanded.ToString());
			if (!CanHide)
				writer.WriteAttributeString(nameof(CanHide), CanHide.ToString());
			if (!CanAutoHide)
				writer.WriteAttributeString(nameof(CanAutoHide), CanAutoHide.ToString(CultureInfo.InvariantCulture));
			if (AutoHideWidth > 0)
				writer.WriteAttributeString(nameof(AutoHideWidth), AutoHideWidth.ToString(CultureInfo.InvariantCulture));
			if (AutoHideHeight > 0)
				writer.WriteAttributeString(nameof(AutoHideHeight), AutoHideHeight.ToString(CultureInfo.InvariantCulture));
			if (AutoHideMinWidth != 25.0)
				writer.WriteAttributeString(nameof(AutoHideMinWidth), AutoHideMinWidth.ToString(CultureInfo.InvariantCulture));
			if (AutoHideMinHeight != 25.0)
				writer.WriteAttributeString(nameof(AutoHideMinHeight), AutoHideMinHeight.ToString(CultureInfo.InvariantCulture));
			if (!CanDockAsTabbedDocument)
				writer.WriteAttributeString(nameof(CanDockAsTabbedDocument), CanDockAsTabbedDocument.ToString(CultureInfo.InvariantCulture));
			if (!CanMove)
				writer.WriteAttributeString(nameof(CanMove), CanMove.ToString());
			base.WriteXml(writer);
		}

		/// <inheritdoc />
		public override void Close() {
			if (Root?.Manager != null) {
				var dockingManager = Root.Manager;
				dockingManager.ExecuteCloseCommand(this);
			}
			else
				CloseAnchorable();
		}

#if TRACE
		/// <inheritdoc />
		public override void ConsoleDump(int tab) {
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine($"Anchorable({ContentId}, {Title})");
		}
#endif

		/// <summary>Method can be invoked to fire the <see cref="Hiding"/> event.</summary>
		/// <param name="args"></param>
		protected virtual void OnHiding(CancelEventArgs args) => Hiding?.Invoke(this, args);

		#endregion Overrides

		#region Public Methods
		public void Hide() {
			if (Root?.Manager is DockingManager dockingManager)
				dockingManager.ExecuteHideCommand(this);
			else
				HideAnchorable(true);
		}

		/// <summary>Method can be invoked to fire the <see cref="Collapsing"/> event.</summary>
		/// <param name="args"></param>
		protected virtual void OnExpanding(CancelEventArgs args) => Collapsing?.Invoke(this, args);

		/// <inheritdoc />


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
		internal void UpdateIsDirectlyHostedInFloatingWindow() => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Internal Methods

		#region Private Methods

		private void OnParentChildrenCollectionChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

		#endregion Private Methods

		/// <summary>Hide this contents.</summary>
		/// <remarks>Add this content to <see cref="ILayoutRoot.Hidden"/> collection of parent root.</remarks>
		/// <param name="cancelable"></param>
		internal bool HideAnchorable(bool cancelable) {
			//Debug.WriteLine($"{cancelable}", "HideAnchorable 1");

			if (!IsVisible) {
				IsSelected = true;
				IsActive = true;
				return false;
			}

			if (cancelable) {
				var args = new CancelEventArgs();
				OnHiding(args);
				if (args.Cancel)
					return false;
			}
			//Debug.WriteLine($"{cancelable}", "HideAnchorable 2");

			RaisePropertyChanging(nameof(IsHidden));
			RaisePropertyChanging(nameof(IsVisible));
			if (Parent is ILayoutGroup) {
				//Debug.WriteLine($"{cancelable}", "HideAnchorable 3");

				var parentAsGroup = Parent as ILayoutGroup;
				PreviousContainer = parentAsGroup;
				PreviousContainerIndex = parentAsGroup.IndexOfChild(this);
			}
			Root?.Hidden?.Add(this as LayoutAnchorable);
			RaisePropertyChanged(nameof(IsVisible));
			RaisePropertyChanged(nameof(IsHidden));
			NotifyIsVisibleChanged();

			return true;
		}

		/// <summary>Show the content.</summary>
		/// <remarks>Try to show the content where it was previously hidden.</remarks>
		public void Show() {
			if (IsVisible)
				return;
			if (!IsHidden)
				throw new InvalidOperationException();
			RaisePropertyChanging(nameof(IsHidden));
			RaisePropertyChanging(nameof(IsVisible));
			var added = false;
			var root = Root;
			if (root?.Manager?.LayoutUpdateStrategy != null)
				added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(root as LayoutRoot, this, PreviousContainer);

			if (!added && PreviousContainer != null) {
				var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;
				if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
					previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
				else
					previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

				Parent = previousContainerAsLayoutGroup;
				IsSelected = true;
				IsActive = true;
			}

			root?.Manager?.LayoutUpdateStrategy?.AfterInsertAnchorable(root as LayoutRoot, this);
			PreviousContainer = null;
			PreviousContainerIndex = -1;
			RaisePropertyChanged(nameof(IsVisible));
			RaisePropertyChanged(nameof(IsHidden));
			NotifyIsVisibleChanged();
		}

		#endregion Public Methods

		#region Internal Methods

		internal bool CloseAnchorable() {
			if (!TestCanClose())
				return false;
			//if(IsAutoHidden)
			//	ToggleAutoHide();
			CloseInternal();
			return true;
		}

		#endregion Internal Methods

		#region Private Methods

		private void NotifyIsVisibleChanged() => IsVisibleChanged?.Invoke(this, EventArgs.Empty);

		private void UpdateParentVisibility() {
			// Element is Hidden since it has no parent but a previous parent
			if (PreviousContainer != null && Parent == null) {
				// Go back to using previous parent
				Parent = PreviousContainer;
				////        PreviousContainer = null;
			}

			if (Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}

		//Todo 寻找更好的解决方法
		/// <summary>
		/// 强制更新高度
		/// </summary>
		/// <param name="value"></param>
		public void ForceHeight(GridLength value) {
			if (value.IsAbsolute)
				_resizableAbsoluteDockHeight = value.Value;
			RaisePropertyChanging(nameof(DockHeight));
			_dockHeight = value;
			RaisePropertyChanged(nameof(DockHeight));
			OnDockHeightChanged();
		}

		public void ForceWidth(GridLength value) {
			if (value.IsAbsolute)
				_resizableAbsoluteDockWidth = value.Value;
			RaisePropertyChanging(nameof(DockWidth));
			_dockWidth = value;
			RaisePropertyChanged(nameof(DockWidth));
			OnDockWidthChanged();
		}

		public double CalculatedDockMinWidth() {
			return _dockMinWidth;
		}

		public double CalculatedDockMinHeight() {
			return _dockMinHeight;
		}



		protected virtual void OnDockWidthChanged() {
		}

		protected virtual void OnDockHeightChanged() {
		}


		private void UpdateSize() {
			//Debug.WriteLine($"{LayoutAnchorable?.Title}", "LayoutAnchorableExpanderPane UpdateSize 0");
			if (IsExpanded) {
				if (ExpandDirection == ExpandDirection.Right) {
					ForceWidth(new GridLength(0, GridUnitType.Auto));
				}
				else {
					ForceHeight(new GridLength(0, GridUnitType.Auto));
				}
			}
			else {
				if (ExpandDirection == ExpandDirection.Right) {
					DockWidth = new GridLength(0, GridUnitType.Auto);
				}
				else {
					DockHeight = new GridLength(0, GridUnitType.Auto);
				}
			}
		}

		#endregion Private Methods
	}
}
