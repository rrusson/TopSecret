using System.Security.Cryptography;
using System.Text;

namespace TopSecret.Helpers
{
	public class CryptoHelper
	{
		private readonly byte[] _salt = Encoding.ASCII.GetBytes("Enter a unique salt value here.");
		private readonly string _uniqueKey = "Enter a unique encryption key here.";
		private readonly string _masterPw;  // This is the master password used to open the app

		/// <summary>
		/// Default CTOR
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if master password hasn't been set</exception>
		public CryptoHelper(string? password)
		{
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new InvalidOperationException("This app cannot operate without a password.");
			}

			_masterPw = password;
		}

		/// <summary>
		/// Makes <paramref name="plainText"/> unreadable
		/// </summary>
		/// <param name="plainText">Garbage in</param>
		/// <returns>Encrypted garbage out</returns>
		public string Encrypt(string plainText)
		{
			var plainTextBytes = Encoding.Unicode.GetBytes(plainText);

			using var aes = Aes.Create();
			string key = _masterPw + _uniqueKey;

			var rfcDecoder = new Rfc2898DeriveBytes(key, _salt, 2600, HashAlgorithmName.SHA512);
			aes.Key = rfcDecoder.GetBytes(32);
			aes.IV = rfcDecoder.GetBytes(16);

			using var memoryStream = new MemoryStream();
			using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
			cryptoStream.Close();

			return Convert.ToBase64String(memoryStream.ToArray());
		}

		/// <summary>
		/// Turns <paramref name="cipherText"/> gibberish into readable text
		/// </summary>
		/// <param name="cipherText">Mystery text</param>
		/// <returns>Readable text</returns>
		internal string Decrypt(string cipherText)
		{
			var cipherTextBytes = Convert.FromBase64String(cipherText);

			using var aes = Aes.Create();
			string key = _masterPw + _uniqueKey;

			var rfcDecoder = new Rfc2898DeriveBytes(key, _salt, 2600, HashAlgorithmName.SHA512);
			aes.Key = rfcDecoder.GetBytes(32);
			aes.IV = rfcDecoder.GetBytes(16);

			using var memoryStream = new MemoryStream();
			using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
			
			cryptoStream.Write(cipherTextBytes, 0, cipherTextBytes.Length);
			cryptoStream.Close();
			
			return Encoding.Unicode.GetString(memoryStream.ToArray());
		}
	}
}
