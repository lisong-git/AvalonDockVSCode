/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>Defines an API for interacting with (selected) content in a pane.</summary>
	public interface ILayoutSelector<T> where T : LayoutElement {
		#region Properties

		/// <summary>Gets or sets the index of the selected content in the pane.</summary>
		int SelectedIndex { get; set; }

		/// <summary>Gets the selected content in the pane.</summary>
		T SelectedItem { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>Gets the index or -1 of the specified child content.</summary>
		/// <param name="content"></param>
		/// <returns></returns>
		int IndexOf(T content);

		#endregion Methods
	}
}