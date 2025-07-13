namespace TopSecret.Core.Interfaces
{
	public interface IPasswordManager : IMasterPasswordProvider
	{
		/// <summary>
		/// The collection of all account records
		/// </summary>
		List<AccountRecord> Records { get; }

		/// <summary>
		/// Updates or inserts a record into the account data
		/// </summary>
		/// <param name="record">Record</param>
		/// <returns>False if record is missing</returns>
		Task<bool> UpdateRecord(AccountRecord record);

		/// <summary>
		/// Removes a record from the account data
		/// </summary>
		/// <param name="recordId">Record ID</param>
		/// <returns>False on failure</returns>
		Task<bool> DeleteRecord(Guid? recordId);

		/// <summary>
		/// Gets all decrypted records
		/// </summary>
		Task PopulateRecordsAsync();
	}
}