using TopSecret.Helpers;

namespace TopSecret
{
	public partial class App : Application
	{
		internal static string? MasterPassword { get; private set; }

		public App()
		{
			InitializeComponent();
			MainPage = new AppShell();
		}

		internal static void SetMasterPassword(string? password)
		{
			MasterPassword = password;
		}

		// Force quit app immediately if it sleeps
		protected override void OnSleep() => Quit();

		protected override void OnStart()
		{
			// Start timer here (automatically quits if user is idle too long)
			base.OnStart();
			KillTimer.Instance.Reset();
		}
	}
}
