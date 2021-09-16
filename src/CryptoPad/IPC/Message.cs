namespace KsWare.CryptoPad.IPC {

	public interface IMessage {
		string Name { get; set; }
		string Type { get; set; }
	} 

	public class Message<T> : Header {

		/// <inheritdoc />
		public Message() {
			Type = typeof(T).FullName;
		}

		public Message(string name, T data) {
			Name = name;
			Data = data;
			Type = data.GetType().FullName;
		}

		public Message(T data) {
			Name = data.GetType().Name;
			Data = data;
		}

		public T Data { get; set; }

		public static Message<T> Create(T data) => new Message<T>(data);
	}

	public class Header : IMessage {
		public string Name { get; set; }
		public string Type { get; set; }
	}

}