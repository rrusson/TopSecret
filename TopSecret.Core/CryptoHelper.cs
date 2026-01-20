using System.Security.Cryptography;
using System.Text;

using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	public class CryptoHelper : ICryptoHelper
	{
		private readonly byte[] _salt = Encoding.ASCII.GetBytes("Enter a unique salt value here.");
		private readonly string _uniqueKey = "Enter a unique encryption key here.";
		private readonly string _masterPw;  // This is the master password used for all encryption/decryption
		private readonly string _deviceId;

		public CryptoHelper(string? password)
		{
			ArgumentNullException.ThrowIfNull(password);

			_masterPw = password;
			_deviceId = DeviceId ?? string.Empty;
		}

		public string Encrypt(string plainText)
		{
			try
			{
				var plainTextBytes = Encoding.Unicode.GetBytes(plainText);

				using Aes aes = GetAes();
				using var memoryStream = new MemoryStream();
				using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

				cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
				cryptoStream.FlushFinalBlock();
				cryptoStream.Close();

				return Convert.ToBase64String(memoryStream.ToArray());
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException("Failed to encrypt data. This may be caused by using different encryption parameters or corrupted data.", ex);
			}
		}

		public string Decrypt(string cipherText)
		{
			try
			{
				var cipherTextBytes = Convert.FromBase64String(cipherText);

				using Aes aes = GetAes();
				using var memoryStream = new MemoryStream();
				using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

				cryptoStream.Write(cipherTextBytes, 0, cipherTextBytes.Length);
				cryptoStream.FlushFinalBlock();

				return Encoding.Unicode.GetString(memoryStream.ToArray());
			}
			catch (FormatException)
			{
				throw new CryptographicException("Invalid Base64 string. The encrypted data may be corrupted.");
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException("Failed to decrypt data. This may be caused by using different encryption parameters or corrupted data.", ex);
			}
		}

		private Aes GetAes()
		{
			Aes aes = Aes.Create();
			string key = _uniqueKey + _masterPw + _deviceId;

			var rfcDecoder = new Rfc2898DeriveBytes(key, _salt, 128_000, HashAlgorithmName.SHA512);
			aes.Key = rfcDecoder.GetBytes(32);
			aes.IV = rfcDecoder.GetBytes(16);
			aes.Padding = PaddingMode.PKCS7;

			return aes;
		}

		/// <summary>
		/// Get unique device ID to make it more difficult to crack the encryption
		/// </summary>
		/// <returns>Unique DeviceId</returns>
		private static string? DeviceId =>
#if ANDROID
	Android.Provider.Settings.Secure.GetString(Platform.CurrentActivity?.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
#elif IOS
	UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#elif WINDOWS
	Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher().Id.ToString();
#elif WINDOWS_UWP
	Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher().Id.ToString();
#elif LINUX
	Environment.MachineName;
#elif MACOS
	NSProcessInfo.ProcessInfo.HostName;
#elif TIZEN
	Tizen.System.Information.DeviceId;
#elif TVOS
	UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#elif WATCHOS
	WKInterfaceDevice.CurrentDevice.IdentifierForVendor.ToString();
#else
	"UNKNOWN";
#endif
	}
}