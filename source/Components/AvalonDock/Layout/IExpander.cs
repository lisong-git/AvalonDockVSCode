using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AvalonDock.Layout {
	public interface IExpander {
		ExpandDirection ExpandDirection { get; set; }
		bool IsExpanded { get; set; }
	}
}
