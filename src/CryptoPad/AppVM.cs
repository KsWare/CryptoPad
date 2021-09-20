using System;
using System.Diagnostics;
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

			if (CommandLine.FileName != null) {
				using var client = Communicator.GetClient();
				if (client != null) {
					try {
						client.Connect();
						var request = MessageSerializer.Serialize("CommandLine", CommandLine);
						var response = client.SendRequest(request);
					}
					catch (Exception ex) {
						Debug.WriteLine(ex.ToString());
						MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); 
						//TODO log error
					}

					Shutdown();
					return;
				}
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
