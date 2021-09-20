using System.Security;

namespace KsWare.CryptoPad {

	public class FileInfoData {
		public string Path { get; set; }
		public string Format { get; set; }
		public SecureString Password { get; set; }
		public bool IsReadOnly { get; set; }
	}

}