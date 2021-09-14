using System.Data;
using System.Windows.Controls;
using KsWare.Presentation.Core.Providers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.TableEditor {

	public class DataGridControllerVM : DataVM<DataGrid> {

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
			}

			if (e.NewData is DataGrid dg) {
				//if(_table!=null) dg.ItemsSource = _table.DefaultView;
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
			}
		}
	}

}