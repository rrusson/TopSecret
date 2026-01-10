using TopSecret.Core.Interfaces;

namespace TopSecret.Core.Helpers
{
	public class CryptoHelperFactory : ICryptoHelperFactory
	{
		public ICryptoHelper CreateCryptoHelper(string? password)
		{
			return new CryptoHelper(password);
		}
	}
}