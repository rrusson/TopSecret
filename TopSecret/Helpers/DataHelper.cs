using System.Text;

namespace TopSecret.Helpers
{
	public class DataHelper : IDataHelper
	{
		/// <summary>
		/// Deserializes <paramref name="acctData"/> into a collection of Account Records
		/// </summary>
		/// <param name="acctData">A string of data to parse into Account Records</param>
		/// <returns>Deserialized Account Records</returns>
		public IEnumerable<AccountRecord> DeserializeAccountRecords(string? acctData)
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
		public string SerializeAccountRecords(IEnumerable<AccountRecord> records)
		{
			var builder = new StringBuilder(records.Count() * 100);

			foreach (var rec in records.Where(r => !string.IsNullOrWhiteSpace(r.AccountName)))
			{
				//Serialize with tabs and CRs
				builder.Append(rec.Id.ToString() + (char)9 + rec.AccountName + (char)9 + rec.UserName + (char)9 + rec.Password + (char)9 + rec.Url + (char)13);
			}

			return builder.ToString();
		}
	}
}
