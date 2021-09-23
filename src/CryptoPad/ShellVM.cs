using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using KsWare.CryptoPad.RichTextEditor;
using KsWare.CryptoPad.TableEditor;
using KsWare.CryptoPad.TextEditor;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

namespace KsWare.CryptoPad {

	public partial class ShellVM : WindowVM {

		private SessionData _sessionData;
		private readonly Communicator _communicator;

		/// <inheritdoc />
		public ShellVM() {
			RegisterChildren(() => this);
			_communicator = new Communicator(this);

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
			// file.AddMenuItem("_Reload", DoReloadFile);
			// file.AddMenuItem("_Save", DoSave);
			// file.AddMenuItem("Save _As..", DoSaveAs);
			// file.AddMenuItem("Save A_ll", DoSaveAll);
			file.AddMenuItem("-");
			file.AddMenuItem("_Close", DoCloseFile);
			file.AddMenuItem("C_lose All", DoCloseAll);
			file.AddMenuItem("-");
			file.AddMenuItem("E_xit", DoExit);
			// file.AddMenuItem("_Save", DoSaveFile);
			// file.AddMenuItem("Save _As..", DoSaveFileAs);

			Fields[nameof(SelectedTab)].ValueChangedEvent.add = AtTabChanged;
			Menu = CommonMenu;

			// UIAccess.WindowChanged += (s, e) => {
			// 	if (e.PreviousValue != null) e.PreviousValue.Closing -= WindowClosing;
			// 	if (e.NewValue != null) e.NewValue.Closing += WindowClosing;
			// };
			// if (UIAccess.HasWindow) UIAccess.Window.Closing += WindowClosing;
			base.ClosingEvent.add = WindowClosing;

			var cmdline = ((AppVM)AppVM.Current).CommandLine;
			cmdline.RestoreTabs = _communicator.IsMaster;
			HandleCommandLine(cmdline);
		}

		private void DoSaveAll() { SaveAll(); }

		private void RestoreTabs() {
			var list = new List<string>();
			foreach (var file in _sessionData.Files) {
				if(File.Exists(file.Path))
					OpenFile(file.Path, file.Format, file.IsReadOnly, file.Password);
				else {
					Debug.WriteLine("RestoreTabs: File not found. "+file.Path);
					list.Add($"Not found: {file.Path}");
				}
			}

			if (list.Count > 0) {
				MessageBox.Show("Some tabs could not be restored\n\n" + string.Join("\n", list));
			}
		}

		private void WindowClosing(object sender, CancelEventArgs e) {
			SaveAll();

			if(MessageBox.Show("Close CryptoPad?","Acknowledge",MessageBoxButton.OKCancel)!=MessageBoxResult.OK) return;

			_sessionData.Files = Tabs.Select(t => t.GetFileInfo()).ToArray();
			FileTools.SaveSessionData(_sessionData);
			_communicator.Close();
		}

		public void SaveAll() {
			Tabs.Where(t => t.PasswordOverlay.IsOpen == false).ForEach(t=>t.Save());
		}

		private void DoCloseAll() {
			Tabs.ForEach(t=>t.OnClose());
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
			(e.PreviousValue as ObjectVM)?.NotifyDeactivated();

			if (e.NewValue is FileTabItemVM tab) {
				Menu = tab.Menu;
			}
			else {
				Menu = CommonMenu;
			}
			ReplacePlaceholders(Menu, CommonMenu);
			UpdateTitle();

			(e.NewValue as ObjectVM)?.NotifyActivated(this);
		}

		private void UpdateTitle() {
			string titleFormat = "Crypto Pad [%Session%] - %FileName%";
			var title = titleFormat
				.Replace("%Session%", _sessionData?.Name)
				.Replace("%FileName%", SelectedTab?.FileName != null ? Path.GetFileName(SelectedTab?.FileName) : "");
			Title.Value = title;
		}

		private void ReplacePlaceholders(IList<MenuItemVM> items, IList<MenuItemVM> templates) {
			for (int i = 0; i < items.Count; i++) {
				if (items[i] is MenuItemPlaceholderVM p) {
					var t=templates.FirstOrDefault(mi => mi.Caption == p.Caption);
					if (t != null) items[i] = t;
				}
				else if(items[i].Items.Count>0) {
					var t = templates.FirstOrDefault(mi => MenuItemMatch(mi, items[i]));
					if (t != null) ReplacePlaceholders(items[i].Items, t.Items);
				}
			}
		}

