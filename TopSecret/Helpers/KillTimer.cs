using System.Timers;

using TopSecret.Core.Interfaces;

namespace TopSecret.Helpers
{
	/// <summary>
	/// Class that kills the app if it's been idle too long
	/// </summary>
	public partial class KillTimer : IKillTimer
	{
		private System.Timers.Timer? _timer;
		private const double _minutesToTimeout = 3;
		private bool _disposed = false;

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
			_timer.Start();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private static void TimerElapsed(object? sender, ElapsedEventArgs? e)
		{
			// Close the app
			Application.Current?.Dispatcher.DispatchAsync(() => Application.Current.Quit());
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_timer?.Dispose();
					_timer = null;
				}
				_disposed = true;
			}
		}

		~KillTimer()
		{
			Dispose(false);
		}
	}
}
