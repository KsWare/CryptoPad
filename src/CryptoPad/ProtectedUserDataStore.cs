using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KsWare.CryptoPad {

	public static class ProtectedUserDataStore {

		private static byte[] _entropy;
		private static Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();
		private static readonly ISerializer _serializer;
		private static readonly IDeserializer _deserializer;

		private static readonly string _userDataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KsWare", "CryptoPad","UserData.dat");

		static ProtectedUserDataStore() {
			_serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
			_deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
			
			if(ReadEntropyFromRegistry(out _entropy))
				Load();
		}

		public static bool AutoSave { get; set; } = true;

		private static void Load() {
			_data = new Dictionary<string, byte[]>();
			if (!File.Exists(_userDataFile)) return;

			using var r = new StreamReader(_userDataFile);
			var data = _deserializer.Deserialize<Dictionary<string, string>>(r).ToDictionary(p => p.Key, p => Convert.FromBase64String(p.Value));
		}

		public static void Save() {
			Directory.CreateDirectory(Path.GetDirectoryName(_userDataFile));
			using var w = new StreamWriter(_userDataFile, false);
			_serializer.Serialize(w, _data.ToDictionary(p => p.Key, p => Convert.ToBase64String(p.Value)));
		}

		private static bool ReadEntropyFromRegistry(out byte[] entropy) {
			entropy = null;
			var sunKey = Registry.CurrentUser.OpenSubKey("Software\\KsWare\\CryptoPad", true);
			if (sunKey != null) {
				entropy = (byte[])sunKey.GetValue("{8004AD74-8691-4C4B-8263-5F8E57400833}");
				return true;
			}

			// Generate additional entropy (will be used as the Initialization vector)
			// This is basically the (2048-bit) encryption key used to encrypt the credentials
			entropy = new byte[256];
			using var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(_entropy);

			var currentUserRegistry = Registry.CurrentUser.OpenSubKey("Software\\KsWare\\CryptoPad", true);
			if (currentUserRegistry == null)
				currentUserRegistry = Registry.CurrentUser.CreateSubKey("Software\\KsWare\\CryptoPad",
					RegistryKeyPermissionCheck.Default);
			currentUserRegistry.SetValue("{8004AD74-8691-4C4B-8263-5F8E57400833}", entropy);

			return false;
		}

		public static byte[] GetBytes(string key) {
			var encryptedData = _data[key];
			var bytes = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
			return bytes;
		}

		public static string GetString(string key) {
			var encryptedData = _data[key];
			var bytes = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(bytes);
		}

		public static SecureString GetSecureString(string key) {
			var encryptedData = _data[key];
			var bytes = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
			var secureString = new SecureString();
			Array.ForEach(Encoding.UTF8.GetChars(bytes),c => secureString.AppendChar(c));
			secureString.MakeReadOnly();
			return secureString;
		}

		public static void Set<T>(string key, T value) {
			byte[] bytes;
			switch (value) {
				case string s: bytes = Encoding.UTF8.GetBytes(s); break;
				case byte[] b: bytes = b; break;
				case SecureString ss: bytes = ss.ToByteArray(); break;
				case double v: bytes = BitConverter.GetBytes(v); break;
				case float v: bytes = BitConverter.GetBytes(v); break;
				case Int16 v: bytes = BitConverter.GetBytes(v); break;
				case Int32 v: bytes = BitConverter.GetBytes(v); break;
				case Int64 v: bytes = BitConverter.GetBytes(v); break;
				case UInt16 v: bytes = BitConverter.GetBytes(v); break;
				case UInt32 v: bytes = BitConverter.GetBytes(v); break;
				case UInt64 v: bytes = BitConverter.GetBytes(v); break;
				default: throw new NotSupportedException();
			}
			var encrypted = ProtectedData.Protect(bytes, _entropy, DataProtectionScope.CurrentUser);
			_data[key] = encrypted;
			if(AutoSave) Save();
		}

	}
}
