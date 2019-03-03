using System;
using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.MineCraft.Structures.Ships;

namespace SolarSystemDemo.Objects.Base
{
	public class BaseInteractiveObject : BaseSceneObject, IUpdateable
	{
		#region Private Fields

		private Vector3 _WorldLineVelocity;
		private Vector3 _LocalAngleVelocity;

		private Vector3 LocalMomentOfForce;
		private Vector3 WorldMomentOfForce;

		private Vector3 LocalLineAcceleration;
		private Vector3 WorldLineAcceleration;

		private Vector3 LocalAngleAcceleration;

		#endregion Private Fields

		public virtual Trajectory Trajectory { get; private set; }


		public virtual Vector3 WorldLineVelocity
		{
			get { return _WorldLineVelocity; }
			set { _WorldLineVelocity = value; }
		}

		public virtual Vector3 LocalAngleVelocity
		{
			get { return _LocalAngleVelocity; }
			set { _LocalAngleVelocity = value; }
		}


		public virtual SpaceVector WorldMassCenterPosition
		{
			get { return WorldPosition; }
		}

		public virtual float Mass
		{
			get { return 1; }
		}

		#region Constructors

		public BaseInteractiveObject()
			: this(null) { }

		public BaseInteractiveObject(Trajectory trajectory)
			: base()
		{
			Trajectory = trajectory;

			_WorldLineVelocity = Vector3.Zero;
			_LocalAngleVelocity = Vector3.Zero;

			LocalMomentOfForce = Vector3.Zero;
			WorldMomentOfForce = Vector3.Zero;

			LocalLineAcceleration = Vector3.Zero;
			WorldLineAcceleration = Vector3.Zero;

			LocalAngleAcceleration = Vector3.Zero;
		}

		#endregion Constructors

		#region Force Thing

		/// Применяет силу к центру масс тела. Результирующая всех сил будет учтена при следующем рассчете изменения 
		/// положения тела в методе UpdateState(float timeDelta). 
		/// </summary>
		/// <param name="worldForce">
		/// Применяемая сила в мировой системе координат. Направление вектора представляет из себя направление
		/// силы, а модуль - значение.
		/// </param>
		public void AddForce(Vector3 worldForce)
		{
			WorldLineAcceleration += worldForce / Mass;
		}

		/// <summary>
		/// Применяет силу к определенной точке тела. Результирующая всех сил будет учтена при следующем рассчете изменения 
		/// положения тела в методе UpdateState(float timeDelta). Предполагается, что точка применения силы действительно
		/// находится на теле.
		/// </summary>
		/// <param name="localForce">
		/// Применяемая сила в локальной системе координат. Направление вектора представляет из себя направление
		/// силы, а модуль - значение.
		/// </param>
		/// <param name="localApplicationPoint">Вектор от центра масс тела к точке приложения силы в локальной системе координат.</param>
		public void AddForce(Vector3 localForce, Vector3 localApplicationPoint)
		{
			LocalLineAcceleration += localForce / Mass;

			WorldMomentOfForce += Vector3.Cross(localApplicationPoint, localForce);
		}

		/// <summary>
		/// Применяет силу к определенной точке тела. Результирующая всех сил будет учтена при следующем рассчете изменения 
		/// положения тела в методе UpdateState(float timeDelta). Предполагается, что точка применения силы действительно
		/// находится на теле.
		/// </summary>
		/// <param name="worldForce">
		/// Применяемая сила в мировой системе координат. Направление вектора представляет из себя направление
		/// силы, а модуль - значение.
		/// </param>
		/// <param name="worldApplicationPoint">Точка приложения силы в мировой системе координат.</param>
		public void AddForce(Vector3 worldForce, SpaceVector worldApplicationPoint)
		{
			WorldLineAcceleration += worldForce / Mass;

			Vector3 worldPointOfForce = SpaceVector.GetDeltaVectorWithinChunkRange(worldApplicationPoint, WorldMassCenterPosition);
			WorldMomentOfForce += Vector3.Cross(worldPointOfForce, worldForce);
		}

		/// <summary>
		/// Оптимизированный метод для применения силы к телу. Результирующая всех сил будет учтена при следующем рассчете изменения 
		/// положения тела в методе UpdateState(float timeDelta).
		/// </summary>
		public void AddForce(
			Vector3 localForce = new Vector3(),
			Vector3 worldForce = new Vector3(),
			Vector3 localMomentOfForce = new Vector3(),
			float localMomentOfInertia = float.MaxValue)
		{
			LocalLineAcceleration += localForce / Mass;
			WorldLineAcceleration += worldForce / Mass;
			LocalAngleAcceleration += localMomentOfForce / localMomentOfInertia;
		}

		/// <summary>
		/// Возвращает значение момента инерции тела относительно заданной оси вращения, проходящей через центр масс тела.
		/// Предполагается, что данное тело - сфера с радиусом BaseObject.Size.
		/// </summary>
		/// <param name="axis">Ось вращения, проходящая через центр масс тела.</param>
		/// <returns>Значение момента инерции.</returns>
		public virtual float CalculateMomentOfInertia(Vector3 axis)
		{
			return Mass * Size * Size * 2.0f / 3.0f;
		}

		#endregion Force Thing

		#region Interactivity

		public virtual bool CanAffect
		{
			get { return false; }
		}

		public virtual void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<BaseInteractiveObject> GetUpperInnerEntities()
		{
			return new List<BaseInteractiveObject> { this };
		}

		public virtual IEnumerable<BaseInteractiveObject> GetAllInnerEntities()
		{
			return new List<BaseInteractiveObject> { this };
		}

		#endregion Interactivity

		public virtual void UpdateState(float timeDelta)
		{
			if (Trajectory != null)
			{
				WorldPosition = Trajectory.CalculateNewPosition(timeDelta);
			}
			else
			{
				WorldLineAcceleration += TwaMath.RotateVector(LocalLineAcceleration, WorldRotationQuaternion);
				WorldLineVelocity += WorldLineAcceleration * timeDelta / 2;
				WorldPosition = WorldPosition + WorldLineVelocity * timeDelta;

				LocalMomentOfForce += TwaMath.RotateVector(WorldMomentOfForce, Quaternion.Invert(WorldRotationQuaternion));
				if (!TwaMath.NearEqual(LocalMomentOfForce, Vector3.Zero))
				{
					LocalAngleAcceleration += LocalMomentOfForce / CalculateMomentOfInertia(LocalMomentOfForce);
				}
				LocalAngleVelocity += LocalAngleAcceleration * timeDelta / 2;
				if (!TwaMath.NearEqual(LocalAngleVelocity, Vector3.Zero))
				{
					RotateAxis(Vector3.Normalize(LocalAngleVelocity), LocalAngleVelocity.Length() * timeDelta);
				}

				LocalMomentOfForce = Vector3.Zero;
				WorldMomentOfForce = Vector3.Zero;

				LocalLineAcceleration = Vector3.Zero;
				WorldLineAcceleration = Vector3.Zero;

				LocalAngleAcceleration = Vector3.Zero;
			}
		}
	}
}
