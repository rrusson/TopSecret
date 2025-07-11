using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	/// <summary>
	/// Base class for secure storage operations with encryption and decryption
	/// </summary>
	public abstract class StorageHelperBase : IStorageHelper
	{
		private int _isBusy;
		private readonly ICryptoHelperFactory _cryptoHelperFactory;
		private readonly IMasterPasswordProvider _masterPasswordProvider;

		protected StorageHelperBase(ICryptoHelperFactory cryptoHelperFactory, IMasterPasswordProvider masterPasswordProvider)
		{
			_cryptoHelperFactory = cryptoHelperFactory;
			_masterPasswordProvider = masterPasswordProvider;
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
			string? value = await GetSecureValueAsync(key).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(value))
			{
				IsBusy = false;
				return null;
			}

			var crypto = _cryptoHelperFactory.CreateCryptoHelper(_masterPasswordProvider.MasterPassword);
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
				await SetSecureValueAsync(key, value).ConfigureAwait(false);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <inheritdoc/>
		public async Task SaveEncryptedAsync(string key, string value)
		{
			var crypto = _cryptoHelperFactory.CreateCryptoHelper(_masterPasswordProvider.MasterPassword);
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
				await RemoveSecureValueAsync(key);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Platform-specific implementation to get a secure value
		/// </summary>
		protected abstract Task<string?> GetSecureValueAsync(string key);

		/// <summary>
		/// Platform-specific implementation to set a secure value
		/// </summary>
		protected abstract Task SetSecureValueAsync(string key, string value);

		/// <summary>
		/// Platform-specific implementation to remove a secure value
		/// </summary>
		protected abstract Task RemoveSecureValueAsync(string key);
	}
}