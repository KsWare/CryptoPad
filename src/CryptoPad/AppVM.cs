using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KsWare.IO.NamedPipes;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class AppVM : ApplicationVM {

		public AppVM() {
			RegisterChildren(()=>this);
			StartupUri = typeof(ShellVM);
		}

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e) {
			// if (e.Args.Length > 0) {
			// 	return;
			// }

			// var currentProcessId = Process.GetCurrentProcess().Id;
			// var processes = Process.GetProcessesByName("CryptoPad");
			// var otherProcesses =processes.Where(p => p.Id != currentProcessId).ToArray();
			// CurrentServer= new NamedPipeServer($"CryptoPad-{currentProcessId}", -1, 1);
			// CurrentServer.RequestReceived+=CurrentServerOnRequestReceived;
			// CurrentServer.Run();
			// if (otherProcesses.Length == 0) {
			// 	MasterServer = new NamedPipeServer("CryptoPad", -1, 1);
			// 	MasterServer.Run();
			// 	MasterServer.RequestReceived+=MasterServerOnRequestReceived;
			// }
			// else {
			// 	MasterClient = new NamedPipeClient("CryptoPad");
			// }

			base.OnStartup(e);
		}

		private void CurrentServerOnRequestReceived(object? sender, PipeMsgEventArgs e) {
			var tokens = e.Request.Split(":", 2, StringSplitOptions.None);
			switch (tokens[0].ToLowerInvariant()) {
				case "ping": e.Response = "pong"; break;
				default: e.Response = "ERROR: Unknown request."; break;
			}
		}

		public NamedPipeServer CurrentServer { get; set; }

		/// <inheritdoc />
		protected override void OnExit(ExitEventArgs e) {
			CurrentServer?.Dispose();
			MasterServer?.Dispose();
			MasterClient?.Dispose();
			base.OnExit(e);
		}

		private void MasterServerOnRequestReceived(object sender, PipeMsgEventArgs e) {
			var tokens = e.Request.Split(":", 2, StringSplitOptions.None);
			switch (tokens[0].ToLowerInvariant()) {
				case "ping": e.Response = "pong"; break;
				default: e.Response = "ERROR: Unknown request."; break;
			}
		}

		public NamedPipeClient MasterClient { get; set; }

		public NamedPipeServer MasterServer { get; set; }
	}
}
