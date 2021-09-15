using System.Diagnostics;
using System.IO;
using System.Windows;
using JetBrains.Annotations;
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
			ctx.AddMenuItem("_Close", DoClose);
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

			FieldBindingOperations.SetBinding(writeProtection.Fields[nameof(MenuItemVM.IsChecked)],new FieldBinding(Fields[nameof(IsReadOnly)], BindingMode.TwoWay));
		}

		protected virtual void InitFileMenu() {
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
		}

		protected ShellVM Shell => (ShellVM)Parent.Parent;

		public string FileName { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public bool HasChanges { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
		public new bool IsReadOnly { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); } // hides ObjectVM.IsReadOnly
		public bool IsTempFile { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }

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
				case TableEditorVM: return "SpreadSheet";
				default: return null;
			}
		}

		public virtual void DoClose() => Shell.CloseTab(this);
		public virtual void DoCloseAllTabs() => Shell.CloseAllTabs();
		public virtual void DoCloseTabsRight() => Shell.CloseTabsRight(this);
		public virtual void DoCloseTabsLeft() => Shell.CloseTabsLeft(this);
		public virtual void DoCloseUnchangedTabs() => Shell.CloseUnchanged();

		public void DoSave() {
			if(!HasChanges) return;
			if (IsReadOnly || string.IsNullOrEmpty(FileName)) { DoSaveAs(); return; } // this should not occur
			
			SaveTo(FileName, null, askPassword:false);
		}

		protected abstract void SaveTo(string fileName, string format, bool askPassword);

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

		public virtual void DoReload() { } //TODO

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
			if(HasChanges && noSave==false) DoSave();
		}
	}

}