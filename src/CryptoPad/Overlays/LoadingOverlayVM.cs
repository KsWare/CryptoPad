using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.Overlays {

	public class LoadingOverlayVM : BaseOverlayVM {

		public LoadingOverlayVM() {
			RegisterChildren(() => this);
			CancelAction.MːDoAction = () => IsOpen = false;
		}

		public bool CanCancel { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
		public ActionVM CancelAction { get; [UsedImplicitly] private set; }
	}
}
