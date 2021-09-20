using System.Security;
using System.Windows.Input;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Overlays {

	public class PasswordPanelVM : ObjectVM {

		/// <inheritdoc />
		public PasswordPanelVM() {
			RegisterChildren(()=>this);
		}

		// public string Password { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public SecureString Password { get => Fields.GetValue<SecureString>(); set => Fields.SetValue(value); }

		public ICommand OKCommand { get => Fields.GetValue<ICommand>(); set => Fields.SetValue(value); }

		public bool IsOpen { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
	}

}