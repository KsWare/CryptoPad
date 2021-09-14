using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class TabVM : ObjectVM {
		public ObjectVM Header { get => Fields.GetValue<ObjectVM>(); set => Fields.SetValue(value); }
		public ObjectVM Content { get => Fields.GetValue<ObjectVM>(); set => Fields.SetValue(value); }
	}

}