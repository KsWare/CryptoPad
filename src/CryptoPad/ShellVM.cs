using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using KsWare.CryptoPad.RichTextEditor;
using KsWare.CryptoPad.TableEditor;
using KsWare.CryptoPad.TextEditor;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

namespace KsWare.CryptoPad {

	public class ShellVM : WindowVM {

		private SessionData _sessionData;
		
		/// <inheritdoc />
		public ShellVM() {
			RegisterChildren(() => this);
			// Tabs.Add(new TextEditorVM { Title = { Value = "Text" } });
			// Tabs.Add(new RichTextEditorVM() { Title = { Value = "RTF" } });
			// Tabs.Add(new TableEditorVM() { Title = { Value = "Table" } });
			// SelectedTab = Tabs[0];

			var file = CommonMenu.AddMenuItem("_File");
			var newFile = file.AddMenuItem("_New");
			newFile.AddMenuItem("_Text document", DoNewFile, DataFormats.Text);
			newFile.AddMenuItem("_Rich text document", DoNewFile, DataFormats.Rtf);
			newFile.AddMenuItem("_Spread sheet", DoNewFile, "text/csv");
			file.AddMenuItem("_Open..", DoOpenFile);
			file.AddMenuItem("-");
			file.AddMenuItem("_Close", DoCloseFile);
			file.AddMenuItem("C_lose All", DoCloseAllFiles);
			file.AddMenuItem("-");
			file.AddMenuItem("E_xit", DoExit);
			// file.AddMenuItem("_Save", DoSaveFile);
			// file.AddMenuItem("Save _As..", DoSaveFileAs);

			Fields[nameof(SelectedTab)].ValueChangedEvent.add = AtTabChanged;
			Menu = CommonMenu;

			UIAccess.WindowChanged += (s, e) => {
				if (e.NewValue != null) e.NewValue.Closing += WindowClosing;
			};

			string fileName = null;
			string sessionName = null;
			string format = null;
			bool readOnly = false;
			var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
			for (int i = 0; i<args.Length; i++ ) {
				var v = args[i];
				switch (v.ToLowerInvariant()) {
					case "-s": case "--session": sessionName = args[++i]; break;
					case "-ro": case "--readonly": readOnly = true; break;
					case "-f": case "--format": format = args[++i]; break;
					case "-h": case "/h":case "-?": case "/?": break; // TODO command line help
					default: if (File.Exists(v)) fileName = v; break;
				}
			}

			if (sessionName != null) {
				// use specified session
			}
			else if (fileName != null) {
				sessionName = DateTime.Now.ToString("yyyyMMddHHmmss");
			}
			else {
				sessionName = "Default";
			}
			_sessionData = FileTools.LoadSessionData(sessionName, createIfNotExist: true);

			Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, DoRestoreTabs);
			Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => OpenFile(fileName, format, readOnly));
		}

		private void DoRestoreTabs() {
			foreach (var file in _sessionData.Files) {
				DoOpenFile();
			}
		}

		private void WindowClosing(object sender, CancelEventArgs e) {
			_sessionData.Files = Tabs.Select(t => t.GetFileInfo()).ToArray();
			FileTools.SaveSessionData(_sessionData);
		}

		private void DoCloseAllFiles() {
			SelectedTab = null;
			Tabs.Clear();
		}

		public void CloseTab(FileTabItemVM item, bool noSave=false) {
			item.OnClose(noSave); 
			var idx = Tabs.IndexOf(item);
			Tabs.RemoveAt(idx);
			if (idx == Tabs.Count) idx--;
			SelectedTab = idx >= 0 ? Tabs[idx] : null;
		}

		private void DoCloseFile() => CloseTab(SelectedTab);

		private void DoExit() {
			ApplicationVM.Current.Shutdown();
		}

		private void AtTabChanged(object sender, ValueChangedEventArgs e) {
			if (false) ;
			else if (e.NewValue is TextEditorVM txt) Menu = txt.Menu;
			else if (e.NewValue is RichTextEditorVM rtf) Menu = rtf.Menu;
			else if (e.NewValue is TableEditorVM tbl) Menu = tbl.Menu;
			else Menu = CommonMenu;
			ReplacePlaceholders(Menu, CommonMenu);
		}

		private void ReplacePlaceholders(IList<MenuItemVM> items, IList<MenuItemVM> templates) {
			for (int i = 0; i < items.Count; i++) {
				if (items[i] is MenuItemPlaceholderVM p) {
					var t=templates.FirstOrDefault(mi => mi.Caption == p.Caption);
					if (t != null) items[i] = t;
				}
				else if(items[i].Items.Count>0){
					var t=templates.FirstOrDefault(mi => mi.Caption == items[i].Caption);
					if (t != null) ReplacePlaceholders(items[i].Items, t.Items);
				}
			}
		}

		public ListVM<MenuItemVM> CommonMenu { get; private set; }

		public IList<MenuItemVM> Menu { get => Fields.GetValue<IList<MenuItemVM>>(); set => Fields.SetValue(value); }

		public ListVM<FileTabItemVM> Tabs { get; private set; }

		[Hierarchy(HierarchyType.Reference)]
		public FileTabItemVM SelectedTab { get => Fields.GetValue<FileTabItemVM>(); set => Fields.SetValue(value); }

		private void DoOpenFile() {
			var dlg = new OpenFileDialog {
				Title = "Select file to load...",
				Filter = /*1*/"Crypto Files|*.crypt"+
			             /*2*/"|Text Files|*.txt" +
			             /*3*/"|Rich Text Files|*.xps;*.rtf;*.wri;*.xaml"+
			             /*4*/"|Spread Sheets|*.csv;*.txt" +
			             /* */"|All Files|*.*",
				FilterIndex = 1,
				ShowReadOnly = true,
				CheckFileExists = true,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: format = "Crypt"; break;
				case 2: format = "Text"; break;
				case 3: format = "RichText"; break;
				case 4: format = "SpreadSheet"; break;
			}
			OpenFile(dlg.FileName, format, dlg.ReadOnlyChecked);
		}

		public void OpenFile(string fileName, string format = null, bool readOnly = false) {
			CryptoStreamInfo info = null;
			var sig = Path.GetExtension(fileName).ToLowerInvariant();
			string password = null;
			SWITCH:
			switch (sig) {
				case "Crypt": case ".crypt": 
					password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, CryptFile.LastPassword, fileName);
					info = CryptFile.OpenRead(fileName, CryptFile.LastPassword);
					sig = info.ContentType;
					goto SWITCH;
				case "Text": case ".txt": case "text/plain": {
					var txt = new TextEditorVM();
					Tabs.Add(txt); SelectedTab = txt;
					//if (info == null) info = new CryptoStreamInfo("text/plain", File.OpenRead(dlg.FileName));
					txt.OpenFile(fileName, readOnly, info, password);
					break;
				}
				case "RichText": case ".xaml": case ".rtf": case "text/rtf": case ".xps": case "application/vnd.ms-xpsdocument": case "application/rtf":
				case "application/xaml+xml": {
					var rtf = new RichTextEditorVM();
					Tabs.Add(rtf); SelectedTab = rtf;
					//if (info == null) info = new CryptoStreamInfo("text/rtf", File.OpenRead(dlg.FileName));
					rtf.OpenFile(fileName, readOnly, info, password);
					break;
				}
				case "SpreadSheet": case "Table": case ".csv": case "text/csv": {
					var tab = new TableEditorVM();
					Tabs.Add(tab); SelectedTab = tab;
					//if (info == null) info = new CryptoStreamInfo("text/csv", File.OpenRead(dlg.FileName));
					tab.OpenFile(fileName, readOnly, info, password);
					break;
				}
				default:
					if (format == null || !format.StartsWith(".")) {
						format = Path.GetExtension(fileName).ToLowerInvariant();
						goto SWITCH;
					}
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{format}'.", "Open File...", MessageBoxButton.OK, MessageBoxImage.Warning);
					throw new NotSupportedException();
			}
		}

		private void DoSaveFile() {
			
		}

		private void DoSaveFileAs() {
			
		}

		private void DoNewFile(object parameter) {
			switch ($"{parameter}") {
				case "Text": case "text/plain": {
					var tab = new TextEditorVM();
					Tabs.Add(tab);
					SelectedTab = tab;
					tab.DoNewFile();
					break;
				}
				case "Rich Text Format": case "application/rtf": {
					var tab = new RichTextEditorVM();
					Tabs.Add(tab);
					SelectedTab = tab;
					tab.DoNewFile();
					break;
				}
				case "text/csv": {
					var tab = new TableEditorVM();
					Tabs.Add(tab);
					SelectedTab = tab;
					tab.DoFileNew();
					break;
				}
			}
		}

		private void CloseTabs(FileTabItemVM[] items) {
			foreach (var item in items) {
				item.OnClose();
				Tabs.Remove(item);
			}
		}

		public void CloseAllTabs() {
			var all = Tabs.ToArray();
			CloseTabs(all);
		}

		public void CloseTabsRight(FileTabItemVM item) {
			var idx = Tabs.IndexOf(item);
			CloseTabs(Tabs.Take(idx - 1).ToArray());
		}

		public void CloseTabsLeft(FileTabItemVM item) {
			var idx = Tabs.IndexOf(item);
			CloseTabs(Tabs.Skip(idx + 1).ToArray());
		}

		public void CloseUnchanged() {
			CloseTabs(Tabs.Where(t=>t.HasChanges==false).ToArray());
		}
	}

}