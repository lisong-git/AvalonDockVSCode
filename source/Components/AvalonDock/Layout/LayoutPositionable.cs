/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AvalonDock.Layout {
	/// <summary>Provides a base class for other layout panel models that support a specific class of panel.</summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class LayoutPositionable :LayoutAnchorable, IXmlSerializable, ILayoutPositionableElementWithActualSize {
		#region fields
		//private T _content;
		//private bool _isVisible = true;
		//private static GridLengthConverter _gridLengthConverter = new GridLengthConverter();

		// DockWidth fields
		private GridLength _dockWidth = new GridLength(1.0, GridUnitType.Star);

		private double? _resizableAbsoluteDockWidth;

		// DockHeight fields
		private GridLength _dockHeight = new GridLength(1.0, GridUnitType.Star);

		private double? _resizableAbsoluteDockHeight;

		private bool _allowDuplicateContent = true;
		//private bool _canRepositionItems = true;

		private double _dockMinWidth = 25.0;
		private double _dockMinHeight = 25.0;
		//private double _floatingWidth = 0.0;
		//private double _floatingHeight = 0.0;
		//private double _floatingLeft = 0.0;
		//private double _floatingTop = 0.0;

		private bool _isMaximized = false;

		[NonSerialized]
		private double _actualWidth;

		[NonSerialized]
		private double _actualHeight;

		#endregion fields

		#region Events

		/// <summary>
		/// Event fired when floating properties were updated.
		/// </summary>
		//public event EventHandler FloatingPropertiesUpdated;

		#endregion Events

		#region Properties

		//public T Content {
		//	get => _content;
		//	set {
		//		if(value == _content)
		//			return;
		//		_content = value;
		//		RaisePropertyChanged(nameof(Content));
		//	}
		//}

		#region DockWidth

		public GridLength DockWidth {
			get => _dockWidth.IsAbsolute && _resizableAbsoluteDockWidth < _dockWidth.Value && _resizableAbsoluteDockWidth.HasValue ?
						new GridLength(_resizableAbsoluteDockWidth.Value) : _dockWidth;
			set {
				if(value == _dockWidth || !(value.Value > 0))
					return;
				if(value.IsAbsolute)
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
				if(!_dockWidth.IsAbsolute)
					return;
				if(value <= _dockWidth.Value && value > 0) {
					RaisePropertyChanging(nameof(DockWidth));
					_resizableAbsoluteDockWidth = value;
					RaisePropertyChanged(nameof(DockWidth));
					OnDockWidthChanged();
				} else if(value > _dockWidth.Value && _resizableAbsoluteDockWidth < _dockWidth.Value)
					_resizableAbsoluteDockWidth = _dockWidth.Value;
			}
		}

		#endregion DockWidth

		#region DockHeight

		public GridLength DockHeight {
			get => _dockHeight.IsAbsolute && _resizableAbsoluteDockHeight < _dockHeight.Value && _resizableAbsoluteDockHeight.HasValue ?
						new GridLength(_resizableAbsoluteDockHeight.Value) : _dockHeight;
			set {
				Debug.WriteLine($"{_dockHeight}", "LayoutPositionable DockHeight 1");

				if(_dockHeight == value || !(value.Value > 0))
					return;
				if(value.IsAbsolute)
					_resizableAbsoluteDockHeight = value.Value;
				RaisePropertyChanging(nameof(DockHeight));
				_dockHeight = value;
				Debug.WriteLine($"{_dockHeight}", "LayoutPositionable DockHeight 2");
				RaisePropertyChanged(nameof(DockHeight));
				OnDockHeightChanged();
			}
		}

		public double FixedDockHeight => _dockHeight.IsAbsolute && _dockHeight.Value >= _dockMinHeight ? _dockHeight.Value : _dockMinHeight;

		public double ResizableAbsoluteDockHeight {
			get => _resizableAbsoluteDockHeight ?? 0;
			set {
				if(!_dockHeight.IsAbsolute)
					return;
				if(value < _dockHeight.Value && value > 0) {
					RaisePropertyChanging(nameof(DockHeight));
					_resizableAbsoluteDockHeight = value;
					RaisePropertyChanged(nameof(DockHeight));
					OnDockHeightChanged();
				} else if(value > _dockHeight.Value && _resizableAbsoluteDockHeight < _dockHeight.Value)
					_resizableAbsoluteDockHeight = _dockHeight.Value;
				else if(value == 0)
					_resizableAbsoluteDockHeight = DockMinHeight;
			}
		}

		#endregion DockHeight

		/// <summary>
		/// Gets or sets the AllowDuplicateContent property.
		/// When this property is true, then the LayoutDocumentPane or LayoutAnchorablePane allows dropping
		/// duplicate content (according to its Title and ContentId). When this dependency property is false,
		/// then the LayoutDocumentPane or LayoutAnchorablePane hides the OverlayWindow.DropInto button to prevent dropping of duplicate content.
		/// </summary>
		public bool AllowDuplicateContent {
			get => _allowDuplicateContent;
			set {
				if(value == _allowDuplicateContent)
					return;
				RaisePropertyChanging(nameof(AllowDuplicateContent));
				_allowDuplicateContent = value;
				RaisePropertyChanged(nameof(AllowDuplicateContent));
			}
		}

		//public bool CanRepositionItems {
		//	get => _canRepositionItems;
		//	set {
		//		if(value == _canRepositionItems)
		//			return;
		//		RaisePropertyChanging(nameof(CanRepositionItems));
		//		_canRepositionItems = value;
		//		RaisePropertyChanged(nameof(CanRepositionItems));
		//	}
		//}

		public double CalculatedDockMinWidth() {
			//var dockContent =  (ILayoutPositionableElement) this;
			//double childrenDockMinWidth = dockContent.CalculatedDockMinWidth();



			//return Math.Max(this._dockMinWidth, DockWidth);
			return _dockMinWidth;
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
				if(value == _dockMinWidth)
					return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinWidth));
				_dockMinWidth = value;
				RaisePropertyChanged(nameof(DockMinWidth));
			}
		}

		public double CalculatedDockMinHeight() {
			Debug.WriteLine($"{Content.GetType()}", "LayoutPositionable CalculatedDockMinHeight");
			//var dockContent =  (ILayoutPositionableElement) Content;
			//double childrenDockMinHeight = dockContent.CalculatedDockMinHeight();
			//return Math.Max(this._dockMinHeight, childrenDockMinHeight);
			return _dockMinHeight;
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
				if(value == _dockMinHeight)
					return;
				MathHelper.AssertIsPositiveOrZero(value);
				RaisePropertyChanging(nameof(DockMinHeight));
				_dockMinHeight = value;
				RaisePropertyChanged(nameof(DockMinHeight));
			}
		}

		//public double FloatingWidth {
		//	get => _floatingWidth;
		//	set {
		//		if(value == _floatingWidth)
		//			return;
		//		RaisePropertyChanging(nameof(FloatingWidth));
		//		_floatingWidth = value;
		//		RaisePropertyChanged(nameof(FloatingWidth));
		//	}
		//}

		//public double FloatingHeight {
		//	get => _floatingHeight;
		//	set {
		//		if(_floatingHeight == value)
		//			return;
		//		RaisePropertyChanging(nameof(FloatingHeight));
		//		_floatingHeight = value;
		//		RaisePropertyChanged(nameof(FloatingHeight));
		//	}
		//}

		//public double FloatingLeft {
		//	get => _floatingLeft;
		//	set {
		//		if(value == _floatingLeft)
		//			return;
		//		RaisePropertyChanging(nameof(FloatingLeft));
		//		_floatingLeft = value;
		//		RaisePropertyChanged(nameof(FloatingLeft));
		//	}
		//}

		//public double FloatingTop {
		//	get => _floatingTop;
		//	set {
		//		if(value == _floatingTop)
		//			return;
		//		RaisePropertyChanging(nameof(FloatingTop));
		//		_floatingTop = value;
		//		RaisePropertyChanged(nameof(FloatingTop));
		//	}
		//}

		//public bool IsMaximized {
		//	get => _isMaximized;
		//	set {
		//		if(value == _isMaximized)
		//			return;
		//		_isMaximized = value;
		//		RaisePropertyChanged(nameof(IsMaximized));
		//	}
		//}

		double ILayoutPositionableElementWithActualSize.ActualWidth {
			get => _actualWidth;
			set => _actualWidth = value;
		}

		double ILayoutPositionableElementWithActualSize.ActualHeight {
			get => _actualHeight;
			set => _actualHeight = value;
		}

		//public bool IsVisible {
		//	get => _isVisible;
		//	protected set {
		//		if(value == _isVisible)
		//			return;
		//		RaisePropertyChanging(nameof(IsVisible));
		//		_isVisible = value;
		//		OnIsVisibleChanged();
		//		RaisePropertyChanged(nameof(IsVisible));
		//	}
		//}

		//protected virtual void OnIsVisibleChanged() {
		//	UpdateParentVisibility();
		//}

		private void UpdateParentVisibility() {
			if(Parent is ILayoutElementWithVisibility parentPane)
				parentPane.ComputeVisibility();
		}
		#endregion Properties

		#region Internal Methods

		void ILayoutElementForFloatingWindow.RaiseFloatingPropertiesUpdated() {
			//FloatingPropertiesUpdated?.Invoke(this, EventArgs.Empty);
		}

		#endregion Internal Methods

		#region Overrides

		//public XmlSchema GetSchema() => null;

		//public void WriteXml(System.Xml.XmlWriter writer) {
		//	if(DockWidth.Value != 1.0 || !DockWidth.IsStar)
		//		writer.WriteAttributeString(nameof(DockWidth), _gridLengthConverter.ConvertToInvariantString(DockWidth.IsAbsolute ? new GridLength(FixedDockWidth) : DockWidth));
		//	if(DockHeight.Value != 1.0 || !DockHeight.IsStar)
		//		writer.WriteAttributeString(nameof(DockHeight), _gridLengthConverter.ConvertToInvariantString(DockHeight.IsAbsolute ? new GridLength(FixedDockHeight) : DockHeight));
		//	if(DockMinWidth != 25.0)
		//		writer.WriteAttributeString(nameof(DockMinWidth), DockMinWidth.ToString(CultureInfo.InvariantCulture));
		//	if(DockMinHeight != 25.0)
		//		writer.WriteAttributeString(nameof(DockMinHeight), DockMinHeight.ToString(CultureInfo.InvariantCulture));
		//	if(FloatingWidth != 0.0)
		//		writer.WriteAttributeString(nameof(FloatingWidth), FloatingWidth.ToString(CultureInfo.InvariantCulture));
		//	if(FloatingHeight != 0.0)
		//		writer.WriteAttributeString(nameof(FloatingHeight), FloatingHeight.ToString(CultureInfo.InvariantCulture));
		//	if(FloatingLeft != 0.0)
		//		writer.WriteAttributeString(nameof(FloatingLeft), FloatingLeft.ToString(CultureInfo.InvariantCulture));
		//	if(FloatingTop != 0.0)
		//		writer.WriteAttributeString(nameof(FloatingTop), FloatingTop.ToString(CultureInfo.InvariantCulture));
		//	if(IsMaximized)
		//		writer.WriteAttributeString(nameof(IsMaximized), IsMaximized.ToString());
		//}

		//public  void ReadXml(System.Xml.XmlReader reader) {
		//	if(reader.MoveToAttribute(nameof(DockWidth)))
		//		_dockWidth = (GridLength) _gridLengthConverter.ConvertFromInvariantString(reader.Value);
		//	if(reader.MoveToAttribute(nameof(DockHeight)))
		//		_dockHeight = (GridLength) _gridLengthConverter.ConvertFromInvariantString(reader.Value);
		//	if(reader.MoveToAttribute(nameof(DockMinWidth)))
		//		_dockMinWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(DockMinHeight)))
		//		_dockMinHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(FloatingWidth)))
		//		_floatingWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(FloatingHeight)))
		//		_floatingHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(FloatingLeft)))
		//		_floatingLeft = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(FloatingTop)))
		//		_floatingTop = double.Parse(reader.Value, CultureInfo.InvariantCulture);
		//	if(reader.MoveToAttribute(nameof(IsMaximized)))
		//		_isMaximized = bool.Parse(reader.Value);
		//}

		protected virtual void OnDockWidthChanged() {
		}

		protected virtual void OnDockHeightChanged() {
		}

		#endregion Overrides
	}
}