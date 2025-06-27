using System.Text;

namespace TopSecret.Helpers
{
	internal static class DataHelper
	{
		private const string _accountDataKey = "AccountData";

		/// <summary>
		/// Deserializes <paramref name="acctData"/> into a collection of Account Records
		/// </summary>
		/// <param name="acctData">A string of data to parse into Account Records</param>
		/// <returns>Deserialized Account Records</returns>
		internal static IEnumerable<AccountRecord> DeserializeAccountRecords(string? acctData)
		{
			if (string.IsNullOrEmpty(acctData))
			{
				yield break; // No Account Data file yet
			}

			string[] allData = acctData.Split((char)13);    // Parse out the individual account records

			for (int i = 0; i < allData.Length; i++)
			{
				if (allData[i].Length > 1)                  // This should protect against empty, terminating (char)13
				{
					yield return new AccountRecord(allData[i]);
				}
			}
		}

		/// <summary>
		/// Serializes a collection of accounts to text to prepare it for encryption
		/// </summary>
		/// <param name="records">A list of AccountRecords</param>
		/// <returns>Serialized records separated with tabs and return characters</returns>
		internal static string SerializeAccountRecords(IEnumerable<AccountRecord> records)
		{
			var builder = new StringBuilder(records.Count() * 100);

			foreach (var rec in records.Where(r => !string.IsNullOrWhiteSpace(r.AccountName)))
			{
				//Serialize with tabs and CRs
				builder.Append(rec.Id.ToString() + (char)9 + rec.AccountName + (char)9 + rec.UserName + (char)9 + rec.Password + (char)9 + rec.Url + (char)13);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Removes a record from the account data
		/// </summary>
		/// <param name="record">Record</param>
		/// <returns>False on failure</returns>
		internal static async Task<bool> DeleteRecord(AccountRecord? record)
		{
			if (record?.Id == null)
			{
				return false;
			}

			List<AccountRecord> allData = PasswordManager.Instance.Records;

			var match = allData.FirstOrDefault(r => r.Id == record.Id);

			if (match == null)
			{
				return false;
			}

			allData.Remove(match);
			string records = SerializeAccountRecords(allData);

			await new StorageHelper().SaveEncryptedAsync(key: _accountDataKey, value: records).ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Updates or inserts a record into the account data
		/// </summary>
		/// <param name="record">Record</param>
		/// <returns>False if record is missing</returns>
		internal static async Task<bool> UpdateRecord(AccountRecord? record)
		{
			if (record?.AccountName == null)
			{
				return false;
			}

			List<AccountRecord> allData = PasswordManager.Instance.Records;

			var match = allData.FirstOrDefault(r => r.Id == record.Id);

			if (match != null)
			{
				int index = allData.IndexOf(match);
				allData[index] = record; // Update the existing record
			}
			else
			{
				allData.Add(record);
			}

			string records = SerializeAccountRecords(allData);
			await new StorageHelper().SaveEncryptedAsync(key: _accountDataKey, value: records).ConfigureAwait(false);
			return true;
		}
	}
}
