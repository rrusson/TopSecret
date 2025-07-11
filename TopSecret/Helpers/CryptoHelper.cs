using System.Security.Cryptography;
using System.Text;

namespace TopSecret.Helpers
{
	public class CryptoHelper : ICryptoHelper
	{
		private readonly byte[] _salt = Encoding.ASCII.GetBytes("Enter a unique salt value here.");
		private readonly string _uniqueKey = "Enter a unique encryption key here.";
		private readonly string _masterPw;  // This is the master password used for all encryption/decryption
		private readonly string _deviceId;

		/// <summary>
		/// Default CTOR
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if master password isn't set</exception>
		public CryptoHelper(string? password)
		{
			ArgumentNullException.ThrowIfNull(password);

			_masterPw = password;
			_deviceId = GetDeviceId() ?? string.Empty;
		}

		/// <inheritdoc/>
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
				// Preserve the original exception but add more context
				throw new CryptographicException("Failed to encrypt data. This may be caused by using different encryption parameters or corrupted data.", ex);
			}
		}

		/// <inheritdoc/>
		public string Decrypt(string cipherText)
		{
			try
			{
				var cipherTextBytes = Convert.FromBase64String(cipherText);

				using Aes aes = GetAes();
				using var memoryStream = new MemoryStream();
				using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

				cryptoStream.Write(cipherTextBytes, 0, cipherTextBytes.Length);
				cryptoStream.FlushFinalBlock(); // Ensure padding is properly handled

				return Encoding.Unicode.GetString(memoryStream.ToArray());
			}
			catch (FormatException)
			{
				throw new CryptographicException("Invalid Base64 string. The encrypted data may be corrupted.");
			}
			catch (CryptographicException ex)
			{
				// Preserve the original exception but add more context
				throw new CryptographicException("Failed to decrypt data. This may be caused by using different encryption parameters or corrupted data.", ex);
			}
		}

		/// <summary>
		/// Creates a new AES object with a unique key
		/// </summary>
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
		private static string? GetDeviceId()
		{
#if ANDROID
	return Android.Provider.Settings.Secure.GetString(Platform.CurrentActivity?.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
#elif IOS
	return UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#elif WINDOWS
	return Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher().Id.ToString();
#elif WINDOWS_UWP
	return Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher().Id.ToString();
#elif LINUX
	return Environment.MachineName;
#elif MACOS
	return NSProcessInfo.ProcessInfo.HostName;
#elif TIZEN
	return Tizen.System.Information.DeviceId;
#elif TVOS
	return UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#elif WATCHOS
	return WKInterfaceDevice.CurrentDevice.IdentifierForVendor.ToString();
#else
	return "UNKNOWN";
#endif
		}
	}
}
