using Microsoft.Extensions.DependencyInjection;
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

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			
			// Set up the initial page using DI once the handler is available
			if (Handler?.MauiContext?.Services != null && MainPage is AppShell shell)
			{
				var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
				if (loginPage != null)
				{
					shell.CurrentItem = new ShellContent
					{
						Title = "[TOP SECRET]",
						Content = loginPage,
						Route = "LoginPage"
					};
				}
			}
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
			var killTimer = Handler?.MauiContext?.Services?.GetService<IKillTimer>();
			killTimer?.Reset();
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
