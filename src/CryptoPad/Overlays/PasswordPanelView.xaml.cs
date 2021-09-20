using System.Security;
using System.Windows.Controls;

namespace KsWare.CryptoPad.Overlays {

	/// <summary>
	/// Interaction logic for PasswordPanelView.xaml
	/// </summary>
	public partial class PasswordPanelView : UserControl {
		private bool _updatingViewModel;

		public PasswordPanelView() {
			InitializeComponent();

			#region Bind PasswordBox.Password <=> PasswordPanelVM.Password

			// PasswordBox.PasswordChanged += (s, e) => {
			// 	if (DataContext is PasswordPanelVM vm) vm.Password = PasswordBox.Password;
			// };
			// DataContextChanged += (s, e) => {
			// 	if (DataContext is PasswordPanelVM vm) {
			// 		vm.Fields[nameof(PasswordPanelVM.Password)].ValueChangedEvent.add =
			// 			(s2, e2) => { PasswordBox.Password = (string)e2.NewValue; };
			// 		PasswordBox.Password = vm.Password;
			// 	}
			// };

			PasswordBox.PasswordChanged += (s, e) => {
				_updatingViewModel = true;
				if (DataContext is PasswordPanelVM vm) vm.Password = PasswordBox.SecurePassword;
				_updatingViewModel = false;
			};
			DataContextChanged += (s, e) => {
				if (DataContext is PasswordPanelVM vm) {
					vm.Fields[nameof(PasswordPanelVM.Password)].ValueChangedEvent.add =
						(s2, e2) => SetPassword((SecureString)e2.NewValue);
					SetPassword(vm.Password);
				}
			};

			#endregion

		}

		private void SetPassword(SecureString password) {
			if(_updatingViewModel) return;
			PasswordBox.Password = password.ToInsecureString();
		}

	}

}
