using TopSecret.Core.Interfaces;

namespace TopSecret.Helpers
{
	public class CryptoHelperFactory : ICryptoHelperFactory
	{
		public ICryptoHelper CreateCryptoHelper(string? password)
		{
			return new CryptoHelper(password);
		}
	}
}