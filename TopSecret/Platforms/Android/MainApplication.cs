using Android.App;
using Android.Runtime;

using TopSecret.Platforms.Android;

namespace TopSecret
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
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
