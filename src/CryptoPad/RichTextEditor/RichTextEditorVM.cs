using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using JetBrains.Annotations;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

namespace KsWare.CryptoPad.RichTextEditor {

	public class RichTextEditorVM : FileTabItemVM {

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

			if(format==".crypt")
				PasswordPanel.Password = CryptFile.LastPassword = PasswordDialog.GetPassword(Application.Current.MainWindow, PasswordPanel.Password ?? CryptFile.LastPassword);

			SaveTo(dlg.FileName, format, PasswordPanel.Password);
			return true;
		}

		/// <summary>
		/// Gets the data format for <see cref="TextRange.Load">TextRange.Load</see>
		/// </summary>
		/// <param name="identifier">A file name, a file extension or a MIME identifier</param>
		/// <returns>A value of <see cref="DataFormats"/> or <c>null</c></returns>
		/// <seealso cref="DataFormats"/>
		private static string GetDataFormat(string identifier) {
			SWITCH:
			switch (identifier) {
				case null: case "": return null;
				case ".txt": case /*DataFormats*/"Text": case "text/plain":
					return DataFormats.Text;
				case ".xps": case /*DataFormats*/"XamlPackage": case "application/vnd.ms-xpsdocument":
					return DataFormats.XamlPackage;
				case ".xaml": case /*DataFormats*/"Xaml": case "application/xaml+xml":
					return DataFormats.Xaml;
				case ".rtf": case ".wri": case /*DataFormats*/"Rich Text Format": case "application/rtf":
					return DataFormats.Rtf;
				default:
					if (!identifier.StartsWith(".") && identifier.Contains(".")) {
						//possibly file name
						identifier = Path.GetExtension(identifier).ToLowerInvariant();
						goto SWITCH;
					}
					return null;
			}
		}

		protected override void SaveTo(string fileName, string format, SecureString password) {
			if (FileTools.IsCryptFile(fileName)) format = ".crypt";
			string contentType;
			SWITCH:
			switch (format) {
				case ".crypt":
					// application/xaml+xml		DataFormats.Xaml
					// application/rtf			DataFormats.Rtf
					contentType = "application/vnd.ms-xpsdocument";
					var stream = CryptFile.Create(fileName, password, contentType);
					SaveToStream(Editor.Document, stream, DataFormats.XamlPackage);
					break;
				case ".txt": case /*DataFormats*/"Text": case "text/plain":
					SaveToFile(Editor.Document, fileName, DataFormats.Text);
					contentType = "text/plain";
					break;
				case ".xps": case /*DataFormats*/"XamlPackage":case "application/vnd.ms-xpsdocument":
					SaveToFile(Editor.Document, fileName, DataFormats.XamlPackage);
					contentType = "application/vnd.ms-xpsdocument";
					break;
				case ".xaml": case /*DataFormats*/"Xaml": case "application/xaml+xml":
					SaveToFile(Editor.Document, fileName, DataFormats.Xaml);
					contentType = "application/application/xaml+xml";
					break;
				case ".rtf": case ".wri": case /*DataFormats*/"Rich Text Format": case "application/rtf":
					SaveToFile(Editor.Document, fileName, DataFormats.Rtf);
					contentType = "application/rtf";
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
			Header.Text = Path.GetFileName(fileName);
			ContentType = contentType;
			HasChanges = false;
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
//			(stream as CryptoStream)?.FlushFinalBlock(); // not documented, but we have to call FlushFinalBlock explicitly.
			stream.Close();
		}

		public override void NewFile(SecureString password) {
			base.NewFile(password);
			Editor.Document = new FlowDocument();
			FileName = FileTools.NewTempFile("NewRtf");
			Header.Text = Path.GetFileName(FileName);
			SaveTo(FileName, "application/vnd.ms-xpsdocument", password);
		}

		/// <inheritdoc/>
		public override void OpenFile(string fileName, bool readOnly = false, CryptoStreamInfo info = null, SecureString password = null) {
			base.OpenFile(fileName, readOnly, info, password);
			if(PasswordPanel.IsOpen) return;

			// Derzeit werden die Datenformate Rtf, Text, Xaml und XamlPackage
			var doc = new FlowDocument();
			var selection = new TextRange(doc.ContentStart, doc.ContentEnd);

			Stream stream;
			if (info?.Stream != null) stream = info.Stream;
			else if (FileTools.IsCryptFile(fileName)) stream = CryptFile.OpenRead(fileName, password).Stream;
			else stream = File.OpenRead(fileName);

			var dataFormat = GetDataFormat(info?.ContentType ?? fileName);

			try {
				selection.Load(stream, dataFormat);
			}
			catch (CryptographicException ex) {
				// https://stackoverflow.com/questions/8583112/padding-is-invalid-and-cannot-be-removed
				PasswordPanel.IsOpen = true;
				throw; //TODO
			}
			catch (Exception ex) {
				throw;
			}

			stream.Close();

			Editor.Document = doc;
			IsLoaded = true;
		}
	}

}
