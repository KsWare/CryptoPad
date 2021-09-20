using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KsWare.CryptoPad.IPC {

	public static class MessageSerializer {

		private static readonly ISerializer serializer = new SerializerBuilder()
			.WithNamingConvention(PascalCaseNamingConvention.Instance)
			.Build();
		private static readonly IDeserializer deserializer = new DeserializerBuilder()
			.WithNamingConvention(PascalCaseNamingConvention.Instance)
			.IgnoreUnmatchedProperties()
			.Build();

		public static string Serialize(IMessage message) {
			return serializer.Serialize(message).Replace("\r\n","\0x1F");
		}
		public static string Serialize<T>(T data) {
			if(data.GetType().IsAssignableTo(typeof(IMessage))) 
				return serializer.Serialize(data).Replace("\r\n","\0x1F");
			return serializer.Serialize(new Message<T>(data)).Replace("\r\n","\0x1F");
		}
		public static string Serialize<T>(string name, T data) {
			return serializer.Serialize(new Message<T>(name, data)).Replace("\r\n","\0x1F");
		}
		public static Message<T> Deserialize<T>(string yaml) {
			return deserializer.Deserialize<Message<T>>(yaml.Replace("\0x1F","\r\n"));
		}
	}

}
