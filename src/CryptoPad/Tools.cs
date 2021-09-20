using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace KsWare.CryptoPad {

	public static class Tools {

		public static bool Try(Action action) {
			try {
				action();
				return true;
			}
			catch (CryptographicException ex) {
				MessageBox.Show(Application.Current.MainWindow, "Cryptographic error.\nPassword wrong?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception exception) {
				MessageBox.Show(Application.Current.MainWindow, exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return false;
		}

		public static string Decrypt(string sourceFile, SecureString password) {
			using var fileStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Inheritable);
			var info = Decrypt(fileStream, password);
			using var reader = new StreamReader(info.Stream, new UTF8Encoding(false));
			return reader.ReadToEnd();
		}

		public static CryptoStreamInfo Decrypt(Stream stream, SecureString password) {
			var header = CryptFile.ReadFileHeader(stream);
			var cryptoStream = new CryptoStream(stream, CryptFile.CreateDecryptor(header, password), CryptoStreamMode.Read);
			var info = CryptFile.ReadDataHeader(cryptoStream, header);
			return info;
		}

		public static void Encrypt(Stream input, Stream output, SecureString password, string contentType) {
			var info = CryptFile.CreateCryptFileInfo(contentType, password);
			CryptFile.WriteFileHeader(output, contentType, info.Salt);
			using var cryptoStream = new CryptoStream(output, info.CryptoTransform, CryptoStreamMode.Write, true);
			CryptFile.WriteDataHeader(cryptoStream);

			Copy(input, cryptoStream);
			cryptoStream.Flush();
		}

		public static void Copy(Stream input, Stream output, int bufferSize = 64 * 1024) {
			var buffer = new byte[bufferSize];
			while (true) {
				var read = input.Read(buffer);
				output.Write(buffer, 0, read);
				if (read < buffer.Length) break;
			}
		}
	}

	public class CryptoStreamInfo {

		public CryptoStreamInfo(CryptFileInfo fileInfo, Stream stream) {
			ContentType = fileInfo.ContentType;
			Password = fileInfo.Password;
			Stream = stream;
		}

		public string ContentType { get; }
		public Stream Stream { get; }
		public SecureString Password { get; set; }
	}

}
