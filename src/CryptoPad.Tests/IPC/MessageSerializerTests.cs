using KsWare.CryptoPad.IPC;
using NUnit.Framework;

namespace CryptoPad.Tests.IPC {

	public class MessageSerializerTests {

		[SetUp]
		public void Setup() {
		}

		[Test]
		public void SerializeDeserializeTest() {
			var yaml = MessageSerializer.Serialize(new Message<string> { Data = "foo bar" });
			var message = MessageSerializer.Deserialize<string>(yaml);

			Assert.That(message.Data,Is.EqualTo("foo bar"));
		}

		[Test]
		public void SerializeDeserialize_OpenFile_Test() {
			var m = new OpenFile {
				FileName = "C:\\Path\\File.test",
				Format = "Text",
				IsReadOnly = true
			};
			var yaml = MessageSerializer.Serialize(m.GetMessage());
			var message = MessageSerializer.Deserialize<OpenFile>(yaml);

			Assert.That(message.Data.FileName, Is.EqualTo("C:\\Path\\File.test"));
			Assert.That(message.Data.Format, Is.EqualTo("Text"));
			Assert.That(message.Data.IsReadOnly, Is.True);
		}
	}
}