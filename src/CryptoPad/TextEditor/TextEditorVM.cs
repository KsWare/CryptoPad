using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace KsWare.CryptoPad.TextEditor {

	public class TextEditorVM : FileTabItemVM {

		/// <inheritdoc />
		public TextEditorVM() {
			RegisterChildren(() => this);

			var edit = Menu.AddMenuItem("_Edit");
			edit.AddMenuItem(ApplicationCommands.Undo);
			edit.AddMenuItem(ApplicationCommands.Redo);
			edit.AddMenuItem("-");
			edit.AddMenuItem(ApplicationCommands.Cut);
			edit.AddMenuItem(ApplicationCommands.Copy);
			edit.AddMenuItem(ApplicationCommands.Paste);

			var view = Menu.AddMenuItem("_View");
			view.AddMenuItem("Options..", DoViewOptions);

			Editor.ContentChanged += (s, e) => HasChanges = true;
		}

		private void DoViewOptions() {
			
		}

		public new TextBoxControllerVM Editor {
			get => (TextBoxControllerVM)base.Editor;
			private set => base.Editor = value;
		}

		public override void NewFile(SecureString password) {
			base.NewFile(password);
			Editor.Text = "";
			FileName = FileTools.NewTempFile("NewTxt");
			Header.Text = Path.GetFileName(FileName);
			SaveTo(FileName, "text/plain", password);
		}

		/// <inheritdoc />
		public override void CommitEdit() {
			Editor.RefreshText();
		}

		/// <inheritdoc />
		protected override void SaveTo(string fileName, string format, SecureString password) {
			Editor.RefreshText(); //workaround for Text not updated on Window.Closing
			if (FileTools.IsCryptFile(fileName)) format = ".crypt";
			string contentType;
			SWITCH:
			switch (format) {
				case ".crypt": {
					contentType = "text/plain";
					CryptFile.Write(Editor.Text, fileName, password);
					break;
				}
				case ".txt": case "text/plain":{
					contentType = "text/plain";
					FileTools.Write(Editor.Text, fileName);
					break;
				}
				default: {
					if (format == null || !format.StartsWith(".")) {
						format = Path.GetExtension(fileName).ToLowerInvariant();
						goto SWITCH;
					}
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{format}'.", "Save as...", MessageBoxButton.OK, MessageBoxImage.Warning);
					throw new NotSupportedException();
				}
			}

			FileName = fileName;
			HasChanges = false;
			IsReadOnly = false;
			IsTempFile = FileTools.IsTempFile(fileName);
			Header.Text = Path.GetFileName(fileName);
			ContentType = contentType;
		}
		
		public override bool SaveAs() {
			var dlg = new SaveFileDialog {
				Title = "Save text to...",
				Filter = "CryptoPad File (*.crypt)|*.crypt|Text-File|*.txt|All Files|*.*",
				FilterIndex = 1,
				FileName = IsTempFile ? Path.GetFileName(FileName) : string.Empty,
				CheckPathExists = true,
				OverwritePrompt = true
			};
			if (dlg.ShowDialog() != true) return false;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: format = ".crypt"; break;
				case 2: format = ".txt"; break;
				default: format = GetFormat(dlg.FileName); break;
			}

			if(format==".crypt")
				PasswordOverlay.Password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, PasswordOverlay.Password ?? CryptFile.LastPassword);

			CommitEdit();
			SaveTo(dlg.FileName, format, PasswordOverlay.Password);
			return true;
		}

		private string GetFormat(string sig) {
			SWITCH:
			switch (sig) {
				case ".txt": return ".txt";
				case ".crypt": return ".crypt";
				default:
					if (!sig.StartsWith(".")) {
						sig = Path.GetExtension(sig).ToLowerInvariant();
						goto SWITCH;
					}
					return null;
			}
		}

		/// <inheritdoc/>
		public override void OpenFile(string fileName, bool readOnly = false, CryptoStreamInfo info = null, SecureString password = null) {
			base.OpenFile(fileName, readOnly, info, password);
			if(PasswordOverlay.IsOpen) return;

			Stream stream;
			if (info?.Stream != null) stream = info.Stream;
			else if (FileTools.IsCryptFile(fileName)) stream = CryptFile.OpenRead(fileName, password).Stream;
			else stream = File.OpenRead(fileName);

			using var reader = new StreamReader(stream);
			Editor.Text = reader.ReadToEnd();
			reader.Close();
			IsLoaded = true;
		}

	}
}
