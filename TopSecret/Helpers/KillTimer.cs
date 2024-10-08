﻿using System.Timers;
using System.Windows;

namespace TopSecret.Helpers
{
	/// <summary>
	/// Class that kills the app if it's been idle too long
	/// </summary>
	internal class KillTimer : IDisposable
    {
        private static KillTimer? _singletonTimer;

        private System.Timers.Timer? _timer;
        private const double _minutesToTimeout = 3;

        internal static DateTime LastTimeOfReset { get; set; }

        /// <summary>
        /// Singleton for the app's auto-timeout
        /// </summary>
        public static KillTimer Instance
        {
            get
            {
				//Create the singleton instance with default timeout of 3 minutes, if MinutesToTimeout wasn't pre-set
				_singletonTimer ??= new KillTimer();
				return _singletonTimer;
            }
        }

        private KillTimer()
        {
            Reset();
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Reset()
        {
            _timer?.Stop();
            _timer?.Dispose();
            
            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(_minutesToTimeout).TotalMilliseconds);
			_timer.Elapsed += TimerElapsed;
			_timer.AutoReset = false;
			_timer.Start();
			LastTimeOfReset = DateTime.Now;
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
