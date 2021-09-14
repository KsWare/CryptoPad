using System.Windows.Controls;

namespace KsWare.CryptoPad.TableEditor {

	/// <summary>
	/// Interaction logic for DataGridControllerView.xaml
	/// </summary>
	public partial class DataGridControllerView : UserControl {

		public DataGridControllerView() {
			InitializeComponent();

			DataContextChanged += (s, e) => {
				if (e.OldValue is DataGridControllerVM ovm) ovm.Data = null;
				if (e.NewValue is DataGridControllerVM vm) vm.Data = this.DataGrid;
			};

			Loaded += (sender, args) => {
				if (DataContext is DataGridControllerVM vm) vm.Data = this.DataGrid;
			};
		}

	}

}


