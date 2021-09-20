using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace KsWare.CryptoPad {

	public static class SecureStringExtensions {

		public static bool IsNullOrEmpty(this SecureString secureString) {
			return secureString == null || secureString.Length == 0;
		}

		public static SecureString ToSecureString(this string password) {
			var secureString = new SecureString();
			Array.ForEach(password.ToCharArray(), secureString.AppendChar);
			secureString.MakeReadOnly();
			return secureString;
		}

		public static string ToInsecureString(this SecureString secureString) {
			if (secureString == null) return null;
			var globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(secureString);
			var stringUni = Marshal.PtrToStringUni(globalAllocUnicode);
			Marshal.ZeroFreeGlobalAllocUnicode(globalAllocUnicode);
			return stringUni;
		}

		public static byte[] ToByteArray(this SecureString secureString, Encoding encoding = null) {
			if (secureString == null) return null;
			encoding ??= Encoding.UTF8;
			var globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(secureString);
			var stringUni = Marshal.PtrToStringUni(globalAllocUnicode);
			Marshal.ZeroFreeGlobalAllocUnicode(globalAllocUnicode);
			return encoding.GetBytes(stringUni);
		}
	}
}
