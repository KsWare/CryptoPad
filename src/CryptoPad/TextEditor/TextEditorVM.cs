using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace KsWare.CryptoPad.TextEditor {

	public class TextEditorVM : FileTabItemVM {
		private string _password;

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

		public TextBoxControllerVM Editor { get; [UsedImplicitly] private set; }

		public void DoNewFile() {
			Editor.Text = "";
			FileName = FileTools.NewTempFile("NewTxt");
			IsTempFile = true;
			IsReadOnly = false;
			HasChanges = false;
			Header.Text = Path.GetFileName(FileName);
		}

		/// <inheritdoc />
		protected override void SaveTo(string fileName, string format, bool askPassword) {
			string password = null;
			SWITCH:
			switch (format) {
				case ".crypt": {
					password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, _password ?? CryptFile.LastPassword);
					CryptFile.Write(Editor.Text, fileName, CryptFile.LastPassword);
					break;
				}
				case ".txt": {
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
			_password = password;
		}
		
		public override bool SaveAs() {
			var dlg = new SaveFileDialog() {
				Title = "Save text to...",
				Filter = "Crypto-File|*.crypt|Text-File|*.txt|All Files|*.*",
				FilterIndex = 1,
				CheckPathExists = true,
				FileName = IsTempFile ? Path.GetFileName(FileName) : string.Empty
			};
			if (dlg.ShowDialog() != true) return false;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: /* .crypt */ {
					format = ".crypt";
					break;
				}
				case 2: /* .txt */ {
					format = ".txt";
					break;
				}
			}

			SaveTo(dlg.FileName, format, true);
			return true;
		}

		public void OpenFile(string fileName, bool readOnly, CryptoStreamInfo info, string password) {
			using var reader = new StreamReader(info.Stream);
			Editor.Text = reader.ReadToEnd();

			Header.Text = Path.GetFileName(fileName);
			FileName = fileName;
			IsReadOnly = readOnly;
			IsTempFile = FileTools.IsTempFile(fileName);
			_password = password;
		}

	}
}
