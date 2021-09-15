using System;
using System.Windows;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class EditorControllerVM<TUIElement,TContent> : DataVM<TUIElement> where TUIElement:UIElement { 
		// TODO use UIElementVM<T>

		public event EventHandler ContentChanged;

		public virtual TContent Content { get; set; }

		protected void OnContentChanged() {
			ContentChanged?.Invoke(this, EventArgs.Empty);
		}
	}

}