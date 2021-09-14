using System.Diagnostics;
using System.Windows.Controls;
using KsWare.CryptoPad.TableEditor;

namespace KsWare.CryptoPad.RichTextEditor {
	/// <summary>
	/// Interaction logic for RichTextEditor.xaml
	/// </summary>
	public partial class RichTextEditorControllerView : UserControl {

		public RichTextEditorControllerView() {
			InitializeComponent();
			DataContextChanged += (s, e) => {
				//Debug.WriteLine($"DataContext: {e.NewValue?.GetType().Name??"Null"}");
				if (e.OldValue is RichTextEditorControllerVM vmo) vmo.Data = null;
				if (e.NewValue is RichTextEditorControllerVM vm) vm.Data = this.RichTextBox;
			};

			Loaded += (sender, args) => {
				if (DataContext is RichTextEditorControllerVM vm) vm.Data = this.RichTextBox;
			};
		}
	}
}
