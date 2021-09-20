using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using KsWare.Presentation.Core.Providers;

namespace KsWare.CryptoPad.TextEditor {

	public class TextBoxControllerVM : EditorControllerVM<TextBox, string> {

		private TextBoxData _cache;

		public TextBoxControllerVM() {
			RegisterChildren(() => this);
		}

		//
		public string Text {
			get => Fields.GetValue<string>();
			set {
				Debug.WriteLine($"Text: {value}");
				Fields.SetValue(value);
			}
		}

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);

			// TextBox properties are reset on unload, so we can not keep the old TextBox as cache, we have to store each relevant property.
			
			if (e.PreviousData is TextBox tb) {
				// the view is disconnected from the controller
				tb.TextChanged -= AtTextBoxOnTextChanged;
				var sv = tb.GetScrollViewer();
				_cache = new TextBoxData {
					Text = tb.Text,
					CaretIndex=tb.CaretIndex,
					SelectionStart=tb.SelectionStart,
					SelectionLength=tb.SelectionLength,
					HorizontalOffset = sv.HorizontalOffset,
					VerticalOffset = sv.VerticalOffset
				};
			}
			
			if (e.NewData is TextBox t) {
				// the view is connected to controller

				if (_cache == null) { 
					// connected for first time
					t.Text = Text;
				}
				else {
					var sv = t.GetScrollViewer();
					t.Text = _cache.Text;
					t.CaretIndex = _cache.CaretIndex;
					t.Select(_cache.SelectionStart, _cache.SelectionLength);
					sv.ScrollToHorizontalOffset(_cache.HorizontalOffset);
					sv.ScrollToVerticalOffset(_cache.VerticalOffset);
				}

				t.Focus();
				t.TextChanged += AtTextBoxOnTextChanged;
			}
		}

		private void AtTextBoxOnTextChanged(object s, TextChangedEventArgs e) {
			OnContentChanged();
		}

		public void Focus() => Data?.Focus();

		/// <summary>
		/// Refresh the <see cref="Text"/> property with text from the view.
		/// </summary>
		/// <remarks>Workaround for: click to 'window close' does not update the source. so we have it to do manually before save.</remarks>
		public void RefreshText() {
			if (Data == null) return;
			var bindingExpression = BindingOperations.GetBindingExpression(Data, System.Windows.Controls.TextBox.TextProperty);
			bindingExpression?.UpdateSource();
		}
	}

}
