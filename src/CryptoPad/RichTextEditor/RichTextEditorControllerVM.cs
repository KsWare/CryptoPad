using System.Windows.Controls;
using System.Windows.Documents;
using KsWare.Presentation.Core.Providers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.RichTextEditor {

	public class RichTextEditorControllerVM : DataVM<RichTextBox> {

		private RichTextBoxData _cache;

		public RichTextEditorControllerVM() {
			RegisterChildren(() => this);
			Document = new FlowDocument(); // Document must not be null
			Fields[nameof(Document)].ValueChangedEvent.add = (s, e) => { if (RichTextBox != null) RichTextBox.Document = (FlowDocument)e.NewValue; };
		}

		/// <inheritdoc />
		protected override void OnDataChanged(DataChangedEventArgs e) {
			base.OnDataChanged(e);
			if (e.PreviousData is RichTextBox r) {
				_cache = new RichTextBoxData {
					HorizontalOffset = r.HorizontalOffset,
					VerticalOffset = r.VerticalOffset,
					SelectionStart=r.Document.ContentStart.GetOffsetToPosition(r.Selection.Start),
					SelectionEnd=r.Document.ContentStart.GetOffsetToPosition(r.Selection.End),
				};
				r.Document = new FlowDocument(); // detach (ArgumentException: The document is already in the ownership of another "RichTextBox".)
			}
			if (e.NewData is RichTextBox rtb) {
				rtb.Document = Document;
				if (_cache != null) {
					rtb.ScrollToHorizontalOffset(_cache.HorizontalOffset);
					rtb.ScrollToVerticalOffset(_cache.VerticalOffset);
					rtb.Selection.Select(Document.ContentStart.GetPositionAtOffset(_cache.SelectionStart),Document.ContentStart.GetPositionAtOffset(_cache.SelectionEnd));
				}
				else {
					
				}
			}

			//DataGrid.ItemsSource = _table.DefaultView;
			// RichTextBox.Document = _flowDocument;
			// RichTextBox.SelectionChanged+=RichTextBoxOnSelectionChanged;
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
