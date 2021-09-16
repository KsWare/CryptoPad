using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KsWare.CryptoPad.IPC {

	public static class MessageSerializer {

		private static readonly ISerializer serializer = new SerializerBuilder()
			.WithNamingConvention(PascalCaseNamingConvention.Instance)
			.Build();
		private static readonly IDeserializer deserializer = new DeserializerBuilder()
			.WithNamingConvention(PascalCaseNamingConvention.Instance)
			.Build();

		public static string Serialize(IMessage message) {
			return serializer.Serialize(message);
		}
		public static string Serialize<T>(T data) {
			return serializer.Serialize(new Message<T>(data));
		}
		public static string Serialize<T>(string name, T data) {
			return serializer.Serialize(new Message<T>(name, data));
		}
		public static Message<T> Deserialize<T>(string yaml) {
			return deserializer.Deserialize<Message<T>>(yaml);
		}
	}

}
