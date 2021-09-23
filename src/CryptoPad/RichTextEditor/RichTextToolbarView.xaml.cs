using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KsWare.CryptoPad.RichTextEditor {
	/// <summary>
	/// Interaction logic for RichTextToolbarView.xaml
	/// </summary>
	public partial class RichTextToolbarView : UserControl {

		public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
			"CommandTarget", typeof(UIElement), typeof(RichTextToolbarView), new FrameworkPropertyMetadata(default(UIElement), (d, e) => ((RichTextToolbarView)d).OnCommandTargetChanged(e)));
		
		public RichTextToolbarView() {
			InitializeComponent();
		}

		public UIElement CommandTarget {
			get => (UIElement)GetValue(CommandTargetProperty);
			set => SetValue(CommandTargetProperty, value);
		}

		private void OnCommandTargetChanged(DependencyPropertyChangedEventArgs e) {
			var newValue = e.NewValue as RichTextBox;
			var oldValue = e.OldValue as RichTextBox;

			if(oldValue !=null) oldValue.SelectionChanged-=RichTextBoxOnSelectionChanged;
			if(newValue !=null) newValue.SelectionChanged+=RichTextBoxOnSelectionChanged;
		}

		private void RichTextBoxOnSelectionChanged(object sender, RoutedEventArgs e) {
			
			ToggleBold.IsChecked = Equ(TextElement.FontWeightProperty, FontWeights.Bold);
			ToggleItalic.IsChecked = Equ(TextElement.FontStyleProperty, FontStyles.Italic);
			ToggleUnderline.IsChecked = Equ(Inline.TextDecorationsProperty, TextDecorations.Underline);

			AlignLeft.IsChecked = Equ(Paragraph.TextAlignmentProperty, TextAlignment.Left);
			AlignCenter.IsChecked = Equ(Paragraph.TextAlignmentProperty, TextAlignment.Center);
			AlignJustify.IsChecked = Equ(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
			AlignRight.IsChecked = Equ(Paragraph.TextAlignmentProperty, TextAlignment.Right);

			UpdateSelectionListType();
		}

		private void UpdateSelectionListType() {
			var rtb = (RichTextBox)CommandTarget;

			Paragraph startParagraph = rtb.Selection.Start.Paragraph;
			Paragraph endParagraph = rtb.Selection.End.Paragraph;
			if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) &&
			    (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List,
				    ((ListItem)endParagraph.Parent).List)) {
				TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
				if (markerStyle == TextMarkerStyle.Disc) {//bullets
					ToggleBullets.IsChecked = true;
				}
				else if (markerStyle == TextMarkerStyle.Decimal) {//numbers
					ToggleNumbering.IsChecked = true;
				}
			}
			else {
				ToggleBullets.IsChecked = false;
				ToggleNumbering.IsChecked = false;
			}
		}


		private bool? Equ<T>(DependencyProperty property, T trueValue) {
			var rtb = (RichTextBox)CommandTarget;
			var value = rtb.Selection.GetPropertyValue(property);
			if (value == DependencyProperty.UnsetValue) return null;
			return Equals(value, trueValue);
		}
	}
}
