using Foundation;

namespace TopSecret
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
