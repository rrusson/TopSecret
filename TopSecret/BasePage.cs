using TopSecret.Helpers;

namespace TopSecret;

public partial class BasePage : ContentPage
{
	public BasePage()
	{
		InitializeComponent();
		KillTimer.Instance.Reset();
	}
}