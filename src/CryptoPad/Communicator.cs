using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KsWare.CryptoPad.IPC;
using KsWare.IO.NamedPipes;

namespace KsWare.CryptoPad {

	public class Communicator {

		public Communicator(ShellVM shell) {
			Shell = shell;
			Initialize();
		}

		public ShellVM Shell { get; }

		public void Initialize() {
			var currentProcessId = Process.GetCurrentProcess().Id;
			var processes = Process.GetProcessesByName("CryptoPad");
			var otherProcesses = processes.Where(p => p.Id != currentProcessId).ToArray();
			// CurrentServer = new NamedPipeServer($"CryptoPad-{currentProcessId}", 5, 1);
			// CurrentServer.RequestReceived += CurrentServerOnRequestReceived;
			// CurrentServer.Start();
			if (otherProcesses.Length == 0) {
				IsMaster = true;
				MasterServer = new NamedPipeServer("CryptoPad", 5, 1);
				MasterServer.RequestReceived += MasterServerOnRequestReceived;
				MasterServer.Start();
			}
			else {
				// Client = new NamedPipeClient("CryptoPad");
			}
		}

		public bool IsMaster { get; set; }

		private void CurrentServerOnRequestReceived(object? sender, PipeMsgEventArgs e) {
			var tokens = e.Request.Split(":", 2, StringSplitOptions.None);
			switch (tokens[0].ToLowerInvariant()) {
				case "ping": e.Response = "pong"; break;
				default: e.Response = "ERROR: Unknown request."; break;
			}
		}

		public NamedPipeServer CurrentServer { get; set; }
		public NamedPipeClient Client { get; set; }
		public NamedPipeServer MasterServer { get; set; }

		/// <inheritdoc />
		protected void OnExit(ExitEventArgs e) {
			
		}

		private void MasterServerOnRequestReceived(object sender, PipeMsgEventArgs e) {
			try {
				var header = MessageSerializer.Deserialize<Header>(e.Request);

				switch (header.Name) {
					case "CommandLine":
						Shell.HandleCommandLine(MessageSerializer.Deserialize<CommandLineData>(e.Request).Data);
						e.Response = "OK";
						break;
					default: 
						e.Response = "ERROR: Unknown request."; 
						break;
				}
			}
			catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}

		public void Close() {
			CurrentServer?.Dispose();
			MasterServer?.Dispose();
			Client?.Dispose();
		}

		public static NamedPipeClient GetClient() {
			var currentProcessId = Process.GetCurrentProcess().Id;
			var processes = Process.GetProcessesByName("CryptoPad");
			var otherProcesses = processes.Where(p => p.Id != currentProcessId).ToArray();

			if (otherProcesses.Length > 0) {
				return new NamedPipeClient("CryptoPad");
			}
			else {
				return null;
			}

		}
	}
}
