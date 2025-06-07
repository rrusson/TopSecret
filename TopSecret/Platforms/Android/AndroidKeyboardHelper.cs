using Android.Content;
using Android.Views.InputMethods;

using TopSecret.Helpers;

namespace TopSecret.Platforms.Android
{
	public partial class AndroidKeyboardHelper : IKeyboardHelper
	{
		public void HideKeyboard()
		{
			var context = Platform.AppContext;
			var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;

			if (inputMethodManager != null)
			{
				var activity = Platform.CurrentActivity;
				var token = activity?.CurrentFocus?.WindowToken;

				inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
				activity?.Window?.DecorView.ClearFocus();
			}
		}
	}
}
