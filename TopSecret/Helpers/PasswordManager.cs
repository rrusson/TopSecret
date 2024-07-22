namespace TopSecret.Helpers
{
	/// <summary>
	/// Loads and saves master password list (core functionality of the app)
	/// </summary>
	internal class PasswordManager
	{
		private const string _accountDataKey = "AccountData";
		private const string _masterPasswordKey = "MasterPassword";

		private static PasswordManager? _instance;


		/// <summary>
		/// The collection of all account records
		/// </summary>
		public List<AccountRecord> Records { get; private set; } = [];

		/// <summary>
		/// Singleton for the app's core functionality
		/// </summary>
		public static PasswordManager Instance
		{
			get
			{
				_instance ??= new PasswordManager();
				return _instance;
			}
		}

		private PasswordManager()
		{
			// Private CTOR for singleton
			//Task.Run(() => PopulateRecords());
		}

		/// <summary>
		/// Returns the (encrypted) master password or null if it hasn't been set
		/// </summary>
		internal async Task<string?> GetMasterPasswordAsync()
		{
			return await SecureStorage.Default.GetAsync(_masterPasswordKey).ConfigureAwait(true);
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

			var match = Records.FirstOrDefault(r => r.Id == record.Id);

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
			await storage.SaveAsync(_accountDataKey, serialized).ConfigureAwait(false);
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

			// Set the app master password that'll be used to encrypt all records
			App.SetMasterPassword(newPassword);
			var storage = new StorageHelper();
			await storage.SaveAsync(_masterPasswordKey, newPassword).ConfigureAwait(false);

			// Encrypt the master password for encrypting everything else
			var crypto = new CryptoHelper(newPassword);
			string encrypted = crypto.Encrypt(newPassword);
			App.SetMasterPassword(encrypted);

			// Resave all records with new (encrypted) master password
			await SaveAllRecordsAsync().ConfigureAwait(true);
		}

		///// <summary>
		///// Gets all decrypted records
		///// </summary>
		//internal async Task PopulateRecordsX()
		//{
		//	var data = await DataHelper.GetAllRecordsAsync().ConfigureAwait(true);
		//	Records = data.ToList();
		//}

		/// <summary>
		/// Gets all decrypted records
		/// </summary>
		internal async Task PopulateRecords()
		{
			var storage = new StorageHelper();
			string? serializedGarbage = await storage.LoadAsync(_accountDataKey).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(serializedGarbage))
			{
				return;
			}

			Records = DataHelper.DeserializeAccountRecords(serializedGarbage).ToList();
		}
	}
}