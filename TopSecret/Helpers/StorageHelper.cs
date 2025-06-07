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
			if (IsBusy)
			{
				return null;	// Prevent displaying stale information if we're saving/removing items
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
		/// Encrypts and saves a value to secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		internal async Task SaveEncryptedAsync(string key, string value)
		{
			if (IsBusy)
			{
				return; // Prevent possible data corruption if we're already busy saving/removing items
			}

			IsBusy = true;  // Lock the app while saving
			var crypto = new CryptoHelper(App.MasterPassword);
			string encrypted = crypto.Encrypt(value);

			try
			{
				await SecureStorage.Default.SetAsync(key, encrypted).ConfigureAwait(false);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Removes a specific record from secure storage
		/// </summary>
		/// <param name="key">The key to wipe</param>
		internal void Remove(string key)
		{
			if (IsBusy)
			{
				return; // Don't remove item if we're currently saving/removing items
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
