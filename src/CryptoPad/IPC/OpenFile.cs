
namespace KsWare.CryptoPad.IPC {

	public class OpenFile {

		public string FileName { get; set; }
		public string Format { get; set; }
		public bool IsReadOnly { get; set; }

		public Message<OpenFile> GetMessage() => new Message<OpenFile>(this);
	}
}
