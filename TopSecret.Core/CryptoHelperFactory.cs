using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	public class CryptoHelperFactory : ICryptoHelperFactory
	{
		public ICryptoHelper CreateCryptoHelper(string? password)
		{
			return new CryptoHelper(password);
		}
	}
}