using AsyncAwaitBestPractices;

using TopSecret.Helpers;

namespace TopSecret;

public partial class BigListPage : ContentPage
{
	public List<AccountRecord> Records => [.. PasswordManager.Instance.Records.OrderBy(r => r.AccountName)];

	private bool _isMasterPasswordVisible;

	public bool IsMasterPasswordVisible
	{
		get => _isMasterPasswordVisible;
		set
		{
			_isMasterPasswordVisible = value;
			OnPropertyChanged(nameof(IsMasterPasswordVisible));
		}
	}

	public BigListPage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		KillTimer.Instance.Reset();

		await RefreshDataAsync().ConfigureAwait(true);
	}

	private async Task RefreshDataAsync()
	{
		BindingContext = null; // Clear the binding context

		if (Records.Count == 0)
		{
			await PasswordManager.Instance.PopulateRecords().ConfigureAwait(true);
		}

		await Task.Delay(50).ConfigureAwait(true); // Small delay to ensure UI is refreshed
		BindingContext = this; // Set the binding context again to force UI refresh
	}

	private void OnAddClicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new AccountEditor(new AccountRecord()));
	}

	private void OnItemTapped(object sender, ItemTappedEventArgs e)
	{
		if (e.Item is AccountRecord record)
		{
			Navigation.PushAsync(new AccountEditor(record));
		}
	}

	private void OnQuitClicked(object sender, EventArgs e)
	{
		Application.Current?.Quit();
	}

	/// <summary>
	/// Resets the Master Password and navigates to the login page for new password entry
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void OnUpdateMasterClicked(object sender, EventArgs e)
	{
		MasterPw.Text = string.Empty;
		ToggleMasterPasswordVisibility(true);
	}

	private void MasterPwChangeButtonClick(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(MasterPw.Text))
		{
			return;
		}

		UpdateMasterPassword().SafeFireAndForget();
	}

	private async Task UpdateMasterPassword()
	{
		App.SetMasterPassword(password: null);
		new StorageHelper().Remove("MasterPassword");

		await PasswordManager.Instance.ChangeMasterPasswordAsync(MasterPw.Text).ConfigureAwait(true);
		ToggleMasterPasswordVisibility(false);
	}

	private void ToggleMasterPasswordVisibility(bool isVisible)
	{
		IsMasterPasswordVisible = !IsMasterPasswordVisible;
		PasswordLabel.IsVisible = isVisible;
		MasterPw.IsVisible = isVisible;
		MasterPwChangeButton.IsVisible = isVisible;
		btnUpdateMaster.IsEnabled = !isVisible;
	}
}

