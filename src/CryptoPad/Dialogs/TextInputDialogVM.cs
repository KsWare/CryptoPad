using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Dialogs {

	public class TextInputDialogVM : DialogWindowVM {

		public static string ShowDialog(IWindowVM owner, string prompt, string title, string defaultValue) {
			var dlg = new TextInputDialogVM {
				Owner = owner,
				Prompt = prompt,
				Input = defaultValue,
				Title = {Value = title}
			};
			return dlg.ShowDialog() == true ? dlg.Input : defaultValue;
		}

		/// <inheritdoc />
		public TextInputDialogVM() {
			RegisterChildren(() => this);
			this.CloseAction.MːDoActionP = DoClose;
		}

		private void DoClose(object parameter) {
			DialogResult = parameter is string and "OK";
		}

		public string Prompt { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public string Input { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
	}
}
