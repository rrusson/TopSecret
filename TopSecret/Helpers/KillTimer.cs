using System.Timers;

namespace TopSecret.Helpers
{
	/// <summary>
	/// Class that kills the app if it's been idle too long
	/// </summary>
	public class KillTimer : IKillTimer
	{
		private System.Timers.Timer? _timer;
		private const double _minutesToTimeout = 3;

		public KillTimer()
		{
			Reset();
		}

		/// <inheritdoc/>
		public void Reset()
		{
			_timer?.Stop();
			_timer?.Dispose();

			_timer = new System.Timers.Timer(TimeSpan.FromMinutes(_minutesToTimeout).TotalMilliseconds);
			_timer.Elapsed += TimerElapsed;
			_timer.AutoReset = false;
			_timer.Start();
		}

		private void TimerElapsed(object? sender, ElapsedEventArgs? e)
		{
			// Close the app
			Application.Current?.Dispatcher.DispatchAsync(() =>
			{
				Application.Current.Quit();
			});
		}

		//Implement IDisposable
		public void Dispose()
		{
			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}
	}
}
