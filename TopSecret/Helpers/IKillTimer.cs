namespace TopSecret.Helpers
{
	public interface IKillTimer : IDisposable
	{
		/// <summary>
		/// Starts or resets the timer
		/// </summary>
		void Reset();
	}
}