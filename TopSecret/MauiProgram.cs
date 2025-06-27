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

#if DEBUG
			builder.Logging.AddDebug();
#endif

			// Consider removing singleton pattern from KillTimer and PasswordManager; updating to use Dependency Injection instead
			//builder.Services.AddSingleton<KillTimer>();
			return builder.Build();
		}
	}
}
