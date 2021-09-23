using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Overlays {

	public class BaseOverlayVM : ObjectVM {
		public bool IsOpen { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
	}

}