using System;
using System.IO;
using System.Security;
using System.Text;
using System.Windows;
using JetBrains.Annotations;
using KsWare.CryptoPad.Dialogs;
using KsWare.CryptoPad.RichTextEditor;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace KsWare.CryptoPad.TableEditor {

	public class TableEditorVM : FileTabItemVM {

		private string _separator;

		/// <inheritdoc />
		public TableEditorVM() {
			RegisterChildren(() => this);

			var table = Menu.AddMenuItem("_Table");
			var column=table.AddMenuItem("_Column");
			column.AddMenuItem("_Add", DoAddColumn);
			column.AddMenuItem("_Delete", DoDeleteColumn);
			column.AddMenuItem("_Rename", DoRenameColumn);

			Editor.ContentChanged += (s, e) => HasChanges = true;
		}

		public new DataGridControllerVM Editor {
			get => (DataGridControllerVM)base.Editor;
			private set => base.Editor = value;
		}

		/// <param name="password"></param>
		/// <inheritdoc />
		public override void NewFile(SecureString password) {
			base.NewFile(password);
			using var csvReader = new TextFieldParser(new StringReader("A,B,C,D"));
			csvReader.SetDelimiters(",");
			Editor.Table = CsvTools.GetDataTable(csvReader);
			FileName = FileTools.NewTempFile("NewTable");
			Header.Text = Path.GetFileName(FileName);
			SaveTo(FileName, "text/csv", password);
		}

		/// <inheritdoc />
		public override void CommitEdit() {
			Editor.CommitEdit();
		}

		private void DoAddColumn() {
			Editor.Table.Columns.Add($"C{Editor.Table.Columns.Count}");
		}


		[UsedImplicitly]
		private void DoDeleteColumn() {
			
		}

		private void DoRenameColumn() {
			var colIndex = Editor.DataGrid.Columns.IndexOf(Editor.DataGrid.CurrentCell.Column);
			var name = Editor.Table.Columns[colIndex].ColumnName;
			var newName = TextInputDialogVM.ShowDialog(ApplicationVM.Current.MainWindow, "New name:", "Rename column", name);
			if (name == newName) return;
			Editor.Table.Columns[colIndex].ColumnName = newName;
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
			var text = reader.ReadToEnd();

			var firstLine = new StringReader(text).ReadLine();
			_separator = firstLine.Split(',').Length > firstLine.Split(';').Length ? "," : ";";
			using var csvReader = new TextFieldParser(new StringReader(text));
			csvReader.SetDelimiters(new string[] { _separator });
			Editor.Table = CsvTools.GetDataTable(csvReader);
			IsLoaded = true;
		}

		public override bool SaveAs() {
			var dlg = new SaveFileDialog() {
				Title = "Save table as...",
				Filter = "CryptoPad File|*.crypt"+
				         "|Comma-Separated (*.csv)|*.csv"+
						 "|Tab-Separated (*.txt)|*.csv",
				         //"|All Files (*.*)|*.*"
				FilterIndex = 1,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return false;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: format = ".crypt"; break;
				case 2: format = ".csv"; break;
				case 3: format = ".txt"; break;
				default: format = null; break;
			}

			if(format==".crypt")
				PasswordOverlay.Password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, PasswordOverlay.Password ?? CryptFile.LastPassword);

			CommitEdit();
			SaveTo(dlg.FileName, format, PasswordOverlay.Password);
			return true;
		}

		protected override void SaveTo(string fileName, string format, SecureString password) {
			if (FileTools.IsCryptFile(fileName)) format = ".crypt";
			string contentType;
			SWITCH:
			switch (format) {
				case ".crypt": {
					contentType = "text/csv";
					using var memStream = new MemoryStream();
					using var writer = new StreamWriter(memStream, new UTF8Encoding(false));
					CsvTools.ToCsv(Editor.Table, writer, new CsvOptions { Separator = ',' });
					memStream.Position = 0;
					CryptFile.Write(memStream, fileName, password, "text/csv");
					break;
				}
				case ".csv": case "text/csv": {
					contentType = "text/csv";
					CsvTools.SaveAsCsv(Editor.Table, fileName, new CsvOptions { Separator = ',' });
					break;
				}
				case ".txt": case "text/tab-separated-values": {
					contentType = "text/tab-separated-values";
					CsvTools.SaveAsCsv(Editor.Table, fileName, new CsvOptions { Separator = '\t' });
					break;
				}
				default: {
					if (format == null || !format.StartsWith(".")) {
						format = Path.GetExtension(fileName).ToLowerInvariant();
						goto SWITCH;
					}
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{format}'", "Save as...", MessageBoxButton.OK, MessageBoxImage.Warning);
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

	}

}
