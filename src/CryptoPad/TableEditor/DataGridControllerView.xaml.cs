using System.Diagnostics;
using System.Windows.Controls;

namespace KsWare.CryptoPad.TableEditor {

	/// <summary>
	/// Interaction logic for DataGridControllerView.xaml
	/// </summary>
	public partial class DataGridControllerView : UserControl {

		public DataGridControllerView() {
			InitializeComponent();

			DataContextChanged += (s, e) => {
				// connect/disconnect view and controller
				if (e.OldValue is DataGridControllerVM ovm) ovm.Data = null;
				if (e.NewValue is DataGridControllerVM vm) vm.Data = this.DataGrid;
			};

			Loaded += (sender, args) => {
				if (DataContext is DataGridControllerVM vm) vm.Data = this.DataGrid;
			};

			// TODO handle Escape. 1 cancel edit cell, 2 cancel edit row

			DataGrid.LostFocus += (s, e) => {
				Debug.WriteLine($"DataGrid.LostFocus");
			};

			DataGrid.IsKeyboardFocusWithinChanged += (s, e) => {
				Debug.WriteLine($"DataGrid.IsKeyboardFocusWithinChanged {e.NewValue}");
			};

			// TODO workaround for: source is updated only at LostFocus but not at LostKeyboardFocus (switch between tabs, open menu, ...)
			DataGrid.PreviewLostKeyboardFocus += (o, e) => {
				Debug.WriteLine($"DataGrid.PreviewLostKeyboardFocus {e.OldFocus.GetType().Name} => {e.NewFocus.GetType().Name}");
				if (!DataGrid.IsKeyboardFocusWithin) {

				}
				// DataGrid.CommitEdit(DataGridEditingUnit.Row, true);
				// var bindingExpression = BindingOperations.GetBindingExpression(DataGrid, System.Windows.Controls.DataGrid.ItemsSourceProperty);
				// bindingExpression.UpdateSource();
				if (e.NewFocus is TabItem) {
					// DataGrid.CommitEdit();
					//(DataGrid.ItemsSource as DataView).;
					//if(e.OriginalSource is DataGridCell cell)cell.
				}
				
			};
		}

	}

}


