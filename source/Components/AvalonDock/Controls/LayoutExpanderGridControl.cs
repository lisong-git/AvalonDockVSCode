/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AvalonDock.Controls {
	/// <inheritdoc cref="Grid"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <inheritdoc cref="IAdjustableSizeLayout"/>
	/// <summary>
	/// The abstract LayoutGridControl<T> class (and its inheriting classes) are used to layout non-floating
	/// windows and documents in AvalonDock. This contains a definition of size proportion per item and
	/// includes user interactions with Grid Splitter elements to resize UI items in an interactive way.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="Grid"/>
	/// <seealso cref="ILayoutControl"/>
	/// <seealso cref="IAdjustableSizeLayout"/>
	public abstract class LayoutExpanderGridControl<T> :Grid,  IAdjustableSizeLayout where T : class, ILayoutElement {
		#region fields

		private LayoutPositionableGroup<T> _model;
		//private readonly Orientation _orientation = Orientation.Vertical;
		private bool _initialized;
		private ChildrenTreeChange? _asyncRefreshCalled;
		private readonly ReentrantFlag _fixingChildrenDockLengths = new ReentrantFlag();
		private Border _resizerGhost = null;
		private Window _resizerWindowHost = null;
		private Vector _initialStartPoint;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="model"></param>
		/// <param name="orientation"></param>
		internal LayoutExpanderGridControl() {
			//FlowDirection = System.Windows.FlowDirection.LeftToRight;
			UseLayoutRounding = false;
			Unloaded += OnUnloaded;
		}

		#endregion Constructors

		#region Properties

		public virtual ILayoutElement Model {
			get => _model;
			set {
				if(_model == value)
					return;
				if(value is LayoutPositionableGroup<T> model) {
					_model = model;
					//Debug.WriteLine($"{_model?.Children?.Count}", "LayoutExpanderGridControl Model 1");
					UpdateChildren();
				}
			}
		}

		private Orientation Orientation => (_model as ILayoutOrientableGroup).Orientation;

		private bool AsyncRefreshCalled => _asyncRefreshCalled != null;

		#endregion Properties

		#region Overrides

		/// <inheritdoc/>
		protected override void OnInitialized(EventArgs e) {
			base.OnInitialized(e);
			if(_model == null || _model.ChildrenCount == 0) { return; }

			_model.ChildrenTreeChanged += (s, args) => {
				if(args.Change != ChildrenTreeChange.DirectChildrenChanged)
					return;
				if(_asyncRefreshCalled.HasValue && _asyncRefreshCalled.Value == args.Change)
					return;
				_asyncRefreshCalled = args.Change;
				Dispatcher.BeginInvoke(new Action(() => {
					_asyncRefreshCalled = null;
					UpdateChildren();
				}), DispatcherPriority.Normal, null);
			};
			this.SizeChanged += OnSizeChanged;
		}

		#endregion Overrides

		#region Internal Methods

		protected void FixChildrenDockLengths() {
			using(_fixingChildrenDockLengths.Enter())
				OnFixChildrenDockLengths();
		}

		protected abstract void OnFixChildrenDockLengths();

		#endregion Internal Methods

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			if(_model == null || _model.ChildrenCount == 0) { return; }

			var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithAtcualSize.ActualWidth = ActualWidth;
			modelWithAtcualSize.ActualHeight = ActualHeight;
			if(!_initialized) {
				//Debug.WriteLine($"{e.PreviousSize.Height}, {e.NewSize.Height}", "ExpanderGridControl OnSizeChanged");
				_initialized = true;
				UpdateChildren();
			}

			AdjustFixedChildrenPanelSizes();
		}

		/// <summary>
		/// Method executes when the element is removed from within an element tree of loaded elements.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUnloaded(object sender, RoutedEventArgs e) {
			// In order to prevent resource leaks, unsubscribe from SizeChanged events.
			SizeChanged -= OnSizeChanged;
			Unloaded -= OnUnloaded;
		}

		public void UpdateChildren() {
			//Debug.WriteLine($"{_model == null}, {_model?.Children?.Count}", "ExpanderGridControl UpdateChildren 1");
			if(_model == null || _model.ChildrenCount == 0) { return; }

			var alreadyContainedChildren = Children.OfType<ILayoutControl>().ToArray();
			DetachOldSplitters();
			DetachPropertyChangeHandler();
			Children.Clear();
			ColumnDefinitions.Clear();
			RowDefinitions.Clear();
			var manager = _model?.Root?.Manager;
			if(manager == null)
				return;
			foreach(var child in _model.Children) {
				var foundContainedChild = alreadyContainedChildren.FirstOrDefault(chVM => chVM.Model == child);
				//Debug.WriteLine($"{child.GetType()}", "ExpanderGridControl UpdateChildren 2");
				if(foundContainedChild != null)
					Children.Add(foundContainedChild as UIElement);
				else
					Children.Add(manager.CreateUIElementForModel(child));
			}
			CreateSplitters();
			UpdateRowColDefinitions();
			AttachNewSplitters();
			AttachPropertyChangeHandler();
			//Debug.WriteLine($"", "ExpanderGridControl UpdateChildren 3");
		}

		private void AttachPropertyChangeHandler() {
			if(_model == null || _model.ChildrenCount == 0) { return; }

			foreach(var child in InternalChildren.OfType<ILayoutControl>())
				child.Model.PropertyChanged += this.OnChildModelPropertyChanged;
		}

		private void DetachPropertyChangeHandler() {
			if(_model == null || _model.ChildrenCount == 0) { return; }

			foreach(var child in InternalChildren.OfType<ILayoutControl>())
				child.Model.PropertyChanged -= this.OnChildModelPropertyChanged;
		}

		private void OnChildModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(_model == null || _model.ChildrenCount == 0) { return; }
			if(AsyncRefreshCalled)
				return;

			if(_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockWidth) && Orientation == Orientation.Horizontal) {
				if(ColumnDefinitions.Count != InternalChildren.Count)
					return;
				var changedElement = sender as ILayoutPositionableElement;
				var childFromModel = InternalChildren.OfType<ILayoutControl>().First(ch => ch.Model == changedElement) as UIElement;
				var indexOfChild = InternalChildren.IndexOf(childFromModel);
				ColumnDefinitions[indexOfChild].Width = changedElement.DockWidth;
				if(sender is LayoutAnchorableExpander pane) {
					if(indexOfChild > 0 && InternalChildren[indexOfChild - 1] is LayoutGridResizerControl sp) {
						sp.IsEnabled = pane.IsExpanded;
					}
				}
			} else if(_fixingChildrenDockLengths.CanEnter && e.PropertyName == nameof(ILayoutPositionableElement.DockHeight) && Orientation == Orientation.Vertical) {
				if(RowDefinitions.Count != InternalChildren.Count)
					return;


				var changedElement = sender as ILayoutPositionableElement;
				var childFromModel = InternalChildren.OfType<ILayoutControl>().First(ch => ch.Model == changedElement) as UIElement;
				var indexOfChild = InternalChildren.IndexOf(childFromModel);

				RowDefinitions[indexOfChild].Height = changedElement.DockHeight;
				if(sender is LayoutAnchorableExpander pane) {
					if(indexOfChild >0 && InternalChildren[indexOfChild - 1] is LayoutGridResizerControl sp) {
						sp.IsEnabled = pane.IsExpanded;
					}
				}
			} else if(e.PropertyName == nameof(ILayoutPositionableElement.IsVisible) || e.PropertyName == nameof(LayoutAnchorableExpander.IsExpanded)) {
				UpdateRowColDefinitions();
			}
		}

		private void UpdateRowColDefinitions() {
			if(_model == null || _model.ChildrenCount == 0) { return; }

			//var root = _model.Root;
			var manager = _model.Root?.Manager;
			if(manager == null)
				return;
			FixChildrenDockLengths();
			//Debug.Assert(InternalChildren.Count == _model.ChildrenCount + (_model.ChildrenCount - 1));

			#region Setup GridRows/Cols
			RowDefinitions.Clear();
			ColumnDefinitions.Clear();
			if(Orientation == Orientation.Horizontal) {
				#region Setup GridCols

				var iColumn = 0;
				var iChild = 0;
				// BD: 24.08.2020 added check for iChild against InternalChildren.Count
				for(var iChildModel = 0; iChildModel < _model.Children.Count && iChild < InternalChildren.Count; iChildModel++, iColumn++, iChild++) {
					var childModel = _model.Children[iChildModel] as LayoutAnchorableExpander;
					var colDef = new ColumnDefinition {
						Width = childModel.IsExpanded ? (childModel.DockWidth.IsStar ? childModel.DockWidth : new GridLength(1, GridUnitType.Star)) : new GridLength(1, GridUnitType.Auto),
						MinWidth = childModel.CalculatedDockMinWidth()

					};

					ColumnDefinitions.Add(colDef);
					Grid.SetColumn(InternalChildren[iChild], iColumn);

					//append column for splitter
					if(iChild >= InternalChildren.Count - 1)
						continue;
					iChild++;
					iColumn++;

					var nextChildModelVisibleExist = false;
					for(var i = iChildModel + 1; i < _model.Children.Count; i++) {
						var nextChildModel = _model.Children[i] as ILayoutPositionableElement;
						if(!nextChildModel.IsVisible)
							continue;
						nextChildModelVisibleExist = true;
						break;
					}

					ColumnDefinitions.Add(new ColumnDefinition {
						Width = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength(manager.GridSplitterWidth) : new GridLength(0.0, GridUnitType.Pixel)
					});
					Grid.SetColumn(InternalChildren[iChild], iColumn);
				}
				#endregion
			} else //if (_model.Orientation == Orientation.Vertical)
				{
				#region Setup GridRows
				var iRow = 0;
				var iChild = 0;
				// BD: 24.08.2020 added check for iChild against InternalChildren.Count
				for(var iChildModel = 0; iChildModel < _model.Children.Count && iChild < InternalChildren.Count; iChildModel++, iRow++, iChild++) {
					var childModel = _model.Children[iChildModel] as LayoutAnchorableExpander;
					//Debug.WriteLine($"{temp.GetType()}, {childModel.DockHeight}", "LayoutGridControl2 UpdateRowColDefinitions 3");
					RowDefinitions.Add(new RowDefinition {
						Height = childModel.IsExpanded ? (childModel.DockHeight.IsStar ? childModel.DockHeight : new GridLength(1, GridUnitType.Auto)) : new GridLength(1, GridUnitType.Auto),
						MinHeight = childModel.CalculatedDockMinHeight()
					});

					var row = RowDefinitions.Last();
					Debug.WriteLine($"{childModel.Title}, {row.Height}, {row.ActualHeight}, {row.MinHeight}, {row.MaxHeight}", "LayoutGridControl2 UpdateRowColDefinitions 3");

					Grid.SetRow(InternalChildren[iChild], iRow);

					//if (RowDefinitions.Last().Height.Value == 0.0)
					//    System.Diagnostics.Debugger.Break();

					//append row for splitter (if necessary)
					if(iChild >= InternalChildren.Count - 1)
						continue;
					iChild++;
					iRow++;

					var nextChildModelVisibleExist = false;
					//var isNextExpanded = false;

					for(var i = iChildModel + 1; i < _model.Children.Count; i++) {
						var nextChildModel = _model.Children[i] as ILayoutPositionableElement;
						if(!nextChildModel.IsVisible)
							continue;
						nextChildModelVisibleExist = true;
						//isNextExpanded = true;
						break;
					}

					RowDefinitions.Add(new RowDefinition {
						Height = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength(manager.GridSplitterHeight) : new GridLength(0.0, GridUnitType.Pixel),
						MinHeight = 1,
					});
					//if (RowDefinitions.Last().Height.Value == 0.0)
					//    System.Diagnostics.Debugger.Break();
					Grid.SetRow(InternalChildren[iChild], iRow);
				}
				#endregion
			}

			#endregion Setup GridRows/Cols
		}

		/// <summary>Apply Horizontal/Vertical cursor style
		/// (and splitter style if optional splitter style is set) to Grid Resizer Control.</summary>
		private void CreateSplitters() {
			for(var iChild = 1; iChild < Children.Count; iChild++) {
				//var splitter = new GridSplitter();
				var splitter = new LayoutGridResizerControl();
				if(Orientation == Orientation.Horizontal) {
					splitter.Cursor = Cursors.SizeWE;
					splitter.Style = _model.Root?.Manager?.GridSplitterVerticalStyle;
					splitter.Width = 3;
					//splitter.Width = _model.Root?.Manager?.GridSplitterWidth ?? 3;
					//splitter.VerticalAlignment = VerticalAlignment.Stretch;
				} else {
					splitter.Cursor = Cursors.SizeNS;
					splitter.Style = _model.Root?.Manager?.GridSplitterHorizontalStyle;
					splitter.Height = 3;
					//splitter.Height = _model.Root?.Manager?.GridSplitterHeight ?? 3;
					//splitter.Margin = new Thickness(0, -1, 0, -1);

					//splitter.Background = new SolidColorBrush(Colors.DarkRed);
					//splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
				}

				Children.Insert(iChild, splitter);
				// TODO: MK Is this a bug????
				iChild++;
			}
		}

		private void DetachOldSplitters() {
			foreach(var splitter in Children.OfType<LayoutGridResizerControl>()) {
				splitter.DragStarted -= OnSplitterDragStarted;
				splitter.DragDelta -= OnSplitterDragDelta;
				splitter.DragCompleted -= OnSplitterDragCompleted;
			}
		}

		private void AttachNewSplitters() {
			foreach(var splitter in Children.OfType<LayoutGridResizerControl>()) {
				splitter.DragStarted += OnSplitterDragStarted;
				splitter.DragDelta += OnSplitterDragDelta;
				splitter.DragCompleted += OnSplitterDragCompleted;
			}
		}

		private void OnSplitterDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => ShowResizerOverlayWindow(sender as LayoutGridResizerControl);

		private void OnSplitterDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
			//Debug.WriteLine($"{e.VerticalChange}, {_initialStartPoint.Y}, {_resizerWindowHost.Height - _resizerGhost.Height}, {_resizerWindowHost.Height}, {_resizerGhost.Height}", "OnSplitterDragDelta");
			var rootVisual = this.FindVisualTreeRoot() as Visual;
			var trToWnd = TransformToAncestor(rootVisual);
			var transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) - trToWnd.Transform(new Point());
			if(Orientation != System.Windows.Controls.Orientation.Horizontal) {
				Canvas.SetTop(_resizerGhost, MathHelper.MinMax(_initialStartPoint.Y + transformedDelta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height));
			} else
				Canvas.SetLeft(_resizerGhost, MathHelper.MinMax(_initialStartPoint.X + transformedDelta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width));
		}

		private void OnSplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			//Debug.WriteLine($"{e.VerticalChange}, {Canvas.GetTop(_resizerGhost)}, {_initialStartPoint.Y}", "OnSplitterDragCompleted");
			var splitter = sender as LayoutGridResizerControl;
			//var rootVisual = this.FindVisualTreeRoot() as Visual;

			//var trToWnd = TransformToAncestor(rootVisual);
			//var transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) - trToWnd.Transform(new Point());

			double delta;
			if(Orientation == System.Windows.Controls.Orientation.Horizontal)
				delta = Canvas.GetLeft(_resizerGhost) - _initialStartPoint.X;
			else
				delta = Canvas.GetTop(_resizerGhost) - _initialStartPoint.Y;

			var indexOfResizer = InternalChildren.IndexOf(splitter);

			var prevChild = InternalChildren[indexOfResizer - 1] as FrameworkElement;
			var nextChild = GetNextVisibleChild(indexOfResizer);

			var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
			var nextChildActualSize = nextChild.TransformActualSizeToAncestor();

			var prevChildModel = (ILayoutPositionableElement)(prevChild as ILayoutControl).Model;
			var nextChildModel = (ILayoutPositionableElement)(nextChild as ILayoutControl).Model;

			if(Orientation == System.Windows.Controls.Orientation.Horizontal) {
				if(prevChildModel.DockWidth.IsStar)
					prevChildModel.DockWidth = new GridLength(prevChildModel.DockWidth.Value * (prevChildActualSize.Width + delta) / prevChildActualSize.Width, GridUnitType.Star);
				else {
					var width = (prevChildModel.DockWidth.IsAuto) ? prevChildActualSize.Width : prevChildModel.DockWidth.Value;
					var resizedWidth = width + delta;
					prevChildModel.DockWidth = new GridLength(double.IsNaN(resizedWidth) ? width : resizedWidth, GridUnitType.Pixel);
				}

				if(nextChildModel.DockWidth.IsStar)
					nextChildModel.DockWidth = new GridLength(nextChildModel.DockWidth.Value * (nextChildActualSize.Width - delta) / nextChildActualSize.Width, GridUnitType.Star);
				else {
					var width = (nextChildModel.DockWidth.IsAuto) ? nextChildActualSize.Width : nextChildModel.DockWidth.Value;
					var resizedWidth = width - delta;
					nextChildModel.DockWidth = new GridLength(double.IsNaN(resizedWidth) ? width : resizedWidth, GridUnitType.Pixel);
				}
			} else {
				if(prevChildModel.DockHeight.IsStar) {
					//Debug.WriteLine($"{prevChildActualSize}, {delta}", "OnSplitterDragCompleted 1");
					prevChildModel.DockHeight = new GridLength(prevChildModel.DockHeight.Value * (prevChildActualSize.Height + delta) / prevChildActualSize.Height, GridUnitType.Star);
					//Debug.WriteLine($"{prevChild.TransformActualSizeToAncestor()}", "OnSplitterDragCompleted 2");

				} else {
					var height = (prevChildModel.DockHeight.IsAuto) ? prevChildActualSize.Height : prevChildModel.DockHeight.Value;
					var resizedHeight = height + delta;
					prevChildModel.DockHeight = new GridLength(double.IsNaN(resizedHeight) ? height : resizedHeight, GridUnitType.Pixel);
				}

				if(nextChildModel.DockHeight.IsStar)
					nextChildModel.DockHeight = new GridLength(nextChildModel.DockHeight.Value * (nextChildActualSize.Height - delta) / nextChildActualSize.Height, GridUnitType.Star);
				else {
					var height = (nextChildModel.DockHeight.IsAuto) ? nextChildActualSize.Height : nextChildModel.DockHeight.Value;
					var resizedHeight = height - delta;
					nextChildModel.DockHeight = new GridLength(double.IsNaN(resizedHeight) ? height : resizedHeight, GridUnitType.Pixel);
				}
			}

			HideResizerOverlayWindow();
		}

		public virtual void AdjustFixedChildrenPanelSizes(Size? parentSize = null) {
			Debug.WriteLine($"", "ExpanderGridControl AdjustFixedChildrenPanelSizes");
			//PrintFirstResizerControlSize();
			var visibleChildren = GetVisibleChildren();
			if(visibleChildren.Count == 0)
				return;

			var layoutChildrenModels = visibleChildren.OfType<ILayoutControl>()
				.Select(child => child.Model)
				.OfType<ILayoutPositionableElementWithActualSize>()
				.ToList();

			var splitterChildren = visibleChildren.OfType<LayoutGridResizerControl>().ToList();
			List<ILayoutPositionableElementWithActualSize> fixedPanels;
			List<ILayoutPositionableElementWithActualSize> relativePanels;

			// Get current available size of panel.
			var availableSize = parentSize ?? new Size(ActualWidth, ActualHeight);

			// Calculate minimum required size and current size of children.
			var minimumSize = new Size(0, 0);
			var currentSize = new Size(0, 0);
			var preferredMinimumSize = new Size(0, 0);
			if(Orientation == Orientation.Vertical) {
				fixedPanels = layoutChildrenModels.Where(child => child.DockHeight.IsAbsolute).ToList();
				relativePanels = layoutChildrenModels.Where(child => !child.DockHeight.IsAbsolute).ToList();
				minimumSize.Width += layoutChildrenModels.Max(child => child.CalculatedDockMinWidth());
				minimumSize.Height += layoutChildrenModels.Sum(child => child.CalculatedDockMinHeight());
				minimumSize.Height += splitterChildren.Sum(child => child.ActualHeight);
				currentSize.Width += layoutChildrenModels.Max(child => child.ActualWidth);
				currentSize.Height += layoutChildrenModels.Sum(child => child.ActualHeight);
				currentSize.Height += splitterChildren.Sum(child => child.ActualHeight);
				preferredMinimumSize.Width += layoutChildrenModels.Max(child => child.CalculatedDockMinWidth());
				preferredMinimumSize.Height += minimumSize.Height + fixedPanels.Sum(child => child.FixedDockHeight) - fixedPanels.Sum(child => child.CalculatedDockMinHeight());
			} else {
				fixedPanels = layoutChildrenModels.Where(child => child.DockWidth.IsAbsolute).ToList();
				relativePanels = layoutChildrenModels.Where(child => !child.DockWidth.IsAbsolute).ToList();
				minimumSize.Width += layoutChildrenModels.Sum(child => child.CalculatedDockMinWidth());
				minimumSize.Height += layoutChildrenModels.Max(child => child.CalculatedDockMinHeight());
				minimumSize.Width += splitterChildren.Sum(child => child.ActualWidth);
				currentSize.Width += layoutChildrenModels.Sum(child => child.ActualWidth);
				currentSize.Height += layoutChildrenModels.Max(child => child.ActualHeight);
				currentSize.Width += splitterChildren.Sum(child => child.ActualWidth);
				preferredMinimumSize.Height += layoutChildrenModels.Max(child => child.CalculatedDockMinHeight());
				preferredMinimumSize.Width += minimumSize.Width + fixedPanels.Sum(child => child.FixedDockWidth) - fixedPanels.Sum(child => child.CalculatedDockMinWidth());
			}

			// Apply corrected sizes for fixed panels.
			if(Orientation == Orientation.Vertical) {
				var delta = availableSize.Height - currentSize.Height;
				var relativeDelta = relativePanels.Sum(child => child.ActualHeight - child.CalculatedDockMinHeight());
				delta += relativeDelta;
				foreach(var fixedChild in fixedPanels) {
					if(minimumSize.Height >= availableSize.Height)
						fixedChild.ResizableAbsoluteDockHeight = fixedChild.CalculatedDockMinHeight();
					else if(preferredMinimumSize.Height <= availableSize.Height)
						fixedChild.ResizableAbsoluteDockHeight = fixedChild.FixedDockHeight;
					else if(relativePanels.All(child => Math.Abs(child.ActualHeight - child.CalculatedDockMinHeight()) <= 1)) {
						double panelFraction;
						var indexOfChild = fixedPanels.IndexOf(fixedChild);
						if(delta < 0) {
							var availableHeightLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
								.Sum(child => child.ActualHeight - child.CalculatedDockMinHeight());
							panelFraction = (fixedChild.ActualHeight - fixedChild.CalculatedDockMinHeight()) / (availableHeightLeft > 0 ? availableHeightLeft : 1);
						} else {
							var fixedHeightLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
								.Sum(child => child.FixedDockHeight);
							panelFraction = fixedChild.FixedDockHeight / (fixedHeightLeft > 0 ? fixedHeightLeft : 1);
						}

						var childActualHeight = fixedChild.ActualHeight;
						var heightToSet = Math.Max(Math.Round(delta * panelFraction + fixedChild.ActualHeight), fixedChild.CalculatedDockMinHeight());
						fixedChild.ResizableAbsoluteDockHeight = heightToSet;
						delta -= heightToSet - childActualHeight;
					}
				}
			} else {
				var delta = availableSize.Width - currentSize.Width;
				var relativeDelta = relativePanels.Sum(child => child.ActualWidth - child.CalculatedDockMinWidth());
				delta += relativeDelta;
				foreach(var fixedChild in fixedPanels) {
					if(minimumSize.Width >= availableSize.Width)
						fixedChild.ResizableAbsoluteDockWidth = fixedChild.CalculatedDockMinWidth();
					else if(preferredMinimumSize.Width <= availableSize.Width)
						fixedChild.ResizableAbsoluteDockWidth = fixedChild.FixedDockWidth;
					else {
						double panelFraction;
						var indexOfChild = fixedPanels.IndexOf(fixedChild);
						if(delta < 0) {
							var availableWidthLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
								.Sum(child => child.ActualWidth - child.CalculatedDockMinWidth());
							panelFraction = (fixedChild.ActualWidth - fixedChild.CalculatedDockMinWidth()) / (availableWidthLeft > 0 ? availableWidthLeft : 1);
						} else {
							var fixedWidthLeft = fixedPanels.Where(child => fixedPanels.IndexOf(child) >= indexOfChild)
								.Sum(child => child.FixedDockWidth);
							panelFraction = fixedChild.FixedDockWidth / (fixedWidthLeft > 0 ? fixedWidthLeft : 1);
						}

						var childActualWidth = fixedChild.ActualWidth;
						var widthToSet = Math.Max(Math.Round(delta * panelFraction + fixedChild.ActualWidth), fixedChild.CalculatedDockMinWidth());
						fixedChild.ResizableAbsoluteDockWidth = widthToSet;
						delta -= widthToSet - childActualWidth;
					}
				}
			}

			foreach(var child in InternalChildren.OfType<IAdjustableSizeLayout>())
				child.AdjustFixedChildrenPanelSizes(availableSize);
		}

		private FrameworkElement GetNextVisibleChild(int index) {
			for(var i = index + 1; i < InternalChildren.Count; i++) {
				if(InternalChildren[i] is LayoutGridResizerControl)
					continue;
				if(IsChildVisible(i))
					return InternalChildren[i] as FrameworkElement;
			}
			return null;
		}

		private List<FrameworkElement> GetVisibleChildren() {
			var visibleChildren = new List<FrameworkElement>();
			for(var i = 0; i < InternalChildren.Count; i++) {
				if(IsChildVisible(i) && InternalChildren[i] is FrameworkElement)
					visibleChildren.Add(InternalChildren[i] as FrameworkElement);
			}
			return visibleChildren;
		}

		private bool IsChildVisible(int index) {
			if(Orientation == Orientation.Horizontal) {
				if(index < ColumnDefinitions.Count)
					return ColumnDefinitions[index].Width.IsStar || ColumnDefinitions[index].Width.Value > 0;
			} else if(index < RowDefinitions.Count)
				return RowDefinitions[index].Height.IsStar || RowDefinitions[index].Height.Value > 0;

			return false;
		}

		private void ShowResizerOverlayWindow(LayoutGridResizerControl splitter) {
			_resizerGhost = new Border { Background = splitter.BackgroundWhileDragging, Opacity = splitter.OpacityWhileDragging };

			var indexOfResizer = InternalChildren.IndexOf(splitter);

			var prevChild = InternalChildren[indexOfResizer - 1] as FrameworkElement;
			var nextChild = GetNextVisibleChild(indexOfResizer);

			var prevChildActualSize = prevChild.TransformActualSizeToAncestor();
			var nextChildActualSize = nextChild.TransformActualSizeToAncestor();

			var prevChildModel = (ILayoutPositionableElement)(prevChild as ILayoutControl).Model;
			var nextChildModel = (ILayoutPositionableElement)(nextChild as ILayoutControl).Model;

			var ptTopLeftScreen = prevChild.PointToScreenDPIWithoutFlowDirection(new Point());

			Size actualSize;

			if(Orientation == System.Windows.Controls.Orientation.Horizontal) {
				actualSize = new Size(
					prevChildActualSize.Width - prevChildModel.CalculatedDockMinWidth() + splitter.ActualWidth + nextChildActualSize.Width - nextChildModel.CalculatedDockMinWidth(),
					nextChildActualSize.Height);

				_resizerGhost.Width = splitter.ActualWidth;
				_resizerGhost.Height = actualSize.Height;
				ptTopLeftScreen.Offset(prevChildModel.CalculatedDockMinWidth(), 0.0);
			} else {
				actualSize = new Size(
					prevChildActualSize.Width,
					prevChildActualSize.Height - prevChildModel.CalculatedDockMinHeight() + splitter.ActualHeight + nextChildActualSize.Height - nextChildModel.CalculatedDockMinHeight());

				_resizerGhost.Width = actualSize.Width;
				_resizerGhost.Height = splitter.ActualHeight;

				ptTopLeftScreen.Offset(0.0, prevChildModel.CalculatedDockMinHeight());
			}

			_initialStartPoint = splitter.PointToScreenDPIWithoutFlowDirection(new Point()) - ptTopLeftScreen;

			if(Orientation == System.Windows.Controls.Orientation.Horizontal)
				Canvas.SetLeft(_resizerGhost, _initialStartPoint.X);
			else
				Canvas.SetTop(_resizerGhost, _initialStartPoint.Y);

			var panelHostResizer = new Canvas { HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch, VerticalAlignment = System.Windows.VerticalAlignment.Stretch };
			panelHostResizer.Children.Add(_resizerGhost);

			_resizerWindowHost = new Window {
				Style = new Style(typeof(Window), null),
				SizeToContent = System.Windows.SizeToContent.Manual,
				ResizeMode = ResizeMode.NoResize,
				WindowStyle = System.Windows.WindowStyle.None,
				ShowInTaskbar = false,
				AllowsTransparency = true,
				Background = null,
				Width = actualSize.Width,
				Height = actualSize.Height,
				Left = ptTopLeftScreen.X,
				Top = ptTopLeftScreen.Y,
				ShowActivated = false,
				Owner = null,
				Content = panelHostResizer
			};
			_resizerWindowHost.Show();
		}

		private void HideResizerOverlayWindow() {
			if(_resizerWindowHost == null)
				return;
			_resizerWindowHost.Close();
			_resizerWindowHost = null;
		}

		#endregion Private Methods
	}
}