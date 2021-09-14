using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using KsWare.CryptoPad.RichTextEditor;
using KsWare.CryptoPad.TableEditor;
using KsWare.CryptoPad.TextEditor;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace KsWare.CryptoPad {

	public class ShellVM : WindowVM {
		
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

			Fields[nameof(SelectedTab)].ValueChangedEvent.add=AtTabChanged;
			Menu = CommonMenu;
		}

		private void DoCloseAllFiles() {
			SelectedTab = null;
			Tabs.Clear();
		}

		private void DoCloseFile() {
			var tabToClose = SelectedTab;
			//TODO if(tabToClose.HasChanges)
			var idx = Tabs.IndexOf(tabToClose);
			Tabs.RemoveAt(idx);
			if (idx == Tabs.Count) idx--;
			SelectedTab = idx >= 0 ? Tabs[idx] : null;
		}

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

		public ListVM<TabVM> Tabs { get; private set; }

		public TabVM SelectedTab { get => Fields.GetValue<TabVM>(); set => Fields.SetValue(value); }

		private void DoOpenFile() {
			var dlg = new OpenFileDialog {
				Title = "Select file to load...",
				Filter = "Crypto-Files|*.crypt"+
						 "|Text-Files (*.txt)|*.txt" +
						 "|XPS-Files|*.xps|RTF-Files|*.rtf|XAML-Files|*.xaml" +
				         "|CSV-Files|*.csv" +
				         "|All Files|*.*",
				FilterIndex = 1,
				ShowReadOnly = true,
				CheckFileExists = true,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return;

			CryptoStreamInfo info = null;
			var sig = Path.GetExtension(dlg.FileName).ToLowerInvariant();
			string password = null;
			SWITCH:
			switch (sig) {
				case ".crypt": 
					password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, CryptFile.LastPassword, dlg.FileName);
					info = CryptFile.OpenRead(dlg.FileName, CryptFile.LastPassword);
					sig = info.ContentType;
					goto SWITCH;
				case ".txt": case "text/plain": {
					var txt = new TextEditorVM();
					Tabs.Add(txt); SelectedTab = txt;
					//if (info == null) info = new CryptoStreamInfo("text/plain", File.OpenRead(dlg.FileName));
					txt.OpenFile(dlg.FileName, dlg.ReadOnlyChecked, info, password);
					break;
				}
				case ".xaml": case ".rtf": case "text/rtf": case ".xps": case "application/vnd.ms-xpsdocument": case "application/rtf":
					case "application/xaml+xml": {
					var rtf = new RichTextEditorVM();
					Tabs.Add(rtf); SelectedTab = rtf;
					//if (info == null) info = new CryptoStreamInfo("text/rtf", File.OpenRead(dlg.FileName));
					rtf.OpenFile(dlg.FileName, dlg.ReadOnlyChecked, info, password);
					break;
				}
				case ".csv": case "text/csv": {
					var tab = new TableEditorVM();
					Tabs.Add(tab); SelectedTab = tab;
					//if (info == null) info = new CryptoStreamInfo("text/csv", File.OpenRead(dlg.FileName));
					tab.OpenFile(dlg.FileName, dlg.ReadOnlyChecked, info, password);
					break;
				}
				default:
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{sig}'.", "Open file...");
					return;
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
	}

}