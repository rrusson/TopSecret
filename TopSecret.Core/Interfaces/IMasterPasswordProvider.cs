namespace TopSecret.Core.Interfaces
{
	/// <summary>
	/// Interface for providing the master password to core components
	/// </summary>
	public interface IMasterPasswordProvider
	{
		/// <summary>
		/// Gets the current master password
		/// </summary>
		string? MasterPassword { get; }
	}
}