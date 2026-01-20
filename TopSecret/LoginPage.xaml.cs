using TopSecret.Core.Interfaces;

namespace TopSecret;

public partial class LoginPage : ContentPage
{
	private readonly ILoginHelper _loginHelper;

	public LoginPage(ILoginHelper loginHelper)
	{
		_loginHelper = loginHelper;

		InitializeComponent();
		Password.Focus();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (!string.IsNullOrEmpty(App.MasterPassword))
		{
			await ForwardToList().ConfigureAwait(true);
		}
	}

	/// <summary>
	/// Check password and navigate to main page if correct
	/// </summary>
	/// <remarks>Fires on button click</remarks>
	protected async void OpenSesame(object sender, EventArgs e)
	{
		btnLogin.Focus();

		bool isPasswordRight = await TestPasswordAsync().ConfigureAwait(true);

		if (isPasswordRight)
		{
			await SecureStorage.Default.SetAsync("badAttempts", "0").ConfigureAwait(false);
			return;
		}

		Password.Text = "";
		ErrorMessage.Text = "Nope. (Data is wiped after 10 failures.)";
		await HandleFailedAttemptAsync().ConfigureAwait(true);
	}

	private async Task<bool> TestPasswordAsync()
	{
		bool isPwRight = await _loginHelper.IsPasswordRightAsync(Password.Text).ConfigureAwait(true);

		if (!isPwRight)
		{
			return false;
		}

		ErrorMessage.Text = string.Empty;

		// Forward user to the BigListPage
		await ForwardToList().ConfigureAwait(true);

		return true;
	}

	private async Task ForwardToList()
	{
		// Get the BigListPage from the service provider and forward to it
		var services = Application.Current?.Handler?.MauiContext?.Services;

		if (services != null)
		{
			var bigListPage = services.GetService<BigListPage>();
			if (bigListPage != null)
			{
				// Remove the login page from the Navigation stack so back button doesn't return to it
				Navigation.RemovePage(this);
				await Navigation.PushAsync(bigListPage).ConfigureAwait(true);
			}
		}
	}

	/// <summary>
	/// Checks number of failed attempts and permanently erases EVERYTHING if too many
	/// </summary>
	private async Task HandleFailedAttemptAsync()
	{
		string? badAttempts = await SecureStorage.Default.GetAsync(nameof(badAttempts)).ConfigureAwait(true) ?? "0";

		if (!int.TryParse(badAttempts, out int attempts) || attempts > 9)
		{
			ErrorMessage.Text = "GAME OVER. All data removed.";
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
