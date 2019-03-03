using System;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.MineCraft.Structures.Ships;

namespace SolarSystemDemo.Objects.Demo.PlayerControls
{
	public class ShipCamera : PlayerCamera
	{
		private float TimeBuffer = 0;

		private bool Shaking;
		private float ShakingRate;

		private int Step;

		private Vector3 P;
		public override Vector3 Position
		{
			get { return base.Position + P; }
		}

		public ShipCamera(float radiusMin, float radiusMax, float radius)
			: base(radiusMin, radiusMax, radius)
		{
			TimeBuffer = 0;

			Shaking = true;
			ShakingRate = 0.015f;

			Step = 0;
		}

		public void Update(float timeDelta, ShipMainStructure ship)
		{
			Vector3 axisPitch = TwaMath.RotateVector(TwaMath.BaseAxisPitch, ship.WorldRotationQuaternion);
			Vector3 axisYaw = TwaMath.RotateVector(TwaMath.BaseAxisYaw, ship.WorldRotationQuaternion);
			Vector3 axisRoll = TwaMath.RotateVector(TwaMath.BaseAxisRoll, ship.WorldRotationQuaternion);

			Vector3 velocity = ship.WorldLineVelocity;

			float k = (float) Math.Pow(velocity.Length(), 0.25) * 0.001f;

			Vector3 shakeX = axisPitch / 2;
			Vector3 shakeY = axisYaw;
			Vector3 shakeZ = axisRoll / 2;

			#region Shaking Script

			switch (Step)
			{
				case 0:
				{
					break;
				}
				case 1:
				{
					shakeY = -shakeY;
					shakeZ = -shakeZ;
					break;
				}
				case 2:
				{
					shakeX = -shakeX;
					break;
				}
				case 3:
				{
					shakeX = -shakeX;
					shakeY = -shakeY;
					shakeZ = -shakeZ;
					break;
				}
				case 4:
				{
					shakeZ = -shakeZ;
					break;
				}
				case 5:
				{
					shakeY = -shakeY;
					break;
				}
				case 6:
				{
					shakeX = -shakeX;
					shakeZ = -shakeZ;
					break;
				}
				case 7:
				{
					shakeX = -shakeX;
					shakeY = -shakeY;
					break;
				}
			}

			#endregion Shaking Script

			TimeBuffer += timeDelta;

			while (TimeBuffer > ShakingRate)
			{
				if (Shaking)
				{
					P = shakeX * k + shakeY * k + shakeZ * k;
				}

				TimeBuffer -= ShakingRate;
				Step = (Step + 1) % 8;
			}

			base.Update(axisPitch, axisYaw, axisRoll);
		}
	}
}
