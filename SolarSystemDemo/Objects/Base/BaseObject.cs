using SharpDX;
using SolarSystemDemo.GeoMath;

namespace SolarSystemDemo.Objects.Base
{
	public class BaseObject
	{
		#region Private Fields

		private SpaceVector _WorldPosition;
		private Quaternion _LocalRotationQuaternion;

		private float _Size;

		#endregion Private Fields

		/// <summary>
		/// Радиус сферы, описывающей данный объект.
		/// To do: изменить "радиус во float'е" на что-нибудь более информативное.
		/// </summary>
		public virtual float Size
		{
			get { return _Size; }
			private set { _Size = value; }
		}

		public virtual SpaceVector WorldPosition
		{
			get { return _WorldPosition; }
			set { _WorldPosition = value; }
		}

		public virtual Quaternion LocalRotationQuaternion
		{
			get { return _LocalRotationQuaternion; }
			set { _LocalRotationQuaternion = value; }
		}

		public virtual Quaternion WorldRotationQuaternion
		{
			get { return LocalRotationQuaternion; }
		}


		public Vector3 AxisPitch
		{
			get { return TwaMath.RotateVector(TwaMath.BaseAxisPitch, WorldRotationQuaternion); }
		}

		public Vector3 AxisYaw
		{
			get { return TwaMath.RotateVector(TwaMath.BaseAxisYaw, WorldRotationQuaternion); }
		}

		public Vector3 AxisRoll
		{
			get { return TwaMath.RotateVector(TwaMath.BaseAxisRoll, WorldRotationQuaternion); }
		}


		public virtual bool Rotated
		{
			get { return LocalRotationQuaternion.IsIdentity; }
		}


		public virtual Vector3 LocalDirection
		{
			get { return TwaMath.CalculateDirection(LocalRotationQuaternion); }
		}

		public virtual Vector3 WorldDirection
		{
			get { return TwaMath.CalculateDirection(WorldRotationQuaternion); }
		}

		#region Constructors

		public BaseObject()
		{
			_WorldPosition = SpaceVector.Zero;
			_LocalRotationQuaternion = Quaternion.Identity;

			Size = 1;
		}

		#endregion Constructors

		#region Methods

		public virtual void SetAngles(float localAngleYaw, float localAnglePitch, float localAngleRoll)
		{
			LocalRotationQuaternion = Quaternion.RotationYawPitchRoll(
				localAngleYaw,
				localAnglePitch,
				localAngleRoll
			);
		}

		public virtual void RotateYaw(float angle)
		{
			RotateAxis(TwaMath.BaseAxisYaw, angle);
		}

		public virtual void RotatePitch(float angle)
		{
			RotateAxis(TwaMath.BaseAxisPitch, angle);
		}

		public virtual void RotateRoll(float angle)
		{
			RotateAxis(TwaMath.BaseAxisRoll, angle);
		}

		public virtual void RotateAxis(Vector3 axis, float angle)
		{
			LocalRotationQuaternion *= Quaternion.RotationAxis(axis, TwaMath.Mod2Pi(angle));
		}

		#endregion Methods
	}
}
