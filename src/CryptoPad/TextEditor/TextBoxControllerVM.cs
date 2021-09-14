using System.Windows.Controls;
using KsWare.Presentation.Core.Providers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.TextEditor {

	public class TextBoxControllerVM : DataVM<TextBox> {

		private TextBoxData _cache;

		public TextBoxControllerVM() {
			RegisterChildren(() => this);
		}

		//
		public string Text { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);

			// TextBox properties are reset on unload, so we can not keep the old TextBox as cache, we have to store each relevant property.
			
			if (e.PreviousData is TextBox tb) {
				// the view is disconnected from the controller

				_cache = new TextBoxData {
					Text = tb.Text,
					CaretIndex=tb.CaretIndex,
					SelectionStart=tb.SelectionStart,
					SelectionLength=tb.SelectionLength,
				};
			}
			
			if (e.NewData is TextBox t) {
				// the view is connected to controller

				if (_cache == null) { 
					// connected for first time
					t.Text = Text;
				}
				else {
					t.Text = _cache.Text;
					t.CaretIndex = _cache.CaretIndex;
					t.Select(_cache.SelectionStart, _cache.SelectionLength);
				}
			}
		}
	}

}
