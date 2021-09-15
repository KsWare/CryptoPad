using System.Windows.Controls;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class TabItemVM : ObjectVM {

		/// <inheritdoc />
		/// <seealso cref="TabItem"/>
		public TabItemVM() {
			RegisterChildren(()=>this);
			Content = this;
		}

		/// <summary>
		/// Gets or sets the data used for the header of each control.
		/// </summary>
		/// <value>The header view model.</value>
		/// <seealso cref="HeaderedContentControl.Header"/>
		public TabHeaderVM Header { get; private set; }

		[Hierarchy(HierarchyType.Reference)]
		public ObjectVM Content { get => Fields.GetValue<ObjectVM>(); set => Fields.SetValue(value); }
	}

	public class TabHeaderVM : ObjectVM {

		public ContextMenuVM ContextMenu { get => Fields.GetValue<ContextMenuVM>(); set => Fields.SetValue(value); }
		
		public string Text { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }
	}
}