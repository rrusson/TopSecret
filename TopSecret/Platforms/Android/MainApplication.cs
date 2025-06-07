using Android.App;
using Android.Runtime;

namespace TopSecret.Platforms.Android
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(nint handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		protected override MauiApp CreateMauiApp()
		{
			DependencyService.Register<AndroidKeyboardHelper>();
			return MauiProgram.CreateMauiApp();
		}
	}
}
