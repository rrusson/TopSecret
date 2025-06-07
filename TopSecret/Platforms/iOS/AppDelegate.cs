using Foundation;

namespace TopSecret.Platforms.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp()
		{
			DependencyService.Register<IosKeyboardHelper>();

			return MauiProgram.CreateMauiApp();
		}
	}
}
