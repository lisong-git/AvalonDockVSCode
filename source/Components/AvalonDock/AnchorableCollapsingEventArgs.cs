/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.ComponentModel;

namespace AvalonDock
{
	/// <summary>
	/// Implements an event that can be raised to inform the client application about an
	/// anchorable that been closed and removed its content (viewmodel) from the docking framework.
	/// </summary>
	public class AnchorableCollapsingEventArgs :CancelEventArgs {
		/// <summary>
		/// Class constructor from the anchorables layout model.
		/// </summary>
		/// <param name="document"></param>
		public AnchorableCollapsingEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		public bool Collapse { get;set; }

		/// <summary>
		/// Gets the model of the anchorable that has been closed.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }
	}
}