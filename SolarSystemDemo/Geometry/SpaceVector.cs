using System;
using SharpDX;

namespace SolarSystemDemo.GeoMath
{
	public struct SpaceVector
	{
		public const int GlobalChunkSize = 31600800; // Количество секунд в году
		public const double LocalChunkSize = 299792458; // Световая секунда (в метрах)
		public const double GlobalChunkSizeM = (double) GlobalChunkSize * (double) LocalChunkSize; // Световой год в метрах
		public const double GlobalChunkSizeOverTwo = GlobalChunkSize / 2;
		public const double LocalChunkSizeOverTwo = LocalChunkSize / 2;

		public static int CurrentGlobalChunkX { get; private set; }
		public static int CurrentGlobalChunkY { get; private set; }
		public static int CurrentGlobalChunkZ { get; private set; }

		public static int CurrentLocalChunkX { get; private set; }
		public static int CurrentLocalChunkY { get; private set; }
		public static int CurrentLocalChunkZ { get; private set; }


		public static readonly SpaceVector Zero = new SpaceVector();


		public readonly int GlobalChunkX;
		public readonly int GlobalChunkY;
		public readonly int GlobalChunkZ;

		public readonly int LocalChunkX;
		public readonly int LocalChunkY;
		public readonly int LocalChunkZ;

		public readonly double X;
		public readonly double Y;
		public readonly double Z;

		public double LengthWithinGlobalChunk
		{
			get
			{
				if (!(GlobalChunkX == 0
					&& GlobalChunkY == 0
					&& GlobalChunkZ == 0))
				{
					throw new ArgumentOutOfRangeException("Одна или несколько GlobalChunk координат != 0");
				}

				double x = LocalChunkX * LocalChunkSize + X;
				double y = LocalChunkY * LocalChunkSize + Y;
				double z = LocalChunkZ * LocalChunkSize + Z;

				return Math.Sqrt(x * x + y * y + z * z);
			}
		}


		public SpaceVector(
			int globalChunkX,
			int globalChunkY,
			int globalChunkZ,
			int localChunkX,
			int localChunkY,
			int localChunkZ,
			double x,
			double y,
			double z)
		{
			GlobalChunkX = globalChunkX;
			GlobalChunkY = globalChunkY;
			GlobalChunkZ = globalChunkZ;

			LocalChunkX = localChunkX;
			LocalChunkY = localChunkY;
			LocalChunkZ = localChunkZ;

			X = x;
			Y = y;
			Z = z;

			#region X

			while (X > LocalChunkSizeOverTwo)
			{
				X -= LocalChunkSize;
				LocalChunkX++;
			}

			while (X < -LocalChunkSizeOverTwo)
			{
				X += LocalChunkSize;
				LocalChunkX--;
			}

			while (LocalChunkX > GlobalChunkSizeOverTwo)
			{
				LocalChunkX -= GlobalChunkSize;
				GlobalChunkX++;
			}

			while (LocalChunkX < -GlobalChunkSizeOverTwo)
			{
				LocalChunkX += GlobalChunkSize;
				GlobalChunkX--;
			}

			#endregion X

			#region Y

			while (Y > LocalChunkSizeOverTwo)
			{
				Y -= LocalChunkSize;
				LocalChunkY++;
			}

			while (Y < -LocalChunkSizeOverTwo)
			{
				Y += LocalChunkSize;
				LocalChunkY--;
			}

			while (LocalChunkY > GlobalChunkSizeOverTwo)
			{
				LocalChunkY -= GlobalChunkSize;
				GlobalChunkY++;
			}

			while (LocalChunkY < -GlobalChunkSizeOverTwo)
			{
				LocalChunkY += GlobalChunkSize;
				GlobalChunkY--;
			}

			#endregion Y

			#region Z

			while (Z > LocalChunkSizeOverTwo)
			{
				Z -= LocalChunkSize;
				LocalChunkZ++;
			}

			while (Z < -LocalChunkSizeOverTwo)
			{
				Z += LocalChunkSize;
				LocalChunkZ--;
			}

			while (LocalChunkZ > GlobalChunkSizeOverTwo)
			{
				LocalChunkZ -= GlobalChunkSize;
				GlobalChunkZ++;
			}

			while (LocalChunkZ < -GlobalChunkSizeOverTwo)
			{
				LocalChunkZ += GlobalChunkSize;
				GlobalChunkZ--;
			}

			#endregion Z
		}


		public static SpaceVector FromVector3(Vector3 v)
		{
			return new SpaceVector(0, 0, 0, 0, 0, 0, (double) v.X, (double) v.Y, (double) v.Z);
		}


		public static void SetCurrentChunk(int globalChunkX, int globalChunkY, int globalChunkZ, int localChunkX, int localChunkY, int localChunkZ)
		{
			CurrentGlobalChunkX = globalChunkX;
			CurrentGlobalChunkY = globalChunkY;
			CurrentGlobalChunkZ = globalChunkZ;

			CurrentLocalChunkX = localChunkX;
			CurrentLocalChunkY = localChunkY;
			CurrentLocalChunkZ = localChunkZ;
		}

		public static SpaceVector FromCurrentChunk(double x, double y, double z)
		{
			return new SpaceVector(
				CurrentGlobalChunkX,
				CurrentGlobalChunkY,
				CurrentGlobalChunkZ,
				CurrentLocalChunkX,
				CurrentLocalChunkY,
				CurrentLocalChunkZ,
				x,
				y,
				z
			);
		}


