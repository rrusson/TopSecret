
namespace TopSecret.Helpers
{
	/// <summary>
	/// Loads and saves master password list (core functionality of the app)
	/// </summary>
	internal class PasswordManager
	{
		// Constants for storage key naming consistency
		private const object? AccountData = null;
		private const object? MasterPassword = null;
		private static readonly Lazy<PasswordManager> _lazyInstance = new(() => new PasswordManager(), isThreadSafe: true);

		/// <summary>
		/// The collection of all account records
		/// </summary>
		public List<AccountRecord> Records { get; private set; } = [];

		/// <summary>
		/// Singleton for the app's core functionality
		/// </summary>
		public static PasswordManager Instance => _lazyInstance.Value;

		private PasswordManager()
		{
			// Private CTOR for singleton
		}

		/// <summary>
		/// Returns the (encrypted) master password or null if it hasn't been set
		/// </summary>
		internal async Task<string?> GetMasterPasswordAsync()
		{
			return await SecureStorage.Default.GetAsync(nameof(MasterPassword)).ConfigureAwait(true);
		}

		/// <summary>
		/// Updates or inserts a record into the account data
		/// </summary>
		/// <param name="record">Record</param>
		/// <returns>False if record is missing</returns>
		internal async Task<bool> UpdateRecord(AccountRecord record)
		{
			if (record?.AccountName == null)
			{
				return false;
			}

			var match = Records.Find(r => r.Id == record.Id);

			if (match != null)
			{
				int index = Records.IndexOf(match);
				Records[index] = record; // Update the existing record
			}
			else
			{
				Records.Add(record);
			}

			await SaveAllRecordsAsync().ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Encrypts and saves all records
		/// </summary>
		private async Task SaveAllRecordsAsync()
		{
			string serialized = DataHelper.SerializeAccountRecords(Records.Where(r => !string.IsNullOrWhiteSpace(r.AccountName)));

			if (string.IsNullOrEmpty(serialized))
			{
				return; // If we don't have records, don't bother storing encrypted emptiness
			}

			var storage = new StorageHelper();

			while (storage.IsBusy)
			{
				await Task.Delay(50).ConfigureAwait(true);
			}

			await storage.SaveEncryptedAsync(nameof(AccountData), serialized).ConfigureAwait(true);
		}

		/// <summary>
		/// Updates the master password used to log in to the app and re-encrypts all records
		/// </summary>
		/// <param name="newPassword">New password to use</param>
		internal async Task ChangeMasterPasswordAsync(string newPassword)
		{
			if (string.IsNullOrWhiteSpace(newPassword))
			{
				return;
			}

			string? currentMasterPassword = App.MasterPassword;

			// Encrypt the new master password
			var crypto = new CryptoHelper(newPassword);
			string encrypted = crypto.Encrypt(newPassword);

			// Wipe and recreate the master password in secure storage
			var storage = new StorageHelper();
			await storage.RemoveAsync("MasterPassword").ConfigureAwait(true);
			await storage.SaveAsync(nameof(MasterPassword), encrypted).ConfigureAwait(true);

			// Set the new app master password that's used by StorageHelper to encrypt/decrypt all records
			App.SetMasterPassword(encrypted);

			string? retrievedPassword = await GetMasterPasswordAsync().ConfigureAwait(true);

			// Make sure what we found in storage matches master password used for encrypting/decrypting 
			if (retrievedPassword != encrypted)
			{
				throw new InvalidOperationException("Master password encryption failed. Please try again.");
			}

			// Resave all records with new (encrypted) master password
			try
			{
				await SaveAllRecordsAsync().ConfigureAwait(true);
			}
			catch (Exception)
			{
				// Rollback MasterPassword to the previous value if saving records failed
				await storage.RemoveAsync("MasterPassword").ConfigureAwait(true);
				await storage.SaveAsync(nameof(MasterPassword), currentMasterPassword!).ConfigureAwait(true);
				App.SetMasterPassword(encrypted);
				throw new InvalidOperationException("Master password encryption failed. Please try again.");
			}
		}

		/// <summary>
		/// Gets all decrypted records
		/// </summary>
		internal async Task PopulateRecordsAsync()
		{
			var storage = new StorageHelper();
			string? serializedGarbage = await storage.LoadAsync(nameof(AccountData)).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(serializedGarbage))
			{
				return;
			}

			Records = DataHelper.DeserializeAccountRecords(serializedGarbage).ToList();
		}
	}
}
