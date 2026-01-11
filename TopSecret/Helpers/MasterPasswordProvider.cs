using TopSecret.Core.Interfaces;

namespace TopSecret.Helpers
{
	/// <summary>
	/// MAUI-specific implementation of master password provider
	/// </summary>
	public class MasterPasswordProvider : IMasterPasswordProvider
	{
		private readonly ICryptoHelperFactory _cryptoHelperFactory;
		private const string MasterPasswordKey = "MasterPassword";

		public MasterPasswordProvider(ICryptoHelperFactory cryptoHelperFactory)
		{
			_cryptoHelperFactory = cryptoHelperFactory;
		}

		public string? MasterPassword => App.MasterPassword;

		/// <inheritdoc/>
		public async Task<string?> GetMasterPasswordAsync()
		{
			return await SecureStorage.Default.GetAsync(MasterPasswordKey).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public void SetMasterPassword(string encryptedPassword)
		{
			if (string.IsNullOrWhiteSpace(encryptedPassword))
			{
				return;
			}

			App.MasterPassword = encryptedPassword;
		}

		/// <inheritdoc/>
		public async Task ChangeMasterPasswordAsync(string newPassword)
		{
			if (string.IsNullOrWhiteSpace(newPassword))
			{
				return;
			}

			// Use the factory to create a crypto helper with the new password
			var cryptoHelper = _cryptoHelperFactory.CreateCryptoHelper(newPassword);
			string encrypted = cryptoHelper.Encrypt(newPassword);

			// Wipe and recreate the master password in secure storage
			SecureStorage.Default.Remove(MasterPasswordKey);
			await SecureStorage.Default.SetAsync(MasterPasswordKey, encrypted).ConfigureAwait(false);

			// Update the in-memory master password
			App.MasterPassword = encrypted;
		}
	}
}