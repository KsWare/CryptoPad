using System.IO;
using System.Windows;

namespace KsWare.CryptoPad {

	/// <summary>
	/// Interaction logic for PasswordDialog.xaml
	/// </summary>
	public partial class PasswordDialog : Window {

		public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
			"Password", typeof(string), typeof(PasswordDialog), new FrameworkPropertyMetadata(default(string), (d, e) => ((PasswordDialog) d).OnPasswordChanged(e)));

		private void OnPasswordChanged(DependencyPropertyChangedEventArgs e) {
			var newValue = (string) e.NewValue;
			var oldValue = (string) e.OldValue;

			PasswordBox.Password = newValue;
			PasswordBox.SelectAll();
		}

		public string Password {
			get => (string) GetValue(PasswordProperty);
			set => SetValue(PasswordProperty, value);
		}

		public string FileName {
			get => (string)FileNameTextBlock.ToolTip;
			set {
				if (string.IsNullOrWhiteSpace(value)) {
					FileNamePanel.Visibility = Visibility.Collapsed;
				}
				else {
					FileNamePanel.Visibility = Visibility.Visible;
					FileNameTextBlock.ToolTip = value;
					FileNameTextBlock.Text = Path.GetFileName(value);
				}
			}
		}


		public PasswordDialog() {
			InitializeComponent();
			PasswordBox.Focus();
		}

		private void OkClick(object sender, RoutedEventArgs e) {
			Password = PasswordBox.Password;
			DialogResult = true;
			Close();
		}

		private void CancelClick(object sender, RoutedEventArgs e) {
			DialogResult = false;
			Close();
		}

		public static string GetPassword(Window owner, string defaultPassword = null, string fileName = null) {
			var dlg = new PasswordDialog{Owner = owner, Password = defaultPassword, FileName = fileName};
			return dlg.ShowDialog()!=true ? defaultPassword : dlg.Password;
		}
	}
}
