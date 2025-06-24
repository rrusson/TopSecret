namespace TopSecret.Helpers
{
	internal class LoginHelper
	{
		internal async Task<bool> IsPasswordRightAsync(string allegedPw)
		{
			if (string.IsNullOrEmpty(allegedPw))
			{
				return false;
			}

			var masterPw = await PasswordManager.Instance.GetMasterPasswordAsync().ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(masterPw))
			{
				// If there's no master password, this is first use, so set the master password
				await PasswordManager.Instance.ChangeMasterPasswordAsync(allegedPw).ConfigureAwait(false);
				await SecureStorage.Default.SetAsync("badAttempts", "0").ConfigureAwait(false);
				return true;
			}

			// Compare encrypted alleged password to the stored master password (which was also encrypted when saved)			
			var crypto = new CryptoHelper(allegedPw);
			var encryptedAlleged = crypto.Encrypt(allegedPw);

			if (encryptedAlleged.Equals(masterPw, StringComparison.InvariantCulture))
			{
				// We have a match, set the master password for the app to use for all encryption/decryption
				App.SetMasterPassword(masterPw);
				await SecureStorage.Default.SetAsync("badAttempts", "0").ConfigureAwait(false);
				return true;
			}

			await HandleFailedAttempt().ConfigureAwait(false);
			return false;
		}


		/// <summary>
		/// Checks number of failed attempts and permanently erases EVERYTHING if too many
		/// </summary>
		private static async Task HandleFailedAttempt()
		{
			string? badAttempts = await SecureStorage.Default.GetAsync(nameof(badAttempts)).ConfigureAwait(false) ?? "0";

			if (!int.TryParse(badAttempts, out int attempts) || attempts > 10)
			{
				// Too many failed attempts, erase the entire database (the Nuclear Option)
				SecureStorage.Default.RemoveAll();
				return;
			}

			await SecureStorage.Default.SetAsync(nameof(badAttempts), (++attempts).ToString()).ConfigureAwait(false);

			if (attempts > 5)
			{
				// Beyond 5 failed attempts, punish the user as a warning
				Application.Current?.Quit();
			}
		}
	}
}
