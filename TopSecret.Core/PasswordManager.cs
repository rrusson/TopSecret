using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	/// <summary>
	/// Loads and saves passwords including master password (core functionality of the app)
	/// </summary>
	public class PasswordManager : IPasswordManager
	{
		// Constant for storage key naming consistency
		private const string AccountDataKey = "AccountData";
		private readonly IDataHelper _dataHelper;
		private readonly IMasterPasswordProvider _masterPasswordProvider;
		private readonly IStorageHelper _storageHelper;

		/// <inheritdoc/>
		public List<AccountRecord> Records { get; private set; } = [];

		public PasswordManager(IDataHelper dataHelper, IMasterPasswordProvider masterPasswordProvider, IStorageHelper storageHelper)
		{
			_dataHelper = dataHelper;
			_masterPasswordProvider = masterPasswordProvider;
			_storageHelper = storageHelper;
		}

		// IMasterPasswordProvider forwarding methods (to ease use of tightly coupled MAUI app code in MasterPasswordProvider)

		/// <inheritdoc/>
		public string? MasterPassword => _masterPasswordProvider.MasterPassword;

		/// <inheritdoc/>
		public Task<string?> GetMasterPasswordAsync() => _masterPasswordProvider.GetMasterPasswordAsync();

		/// <inheritdoc/>
		public void SetMasterPassword(string encryptedPassword) => _masterPasswordProvider.SetMasterPassword(encryptedPassword);

		public async Task ChangeMasterPasswordAsync(string newPassword)
		{
			string? oldMasterPassword = _masterPasswordProvider.MasterPassword;
			await _masterPasswordProvider.ChangeMasterPasswordAsync(newPassword).ConfigureAwait(true);

			try
			{
				// Resave all records with new (encrypted) master password
				await SaveAllRecordsAsync().ConfigureAwait(true);
			}
			catch (Exception)
			{
				// Rollback MasterPassword to the previous value if saving records failed
				_masterPasswordProvider.SetMasterPassword(oldMasterPassword!);
				throw new InvalidOperationException("Master password encryption failed. Please try again.");
			}
		}

		/// <inheritdoc/>
		public async Task PopulateRecordsAsync()
		{
			string? serializedData = await _storageHelper.LoadAsync(AccountDataKey).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(serializedData))
			{
				return;
			}

			Records = [.. _dataHelper.DeserializeAccountRecords(serializedData)];
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
	}
}