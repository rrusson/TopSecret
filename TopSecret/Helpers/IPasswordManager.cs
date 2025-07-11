namespace TopSecret.Helpers
{
    public interface IPasswordManager
    {
        /// <summary>
        /// The collection of all account records
        /// </summary>
        List<AccountRecord> Records { get; }

        /// <summary>
        /// Returns the (encrypted) master password or null if it hasn't been set
        /// </summary>
        Task<string?> GetMasterPasswordAsync();

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
        Task<bool> DeleteRecord(int recordId);

        /// <summary>
        /// Updates the master password used to log in to the app and re-encrypts all records
        /// </summary>
        /// <param name="newPassword">New password to use</param>
        Task ChangeMasterPasswordAsync(string newPassword);

        /// <summary>
        /// Gets all decrypted records
        /// </summary>
        Task PopulateRecordsAsync();
    }
}