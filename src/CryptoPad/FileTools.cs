using System;
using System.IO;

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
	}
}
