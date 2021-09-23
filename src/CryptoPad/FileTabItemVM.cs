using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using KsWare.CryptoPad.Dialogs;
using KsWare.CryptoPad.Overlays;
using KsWare.CryptoPad.RichTextEditor;
using KsWare.CryptoPad.TableEditor;
using KsWare.CryptoPad.TextEditor;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;
using BindingMode = System.Windows.Data.BindingMode;

namespace KsWare.CryptoPad {

	public abstract class FileTabItemVM : TabItemVM {

		/// <inheritdoc />
		public FileTabItemVM() {
			RegisterChildren(()=>this);
			InitFileMenu();
			Content = this; // init TabItemVM.Content

			MenuItemVM writeProtection;
			var ctx = Header.ContextMenu = new ContextMenuVM{Parent = Header};
			ctx.AddMenuItem("Change Password", DoChangePassword);
			ctx.AddMenuItem("Close", DoClose);
			ctx.AddMenuItem("Close _All", DoCloseAllTabs);
			ctx.AddMenuItem("Close Tabs Left", DoCloseTabsLeft);
			ctx.AddMenuItem("Close Tabs Right", DoCloseTabsRight);
			ctx.AddMenuItem("Close Unchanged Tabs", DoCloseUnchangedTabs);
			ctx.AddMenuItem("Save", DoSave);
			ctx.AddMenuItem("Save As ...", DoSaveAs);
			ctx.AddMenuItem("Rename ...", DoRename);
			ctx.AddMenuItem("Delete", DoDelete);
			ctx.AddMenuItem("Reload", DoReload);
			ctx.AddMenuItem("Print ...", DoPrint);
			ctx.AddMenuItem("-");
			ctx.AddMenuItem("Open Folder in Explorer", DoOpenFolderInExplorer);
			ctx.AddMenuItem("Open Commandline for Folder", DoOpenCommandLineForFolder);
			// Enthaltenen Ordner im Arbeitsbereich öffnen
			ctx.AddMenuItem("-");
			// Im Standard-Betrachter öffnen
			ctx.Items.Add(writeProtection=new MenuItemVM{Caption = "Write protection", IsCheckable = true});
			ctx.AddMenuItem("-");
			ctx.AddMenuItem("Copy Full Path", DoCopyFullPath);
			ctx.AddMenuItem("Copy Filename", DoCopyFileName);
			ctx.AddMenuItem("Copy Folder path", DoCopyFolderPath);
			// ctx.AddMenuItem("-");
			// In zweite Ansicht verschieben
			// In zweite Ansicht duplizieren
			// In neue Instanz verschieben
			// In neue Instanz duplizieren

			PasswordOverlay.OKCommand = PasswordChangedAction;

			FieldBindingOperations.SetBinding(writeProtection.Fields[nameof(MenuItemVM.IsChecked)],new FieldBinding(Fields[nameof(IsReadOnly)], BindingMode.TwoWay));
		}

		public IEditorController Editor { get; protected set; }

		private void DoChangePassword() {
			PasswordOverlay.IsOpen = true;
		}

		/// <summary>
		/// Gets the <see cref="ActionVM"/> to PasswordChanged
		/// </summary>
		/// <seealso cref="DoPasswordChanged"/>
		public ActionVM PasswordChangedAction { get; [UsedImplicitly] private set; }

		/// <summary>
		/// Method for <see cref="PasswordChangedAction"/>
		/// </summary>
		[UsedImplicitly]
		protected virtual void DoPasswordChanged() {
			if (PasswordOverlay.Password.Length == 0) return; //leave open
			PasswordOverlay.IsOpen = false;
			if (!IsLoaded && FileTools.IsCryptFile(FileName)) {
				LoadingOverlay.IsOpen = true;
				ApplicationDispatcher.BeginInvoke(DispatcherPriority.Normal, () => {
					try {
						var info = CryptFile.OpenRead(FileName, PasswordOverlay.Password);
						OpenFile(FileName, IsReadOnly, info, PasswordOverlay.Password);
					}
					catch (Exception ex) {
						PasswordOverlay.IsOpen = true;
						ErrorOverlay.Message = ex.Message;
						ErrorOverlay.ExceptionMessage = ex.ToString();
						ErrorOverlay.IsOpen = true;
					}
					LoadingOverlay.IsOpen = false;
				});
			}
		}

		public LoadingOverlayVM LoadingOverlay { get; [UsedImplicitly] private set; }
		public PasswordOverlayVM PasswordOverlay { get; [UsedImplicitly] private set; }
		public ErrorOverlayVM ErrorOverlay { get; [UsedImplicitly] private set; }