		public static SpaceVector operator +(SpaceVector left, SpaceVector right)
		{
			return new SpaceVector(
				left.GlobalChunkX + right.GlobalChunkX,
				left.GlobalChunkY + right.GlobalChunkY,
				left.GlobalChunkZ + right.GlobalChunkZ,
				left.LocalChunkX + right.LocalChunkX,
				left.LocalChunkY + right.LocalChunkY,
				left.LocalChunkZ + right.LocalChunkZ,
				left.X + right.X,
				left.Y + right.Y,
				left.Z + right.Z
			);
		}

		public static SpaceVector operator +(SpaceVector left, Vector3 right)
		{
			return new SpaceVector(
				left.GlobalChunkX,
				left.GlobalChunkY,
				left.GlobalChunkZ,
				left.LocalChunkX,
				left.LocalChunkY,
				left.LocalChunkZ,
				left.X + (double) right.X,
				left.Y + (double) right.Y,
				left.Z + (double) right.Z
			);
		}

		public static SpaceVector operator -(SpaceVector left, SpaceVector right)
		{
			return new SpaceVector(
				left.GlobalChunkX - right.GlobalChunkX,
				left.GlobalChunkY - right.GlobalChunkY,
				left.GlobalChunkZ - right.GlobalChunkZ,
				left.LocalChunkX - right.LocalChunkX,
				left.LocalChunkY - right.LocalChunkY,
				left.LocalChunkZ - right.LocalChunkZ,
				left.X - right.X,
				left.Y - right.Y,
				left.Z - right.Z
			);
		}

		public static SpaceVector operator -(SpaceVector left, Vector3 right)
		{
			return new SpaceVector(
				left.GlobalChunkX,
				left.GlobalChunkY,
				left.GlobalChunkZ,
				left.LocalChunkX,
				left.LocalChunkY,
				left.LocalChunkZ,
				left.X - (double) right.X,
				left.Y - (double) right.Y,
				left.Z - (double) right.Z
			);
		}


		public static Vector3 Normalize(SpaceVector v)
		{
			double x, y, z;

			if (!(v.GlobalChunkX == 0
				&& v.GlobalChunkY == 0
				&& v.GlobalChunkZ == 0))
			{
				x = v.GlobalChunkX * GlobalChunkSize + v.LocalChunkX;
				y = v.GlobalChunkY * GlobalChunkSize + v.LocalChunkY;
				z = v.GlobalChunkZ * GlobalChunkSize + v.LocalChunkZ;
			}
			else
			{
				x = v.LocalChunkX * LocalChunkSize + v.X;
				y = v.LocalChunkY * LocalChunkSize + v.Y;
				z = v.LocalChunkZ * LocalChunkSize + v.Z;
			}

			double length = Math.Sqrt(x * x + y * y + z * z);

			return new Vector3((float) (x / length), (float) (y / length), (float) (z / length));
		}

		public static Vector3 GetDeltaVectorWithinChunkRange(SpaceVector left, SpaceVector right)
		{
			SpaceVector delta = left - right;

			if (!(delta.GlobalChunkX == 0
				&& delta.GlobalChunkY == 0
				&& delta.GlobalChunkZ == 0
				&& delta.LocalChunkX == 0
				&& delta.LocalChunkY == 0
				&& delta.LocalChunkZ == 0))
			{
				throw new ArgumentOutOfRangeException(
					string.Format("Space Vectors are not within chunk ({0} m) range.", LocalChunkSize)
				);
			}

			return new Vector3((float) delta.X, (float) delta.Y, (float) delta.Z);
		}

		public static bool AreWithinLocalChunkRange(SpaceVector v1, SpaceVector v2)
		{
			SpaceVector delta = v1 - v2;

			return delta.GlobalChunkX == 0
				&& delta.GlobalChunkY == 0
				&& delta.GlobalChunkZ == 0
				&& delta.LocalChunkX == 0
				&& delta.LocalChunkY == 0
				&& delta.LocalChunkZ == 0;
		}

		public static bool AreWithinGlobalChunkRange(SpaceVector v1, SpaceVector v2)
		{
			SpaceVector delta = v1 - v2;

			return delta.GlobalChunkX == 0
				&& delta.GlobalChunkY == 0
				&& delta.GlobalChunkZ == 0;
		}


		public static bool operator ==(SpaceVector left, SpaceVector right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SpaceVector left, SpaceVector right)
		{
			return !left.Equals(right);
		}

		public override bool Equals(object value)
		{
			if (!(value is SpaceVector)) { return false; }
			else { return this.Equals((SpaceVector) value); }
		}

		public bool Equals(SpaceVector other)
		{
			if (this.GlobalChunkX == other.GlobalChunkX
				&& this.GlobalChunkY == other.GlobalChunkY
				&& this.GlobalChunkZ == other.GlobalChunkZ
				&& this.LocalChunkX == other.LocalChunkX
				&& this.LocalChunkY == other.LocalChunkY
				&& this.LocalChunkZ == other.LocalChunkZ
				&& TwaMath.NearEqual(this.X, other.X)
				&& TwaMath.NearEqual(this.Y, other.Y))
			{
				return TwaMath.NearEqual(this.Z, other.Z);
			}
			else
			{
				return false;
			}
		}

		public override string ToString()
		{
			return string.Format(
				"[{0}, {1}, {2}] [{3}, {4}, {5}] [{6}, {7}, {8}]",
				GlobalChunkX, GlobalChunkY, GlobalChunkZ,
				LocalChunkX, LocalChunkY, LocalChunkZ,
				X, Y, Z
			);
		}
	}
}
