/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace AvalonDock.Controls
{
	/// <summary>
	/// This class manages the drag & drop behavior when the user drags a:
	/// - document (<see cref="LayoutDocument"/>) or
	/// - tool window (<see cref="LayoutAnchorable"/>) and drops it in an alternative position.
	///
	/// The <see cref="LayoutFloatingWindowControl"/> contains a <see cref="DragService"/> field
	/// in order to implement the drag behavior for inheriting classes
	/// (<see cref="LayoutDocumentFloatingWindowControl"/> and <see cref="LayoutAnchorableFloatingWindowControl"/>).
	///
	/// Dragging a <see cref="LayoutDocument"/> usually results in a converted <see cref="LayoutDocumentFloatingWindow"/>
	/// being actually dragged around. Likewise, Dragging a <see cref="LayoutAnchorable"/> usually
	/// results in a converted <see cref="LayoutAnchorableFloatingWindow"/> being dragged around.
	///
	/// The behavior at the drop position can be that the floating window control is converted back into its
	/// original class type and being inserted/dropped at the final drop target position. But its also possible
	/// that the floating window control remains a floating window if the user simply drags an item out and positions
	/// it at no specific drop target position. The behavior at the final drop position is not always the
	/// same since it depends on:
	/// 1- the item being dragged around (and its content) and
	/// 2- the drop position (and its content).
	/// </summary>
	/// <seealso cref="LayoutAnchorable"/>
	/// <seealso cref="LayoutDocument"/>
	/// <seealso cref="LayoutAnchorableFloatingWindow"/>
	/// <seealso cref="LayoutDocumentFloatingWindow"/>
	/// <seealso cref="LayoutDocumentFloatingWindowControl"/>
	/// <seeslso cref="LayoutAnchorableFloatingWindowControl"/>).
	internal class DragService
	{
		#region fields

		private DockingManager _manager;
		private LayoutFloatingWindowControl _floatingWindow;

		// A list of hosts that can display an overlaywindow and offer a drop target (docking position)
		private List<IOverlayWindowHost> _overlayWindowHosts = new List<IOverlayWindowHost>();

		private IOverlayWindowHost _currentHost;
		private IOverlayWindow _currentWindow;
		private List<IDropArea> _currentWindowAreas = new List<IDropArea>();
		private IDropTarget _currentDropTarget;
		private bool _isDrag;

		#endregion fields

		#region Constructors

		/// <summary>
		/// Class constructor from <see cref="LayoutFloatingWindowControl"/> that is using this
		/// service to implement its drag and drop (dock) behavior.
		/// </summary>
		/// <param name="floatingWindow">Floating window manipulated by this drag service.</param>
		public DragService(LayoutFloatingWindowControl floatingWindow)
		{
			_floatingWindow = floatingWindow;
			_manager = floatingWindow.Model.Root.Manager;
		}

		#endregion Constructors

		#region Internal Methods

		//static int debugCount = 0;
		/// <summary>
		/// Method is invoked by the <see cref="LayoutFloatingWindowControl"/> to update the
		/// current mouse position as the user drags the floating window with the mouse cursor.
		/// </summary>
		/// <param name="dragPosition">The screen coordinates of the current mouse cursor position.</param>
		internal void UpdateMouseLocation(Point dragPosition)
		{
			////var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
			// TODO - pass in without DPI adjustment, screen co-ords, adjust inside the target window

			if (!_isDrag)
			{
				GetOverlayWindowHosts();
				_isDrag = true;
			}

			var newHost = _overlayWindowHosts.FirstOrDefault(oh => oh.HitTestScreen(dragPosition));
			if (_currentHost != null || _currentHost != newHost)
			{
			//Debug.WriteLine($"{newHost}", "DragService UpdateMouseLocation");
				//is mouse still inside current overlay window host?
				if ((_currentHost != null && !_currentHost.HitTestScreen(dragPosition)) ||
					_currentHost != newHost)
				{
					//esit drop target
					if (_currentDropTarget != null)
						_currentWindow.DragLeave(_currentDropTarget);

					_currentDropTarget = null;

					//exit area
					_currentWindowAreas.ForEach(a =>
						_currentWindow.DragLeave(a));
					_currentWindowAreas.Clear();

					//hide current overlay window
					if (_currentWindow != null)
						_currentWindow.DragLeave(_floatingWindow);
					if (_currentHost != null)
					{
						_currentHost.HideOverlayWindow();
						GetOverlayWindowHosts();
					}

					_currentHost = null;
				}

				if (_currentHost != newHost)
				{
					_currentHost = newHost;
					_currentWindow = _currentHost.ShowOverlayWindow(_floatingWindow);
					_currentWindow.DragEnter(_floatingWindow);

					// Set the target window to topmost
					//if (_currentHost is LayoutFloatingWindowControl fwc &&
					//	(fwc.OwnedByDockingManagerWindow == _floatingWindow.OwnedByDockingManagerWindow || fwc.OwnedByDockingManagerWindow))
					//{
					//	BringWindowToTop2(fwc);
					//}
					//else if (_currentHost is DockingManager dockingManager)
					//{
					//	BringWindowToTop2(Window.GetWindow(dockingManager));
					//}

					//GetOverlayWindowHosts();

					//BringWindowToTop2(_floatingWindow);
					_floatingWindow.Topmost = true; 
					//if(_currentWindow is Window overlayWindow) {
					//	BringWindowToTop2(overlayWindow);
					//}
				}
			}

			if (_currentHost == null)
				return;

			if (_currentDropTarget != null &&
				!_currentDropTarget.HitTestScreen(dragPosition))
			{
				_currentWindow.DragLeave(_currentDropTarget);
				_currentDropTarget = null;
			}

			List<IDropArea> areasToRemove = new List<IDropArea>();
			_currentWindowAreas.ForEach(a =>
			{
				//is mouse still inside this area?
				if (!a.DetectionRect.Contains(a.TransformToDeviceDPI(dragPosition)))
				{
					_currentWindow.DragLeave(a);
					areasToRemove.Add(a);
				}
			});

			areasToRemove.ForEach(a =>
				_currentWindowAreas.Remove(a));

			//foreach(var v in _currentHost.GetDropAreas(_floatingWindow)) {
			//	Debug.WriteLine($"{v.Type}", "UpdateMouseLocation 01");
			//}
			//获取当前所处区域下的可拖拽停靠控件
			var areasToAdd =
				_currentHost.GetDropAreas(_floatingWindow)
								.Where(cw => !_currentWindowAreas.Contains(cw) && cw.DetectionRect.Contains(cw.TransformToDeviceDPI(dragPosition)))
								.ToList();
			//Debug.WriteLine($"=================================================================================");
			_currentWindowAreas.AddRange(areasToAdd);

			Debug.WriteLine($"{_currentHost.GetDropAreas(_floatingWindow).Count()}; {string.Join(",", _currentHost.GetDropAreas(_floatingWindow).Select(o => o.Type))}", "UpdateMouseLocation 1");
			//Debug.WriteLine($"{areasToAdd.Count}; {string.Join(",", areasToAdd.Select(o => o.Type))}", "UpdateMouseLocation 2");
			//Debug.WriteLineIf(areasToAdd.Count > 0, $"{areasToAdd.Count}; {string.Join(",", areasToAdd.Select(o => o.Type))}", "UpdateMouseLocation 1");
			//显示可插入的区域小图预览
			areasToAdd.ForEach(a => _currentWindow.DragEnter(a));
			//Debug.WriteLine($"{_currentDropTarget?.Type}, {_currentWindow.GetType().Name}", "UpdateMouseLocation 2");

			if(_currentDropTarget == null) {
				_currentWindowAreas.ForEach(wa => {
					if(_currentDropTarget != null)
						return;

					_currentDropTarget = _currentWindow.GetTargets().FirstOrDefault(dt => dt.HitTestScreen(dragPosition));
					//var v = _currentWindow.GetTargets().OfType<AnchorableExpanderGroupDropTarget>().Where(o=> o.Type == DropTargetType.AnchorableExpanderDockLeft).SingleOrDefault();
					//Debug.WriteLine($"{v.DetectionRects.FirstOrDefault()}; {dragPosition}", "UpdateMouseLocation 4");
					//Debug.WriteLine($"{_currentDropTarget != null}, {_currentWindow.GetTargets().Select(o => o.Type.ToString()).Aggregate((o1, o2) => $"{o1}, {o2}")}", "UpdateMouseLocation 4");
					if(_currentDropTarget != null) {
						// 显示可插入区域大图预览
						_currentWindow.DragEnter(_currentDropTarget);
						BringWindowToTop2((Window) _currentWindow);
						return;
					}
				});
			}
		}

		/// <summary>
		/// Method is invoked by the <see cref="LayoutFloatingWindowControl"/> to indicate that the
		/// <see cref="LayoutFloatingWindowControl"/> (and its content) should be dropped/docked at
		/// the current mouse position.
		///
		/// The drop/dock behavior depends on whether the current mouse position is an actual drop target
		/// the item being dragged, and the item being docked (if any) etc.
		/// </summary>
		/// <param name="dropLocation">The screen coordinates of the drop/dock position.</param>
		/// <param name="dropHandled">Indicates whether the drop was handled such that the
		/// dropped <see cref="LayoutFloatingWindowControl"/> can be removed now (since it content
		/// is docked into a new visual tree position).</param>
		internal void Drop(Point dropLocation, out bool dropHandled)
		{
			// TODO - pass in without DPI adjustment, screen co-ords, adjust inside the target window
			dropHandled = false;

			UpdateMouseLocation(dropLocation);

			var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;
			var root = floatingWindowModel.Root;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			if (_currentDropTarget != null)
			{
				_currentWindow.DragDrop(_currentDropTarget);
				root.CollectGarbage();
				dropHandled = true;
			}

			_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

			if (_currentDropTarget != null)
				_currentWindow.DragLeave(_currentDropTarget);

			if (_currentWindow != null)
				_currentWindow.DragLeave(_floatingWindow);

			_currentWindow = null;
			_currentHost = null;
			_isDrag = false;
		}

		/// <summary>
		/// Method can be invoked to cancel the current drag and drop process and leave the
		/// <see cref="LayoutFloatingWindowControl"/> at its current position without performing
		/// a drop/dock operation.
		/// </summary>
		internal void Abort()
		{
			var floatingWindowModel = _floatingWindow.Model as LayoutFloatingWindow;

			_currentWindowAreas.ForEach(a => _currentWindow.DragLeave(a));

			if (_currentDropTarget != null)
				_currentWindow.DragLeave(_currentDropTarget);

			if (_currentWindow != null)
				_currentWindow.DragLeave(_floatingWindow);

			_currentWindow = null;

			if (_currentHost != null)
				_currentHost.HideOverlayWindow();

			_currentHost = null;
		}

		#endregion Internal Methods

		#region Private Methods

		private void BringWindowToTop2(Window window)
		{
			if (window == null) return;

			Win32Helper.SetWindowPos(new WindowInteropHelper(window).Handle,
				IntPtr.Zero, 0, 0, 0, 0, Win32Helper.SetWindowPosFlags.IgnoreResize | Win32Helper.SetWindowPosFlags.IgnoreMove | Win32Helper.SetWindowPosFlags.DoNotActivate);
		}

		/// <summary>
		/// Adds <see cref="IOverlayWindowHost"/>s into a private collection of possible
		/// drop target hosts that can show a drop target button to drop a dragged
		/// <see cref="LayoutAnchorableFloatingWindowControl"/> or
		/// <see cref="LayoutDocumentFloatingWindowControl"/> into it.
		/// </summary>
		private void GetOverlayWindowHosts()
		{
			if (_manager.Layout.RootPanel.CanDock)
			{
				_manager.GetOverlayWindowHostsByZOrder(ref _overlayWindowHosts, _floatingWindow);
			}
		}

		#endregion Private Methods
	}
}