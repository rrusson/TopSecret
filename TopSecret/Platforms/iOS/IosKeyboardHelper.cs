using TopSecret.Helpers;

using UIKit;

namespace TopSecret
{
	public partial class IosKeyboardHelper : IKeyboardHelper
	{
		public void HideKeyboard()
		{
			UIApplication.SharedApplication.Windows.FirstOrDefault(XamlCompilationAttribute => XamlCompilationAttribute.IsKeyWindow)?.EndEditing(true);

			// Is iOS version 13 or higher?
			if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
			{
				var windowScene = UIApplication.SharedApplication.ConnectedScenes
					.OfType<UIWindowScene>()
					.FirstOrDefault(scene => scene.ActivationState == UISceneActivationState.ForegroundActive);

				windowScene?.Windows.FirstOrDefault(window => window.IsKeyWindow)?.EndEditing(true);
				return;
			}

			// How about 11 or higher?
			if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
			{
				_ = (UIApplication.SharedApplication.Windows.FirstOrDefault(window => window.IsKeyWindow)?.EndEditing(true));
				return;
			}

			UIApplication.SharedApplication.KeyWindow?.EndEditing(true);
		}
	}
}
