using Microsoft.Extensions.DependencyInjection;
using TopSecret.Core;
using TopSecret.Core.Interfaces;
using TopSecret.Helpers;

namespace TopSecret;

public partial class BasePage : ContentPage
{
	public BasePage()
	{
		InitializeComponent();
		// Reset the kill timer when any page is loaded
		var killTimer = Application.Current?.Handler?.MauiContext?.Services?.GetService<IKillTimer>();
		killTimer?.Reset();
	}
}