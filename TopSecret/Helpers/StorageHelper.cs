using System.Runtime.CompilerServices;

namespace TopSecret.Helpers
{
	/// <summary>
	/// Class to handle secure storage operations with encryption and decryption
	/// </summary>
	internal class StorageHelper
	{
		private int _isBusy;

		/// <summary>
		/// Thread-safe property that indicates saving/removing items is in progress
		/// </summary>
		internal bool IsBusy
		{
			get
			{
				return Interlocked.CompareExchange(ref _isBusy, 1, 1) == 1;
			}
			private set
			{
				Interlocked.Exchange(ref _isBusy, value ? 1 : 0);
			}
		}

		/// <summary>
		/// Decrypts a value from secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		internal async Task<string?> LoadAsync(string key)
		{
			while (IsBusy)
			{
				await Task.Delay(50); // Wait, so we don't show stale information
			}

			IsBusy = true;	// Prevent saving until we finish loading current data
			string? value = await SecureStorage.Default.GetAsync(key).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(value))
			{
				IsBusy = false;
				return null;
			}

			var crypto = new CryptoHelper(App.MasterPassword);
			try
			{
				return crypto.Decrypt(value);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Saves a value to secure storage (no encryption is applied)
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		internal async Task SaveAsync(string key, string value)
		{
			while (IsBusy)
			{
				// Wait and keep trying. Saving must not fail.
				await Task.Delay(50);
			}

			IsBusy = true;  // Lock the app while saving

			try
			{
				await SecureStorage.Default.SetAsync(key, value).ConfigureAwait(false);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Encrypts and saves a value to secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		internal async Task SaveEncryptedAsync(string key, string value)
		{
			var crypto = new CryptoHelper(App.MasterPassword);
			string encrypted = crypto.Encrypt(value);

			await SaveAsync(key, encrypted).ConfigureAwait(false);
		}

		/// <summary>
		/// Removes a specific record from secure storage
		/// </summary>
		/// <param name="key">The key to wipe</param>
		internal async Task RemoveAsync(string key)
		{
			while (IsBusy)
			{
				await Task.Delay(50);
			}

			try
			{
				IsBusy = true;
				SecureStorage.Default.Remove(key);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
