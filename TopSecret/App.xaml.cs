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

			// Global exception handlers
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

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

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			ShowException(e.ExceptionObject as Exception);
		}

		private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			ShowException(e.Exception);
			e.SetObserved();
		}

		private void ShowException(Exception? ex)
		{
			if (ex == null)
			{
				return;
			}

			// Use MainThread to ensure UI access
			Application.Current?.Dispatcher.Dispatch(() =>
			{
				Shell.Current?.DisplayAlert("Unhandled Exception", ex.Message, "OK");
			});
		}
	}
}
