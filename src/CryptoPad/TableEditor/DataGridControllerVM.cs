using System.Data;
using System.Windows.Controls;
using KsWare.Presentation.Core.Providers;

namespace KsWare.CryptoPad.TableEditor {

	public class DataGridControllerVM : EditorControllerVM<DataGrid,DataTable> {

		private DataGridData _cache;

		public DataGridControllerVM() {
			RegisterChildren(() => this);
			Fields[nameof(Table)].ValueChanged += (s, e) => {
				if (Data != null) Data.ItemsSource = ((DataTable)e.NewValue)?.DefaultView;
			};
		}

		public DataTable Table { get => Fields.GetValue<DataTable>(); set => Fields.SetValue(value); }

		public DataGrid DataGrid => Data;

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);

			if (e.PreviousData is DataGrid d) {
				var sv = d.GetScrollViewer();
				_cache = new DataGridData {
					HorizontalOffset=sv.HorizontalOffset,
					VerticalOffset=sv.VerticalOffset,
				};
				d.ItemsSource = null;
				d.CellEditEnding -= DataGridOnCellEditEnding;
			}

			if (e.NewData is DataGrid dg) {
				dg.CellEditEnding += DataGridOnCellEditEnding;
				dg.ItemsSource = Table?.DefaultView;
				if (_cache == null) {
					
				}
				else {
					var v = dg.GetScrollViewer();
					v.ScrollToHorizontalOffset(_cache.HorizontalOffset);
					v.ScrollToVerticalOffset(_cache.VerticalOffset);
					// foreach (var cell in _cache.SelectedCells) {
					// 	var col = cell.Column;
					// }
				}
			}
		}

		private void DataGridOnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e) {
			// var yourClassInstance = e.EditingElement.DataContext;
			// var editingTextBox = e.EditingElement as TextBox;
			// var newValue = editingTextBox.Text;
			OnContentChanged();
		}

		public void CommitEdit() {
			if (DataGrid == null) return;
			DataGrid.CommitEdit(DataGridEditingUnit.Row, true);
		}
	}

	public class DataGridData {
		public double HorizontalOffset { get; set; }
		public double VerticalOffset { get; set; }
	}

}