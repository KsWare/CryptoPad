using System;
using System.Windows;
using JetBrains.Annotations;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Overlays {

	public class ErrorOverlayVM : BaseOverlayVM {

		/// <inheritdoc />
		public ErrorOverlayVM() {
			RegisterChildren(() => this);
			if (IsInDesignMode) {
				Message = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
				ExceptionMessage = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, \nsed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, \nno sea takimata sanctus est Lorem ipsum dolor sit amet.";
				return;
			}
			OkAction.UI.Visibility=Visibility.Collapsed;
			RepeatAction.UI.Visibility=Visibility.Collapsed;
			CancelAction.UI.Visibility=Visibility.Collapsed;
			CloseAction.UI.Visibility=Visibility.Collapsed;
		}

		public string Message { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
		public string ExceptionMessage { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		[Hierarchy(HierarchyType.Reference)]
		public ActionVM DefaultAction { get => Fields.GetValue<ActionVM>(); set => Fields.SetValue(value); }

		public ActionVM OkAction { get; [UsedImplicitly] private set; }
		public ActionVM RepeatAction { get; [UsedImplicitly] private set; }
		public ActionVM CancelAction { get; [UsedImplicitly] private set; }
		public ActionVM CloseAction { get; [UsedImplicitly] private set; }
	}

}
