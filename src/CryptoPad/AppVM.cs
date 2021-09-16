using System;
using System.IO;
using System.Linq;
using System.Windows;
using KsWare.CryptoPad.IPC;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class AppVM : ApplicationVM {

		public AppVM() {
			RegisterChildren(()=>this);
			StartupUri = typeof(ShellVM);
		}

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e) {
			ParseCommandline(e.Args);

			var client = Communicator.GetClient();
			if (client != null) {
				client.Connect();
				var response = client.SendRequest(MessageSerializer.Serialize("CommandLine", CommandLine));
				Shutdown();
				return;
			}

			base.OnStartup(e);
		}

		private void ParseCommandline(string[] args) {
			var c = new CommandLineData();
			for (int i = 0; i<args.Length; i++ ) {
				var v = args[i];
				switch (v.ToLowerInvariant()) {
					case "-s": case "--session": c.SessionName = args[++i]; break;
					case "-ro": case "--readonly": c.ReadOnly = true; break;
					case "-f": case "--format": c.Format = args[++i]; break;
					case "-h": case "/h":case "-?": case "/?": break; // TODO command line help
					default: if (File.Exists(v)) c.FileName = v; break;
				}
			}

			CommandLine = c;
		}

		public CommandLineData CommandLine { get; set; }

	}

}