		public bool MenuItemMatch(MenuItemVM a, MenuItemVM b) {
			var aCaption = a.Caption.Replace("_", "").Replace(".", "");
			var bCaption = b.Caption.Replace("_", "").Replace(".", "");
			return aCaption == bCaption;
		}

		public ListVM<MenuItemVM> CommonMenu { get; [UsedImplicitly] private set; }

		public IList<MenuItemVM> Menu { get => Fields.GetValue<IList<MenuItemVM>>(); set => Fields.SetValue(value); }

		public ListVM<FileTabItemVM> Tabs { get; [UsedImplicitly] private set; }

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
				default: format = GetFormat(dlg.FileName); break;
			}

			SecureString password = null;
			if (format == "Crypt") {
				password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, CryptFile.LastPassword, dlg.FileName);
			}

			OpenFile(dlg.FileName, format, dlg.ReadOnlyChecked, password);
		}

		private string GetFormat(string sig) {
			SWITCH:
			switch (sig) {
				case ".txt": return "Text";
				case ".xps": case ".rtf": case ".wri": case ".xaml": return "RichText";
				case ".csv": return "SpreadSheet";
				default:
					if (!sig.StartsWith(".")) {
						sig = Path.GetExtension(sig).ToLowerInvariant();
						goto SWITCH;
					}
					return null;
			}
		}

		public void OpenFile(string fileName, string format = null, bool readOnly = false, SecureString password = null) {
			var sig = FileTools.IsCryptFile(fileName) ? ".crypt" : format;
			CryptoStreamInfo info = null;
			FileTabItemVM tab;
			SWITCH:
			switch (sig) {
				case "Crypt": case ".crypt":
					if (password.IsNullOrEmpty()) {
						var header = CryptFile.ReadInfo(fileName);
						info = new CryptoStreamInfo(header, null);
						sig = header.ContentType;
					}
					else {
						info = CryptFile.OpenRead(fileName, password);
						sig = info.ContentType;
					}
					goto SWITCH;
				case "Text": case ".txt": case "text/plain": {
					tab = new TextEditorVM();
					break;
				}
				case "RichText": case ".xaml": case ".rtf": case "text/rtf": case ".xps": case "application/vnd.ms-xpsdocument": case "application/rtf":
				case "application/xaml+xml": {
					tab = new RichTextEditorVM();
					break;
				}
				case "SpreadSheet": case "Table": case ".csv": case "text/csv": case "text/tab-separated-values": {
					tab = new TableEditorVM();
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

			Tabs.Add(tab); SelectedTab = tab;
			tab.LoadingOverlay.IsOpen = true;
			ApplicationDispatcher.BeginInvoke(DispatcherPriority.Normal, () => {
				tab.OpenFile(fileName, readOnly, info, password);
				tab.LoadingOverlay.IsOpen = false;
			});
		}

		private void DoSaveFile() {
			
		}

		private void DoSaveFileAs() {
			
		}

		private void DoNewFile(object parameter) {
			FileTabItemVM tab = null;
			switch ($"{parameter}") {
				case "Text": case "text/plain": 
					tab = new TextEditorVM(); break;
				case "Rich Text Format": case "application/rtf": case "RichText": 
					tab = new RichTextEditorVM(); break;
				case "text/csv": case "Table": case "SpreadSheet":
					tab = new TableEditorVM(); break;
				default: throw new NotSupportedException(); // TODO handle NotSupportedException
			}
			Tabs.Add(tab);
			SelectedTab = tab;

			var password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, CryptFile.LastPassword, "New File");
			tab.NewFile(password);
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

		public void HandleCommandLine(CommandLineData data) {
			if (_sessionData == null) {
				if (data.SessionName != null) {
					// use specified session
				}
				else if (data.FileName != null) {
					data.SessionName = DateTime.Now.ToString("yyyyMMddHHmmss");
				}
				else {
					data.SessionName = "Default";
				}
				_sessionData = FileTools.LoadSessionData(data.SessionName, createIfNotExist: true);
				UpdateTitle();
			}

			if (data.RestoreTabs) {
				Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, RestoreTabs);
			}

			if (data.FileName != null) {
				Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => OpenFile(data.FileName, data.Format, data.ReadOnly));
			}
		}
	}

}