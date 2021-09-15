using System.Windows.Data;

namespace KsWare.CryptoPad.TextEditor {

	/// <summary>
	/// Interaction logic for TextBoxControllerView.xaml
	/// </summary>
	public partial class TextBoxControllerView {

		public TextBoxControllerView() {
			InitializeComponent();

			DataContextChanged += (s, e) => {
				// connect/disconnect view and controller
				if (e.OldValue is TextBoxControllerVM vmOld) vmOld.Data = null;
				if (e.NewValue is TextBoxControllerVM vm) vm.Data = this.TextBox;
			};

			// TextBox.Loaded += (o, e) => Debug.Print("Loaded");
			// TextBox.LostFocus += (o, e) => Debug.Print("LostFocus");
			// TextBox.PreviewLostKeyboardFocus+= (o, e) => Debug.Print("PreviewLostKeyboardFocus");
			// TextBox.LostKeyboardFocus += (o, e) => Debug.Print("LostKeyboardFocus");
			// TextBox.TargetUpdated+= (o, e) => Debug.Print($"TargetUpdated {e.Property.Name}");
			// TextBox.SourceUpdated+= (o, e) => Debug.Print($"SourceUpdated {e.Property.Name}");
			// TextBox.Unloaded += (o, e) => Debug.Print("Unloaded");

			// workaround for: source is updated only at LostFocus but not at LostKeyboardFocus (switch between tabs, open menu, ...)
			TextBox.PreviewLostKeyboardFocus += (o, e) => {
				var bindingExpression = BindingOperations.GetBindingExpression(TextBox, System.Windows.Controls.TextBox.TextProperty);
				bindingExpression.UpdateSource();
			};

			Loaded += (sender, args) => {
				if (DataContext is TextBoxControllerVM vm) vm.Data = this.TextBox;
			};
		}

	}
}
