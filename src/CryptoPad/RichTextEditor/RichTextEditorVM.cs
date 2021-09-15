using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using JetBrains.Annotations;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

namespace KsWare.CryptoPad.RichTextEditor {

	public class RichTextEditorVM : FileTabItemVM {

		private string _fileType;
		private string _contentType;
		private string _password;

		/// <inheritdoc />
		public RichTextEditorVM() {
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

		public RichTextControllerVM Editor { get; private set; }

		private void DoViewOptions() {
			
		}

		public override bool SaveAs() {
			var dlg = new SaveFileDialog() {
				Title = "Save text to...",
				Filter = "Crypto-File|*.crypt"+
				         "|XAMl-Package|*.xps"+
				         "|Rich Text File|*.rtf;*.wri"+
				         "|XAML-File|*.xaml"+
				         "|Text-File|*.txt",
				FilterIndex = 1,
				CheckPathExists = true
			};
			if (dlg.ShowDialog() != true) return false;

			string format = null;
			switch (dlg.FilterIndex) {
				case 1: format = ".crypt"; break;
				case 2: format = ".xps"; break;
				case 3: format = ".rtf"; break;
				case 4: format = ".xaml"; break;
				case 5: format = ".txt"; break;
			}

			SaveTo(dlg.FileName, format, askPassword:true);
			return true;
		}

		private static string GetDataFormat(string s) {
			SWITCH:
			switch (s) {
				case ".txt": case /*DataFormats*/"Text": case "text/plain":
					return DataFormats.Text;
				case ".xps": case /*DataFormats*/"XamlPackage": case "application/vnd.ms-xpsdocument":
					return DataFormats.XamlPackage;
				case ".xaml": case /*DataFormats*/"Xaml": case "application/xaml+xml":
					return DataFormats.Xaml;
				case ".rtf": case ".wri": case /*DataFormats*/"Rich Text Format": case "application/rtf":
					return DataFormats.Rtf;
				default:
					if (!s.StartsWith(".") && s.Contains(".")) {
						//possibly file name
						s = Path.GetExtension(s).ToLowerInvariant();
						goto SWITCH;
					}
					return null;
			}
		}

		protected override void SaveTo(string fileName, string format, bool askPassword) {
			string contentType = null;
			string password = null;
			SWITCH:
			switch (format) {
				case ".crypt":
					contentType = "application/vnd.ms-xpsdocument";
					if(askPassword || _password==null)
						password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, _password ?? CryptFile.LastPassword);
					var stream = CryptFile.Create(fileName, password, contentType);
					SaveToStream(Editor.Document, stream, DataFormats.XamlPackage);
					// application/xaml+xml		DataFormats.Xaml
					// application/rtf			DataFormats.Rtf
					break;
				case ".txt": case /*DataFormats*/"Text": case "text/plain":
					SaveToFile(Editor.Document, fileName, DataFormats.Text);
					break;
				case ".xps": case /*DataFormats*/"XamlPackage":case "application/vnd.ms-xpsdocument":
					SaveToFile(Editor.Document, fileName, DataFormats.XamlPackage);
					break;
				case ".xaml": case /*DataFormats*/"Xaml": case "application/xaml+xml":
					SaveToFile(Editor.Document, fileName, DataFormats.Xaml);
					break;
				case ".rtf": case ".wri": case /*DataFormats*/"Rich Text Format": case "application/rtf":
					SaveToFile(Editor.Document, fileName, DataFormats.Rtf);
					break;
				default:
					if (format == null || !format.StartsWith(".")) {
						format = Path.GetExtension(fileName).ToLowerInvariant();
						goto SWITCH;
					}
					MessageBox.Show(Application.Current.MainWindow, $"Unsupported file format '{format}'.", "Save as...", MessageBoxButton.OK, MessageBoxImage.Warning);
					throw new NotSupportedException();
			}

			FileName = fileName;
			_password = password;
			Header.Text = Path.GetFileName(fileName);
			_fileType = Path.GetExtension(fileName);
			_contentType = contentType;
		}

		private static void SaveToFile(FlowDocument document, string fileName, string dataFormat) {
			// XamlPackage needs Read/Write
			var stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			SaveToStream(document, stream, dataFormat);
		}

		private static void SaveToStream(FlowDocument document, Stream stream, string dataFormat) {
			var selection = new TextRange(document.ContentStart, document.ContentEnd);
			if(dataFormat == DataFormats.XamlPackage && (!stream.CanRead || !stream.CanSeek)) {
				// workaround for: ArgumentException: Update mode requires a stream with read, write, and seek capabilities.
				// CryptoStream can not read/seek
				using var tempStream = new MemoryStream();
				selection.Save(tempStream, dataFormat);
				tempStream.Position = 0;
				Tools.Copy(tempStream,stream);
			}
			else {
				selection.Save(stream, dataFormat);
			}
			stream.Close();
		}

		public void DoNewFile() {
			Editor.Document = new FlowDocument();
			FileName = FileTools.NewTempFile("NewRtf");
			Header.Text = Path.GetFileName(FileName);
			IsTempFile = true;
			HasChanges = false;
			IsReadOnly = false;
		}

		// private void RichTextBoxOnSelectionChanged(object sender, RoutedEventArgs e) {
		// 	var RTBText = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd);
		// 	RTBText.Save(fs, DataFormats.Rtf);
		// }

		public void OpenFile(string fileName, bool readOnly, CryptoStreamInfo info, string password) {

			// Derzeit werden die Datenformate Rtf, Text, Xaml und XamlPackage
			var doc = new FlowDocument();
			var selection = new TextRange(doc.ContentStart, doc.ContentEnd);

			var stream = info?.Stream ?? File.OpenRead(fileName);
			var dataFormat = GetDataFormat(info?.ContentType ?? Path.GetExtension(fileName));

			selection.Load(stream, dataFormat);
			stream.Close();

			Editor.Document = doc;
			FileName = fileName;
			IsReadOnly = readOnly;
			IsTempFile = FileTools.IsTempFile(fileName);
			Header.Text = Path.GetFileName(fileName);
			_fileType = Path.GetExtension(fileName);
			_contentType = info?.ContentType;
			_password = password;
		}
	}

}
