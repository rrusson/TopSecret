using System.Security.Cryptography;

using AsyncAwaitBestPractices;

using TopSecret.Helpers;

namespace TopSecret;

public partial class BigListPage : ContentPage
{
	private bool _isLoading;
	private bool _isMasterPasswordVisible;
	private List<AccountRecord> _records = [];
	private readonly IKeyboardHelper _keyboardHelper;

	public bool IsLoading
	{
		get => _isLoading;
		set
		{
			_isLoading = value;
			OnPropertyChanged(nameof(IsLoading));
		}
	}

	public bool IsMasterPasswordVisible
	{
		get => _isMasterPasswordVisible;
		set
		{
			_isMasterPasswordVisible = value;
			OnPropertyChanged(nameof(IsMasterPasswordVisible));
		}
	}

	public List<AccountRecord> Records
	{
		get => _records;
		private set
		{
			_records = value;
			OnPropertyChanged(nameof(Records));
		}
	}

	public BigListPage()
	{
		InitializeComponent();
		_keyboardHelper = DependencyService.Get<IKeyboardHelper>();
		BindingContext = this;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		KillTimer.Instance.Reset();

		ToggleMasterPasswordVisibility(false);
		_keyboardHelper?.HideKeyboard();

		await RefreshDataAsync().ConfigureAwait(true);
	}

	private async Task RefreshDataAsync()
	{
		IsLoading = true;

		try
		{
			await PasswordManager.Instance.PopulateRecordsAsync().ConfigureAwait(true);
			Records = [.. PasswordManager.Instance.Records.OrderBy(r => r.AccountName)];
		}
		catch (Exception)
		{
			await DisplayAlert("ERROR", "Unable to load account data. Please try again or reinstall.", "OK").ConfigureAwait(true);
		}

		IsLoading = false;
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
		if (string.IsNullOrWhiteSpace(MasterPw.Text))
		{
			await DisplayAlert("ERROR", "Master Password can't be empty.", "OK").ConfigureAwait(true);
		}

		// Set the new app master password that's used by StorageHelper to encrypt/decrypt all records
		string? currentMasterPassword = App.MasterPassword;

		try
		{
			App.SetMasterPassword(password: null);
			await PasswordManager.Instance.ChangeMasterPasswordAsync(MasterPw.Text).ConfigureAwait(true);
			ToggleMasterPasswordVisibility(false);
			return;
		}
		catch (InvalidOperationException invEx)
		{
			await DisplayAlert("ERROR", invEx.Message, "OK").ConfigureAwait(true);
		}
		catch (CryptographicException cryptoEx)
		{
			await DisplayAlert("ERROR", cryptoEx.Message, "OK").ConfigureAwait(true);
		}
		catch (Exception)
		{
			await DisplayAlert("ERROR", "Oops. The encrypted data didn't save correctly. Please try again.", "OK").ConfigureAwait(true);
		}

		ToggleMasterPasswordVisibility(true);   // Show the password input in case of error so user can try again
		App.SetMasterPassword(currentMasterPassword); // Reset to previous master password on failure
	}

	private void ToggleMasterPasswordVisibility(bool isVisible)
	{
		IsMasterPasswordVisible = isVisible;
		PasswordLabel.IsVisible = isVisible;
		MasterPw.IsVisible = isVisible;
		MasterPwChangeButton.IsVisible = isVisible;
		btnUpdateMaster.IsEnabled = !isVisible;
	}
}
