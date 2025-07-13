using TopSecret.Core;
using TopSecret.Core.Interfaces;

namespace TopSecret.Helpers
{
	/// <summary>
	/// MAUI-specific implementation of StorageHelper that uses SecureStorage
	/// </summary>
	public class StorageHelper : StorageHelperBase
	{
		public StorageHelper(ICryptoHelperFactory cryptoHelperFactory, IMasterPasswordProvider masterPasswordProvider)
			: base(cryptoHelperFactory, masterPasswordProvider)
		{
		}

		/// <inheritdoc/>
		protected override async Task<string?> GetSecureValueAsync(string key)
		{
			return await SecureStorage.Default.GetAsync(key).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		protected override async Task SetSecureValueAsync(string key, string value)
		{
			await SecureStorage.Default.SetAsync(key, value).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		protected override async Task RemoveSecureValueAsync(string key)
		{
			await Task.Run(() => SecureStorage.Default.Remove(key)).ConfigureAwait(false);
		}
	}
}
