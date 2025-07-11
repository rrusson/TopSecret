namespace TopSecret.Core.Interfaces
{
	public interface IDataHelper
	{
		/// <summary>
		/// Deserializes <paramref name="acctData"/> into a collection of Account Records
		/// </summary>
		/// <param name="acctData">A string of data to parse into Account Records</param>
		/// <returns>Deserialized Account Records</returns>
		IEnumerable<AccountRecord> DeserializeAccountRecords(string? acctData);

		/// <summary>
		/// Serializes a collection of accounts to text to prepare it for encryption
		/// </summary>
		/// <param name="records">A list of AccountRecords</param>
		/// <returns>Serialized records separated with tabs and return characters</returns>
		string SerializeAccountRecords(IEnumerable<AccountRecord> records);
	}
}