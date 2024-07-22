namespace TopSecret.Helpers
{
	internal class StorageHelper
	{
		internal bool IsBusy { get; set; }

		/// <summary>
		/// Decrypts a value from secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		internal async Task<string?> LoadAsync(string key)
		{
			if (IsBusy)
			{
				return null;
			}

			string value = await SecureStorage.Default.GetAsync(key).ConfigureAwait(false) ?? string.Empty;

			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			var crypto = new CryptoHelper(App.MasterPassword);
			try
			{
				string decrypted = crypto.Decrypt(value);
				return decrypted;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to decrypt value: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Encrypts and saves a value to secure storage
		/// </summary>
		/// <param name="key">Key for the record</param>
		/// <param name="value">Value to store</param>
		internal async Task SaveAsync(string key, string value)
		{
			if (IsBusy)
			{
				return;
			}

			var crypto = new CryptoHelper(App.MasterPassword);
			string encrypted = crypto.Encrypt(value);

			IsBusy = true;  // Lock the app while saving
			await SecureStorage.Default.SetAsync(key, encrypted).ConfigureAwait(false);
			IsBusy = false;
		}

		/// <summary>
		/// Removes a specific record from secure storage
		/// </summary>
		/// <param name="key">The key to wipe</param>
		internal void Remove(string key)
		{
			if (IsBusy)
			{
				return;
			}

			IsBusy = true;
			SecureStorage.Default.Remove(key);
			IsBusy = false;
		}
	}
}
