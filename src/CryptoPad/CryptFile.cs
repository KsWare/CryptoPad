using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KsWare.CryptoPad {

	public static class CryptFile {

		public static void Write(string text, string targetFile, string password, string contentType = "text/plain") {
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
			Write(stream, targetFile, password, contentType);
		}

		public static void Write(Stream stream, string targetFileName, string password, string contentType) {
			if(File.Exists(targetFileName + ".~tmp")) File.Delete(targetFileName + ".~tmp");
			var fileStream = File.Open(targetFileName + ".~tmp", FileMode.CreateNew, FileAccess.Write, FileShare.None);
			Tools.Encrypt(stream, fileStream, password, contentType);
			fileStream.Close();
			File.Delete(targetFileName);
			File.Move(targetFileName + ".~tmp", targetFileName);
		}

		private static void RemoveExistingBackupFile(string backupFileName) {
			if (File.Exists(backupFileName)) File.Delete(backupFileName);
		}

		private static void RenameExistingAsBackupFile(string targetFileName, string backupFileName) {
			if (File.Exists(targetFileName)) File.Move(targetFileName, backupFileName);
		}

		public static Stream Create(string fileName, string password, string contentType) {
			var output = File.Create(fileName); // will be closed when cryptoStream is closed

			var info = CreateCryptFileInfo(password);
			WriteFileHeader(output, info.Salt);
			var cryptoStream = new CryptoStream(output, info.CryptoTransform, CryptoStreamMode.Write, leaveOpen:false);
			WriteDataHeader(cryptoStream, contentType);

			cryptoStream.Flush();
			return cryptoStream;
		}

		public static CryptoStreamInfo OpenRead(string fileName, string password) {
			var stream = File.OpenRead(fileName);
			var info = ReadFileHeader(stream);
			var decryptor = CreateDecryptor(info, password);
			var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, leaveOpen:false);
			var info2 = ReadDataHeader(cryptoStream);
			return info2;
		}

		internal static void WriteFileHeader(Stream output, byte[] salt) {
			using var w = new BinaryWriter(output, Encoding.UTF8, true);
			w.Write(Encoding.ASCII.GetBytes("CRYPT"));	// Signature (5 bytes)
			w.Write((ushort)1);							// Version 1 (2 bytes)
			w.Write((short)salt.Length);				// salt size (2 byte)
			w.Write(salt, 0, salt.Length);				// salt
			w.Flush();
		}

		internal static CryptFileInfo ReadFileHeader(Stream input) {
			using var r = new BinaryReader(input, Encoding.UTF8, true);
			var sig = r.ReadBytes(5); // signature "CRYPT"
			var version = r.ReadInt16(); // Version 1 (2 bytes)
			if (version != 1) throw new InvalidDataException("Incompatible file version!\nUpdate CryptoPad.");
			var saltSize = r.ReadInt16(); // salt size 8 (2 byte)
			var salt = r.ReadBytes(saltSize);
			return new CryptFileInfo(version, salt, null);
		}

		public static void WriteDataHeader(CryptoStream cryptoStream, string contentType) {
			using var w = new BinaryWriter(cryptoStream, Encoding.UTF8, true);
			w.Write(contentType); // content type (x+1 byte)
			//w.Write(ComputeHash(input)); // SHA256-256 checksum (32 bytes)
			w.Flush();
		}

		public static CryptoStreamInfo ReadDataHeader(CryptoStream cryptoStream) {
			using var binaryReader2 = new BinaryReader(cryptoStream, Encoding.UTF8, true);
			var contentType = binaryReader2.ReadString();
			//var hash = binaryReader2.ReadBytes(32); // SHA256-256 checksum (32 bytes)

			var info = new CryptoStreamInfo(contentType, cryptoStream);
			return info;
		}

		/// <summary>
		/// Create the decryptor vor for V1.
		/// </summary>
		/// <param name="header">Header from file.</param>
		/// <param name="password">The password</param>
		/// <returns></returns>
		public static ICryptoTransform CreateDecryptor(CryptFileInfo header, string password) {
			// initialize algorithm with salt
			var keyGenerator = new Rfc2898DeriveBytes(password, header.Salt, 10000, HashAlgorithmName.SHA256);
			var rijndael = Rijndael.Create();

			rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / 8);
			rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / 8);
			return rijndael.CreateDecryptor();
		}

		public static CryptFileInfo CreateCryptFileInfo(string password) {
			var saltSize = 8;
			var keyGenerator = new Rfc2898DeriveBytes(password, saltSize, 10000, HashAlgorithmName.SHA256);
			var rijndael = Rijndael.Create();

			// BlockSize, KeySize in bit --> divide by 8
			rijndael.IV = keyGenerator.GetBytes(rijndael.BlockSize / 8);
			rijndael.Key = keyGenerator.GetBytes(rijndael.KeySize / 8);

			return new CryptFileInfo(1, keyGenerator.Salt, rijndael.CreateEncryptor());
		}

		public static string LastPassword { get; set; }
	}

	public class CryptFileInfo {

		public CryptFileInfo(int version, byte[] salt, ICryptoTransform transform ) {
			Version = version;
			Salt = salt;
			CryptoTransform = transform;
		}

		public int Version { get; }

		public byte[] Salt { get; }

		public ICryptoTransform CryptoTransform { get; }
	}

}
