using System;
using System.Windows.Controls;
using System.Windows.Documents;
using KsWare.Presentation.Core.Providers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.RichTextEditor {

	public class RichTextControllerVM : EditorControllerVM<RichTextBox,FlowDocument> {
		// RichTextBox.Document must not be null => workaround: use NullDocument
		// detaching document because ArgumentException: The document is already in the ownership of another "RichTextBox".)

		private RichTextBoxData _cache;
		private static FlowDocument NullDocument => new FlowDocument{Tag = "NULL"};

		public RichTextControllerVM() {
			RegisterChildren(() => this);
			Fields[nameof(Document)].ValueChangedEvent.add = (s, e) => { if (RichTextBox != null) RichTextBox.Document = (FlowDocument)e.NewValue ?? NullDocument; };
		}

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);
			if (e.PreviousData is RichTextBox r) {
				r.TextChanged -= AtRichTextBoxOnTextChanged;
				_cache = new RichTextBoxData {
					HorizontalOffset = r.HorizontalOffset,
					VerticalOffset = r.VerticalOffset,
					SelectionStart=r.Document.ContentStart.GetOffsetToPosition(r.Selection.Start),
					SelectionEnd=r.Document.ContentStart.GetOffsetToPosition(r.Selection.End),
				};
				r.Document = NullDocument; // detach document
			}
			if (e.NewData is RichTextBox rtb) {
				rtb.Document = Document ?? NullDocument;
				if (_cache != null) {
					rtb.ScrollToHorizontalOffset(_cache.HorizontalOffset);
					rtb.ScrollToVerticalOffset(_cache.VerticalOffset);
					rtb.Selection.Select(Document.ContentStart.GetPositionAtOffset(_cache.SelectionStart),Document.ContentStart.GetPositionAtOffset(_cache.SelectionEnd));
				}
				else {
					
				}
				rtb.TextChanged += AtRichTextBoxOnTextChanged;
				rtb.Focus();
			}
		}

		private void AtRichTextBoxOnTextChanged(object sender, TextChangedEventArgs e) {
			OnContentChanged();
		}

		private RichTextBox RichTextBox => Data;

		public FlowDocument Document { get => Fields.GetValue<FlowDocument>(); set => Fields.SetValue(value); }
	}

	public class RichTextBoxData {
		public double HorizontalOffset;
		public double VerticalOffset;
		public int SelectionStart;
		public int SelectionEnd;

	}
}
