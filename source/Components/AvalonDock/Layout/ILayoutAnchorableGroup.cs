
namespace AvalonDock.Layout
{
	/// <summary>Defines an interface for that identifies a <see cref="LayoutPaneComposite"/>
	/// or an equivalent class (<see cref="LayoutPaneComposite"/>, <see cref="LayoutAnchorableGroupControl"/> etc.)</summary>
	public interface ILayoutAnchorableGroup
		: ILayoutPanelElement, ILayoutPane
	{
	}
}