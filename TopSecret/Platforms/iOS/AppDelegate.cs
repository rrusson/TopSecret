using Foundation;

using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace TopSecret.Platforms.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
