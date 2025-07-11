using System.ComponentModel;
using System.Windows.Input;

using TopSecret.Helpers;

namespace TopSecret;

public partial class AccountEditor : BasePage
{
	private bool _isExistingRecord;
	private AccountRecord? _record;
	private readonly IPasswordManager _passwordManager;
	private readonly IDataHelper _dataHelper;

	public bool IsExistingRecord
	{
		get => _isExistingRecord;
		set
		{
			if (_isExistingRecord != value)
			{
				_isExistingRecord = value;
				OnPropertyChanged(nameof(IsExistingRecord));
			}
		}
	}

	public AccountRecord? Record
	{
		get => _record;
		set
		{
			_record = value;
			OnPropertyChanged(nameof(Record));
		}
	}

	public ICommand CloneCommand { get; set; }

	public ICommand DeleteCommand { get; set; }

	public ICommand ListCommand { get; set; }

	public ICommand SaveCommand { get; set; }

	public AccountEditor(IPasswordManager passwordManager, IDataHelper dataHelper) : base()
	{
		_passwordManager = passwordManager;
		_dataHelper = dataHelper;
		
		CloneCommand = new Command(async () => await CloneRecord());
		DeleteCommand = new Command(async () => await DeleteRecord());
		SaveCommand = new Command(async () => await SaveRecord());
		ListCommand = new Command(async () => await Navigation.PopAsync());
		BindingContext = this;
	}

	public void SetRecord(AccountRecord record)
	{
		Record = record;
		IsExistingRecord = !string.IsNullOrEmpty(record.AccountName);
	}

	private async Task CloneRecord()
	{
		if (Record == null)
		{
			return;
		}

		BindingContext = null; // Clear the binding context
		Record = new AccountRecord("NEW", Record.UserName, Record.Password, string.Empty);
		
		// Set IsExistingRecord to false for the new cloned record
		IsExistingRecord = false;

		await Task.Delay(50).ConfigureAwait(true); // Small delay to ensure UI is refreshed
		BindingContext = this; // Set the binding context again to force UI refresh
	}

	private async Task DeleteRecord()
	{
		try
		{
			_ = await _passwordManager.DeleteRecord(record: Record).ConfigureAwait(true);
			await Navigation.PopAsync().ConfigureAwait(true);
		}
		catch (Exception ex)
		{
			await DisplayAlert("ERROR", ex.Message, "OK").ConfigureAwait(true);
		}
	}

	private async Task SaveRecord()
	{
		if (Record is null
			|| string.IsNullOrWhiteSpace(Record.AccountName)
			|| string.IsNullOrWhiteSpace(Record.UserName)
			|| string.IsNullOrWhiteSpace(Record.Password))
		{
			await DisplayAlert("ERROR", "Account Name, User Name, and Password are required.", "OK").ConfigureAwait(true);
			return;
		}

		Record.AccountName = SanitizeInput(Record.AccountName);
		Record.UserName = SanitizeInput(Record.UserName);
		Record.Password = SanitizeInput(Record.Password);

		try
		{
			if (await _passwordManager.UpdateRecord(Record).ConfigureAwait(true))
			{
				IsExistingRecord = true;
				await Navigation.PopAsync().ConfigureAwait(true);
				return;
			}

			await DisplayAlert("ERROR", "Record not saved, please try again.", "OK").ConfigureAwait(true);
		}
		catch (Exception ex)
		{
			await DisplayAlert("ERROR", ex.Message, "OK").ConfigureAwait(true);
		}
	}

	private string SanitizeInput(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}

		// Remove any non-printable characters that would only cause us grief
		return new string([.. input.Where(c => !char.IsControl(c))]);
	}
}
