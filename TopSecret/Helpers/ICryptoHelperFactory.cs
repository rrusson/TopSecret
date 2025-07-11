namespace TopSecret.Helpers
{
	public interface ICryptoHelperFactory
	{
		/// <summary>
		/// Creates a CryptoHelper instance with the specified password
		/// </summary>
		/// <param name="password">The password to use for encryption/decryption</param>
		/// <returns>A CryptoHelper instance</returns>
		ICryptoHelper CreateCryptoHelper(string? password);
	}
}