using TopSecret.Core.Interfaces;

namespace TopSecret.Helpers
{
	/// <summary>
	/// MAUI-specific implementation of master password provider
	/// </summary>
	public class MasterPasswordProvider : IMasterPasswordProvider
	{
		public string? MasterPassword => App.MasterPassword;
	}
}