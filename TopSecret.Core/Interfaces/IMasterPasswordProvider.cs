namespace TopSecret.Core.Interfaces
{
	/// <summary>
	/// Interface for providing the master password to core components
	/// </summary>
	public interface IMasterPasswordProvider
	{
		/// <summary>
		/// Gets the current master password
		/// </summary>
		string? MasterPassword { get; }

		/// <summary>
		/// Returns the (encrypted) master password or null if it hasn't been set
		/// </summary>
		Task<string?> GetMasterPasswordAsync();

		/// <summary>
		/// Sets the master password used for all encryption/decryption operations
		/// </summary>
		/// <param name="encryptedPassword">Master password</param>
		void SetMasterPassword(string encryptedPassword);

		/// <summary>
		/// Updates the master password used to log in to the app
		/// </summary>
		/// <param name="newPassword">New password to use</param>
		Task ChangeMasterPasswordAsync(string newPassword);
	}
}