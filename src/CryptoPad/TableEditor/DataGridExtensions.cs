using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DataGridExtensions {

	public static ScrollViewer GetScrollViewer(this UIElement elmt) {
		var border = VisualTreeHelper.GetChild(elmt, 0); // DataGrid: Decorator 
		var sv = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
		return sv;
	}
}