using System.Windows.Controls;

namespace KsWare.CryptoPad.RichTextEditor {
	/// <summary>
	/// Interaction logic for RichTextBoxControllerView.xaml
	/// </summary>
	public partial class RichTextBoxControllerView : UserControl {

		public RichTextBoxControllerView() {
			InitializeComponent();
			DataContextChanged += (s, e) => {
				// connect/disconnect view and controller
				if (e.OldValue is RichTextControllerVM vmo) vmo.Data = null;
				if (e.NewValue is RichTextControllerVM vm) vm.Data = this.RichTextBox;
			};

			Loaded += (sender, args) => {
				if (DataContext is RichTextControllerVM vm) vm.Data = this.RichTextBox;
			};
		}
	}
}
