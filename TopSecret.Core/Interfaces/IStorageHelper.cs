namespace TopSecret.Core.Interfaces
{
	public interface IStorageHelper
	{
		/// <summary>
		/// Thread-safe property that indicates saving/removing items is in progress
		/// </summary>
		bool IsBusy { get; }

		/// <summary>
		/// Decrypts a value from secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		Task<string?> LoadAsync(string key);

		/// <summary>
		/// Saves a value to secure storage (no encryption is applied)
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		Task SaveAsync(string key, string value);

		/// <summary>
		/// Encrypts and saves a value to secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		Task SaveEncryptedAsync(string key, string value);

		/// <summary>
		/// Removes a specific record from secure storage
		/// </summary>
		/// <param name="key">The key to wipe</param>
		Task RemoveAsync(string key);
	}
}