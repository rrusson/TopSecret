using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;

using TopSecret.Core.Interfaces;

namespace TopSecret.Platforms.Android
{
	public partial class AndroidKeyboardHelper : IKeyboardHelper
	{
		public void HideKeyboard()
		{
			Context context = Platform.AppContext;

			if (context.GetSystemService(Context.InputMethodService) is InputMethodManager inputMethodManager)
			{
				Activity? activity = Platform.CurrentActivity;
				IBinder? token = activity?.CurrentFocus?.WindowToken;

				inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
				activity?.Window?.DecorView.ClearFocus();
			}
		}
	}
}
