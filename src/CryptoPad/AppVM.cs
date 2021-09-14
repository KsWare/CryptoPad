using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.CryptoPad {

	public class AppVM : ApplicationVM {

		public AppVM() {
			RegisterChildren(()=>this);
			StartupUri = typeof(ShellVM);
		}
	}
}
