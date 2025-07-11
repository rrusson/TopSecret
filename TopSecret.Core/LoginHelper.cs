using TopSecret.Core.Interfaces;

namespace TopSecret.Core
{
	public class LoginHelper : ILoginHelper
	{
		private readonly IPasswordManager _passwordManager;
		private readonly ICryptoHelperFactory _cryptoHelperFactory;

		public LoginHelper(IPasswordManager passwordManager, ICryptoHelperFactory cryptoHelperFactory)
		{
			_passwordManager = passwordManager;
			_cryptoHelperFactory = cryptoHelperFactory;
		}

		/// <inheritdoc/>
		public async Task<bool> IsPasswordRightAsync(string allegedPw)
		{
			if (string.IsNullOrEmpty(allegedPw))
			{
				return false;
			}

			var masterPw = await _passwordManager.GetMasterPasswordAsync().ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(masterPw))
			{
				// If there's no master password, this is first use, so set the master password
				await _passwordManager.ChangeMasterPasswordAsync(allegedPw).ConfigureAwait(false);
				return true;
			}

			// Compare encrypted alleged password to the stored master password (which was also encrypted when saved)			
			var crypto = _cryptoHelperFactory.CreateCryptoHelper(allegedPw);
			var encryptedAlleged = crypto.Encrypt(allegedPw);

			return encryptedAlleged.Equals(masterPw, StringComparison.InvariantCulture);
		}
	}
}