using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KsWare.CryptoPad {

	public static class FileTools {

		public static void Write(string text, string fileName) {
			Write(fileName, () => {
				using var writer = new StreamWriter(fileName, false);
				writer.Write(text);
			});
		}

		public static void Write(string fileName, Action writeAction) {
			if(File.Exists(fileName + ".~tmp")) File.Delete(fileName + ".~tmp");
			using var fileStream = File.Open(fileName + ".~tmp", FileMode.CreateNew, FileAccess.Write, FileShare.None);
			writeAction();
			File.Delete(fileName);
			File.Move(fileName + ".~tmp", fileName);
		}

		public static string Read(string fileName) {
			using var reader = new StreamReader(fileName);
			return reader.ReadToEnd();
		}

		private const string TempFilesName = "TempFiles";

		public static string NewTempFile(string name) {
			var f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KsWare", "CryptoPad", TempFilesName);
			Directory.CreateDirectory(f);
			var files=Directory.GetFiles(f, $"{name}*.crypt");
			var numbers = files.Select(f => {
				var match = Regex.Match(Path.GetFileNameWithoutExtension(f), @"\d+$", RegexOptions.Compiled);
				return match.Success ? int.Parse(match.Value) : 0;
			});
			var highestNumber = numbers.OrderBy(v => v).LastOrDefault();
			var n = Path.Combine(f, $"{name}{highestNumber + 1}.crypt");
			using var stream = File.Open(n, FileMode.CreateNew);
			return n;
		}

		// public static string NewTempFile(string name, string contentType, string password) {
		// 	var f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KsWare", "CryptoPad", TempFilesName);
		// 	Directory.CreateDirectory(f);
		// 	var files=Directory.GetFiles(f, $"{name}*.crypt");
		// 	var numbers = files.Select(f => {
		// 		var match = Regex.Match(Path.GetFileNameWithoutExtension(f), @"\d+$", RegexOptions.Compiled);
		// 		return match.Success ? int.Parse(match.Value) : 0;
		// 	});
		// 	var highestNumber = numbers.OrderBy(v => v).LastOrDefault();
		// 	var n = Path.Combine(f, $"{name}{highestNumber + 1}.crypt");
		// 	using var stream = File.Open(n, FileMode.CreateNew);
		// 	CryptFile.WriteFileHeader(stream, contentType, Array.Empty<byte>());
		// 	return n;
		// }

		public static bool IsTempFile(string fileName) {
			return Regex.IsMatch(fileName, @"\\"+TempFilesName+@"\\", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}

		public static void SaveSessionData(SessionData data) {
			var dn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KsWare", "CryptoPad", "SessionData");
			Directory.CreateDirectory(dn);
			var fn = Path.Combine(dn, data.Name + ".yaml");

			var serializer = new SerializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();
			using var writer = new StreamWriter(fn, false);
			serializer.Serialize(writer,data);
		}

		public static SessionData LoadSessionData(string name, bool createIfNotExist) {
			var dn = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KsWare", "CryptoPad", "SessionData");
			var fn = Path.Combine(dn, name + ".yaml");
			if (!File.Exists(fn)) {
				if (!createIfNotExist) return null;
				return new SessionData { Name = name };
			}
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();
			using var reader = new StreamReader(fn);
			return deserializer.Deserialize<SessionData>(reader);
		}

		public static bool IsCryptFile(string fileName) {
			return Path.GetExtension(fileName).ToLowerInvariant() == ".crypt";
		}
	}
}
