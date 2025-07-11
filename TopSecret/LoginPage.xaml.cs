using TopSecret.Core;
using TopSecret.Core.Interfaces;
using TopSecret.Helpers;

namespace TopSecret;

public partial class LoginPage : ContentPage
{
	private readonly ILoginHelper _loginHelper;
	private readonly IKillTimer _killTimer;

	public LoginPage(ILoginHelper loginHelper, IKillTimer killTimer)
	{
		_loginHelper = loginHelper;
		_killTimer = killTimer;
		InitializeComponent();
		Password.Focus();
	}

	/// <summary>
	/// Check password and navigate to main page if correct
	/// </summary>
	/// <remarks>Fires on button click</remarks>
	protected async void OpenSesame(object sender, EventArgs e)
	{
		btnLogin.Focus();

		bool isPasswordRight = await TestPasswordAsync().ConfigureAwait(true);

		if (!isPasswordRight)
		{
			Password.Text = "";
			ErrorMessage.Text = "Nope. (Data is wiped after 10 failures.)";
		}
	}

	private async Task<bool> TestPasswordAsync()
	{
		bool isPwRight = await _loginHelper.IsPasswordRightAsync(Password.Text).ConfigureAwait(true);

		if (!isPwRight)
		{
			return false;
		}

		ErrorMessage.Text = string.Empty;

		// Set the master password in the App for encryption/decryption operations
		App.SetMasterPassword(Password.Text);

		// Get the BigListPage from the service provider
		var services = Application.Current?.Handler?.MauiContext?.Services;
		if (services != null)
		{
			var bigListPage = services.GetService<BigListPage>();
			if (bigListPage != null)
			{
				await Navigation.PushAsync(bigListPage).ConfigureAwait(true);
			}
		}

		// Remove the login page from the Navigation stack so back button doesn't return to it
		Navigation.RemovePage(this);
		return true;
	}
}