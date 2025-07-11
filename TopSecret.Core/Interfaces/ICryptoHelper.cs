namespace TopSecret.Core.Interfaces
{
	public interface ICryptoHelper
	{
		/// <summary>
		/// Makes <paramref name="plainText"/> unreadable
		/// </summary>
		/// <param name="plainText">Garbage in</param>
		/// <returns>Encrypted garbage out</returns>
		string Encrypt(string plainText);

		/// <summary>
		/// Turns <paramref name="cipherText"/> gibberish into readable text
		/// </summary>
		/// <param name="cipherText">Mystery text</param>
		/// <returns>Readable text</returns>
		string Decrypt(string cipherText);
	}
}