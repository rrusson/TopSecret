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

		await PasswordManager.Instance.PopulateRecordsAsync().ConfigureAwait(true);
		Records = [.. PasswordManager.Instance.Records.OrderBy(r => r.AccountName)];

		//await Task.Delay(50).ConfigureAwait(true); // Small delay to ensure UI is refreshed
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
		ToggleMasterPasswordVisibility(false);

		App.SetMasterPassword(password: null);
		await new StorageHelper().RemoveAsync("MasterPassword").ConfigureAwait(true);

		await PasswordManager.Instance.ChangeMasterPasswordAsync(MasterPw.Text).ConfigureAwait(true);
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
