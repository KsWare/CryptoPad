using System.Data;
using System.Windows.Controls;
using KsWare.Presentation.Core.Providers;

namespace KsWare.CryptoPad.TableEditor {

	public class DataGridControllerVM : EditorControllerVM<DataGrid,DataTable> {

		private DataGrid _cache;

		public DataGridControllerVM() {
			RegisterChildren(() => this);
		}

		public DataTable Table { get => Fields.GetValue<DataTable>(); set => Fields.SetValue(value); }

		public DataGrid DataGrid => Data;

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);

			if (e.PreviousData is DataGrid d) {
				_cache = d;
				d.CellEditEnding -= DataGridOnCellEditEnding;
			}

			if (e.NewData is DataGrid dg) {
				dg.CellEditEnding += DataGridOnCellEditEnding;
				dg.ItemsSource = Table?.DefaultView;
				if (_cache == null) {
					
				}
				else {
					var c = _cache.GetScrollViewer();
					var v = dg.GetScrollViewer();
					v.ScrollToHorizontalOffset(c.HorizontalOffset);
					v.ScrollToVerticalOffset(c.VerticalOffset);
					// foreach (var cell in _cache.SelectedCells) {
					// 	var col = cell.Column;
					// }
				}

				dg.Focus();
			}
		}

		private void DataGridOnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e) {
			// var yourClassInstance = e.EditingElement.DataContext;
			// var editingTextBox = e.EditingElement as TextBox;
			// var newValue = editingTextBox.Text;
			OnContentChanged();
		}

		public void CommitEdit() {
			if(DataGrid==null) return;
			DataGrid.CommitEdit(DataGridEditingUnit.Row, true);
		}
	}

}