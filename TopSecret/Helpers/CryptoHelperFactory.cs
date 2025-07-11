namespace TopSecret.Helpers
{
    public class CryptoHelperFactory : ICryptoHelperFactory
    {
        /// <inheritdoc/>
        public ICryptoHelper CreateCryptoHelper(string? password)
        {
            return new CryptoHelper(password);
        }
    }
}