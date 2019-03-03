using SharpDX;

namespace SolarSystemDemo.GeoMath
{
	public abstract class Trajectory
	{
		public abstract SpaceVector Center { get; set; } // или точка отсчета
		public abstract SpaceVector GetCurrentWorldPosition();
		public abstract SpaceVector CalculateNewPosition(float timeDelta);
	}
}
