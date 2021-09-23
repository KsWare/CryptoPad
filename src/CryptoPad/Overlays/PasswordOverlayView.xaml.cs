using System.Security;
using System.Windows.Controls;
using KsWare.Presentation;

namespace KsWare.CryptoPad.Overlays {

	/// <summary>
	/// Interaction logic for PasswordPanelView.xaml
	/// </summary>
	public partial class PasswordOverlayView : UserControl {
		private bool _updatingViewModel;

		public PasswordOverlayView() {
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
				if (DataContext is PasswordOverlayVM vm) vm.Password = PasswordBox.SecurePassword;
				_updatingViewModel = false;
			};

			DataContextChanged += (s, e) => {
				if (DataContext is PasswordOverlayVM vm) {
					vm.Fields[nameof(PasswordOverlayVM.Password)].ValueChangedEvent.add = (s2, e2) => SetPassword((SecureString)e2.NewValue);
					SetPassword(vm.Password);
					vm.Fields[nameof(PasswordOverlayVM.IsOpen)].ValueChangedEvent.add = IsOpenChanged;
					if (vm.IsOpen) PasswordBox.Focus();
				}
			};

			#endregion

		}

		private void IsOpenChanged(object sender, ValueChangedEventArgs e) {
			if (e.NewValue is bool b && b == true) PasswordBox.Focus();
		}

		private void SetPassword(SecureString password) {
			if(_updatingViewModel) return;
			PasswordBox.Password = password.ToInsecureString();
		}

	}

}
