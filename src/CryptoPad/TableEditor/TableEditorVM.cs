using System.IO;
using System.Text;
using System.Windows;
using JetBrains.Annotations;
using KsWare.CryptoPad.Dialogs;
using KsWare.CryptoPad.TextEditor;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace KsWare.CryptoPad.TableEditor {

	public class TableEditorVM : TabVM {

		private string _fileName;
		private string _readonlyFileName;
		private string _separator;
		private string _password;

		/// <inheritdoc />
		public TableEditorVM() {
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

			var table = Menu.AddMenuItem("_Table");
			var column=table.AddMenuItem("_Column");
			column.AddMenuItem("_Add", DoAddColumn);
			column.AddMenuItem("_Delete", DoDeleteColumn);
			column.AddMenuItem("_Rename", DoRenameColumn);
		}

		public StringVM Title { get; [UsedImplicitly] private set; }

		public DataGridControllerVM Editor { get; [UsedImplicitly] private set; }		
		
		public ListVM<MenuItemVM> Menu { get; [UsedImplicitly] private set; }

		public void DoFileNew() {
			using var csvReader = new TextFieldParser(new StringReader("A,B,C,D"));
			csvReader.SetDelimiters(",");
			Editor.Table = CsvTools.GetDataTable(csvReader);
			Title.Value = "NewTable";
		}

		private void DoAddColumn() {
			Editor.Table.Columns.Add($"C{Editor.Table.Columns.Count}");
		}

		/// <summary>
		/// Method for <see cref="DeleteColumnAction"/>
		/// </summary>
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

		public void OpenFile(string fileName, bool readOnly, CryptoStreamInfo info, string password) {
			using var reader = new StreamReader(info.Stream);
			var text = reader.ReadToEnd();

			var firstLine = new StringReader(text).ReadLine();
			_separator = firstLine.Split(',').Length > firstLine.Split(';').Length ? "," : ";";
			using var csvReader = new TextFieldParser(new StringReader(text));
			csvReader.SetDelimiters(new string[] { _separator });
			Editor.Table = CsvTools.GetDataTable(csvReader);

			_fileName = readOnly ? null : fileName;
			_password = password;
			_readonlyFileName = readOnly ? fileName : null;
			Title.Value = Path.GetFileName(fileName);
		}

		private void DoSave() {
			if (string.IsNullOrEmpty(_fileName)) {
				DoSaveAs();
				return;
			}
			SaveTo(_fileName, null, askPassword:false);
		}

		private void DoSaveAs() {
			var dlg = new SaveFileDialog() {
				Title = "Save table as...",
				Filter = "Crypto-File|*.crypt"+
				         "|Comma-Separated (*.csv)|*.csv"+
						 "|Tab-Separated (*.txt)|*.csv",
				         //"|All Files (*.*)|*.*"
				FilterIndex = 1,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: format = ".crypt"; break;
				case 2: format = ".csv"; break;
				case 3: format = ".txt"; break;
				default: format = null; break;
			}
			SaveTo(dlg.FileName, format, askPassword:true);
		}

		private void SaveTo(string fileName, string format, bool askPassword) {
			string password=null;
			SWITCH:
			switch (format) {
				case ".crypt": {
					if(askPassword || _password == null)
						password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, _password ?? CryptFile.LastPassword);
					using var memStream = new MemoryStream();
					using var writer = new StreamWriter(memStream, new UTF8Encoding(false));
					CsvTools.ToCsv(Editor.Table, writer, new CsvOptions { Separator = ',' });
					memStream.Position = 0;
					CryptFile.Write(memStream, fileName, CryptFile.LastPassword, "text/csv");
					break;
				}
				case ".csv": case "text/csv": {
					CsvTools.SaveAsCsv(Editor.Table, fileName, new CsvOptions { Separator = ',' });
					break;
				}
				case ".txt": case "text/tab-separated-values": {
					CsvTools.SaveAsCsv(Editor.Table, fileName, new CsvOptions { Separator = '\t' });
					break;
				}
				default: {
					if (format == null || !format.StartsWith(".")) {
						format = Path.GetExtension(fileName).ToLowerInvariant();
						goto SWITCH;
					}
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{format}'", "Save table as...",
						MessageBoxButton.OK, MessageBoxImage.Warning);
					break;
				}
			}

			_fileName = fileName;
			_password = password;
			_readonlyFileName = null;
			Title.Value = Path.GetFileName(fileName);
		}

	}

}
