using System;
using SharpDX;

namespace SolarSystemDemo.GeoMath
{
	public enum Directions
	{
		/// <summary>
		/// Сонаправлен оси Z
		/// </summary>
		Toward,

		/// <summary>
		/// Противоположно направлен оси Z
		/// </summary>
		Backward,

		/// <summary>
		/// Сонаправлен оси Y
		/// </summary>
		Up,

		/// <summary>
		/// Противоположно направлен оси Y
		/// </summary>
		Down,

		/// <summary>
		/// Сонаправлен оси X
		/// </summary>
		Left,

		/// <summary>
		/// Противоположно направлен оси X
		/// </summary>
		Right
	}

	public static class TwaMath
	{
		#region Math

		public static int Accuracy = 10000; // точность - кол-во цифр после запятой
		public static double PossibleDelta = 0.001;

		public static double Round(double value)
		{
			value *= Accuracy;
			value = Math.Round(value);
			value /= Accuracy;

			return value;
		}

		public static bool NearEqual(float v1, float v2)
		{
			float e = 0.000001f;
			return Math.Abs(v1 - v2) < e;
		}

		public static bool NearEqual(Vector3 v1, Vector3 v2)
		{
			float e = 0.0001f;
			return Vector3.NearEqual(v1, v2, new Vector3(e, e, e));
		}

		public static bool NearEqual(double v1, double v2)
		{
			double e = 0.00001;
			return Math.Abs(v1 - v2) < e;
		}

        public static bool NearZero(Vector3 v)
        {
            return NearEqual(v, Vector3.Zero);
        }

		public static float Clamp(float value, float min, float max)
		{
			return value < min ? min : value > max ? max : value;
		}

		public static float Mod2Pi(float angle)
		{
			while (angle >= MathUtil.Pi)
			{
				angle -= 2 * MathUtil.Pi;
			}

			while (angle <= -MathUtil.Pi)
			{
				angle += 2 * MathUtil.Pi;
			}

			//if (MathUtil.NearEqual(angle, MathUtil.Pi))
			//{
			//	angle = -MathUtil.Pi + 0.001f;
			//}

			//if (MathUtil.NearEqual(angle, -MathUtil.Pi))
			//{
			//	angle = MathUtil.Pi - 0.001f;
			//}

			return angle;
		}

		#endregion Math

		#region Geometry

		public static readonly Vector3 BaseAxisX = Vector3.Right;
		public static readonly Vector3 BaseAxisY = Vector3.Up;
		public static readonly Vector3 BaseAxisZ = Vector3.BackwardRH;

		public static readonly Vector3 BaseAxisYaw = BaseAxisY;
		public static readonly Vector3 BaseAxisPitch = BaseAxisX;
		public static readonly Vector3 BaseAxisRoll = BaseAxisZ;

		public static readonly Vector3 BaseLocalDirection = BaseAxisZ;


		public static Quaternion GetLocalRotationQuaternion(Directions direction)
		{
			switch (direction)
			{
				case Directions.Toward:
				{
					return Quaternion.RotationYawPitchRoll(0, 0, 0);
				}
				case Directions.Backward:
				{
					return Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
				}
				case Directions.Up:
				{
					return Quaternion.RotationYawPitchRoll(0, -MathUtil.PiOverTwo, 0);
				}
				case Directions.Down:
				{
					return Quaternion.RotationYawPitchRoll(0, MathUtil.PiOverTwo, 0);
				}
				case Directions.Left:
				{
					return Quaternion.RotationYawPitchRoll(MathUtil.PiOverTwo, 0, 0);
				}
				case Directions.Right:
				{
					return Quaternion.RotationYawPitchRoll(-MathUtil.PiOverTwo, 0, 0);
				}
				default:
				{
					throw new Exception();
				}
			}
		}


		public static Vector3 GetProjectionToPlane(Vector3 vector, Vector3 planeNormal)
		{
			if (NearEqual(vector, Vector3.Zero))
			{
				return Vector3.Zero;
			}

			float cos = GetCosBetweenVectors(vector, planeNormal);

			if (NearEqual(cos, 1))
			{
				return Vector3.Zero;
			}

			Vector3 shift = planeNormal * vector.Length() * cos;
			return vector - shift;
		}

		public static Vector3 GetProjectionToVector(Vector3 vector, Vector3 axis)
		{
			if (vector.IsZero)
			{
				return vector;
			}

			float cos = GetCosBetweenVectors(vector, axis);
			return Vector3.Normalize(axis) * vector.Length() * cos;
		}

		public static Vector3 GetProjectionToVector(Vector3 vector, Vector3 axis, out float sign)
		{
			float cos = GetCosBetweenVectors(vector, axis);
			sign = cos >= 0 ? 1 : -1;
			return Vector3.Normalize(axis) * vector.Length() * cos;
		}

		public static float GetCosBetweenVectors(Vector3 v1, Vector3 v2)
		{
			float dot = Vector3.Dot(v1, v2);
			return dot / (v1.Length() * v2.Length());
		}


		public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float angle)
		{
			return Vector3.TransformCoordinate(
				vector,
				Matrix.RotationAxis(axis, angle)
			);
		}

		public static Vector3 RotateVector(Vector3 vector, Quaternion rotationQuaternion)
		{
			return rotationQuaternion.IsIdentity ? vector : Vector3.Transform(vector, rotationQuaternion);
		}


		public static Vector3 CalculateDirection(Quaternion rotationQuaternion)
		{
			return Vector3.Normalize(RotateVector(BaseLocalDirection, rotationQuaternion));
		}


		public static float FromDegrees(double degrees)
		{
			return FromDegrees((float) degrees);
		}

		public static float FromDegrees(float degrees)
		{
			float value = degrees * MathUtil.Pi / 180;
			return Mod2Pi(value);
		}

		#endregion Geometry
	}
}
