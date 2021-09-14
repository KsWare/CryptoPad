using System;
using System.Windows.Input;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad.TextEditor {

	public static class ListVMMenuItemExtensions {

		public static MenuItemVM AddMenuItem(this ListVM<MenuItemVM> list, MenuItemVM item) {
			list.Add(item);
			return item;
		}

		public static MenuItemVM AddMenuItem(this ListVM<MenuItemVM> list, string caption, Action action=null) {
			var item = new MenuItemVM {
				Caption = caption,
				CommandAction = { MːDoAction = action}
			};
			list.Add(item);
			return item;
		}

		public static MenuItemVM AddMenuItem(this ListVM<MenuItemVM> list, ICommand command) {
			return AddMenuItem(list, new MenuItemVM { Command = command });
		}

		public static MenuItemVM AddMenuItem(this MenuItemVM parent, MenuItemVM item) {
			parent.Items.Add(item);
			return item;
		}
		public static MenuItemVM AddMenuItem(this MenuItemVM item, string caption, Action action=null) {
			return AddMenuItem(item.Items, caption, action);
		}
		public static MenuItemVM AddMenuItem(this MenuItemVM parent, ICommand command) {
			return AddMenuItem(parent, new MenuItemVM { Command = command });
		}
		public static MenuItemVM AddMenuItem(this MenuItemVM item, string caption, Action<object> action, string commandParameter) {
			var newItem = new MenuItemVM {
				Caption = caption,
				CommandAction = { MːDoActionP = action},
				CommandParameter = commandParameter
			};
			item.Items.Add(newItem);
			return item;
		}
	}

}