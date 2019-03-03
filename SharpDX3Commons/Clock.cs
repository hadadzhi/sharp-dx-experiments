using System;
using System.Diagnostics;

namespace SharpDXCommons
{
	public class Clock
	{
		private Stopwatch Stopwatch;

		private long StartTicks = 0;
		private long LastTicks = 0;

		private double Frequency;

		private bool Running = false;

		public Clock()
		{
			Stopwatch = new Stopwatch();
			Frequency = Stopwatch.Frequency;

			if (!Stopwatch.IsHighResolution)
			{
				throw new Exception("High resolution timer is not available");
			}
		}

		/// <summary>
		/// See <see cref="Clock.Delta()"/>
		/// </summary>
		public void Start()
		{
			Stopwatch.Start();
			StartTicks = LastTicks;
			LastTicks = Stopwatch.ElapsedTicks;
			Running = true;
		}

		/// <summary>
		/// See <see cref="Clock.Delta()"/>
		/// </summary>
		public void Stop()
		{
			Stopwatch.Stop();
			Running = false;
		}

		/// <summary>
		/// Returns the amount of time, in seconds, since last call to Delta() or Start(), whichever was called last. If the clock is stopped, 0 is returned.
		/// </summary>
		public double Delta()
		{
			double delta = 0;

			if (Running)
			{
				long elapsedTicks = Stopwatch.ElapsedTicks;
				delta = (elapsedTicks - LastTicks) / Frequency;
				LastTicks = elapsedTicks;
			}

			return delta;
		}

		/// <summary>
		/// Returns the amount of time, in seconds, since last call to Start(). If the clock is stopped, 0 is returned.
		/// </summary>
		public double Total()
		{
			double total = 0;

			if (Running)
			{
				long elapsedTicks = Stopwatch.ElapsedTicks;
				total = (elapsedTicks - StartTicks) / Frequency;
			}

			return total;
		}
	}
}
