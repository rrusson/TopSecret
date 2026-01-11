using UIKit;

using TopSecret.Core.Interfaces;

namespace TopSecret.Platforms.iOS
{
	public partial class IosKeyboardHelper : IKeyboardHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Checked before use")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "Checked before use")]
		public void HideKeyboard()
		{
			// Validate platform compatibility and try the most compatible approach first
			if (UIApplication.SharedApplication.KeyWindow != null)
			{
				UIApplication.SharedApplication.KeyWindow.EndEditing(true);
				return;
			}

			// Try iOS 15+ approach
			if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
			{
				var scenes = UIApplication.SharedApplication.ConnectedScenes;
				foreach (var scene in scenes)
				{
					if (scene is UIWindowScene windowScene)
					{
						foreach (var window in windowScene.Windows)
						{
							if (window.IsKeyWindow)
							{
								window.EndEditing(true);
								return;
							}
						}
					}
				}
			}
			// iOS 11+ approach
			else if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
			{
				var keyWindow = UIApplication.SharedApplication.Windows.FirstOrDefault(window => window.IsKeyWindow);
				if (keyWindow != null)
				{
					keyWindow.EndEditing(true);
					return;
				}
			}

			// Fallback approach
			UIApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("resignFirstResponder"), null, null, null);
		}
	}
}
