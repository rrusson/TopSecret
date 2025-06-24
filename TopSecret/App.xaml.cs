using TopSecret.Helpers;

namespace TopSecret
{
	public partial class App : Application
	{
		private static readonly object _passwordLock = new();
		private volatile static string? _masterPassword;

		internal static string? MasterPassword
		{
			get
			{
				lock (_passwordLock)
				{
					return _masterPassword;
				}
			}
			private set
			{
				lock (_passwordLock)
				{
					_masterPassword = value;
				}
			}
		}

		public App()
		{
			InitializeComponent();
			MainPage = new AppShell();
		}

		internal static void SetMasterPassword(string? password)
		{
			lock (_passwordLock)
			{
				_masterPassword = password;
			}
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
