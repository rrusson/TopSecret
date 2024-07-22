using System.Windows.Input;

using TopSecret.Helpers;

namespace TopSecret;

public partial class AccountEditor : BasePage
{
	public AccountRecord Record { get; set; }

	public ICommand CloneCommand { get; set; }

	public ICommand DeleteCommand { get; set; }

	public ICommand SaveCommand { get; set; }

	public ICommand ListCommand { get; set; }


	public AccountEditor(AccountRecord record) : base()
	{	
		Record = record;
		CloneCommand = new Command(async () => await CloneRecord());
		DeleteCommand = new Command(async () => await DeleteRecord());
		SaveCommand = new Command(async () => await SaveRecord());
		ListCommand = new Command(async () => await Navigation.PopAsync());
		BindingContext = this;		
	}

	private async Task CloneRecord()
	{
		if (Record == null)
		{
			return;
		}

		BindingContext = null; // Clear the binding context
		Record = new AccountRecord("NEW", Record.UserName, Record.Password, string.Empty);

		await Task.Delay(50).ConfigureAwait(true); // Small delay to ensure UI is refreshed
		BindingContext = this; // Set the binding context again to force UI refresh
	}

	private async Task DeleteRecord()
	{
		await DataHelper.DeleteRecord(record: Record).ConfigureAwait(true);
		
		await Navigation.PopAsync().ConfigureAwait(true);
	}

	private async Task<bool> SaveRecord()
	{
		if (Record is null
			|| string.IsNullOrWhiteSpace(Record.AccountName)
			|| string.IsNullOrWhiteSpace(Record.UserName)
			|| string.IsNullOrWhiteSpace(Record.Password))
		{
			await DisplayAlert("Error", "Account Name, User Name, and Password are required.", "OK").ConfigureAwait(true);
			return false;
		}

		if (await PasswordManager.Instance.UpdateRecord(Record).ConfigureAwait(true))
		{
			await Navigation.PopAsync().ConfigureAwait(true);
			return true;
		}

		return false;
	}
}
