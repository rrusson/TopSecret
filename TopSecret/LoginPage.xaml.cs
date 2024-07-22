using TopSecret.Helpers;

namespace TopSecret;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		KillTimer.Instance.Reset();
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
			ErrorMessage.Text = "Nope.";
		}
	}

	private async Task<bool> TestPasswordAsync()
	{
		var loginHelper = new LoginHelper();
		bool isPwRight = await loginHelper.IsPasswordRightAsync(Password.Text).ConfigureAwait(true);

		if (!isPwRight)
		{
			return false;
		}

		ErrorMessage.Text = string.Empty;

		await Navigation.PushAsync(new BigListPage()).ConfigureAwait(true);

		// Remove the login page from the Navigation stack so back button doesn't return to it
		Navigation.RemovePage(this);
		return true;
	}
}