using System.Security;
using System.Windows.Input;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Overlays {

	public class PasswordOverlayVM : BaseOverlayVM {

		/// <inheritdoc />
		public PasswordOverlayVM() {
			RegisterChildren(()=>this);
		}

		// public string Password { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public SecureString Password { get => Fields.GetValue<SecureString>(); set => Fields.SetValue(value); }

		public ICommand OKCommand { get => Fields.GetValue<ICommand>(); set => Fields.SetValue(value); }
	}

}