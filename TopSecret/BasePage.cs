using TopSecret.Core.Interfaces;

namespace TopSecret;

/// <summary>
/// A base class for all pages in the application. Manages the kill timer.
/// </summary>
public partial class BasePage(IKillTimer killTimer) : ContentPage
{
	private readonly IKillTimer? _killTimer = killTimer;

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Reset the kill timer when any page is loaded
		_killTimer?.Reset();
	}

	protected override bool OnBackButtonPressed()
	{
		// Don't allow back navigation
		return true;
	}
}