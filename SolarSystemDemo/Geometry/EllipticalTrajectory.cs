using System;
using System.Collections.Generic;
using SharpDX;

namespace SolarSystemDemo.GeoMath
{
	public class EllipticalTrajectory : Trajectory
	{
		private float a;
		private float b;

		/// <summary>
		/// Время, за которое объект совершает один оборот. В секундах.
		/// </summary>
		public float CircleTime { get; private set; }

		public override SpaceVector Center { get; set; }

		public Vector3 Normal { get; private set; }

		/// <summary>
		/// Вектор, направленный от центра в сторону короткой полуоси.
		/// </summary>
		public Vector3 Direction { get; private set; }


		private float _length = -1;
		/// <summary>
		/// Длина траектории. В метрах.
		/// </summary>
		public float Length
		{
			get
			{
				if (_length < 0)
				{
					_length = MathUtil.Pi * (3 * (a + b) - (float) Math.Sqrt((3 * a + b) * (a + 3 * b)));
				}

				return _length;
			}
		}

		public float LineVelocity
		{
			get { return Length / CircleTime; }
		}

		public SpaceVector LastPosition { get; private set; }

		//private float freeTime;
		//private float dt;
		//private float dAngle;
		//private float ds;

		public EllipticalTrajectory(float a, float b, SpaceVector center, Vector3 normal, Vector3 direction, float time, float startPosAngle)
		{
			CircleTime = time;

			if (a > b)
			{
				this.a = a;
				this.b = b;
			}
			else
			{
				this.b = a;
				this.a = b;
			}

			Center = center;

			if (!TwaMath.NearEqual(Vector3.Dot(normal, direction), 0))
			{
				// To do: разобрать случай, когда они не перпендикулярны
				throw new Exception("Вектора Normal и Direction должны быть перпендикулярны");
			}

			Normal = normal;
			Normal.Normalize();
			Direction = Vector3.Normalize(direction) * a;

			LastPosition = SpaceVector.FromVector3(TwaMath.RotateVector(Direction, Normal, -startPosAngle));

			//freeTime = 0;
			//dt = 0.005f; // раз в 5 милисекунд
			//dAngle = -dt * 0.7f; // минус для движения против часовой стрелки
			//ds = LineVelocity * dt;
		}

		public override SpaceVector CalculateNewPosition(float timeDelta)
		{
			return LastPosition; // !!!
			//Vector3 newPosition = LastPosition;

			//freeTime += timeDelta;

			//while (freeTime >= dt)
			//{
			//	Vector3 preNewPosition = GetNewPosition(newPosition, dAngle);
			//	float passed = (preNewPosition - newPosition).Length();

			//	float k = ds / passed;
			//	float deltaAngle = dAngle * k;

			//	newPosition = GetNewPosition(newPosition, deltaAngle);

			//	//passed = (newPosition - OldPosition).Length();
			//	//Log.Debug(string.Format("{0}, {1}", k.ToString().Remove(6), passed.ToString().Remove(6)));

			//	LastPosition = newPosition;
			//	freeTime -= dt;
			//}

			//return newPosition + Center;
		}

		private SpaceVector GetNewPosition(Vector3 oldPosition, float dAngle)
		{
			return LastPosition; // !!!
			//Vector3 newPosition = TwaMath.RotateVector(oldPosition, Normal, dAngle);

			//float cos = TwaMath.GetCosBetweenVectors(newPosition, Direction);
			//float cosSquare = cos * cos;

			//float radius = a * b / (float) Math.Sqrt(a * a * (1 - cosSquare) + b * b * cosSquare);

			//newPosition = Vector3.Normalize(newPosition) * radius;

			//return newPosition;
		}


		//public List<Vector3> CalculateOrbitalPoints(float timeStep, float pointsCount)
		//{
		//	Vector3 lastPositionCopy = new Vector3(LastPosition.X, LastPosition.Y, LastPosition.Z);

		//	List<Vector3> points = new List<Vector3>();

		//	for (int i = 0; i < pointsCount; i++)
		//	{
		//		points.Add(CalculateNewPosition(timeStep));
		//	}

		//	LastPosition = lastPositionCopy;

		//	return points;
		//}

		public override SpaceVector GetCurrentWorldPosition()
		{
			return LastPosition + Center;
		}
	}
}
