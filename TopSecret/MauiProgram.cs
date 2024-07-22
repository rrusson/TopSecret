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

			//Add a timer that automatically kills the app if it's been idle too long
			builder.Services.AddSingleton<KillTimer>();
			return builder.Build();
		}
	}
}
