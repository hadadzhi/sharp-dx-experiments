using System;
using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft.Cubes.Engines
{
	public class SimpleEngine : Cube, IActivatable
	{
		public override StructureBlockFunctions BlockFunction
		{
			get { return StructureBlockFunctions.Engine; }
		}

		public override int Id
		{
			get { return -1; }
		}


        public virtual float PowerOutput
        {
            get { throw new NotImplementedException(); }
            protected set { throw new NotImplementedException(); }
        }

        private bool Initialized;
		protected Vector3 BaseRelMomentOfForce;
		protected float BaseRelMomentOfInertia;

		#region Constructors

		public SimpleEngine()
			: base()
		{
			Initialized = false;
			InitializeDirectionDots();
		}

		#endregion Constructors

		#region IActivatable Implementation

		/// <summary>
		/// Получает значение, показывающее является ли этот объект активным в данный момент.
		/// </summary>
		public bool Active { get; private set; }

		/// <summary>
		/// Активирует объект. Значение поля Active после активации будет равно true.
		/// Объект не будет активирован, если он заморожен (Frozen = true).
		/// </summary>
		public virtual void Activate()
		{
			if (!Frozen)
			{
				for (int i = 1; i < DirectionsDots.Count; i++)
				{
					DirectionsDots[i].IsVisible = true;
				}

				Active = true;
			}
		}

		/// <summary>
		/// Деактивирует объект. Значение поля Active после активации будет равно false.
		/// </summary>
		public virtual void Deactivate()
		{
			for (int i = 1; i < DirectionsDots.Count; i++)
			{
				DirectionsDots[i].IsVisible = false;
			}

			Active = false;
		}

		/// <summary>
		/// Получает значение, показывающее является ли этот объект замороженным (приостановленным) в данный момент.
		/// Объект не может быть активирован пока заморожен.
		/// </summary>
		public bool Frozen { get; private set; }

		/// <summary>
		/// Замораживает объект (приостанавливает работу). Значение поля Frozen после заморозки будет равно true.
		/// Если объект был активным во время вызова этого метода, объект будет деактивирован.
		/// </summary>
		public virtual void Freeze()
		{
			if (Active)
			{
				Deactivate();
			}

			Frozen = true;
		}

		/// <summary>
		/// Размораживает объект (возобнавляет работу). Значение поля Frozen после заморозки будет равно false.
		/// </summary>
		public virtual void Defreeze()
		{
			Frozen = false;
		}

		#endregion IActivatable Implementation

		public override bool CanAffect
		{
			get { return Active; }
		}

		public override void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
			if (!Initialized)
			{
				Vector3 pointOfForce = BaseRelPosition - Overstructure.LocalMassCenterShift;
				BaseRelMomentOfForce = Vector3.Cross(pointOfForce, BaseRelDirection);

				// !!! леворукая система координат // ???
				//BaseRelMomentOfForce = new Vector3(-BaseRelMomentOfForce.X, BaseRelMomentOfForce.Y, BaseRelMomentOfForce.Z);
				BaseRelMomentOfForce = new Vector3(BaseRelMomentOfForce.X, BaseRelMomentOfForce.Y, BaseRelMomentOfForce.Z);

				if (!TwaMath.NearEqual(BaseRelMomentOfForce, Vector3.Zero))
				{
					Vector3 rotationAxis = Vector3.Normalize(BaseRelMomentOfForce);
					BaseRelMomentOfInertia = BaseOverstructure.CalculateMomentOfInertia(rotationAxis);
				}
				else
				{
					BaseRelMomentOfInertia = float.MaxValue;
				}

				Initialized = true;
			}

			containerObject.AddForce(
				worldForce: WorldDirection * PowerOutput,
				localMomentOfForce: BaseRelMomentOfForce * PowerOutput,
				localMomentOfInertia: BaseRelMomentOfInertia
			);
		}

		#region Direction Visualization

		protected List<SimpleEngineDirectionDot> DirectionsDots;

		protected virtual void InitializeDirectionDots()
		{
			int dotsCount = 9;
			DirectionsDots = new List<SimpleEngineDirectionDot>();

			float shift = 0.5f;
			float radius;

			for (int i = 0; i < dotsCount; i++)
			{
				radius = 0.4f / (i + 1);

				if (i != 0)
				{
					shift += 3.5f * radius;
				}

				SimpleEngineDirectionDot dot = new SimpleEngineDirectionDot(this, radius, shift);

				dot.IsVisible = i == 0;
				dot.MeshDataId = StaticGraphicsResources.EngineDirectionDotMeshDataId;
				dot.MaterialId = StaticGraphicsResources.WhiteMaterialId;

				DirectionsDots.Add(dot);
			}
		}

		protected class SimpleEngineDirectionDot : BaseSceneObject
		{
			private SimpleEngine EngineRef;

			private float _Radius;
			public virtual float Radius // А нужен ли он здесь?
			{
				get { return _Radius; }
				set { _Radius = value; ScalingVector = new Vector3(Radius); }
			}

			private float _Shift;
			public virtual float Shift
			{
				get { return _Shift; }
				set { _Shift = value; }
			}

			public override SpaceVector WorldPosition
			{
				get { return EngineRef.WorldPosition - EngineRef.WorldDirection * Shift; }
				set { }
			}

			public SimpleEngineDirectionDot(SimpleEngine engineRef, float radius, float shift)
				: base()
			{
				EngineRef = engineRef;
				Radius = radius;
				Shift = shift;
			}
		}

		#endregion Direction Visualization

		#region Graphics Overrides

		public override List<GraphicsData> GetGraphicsData()
		{
			List<GraphicsData> data = base.GetGraphicsData();

			foreach (BaseSceneObject directionDot in DirectionsDots)
			{
				data.AddRange(directionDot.GetGraphicsData());
			}

			return data;
		}

		#endregion Graphics Overrides
	}
}
