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
		private bool _changingDocument;
		private static FlowDocument NullDocument => new FlowDocument{Tag = "NULL"};

		public RichTextControllerVM() {
			RegisterChildren(() => this);
			Fields[nameof(Document)].ValueChangedEvent.add = (s, e) => {
				if (RichTextBox != null) {
					_changingDocument = true;
					RichTextBox.Document = (FlowDocument)e.NewValue ?? NullDocument;
					_changingDocument = false;
				}
			};
		}

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);
			if (e.PreviousData is RichTextBox r) {
				r.TextChanged -= RichTextBoxOnTextChanged;
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
					var selStart = rtb.Document.ContentStart.GetPositionAtOffset(_cache.SelectionStart);
					var selEnd = rtb.Document.ContentStart.GetPositionAtOffset(_cache.SelectionEnd);
					if (selStart != null && selEnd != null) rtb.Selection.Select(selStart, selEnd);
				}
				else {
					
				}
				rtb.TextChanged += RichTextBoxOnTextChanged;
			}
		}

		private void RichTextBoxOnTextChanged(object sender, TextChangedEventArgs e) {
			if(_changingDocument) return;
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
