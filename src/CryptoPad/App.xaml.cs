using System.Diagnostics;
using System.Windows;

namespace KsWare.CryptoPad {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		/// <inheritdoc />
		public App() {
			CatchUnhandledExceptions = Debugger.IsAttached == false;
		}
	}
}
