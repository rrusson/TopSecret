
namespace TopSecret.Helpers
{
	/// <summary>
	/// Class to handle secure storage operations with encryption and decryption
	/// </summary>
	public class StorageHelper : IStorageHelper
	{
		private int _isBusy;
		private readonly ICryptoHelperFactory _cryptoHelperFactory;

		public StorageHelper(ICryptoHelperFactory cryptoHelperFactory)
		{
			_cryptoHelperFactory = cryptoHelperFactory;
		}

		/// <summary>
		/// Thread-safe property that indicates saving/removing items is in progress
		/// </summary>
		public bool IsBusy
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

		/// <inheritdoc/>
		public async Task<string?> LoadAsync(string key)
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

			var crypto = _cryptoHelperFactory.CreateCryptoHelper(App.MasterPassword);
			try
			{
				return crypto.Decrypt(value);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <inheritdoc/>
		public async Task SaveAsync(string key, string value)
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

		/// <inheritdoc/>
		public async Task SaveEncryptedAsync(string key, string value)
		{
			var crypto = _cryptoHelperFactory.CreateCryptoHelper(App.MasterPassword);
			string encrypted = crypto.Encrypt(value);

			await SaveAsync(key, encrypted).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async Task RemoveAsync(string key)
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
