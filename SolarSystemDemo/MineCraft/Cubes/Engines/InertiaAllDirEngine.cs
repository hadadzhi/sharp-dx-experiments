using SolarSystemDemo.GeoMath;

namespace SolarSystemDemo.MineCraft.Cubes.Engines
{
	public class InertiaAllDirEngine : JetEngine
	{
		public override int Id
		{
			get { return EngineRegistry.InertiaAllDirEngineId; }
		}

		private Directions _EngineDirection;
		public Directions EngineDirection
		{
			get { return _EngineDirection; }
			set
			{
				_EngineDirection = value;
				LocalRotationQuaternion = TwaMath.GetLocalRotationQuaternion(value);
			}
		}

		public InertiaAllDirEngine()
			: base()
		{
			EngineDirection = Directions.Toward;
		}
	}
}
