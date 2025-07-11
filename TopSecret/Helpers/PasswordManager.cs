﻿
namespace TopSecret.Helpers
{
	/// <summary>
	/// Loads and saves master password list (core functionality of the app)
	/// </summary>
	public class PasswordManager : IPasswordManager
	{
		// Constants for storage key naming consistency
		private const object? AccountData = null;
		private const object? MasterPassword = null;
		private readonly IDataHelper _dataHelper;
		private readonly IStorageHelper _storageHelper;
		private readonly ICryptoHelperFactory _cryptoHelperFactory;

		/// <summary>
		/// The collection of all account records
		/// </summary>
		public List<AccountRecord> Records { get; private set; } = [];

		public PasswordManager(IDataHelper dataHelper, IStorageHelper storageHelper, ICryptoHelperFactory cryptoHelperFactory)
		{
			_dataHelper = dataHelper;
			_storageHelper = storageHelper;
			_cryptoHelperFactory = cryptoHelperFactory;
		}

		/// <inheritdoc/>
		public async Task<string?> GetMasterPasswordAsync()
		{
			return await SecureStorage.Default.GetAsync(nameof(MasterPassword)).ConfigureAwait(true);
		}

		/// <inheritdoc/>
		public async Task<bool> UpdateRecord(AccountRecord record)
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

		/// <inheritdoc/>
		public async Task<bool> DeleteRecord(Guid? recordId)
		{
			ArgumentNullException.ThrowIfNull(recordId, nameof(recordId));

			var match = Records.FirstOrDefault(r => r.Id == recordId);

			if (match == null)
			{
				return false;
			}

			Records.Remove(match);
			await SaveAllRecordsAsync().ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Encrypts and saves all records
		/// </summary>
		private async Task SaveAllRecordsAsync()
		{
			string serialized = _dataHelper.SerializeAccountRecords(Records.Where(r => !string.IsNullOrWhiteSpace(r.AccountName)));

			if (string.IsNullOrEmpty(serialized))
			{
				return; // If we don't have records, don't bother storing encrypted emptiness
			}

			while (_storageHelper.IsBusy)
			{
				await Task.Delay(50).ConfigureAwait(true);
			}

			await _storageHelper.SaveEncryptedAsync(nameof(AccountData), serialized).ConfigureAwait(true);
		}

		/// <inheritdoc/>
		public async Task ChangeMasterPasswordAsync(string newPassword)
		{
			if (string.IsNullOrWhiteSpace(newPassword))
			{
				return;
			}

			string? currentMasterPassword = App.MasterPassword;

			// Encrypt the new master password
			var crypto = _cryptoHelperFactory.CreateCryptoHelper(newPassword);
			string encrypted = crypto.Encrypt(newPassword);

			// Wipe and recreate the master password in secure storage
			await _storageHelper.RemoveAsync("MasterPassword").ConfigureAwait(true);
			await _storageHelper.SaveAsync(nameof(MasterPassword), encrypted).ConfigureAwait(true);

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
				await _storageHelper.RemoveAsync("MasterPassword").ConfigureAwait(true);
				await _storageHelper.SaveAsync(nameof(MasterPassword), currentMasterPassword!).ConfigureAwait(true);
				App.SetMasterPassword(encrypted);
				throw new InvalidOperationException("Master password encryption failed. Please try again.");
			}
		}

		/// <summary>
		/// Gets all decrypted records
		/// </summary>
		public async Task PopulateRecordsAsync()
		{
			string? serializedGarbage = await _storageHelper.LoadAsync(nameof(AccountData)).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(serializedGarbage))
			{
				return;
			}

			Records = _dataHelper.DeserializeAccountRecords(serializedGarbage).ToList();
		}
	}
}
