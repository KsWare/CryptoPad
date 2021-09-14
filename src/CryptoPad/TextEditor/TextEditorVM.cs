using System.IO;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

namespace KsWare.CryptoPad.TextEditor {

	public class TextEditorVM:TabVM {
		private string _fileName;
		private string _readOnlyFileName;
		private string _password;

		/// <inheritdoc />
		public TextEditorVM() {
			RegisterChildren(() => this);

			Header = Title;
			Content = this;

			var file = Menu.AddMenuItem("_File");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_New"});
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_Open.."});
			file.AddMenuItem("_Save", DoSave);
			file.AddMenuItem("Save _As..", DoSaveAs);
			file.AddMenuItem("-");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_Close"});
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "C_lose All"});
			file.AddMenuItem("-");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "E_xit"});

			var edit = Menu.AddMenuItem("_Edit");
			edit.AddMenuItem(ApplicationCommands.Undo);
			edit.AddMenuItem(ApplicationCommands.Redo);
			edit.AddMenuItem("-");
			edit.AddMenuItem(ApplicationCommands.Cut);
			edit.AddMenuItem(ApplicationCommands.Copy);
			edit.AddMenuItem(ApplicationCommands.Paste);

			var view = Menu.AddMenuItem("_View");
			view.AddMenuItem("Options..", DoViewOptions);
		}

		private void DoViewOptions() {
			
		}

		public StringVM Title { get; [UsedImplicitly] private set; }

		public TextBoxControllerVM Editor { get; [UsedImplicitly] private set; }

		public ListVM<MenuItemVM> Menu { get; [UsedImplicitly] private set; }

		public void DoNewFile() {
			Editor.Text = "";
			_fileName = null;
			Title.Value = "NewTxt";
		}

		private void DoSave() {
			if (string.IsNullOrEmpty(_fileName)) {
				DoSaveAs();
				return;
			}
			CryptFile.Write(Editor.Text, _fileName, _password);
		}
		
		private void DoSaveAs() {
			var dlg = new SaveFileDialog() {
				Title = "Save text to...",
				Filter = "Crypto-File|*.crypt|Text-File|*.txt|All Files|*.*",
				FilterIndex = 1,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return;
			string password = null;

			switch (dlg.FilterIndex) {
				case 1: /* .crypt */ {
					password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, _password ?? CryptFile.LastPassword);
					CryptFile.Write(Editor.Text, dlg.FileName, CryptFile.LastPassword);
					break;
				}
				case 2: /* .txt */ {
					FileTools.Write(Editor.Text, dlg.FileName);
					break;
				}

				default: {
					var ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
					if (ext == ".crypt") goto case 1;
					goto case 2;
				}
			}

			_fileName = dlg.FileName;
			_password = password;
			Title.Value = Path.GetFileName(dlg.FileName);
		}

		public void OpenFile(string fileName, bool readOnly, CryptoStreamInfo info, string password) {
			using var reader = new StreamReader(info.Stream);
			Editor.Text = reader.ReadToEnd();
			Title.Value = Path.GetFileName(fileName);
			_readOnlyFileName = readOnly ? fileName : null;
			_fileName = readOnly ? null : fileName;
			_password = password;
		}

	}
}
