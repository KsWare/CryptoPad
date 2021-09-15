using System;
using System.Collections.Generic;

namespace KsWare.CryptoPad {

	public class SessionData {

		public string Name { get; set; }

		public FileInfoData[] Files { get; set; } = Array.Empty<FileInfoData>();
	}

}