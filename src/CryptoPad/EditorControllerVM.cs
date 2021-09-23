using System;
using System.Windows;
using KsWare.Presentation.Core.Providers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class EditorControllerVM<TUIElement,TContent> : DataVM<TUIElement>, IEditorController where TUIElement:UIElement { 
		// TODO use UIElementVM<T>

		public event EventHandler ContentChanged;

		public virtual TContent Content { get; set; }

		protected void OnContentChanged() {
			ContentChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc />
		public void FocusEditor() { //TODO REVISE set focus logic
			Data?.Focus();
		}

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);
			OnPropertyChanged(nameof(View));
		}

		public UIElement View => Data;
	}

	public interface IEditorController {
		void FocusEditor();
		UIElement View { get; }
	}

}