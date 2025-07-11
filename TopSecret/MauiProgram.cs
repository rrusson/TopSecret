using Microsoft.Extensions.Logging;

using TopSecret.Helpers;

namespace TopSecret
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
					fonts.AddFont("fa_solid.ttf", "FontAwesome");
				});

			// Register services for dependency injection
			builder.Services.AddSingleton<ICryptoHelperFactory, CryptoHelperFactory>();
			builder.Services.AddSingleton<IDataHelper, DataHelper>();
			builder.Services.AddSingleton<IStorageHelper, StorageHelper>();
			builder.Services.AddSingleton<IPasswordManager, PasswordManager>();
			builder.Services.AddSingleton<ILoginHelper, LoginHelper>();
			builder.Services.AddSingleton<IKillTimer, KillTimer>();

			// Register pages - using transient so we get new instances each time
			builder.Services.AddTransient<LoginPage>();
			builder.Services.AddTransient<BigListPage>();
			builder.Services.AddTransient<AccountEditor>();

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