		protected virtual void InitFileMenu() {
			var file = Menu.AddMenuItem("_File");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_New"});
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_Open.."});
			file.AddMenuItem("_Reload", DoReload);
			file.AddMenuItem("_Save", DoSave);
			file.AddMenuItem("Save _As..", DoSaveAs);
			// Save copy
			file.AddMenuItem("Save A_ll", DoSaveAll);
			// Rename
			file.AddMenuItem("-");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "_Close"});
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "C_lose All"});
			file.AddMenuItem("-");
			file.AddMenuItem(new MenuItemPlaceholderVM{Caption = "E_xit"});
		}

		private void DoReload() {
			if (ErrorOverlay.IsOpen) {
				ErrorOverlay.IsOpen = false;
				PasswordOverlay.IsOpen = FileTools.IsCryptFile(FileName);
			}
			if(PasswordOverlay.IsOpen) return;
			OpenFile(FileName, IsReadOnly, CryptFile.OpenRead(FileName, PasswordOverlay.Password), PasswordOverlay.Password);
		}

		private void DoSaveAll() {
			Shell.SaveAll();
		}

		protected ShellVM Shell => (ShellVM)Parent.Parent;

		public string FileName { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public string ContentType { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public bool HasChanges { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
		public new bool IsReadOnly { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); } // hides ObjectVM.IsReadOnly
		public bool IsTempFile { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
		public bool IsLoaded { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }

		public ListVM<MenuItemVM> Menu { get; [UsedImplicitly] private set; }

		public FileInfoData GetFileInfo() {
			return new FileInfoData {
				Path = FileName,
				Format = GetFormat(this),
				IsReadOnly = IsReadOnly,
				// Password = GetEncryptedPassword() //TODO save encrypted passwords
			};
		}

		private string GetFormat(object format) {
			switch (format) {
				case TextEditorVM : return "Text";
				case RichTextEditorVM: return "RichText";
				case TableEditorVM: return "Table";
				default: return null;
			}
		}

		public virtual void DoClose() => Shell.CloseTab(this);
		public virtual void DoCloseAllTabs() => Shell.CloseAllTabs();
		public virtual void DoCloseTabsRight() => Shell.CloseTabsRight(this);
		public virtual void DoCloseTabsLeft() => Shell.CloseTabsLeft(this);
		public virtual void DoCloseUnchangedTabs() => Shell.CloseUnchanged();

		public void DoSave() { Save(); }

		protected abstract void SaveTo(string fileName, string format, SecureString password);

		public void DoSaveAs() { SaveAs();}

		public abstract bool SaveAs();

		public virtual void DoRename() {
			var fileName = FileName;
			if(!SaveAs()) return;
			if (FileName != fileName) File.Delete(fileName);
		} 

		public virtual void DoDelete() {
			var fileName = FileName;
			Shell.CloseTab(this, noSave:true);
			File.Delete(fileName);
		}

		public virtual void DoPrint() {} //TODO

		private void DoOpenFolderInExplorer() {
			Process.Start(new ProcessStartInfo("explorer.exe",Path.GetDirectoryName(FileName)){UseShellExecute = true});
		}
		private void DoOpenCommandLineForFolder() {
			Process.Start(new ProcessStartInfo("cmd.exe"){WorkingDirectory = Path.GetDirectoryName(FileName), UseShellExecute = true});
		}
		private void DoCopyFullPath() {
			Clipboard.SetText(FileName);
		}
		private void DoCopyFileName() {
			Clipboard.SetText(Path.GetFileName(FileName));
		}
		private void DoCopyFolderPath() {
			Clipboard.SetText(Path.GetDirectoryName(FileName));
		}

		public virtual void OnClose(bool noSave = false) {
			if(noSave==false) Save();
		}

		/// <summary>
		/// Opens the specified file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="readOnly">if set to <c>true</c> the tab is read-only. [optional]</param>
		/// <param name="info">The <see cref="CryptoStreamInfo"/> [optional]</param>
		/// <param name="password">The password [optional]</param>
		public virtual void OpenFile(string fileName, bool readOnly = false, CryptoStreamInfo info = null, SecureString password = null) {
			if (FileTools.IsCryptFile(fileName) && info?.Stream == null) {
				PasswordOverlay.Password = password;
				PasswordOverlay.IsOpen = password.IsNullOrEmpty();
				IsLoaded = false;
			}
			else {
				IsLoaded = false;
				// must be loaded in override method
			}

			FileName = fileName;
			IsReadOnly = readOnly;
			IsTempFile = FileTools.IsTempFile(fileName);
			Header.Text = Path.GetFileName(fileName);
			// _fileType = Path.GetExtension(fileName);
			ContentType = info?.ContentType;
			PasswordOverlay.Password = password;
			
			if(IsSelected) FocusFirst();
		}

		public virtual void NewFile(SecureString password) {
			IsTempFile = true;
			HasChanges = false;
			IsReadOnly = false;
			PasswordOverlay.Password = password;
			PasswordOverlay.IsOpen = password.IsNullOrEmpty();

			if(IsSelected) FocusFirst();
		}

		public virtual void Save() {
			if(PasswordOverlay.IsOpen || IsReadOnly || ErrorOverlay.IsOpen) return;
			CommitEdit();
			if(!HasChanges) return;
			SaveTo(FileName, GetFormat(FileName), PasswordOverlay.Password);
		}

		public abstract void CommitEdit();

		/// <inheritdoc />
		protected override void OnActivated(IObjectVM refferer) {
			base.OnActivated(refferer);
			IsSelected = true;

			FocusFirst();
		}

		private void FocusFirst() {
			//TODO REVISE set focus logic
			if(PasswordOverlay.IsOpen) PasswordOverlay.Focus();
			else if(ErrorOverlay.IsOpen) ErrorOverlay.Focus();
			else if(LoadingOverlay.IsOpen) LoadingOverlay.Focus();
			else if(this is TextEditorVM txt) txt.Editor.FocusEditor();
			else if(this is RichTextEditorVM rtf) rtf.Editor.FocusEditor();
			else if(this is TableEditorVM tbl) tbl.Editor.FocusEditor();
		}

		/// <inheritdoc />
		protected override void OnDeactivated() {
			base.OnDeactivated();
			IsSelected = false;
		}
	}

}