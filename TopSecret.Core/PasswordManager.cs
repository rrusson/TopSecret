using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	/// <summary>
	/// Loads and saves master password list (core functionality of the app)
	/// </summary>
	public class PasswordManager : IPasswordManager
	{
		// Constants for storage key naming consistency
		private const string AccountDataKey = "AccountData";
		private const string MasterPasswordKey = "MasterPassword";
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
			return await _storageHelper.LoadAsync(MasterPasswordKey).ConfigureAwait(false);
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
				await Task.Delay(50).ConfigureAwait(false);
			}

			await _storageHelper.SaveEncryptedAsync(AccountDataKey, serialized).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		public async Task ChangeMasterPasswordAsync(string newPassword)
		{
			if (string.IsNullOrWhiteSpace(newPassword))
			{
				return;
			}

			// Encrypt the new master password
			var crypto = _cryptoHelperFactory.CreateCryptoHelper(newPassword);
			string encrypted = crypto.Encrypt(newPassword);

			// Wipe and recreate the master password in secure storage
			await _storageHelper.RemoveAsync(MasterPasswordKey).ConfigureAwait(false);
			await _storageHelper.SaveAsync(MasterPasswordKey, encrypted).ConfigureAwait(false);

			// Resave all records with new (encrypted) master password
			await SaveAllRecordsAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Gets all decrypted records
		/// </summary>
		public async Task PopulateRecordsAsync()
		{
			string? serializedData = await _storageHelper.LoadAsync(AccountDataKey).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(serializedData))
			{
				return;
			}

			Records = _dataHelper.DeserializeAccountRecords(serializedData).ToList();
		}
	}
}