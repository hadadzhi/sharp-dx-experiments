using System;
using System.Collections.Generic;
using SharpDX;
using SharpDXCommons;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.MineCraft.Cubes.Hulls;
using SolarSystemDemo.MineCraft.Structures;
using SolarSystemDemo.MineCraft.Structures.Ships;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft.Cubes.Engines
{
	public class WarpDrive : CubicalStructure, IActivatable
	{
		public override int Id
		{
			get { return EngineRegistry.WarpDriveId; }
		}

		public override StructureBlockFunctions BlockFunction
		{
			get { return StructureBlockFunctions.Engine; }
		}

		public override float Size
		{
			get { return 11; }
		}


		public float MaxActivationTime { get; private set; }
		public float CurrentActivationTime { get; private set; }

		public float DeploymentTime
		{
			get { return 0.5f; }
		}

		public float ActivatingAngle
		{
			get
			{
				float k = TwaMath.Clamp(CurrentActivationTime, 0, DeploymentTime) / DeploymentTime;
				return MathUtil.PiOverTwo * (1 - k);
			}
		}

		public bool WarpDriving
		{
			get { return CurrentActivationTime >= DeploymentTime; }
		}

		public float WarpDrivingLevel
		{
			get
			{
				return TwaMath.Clamp(CurrentActivationTime - DeploymentTime, 0, MaxActivationTime - DeploymentTime)
					/ (MaxActivationTime - DeploymentTime);
			}
		}

		private CubicalStructure Left;
		private CubicalStructure Right;

		private List<IUpdateable> Updateables;

		#region Constructors

		public WarpDrive()
			: base()
		{
			Updateables = new List<IUpdateable>();

			CurrentActivationTime = 0;
			MaxActivationTime = 5;


			Left = new ShipStructure();
			Left[0, 0, 0] = new BaseHullCube();
			Left[-1, 0, 0] = new BaseHullCube();
			Left[-2, 0, 0] = new BaseHullCube();

			foreach (int x in new[] { -3, -4, -5 })
			{
				foreach (int y in new[] { -1, 0, 1 })
				{
					Left[x, y, 0] = new BaseHullCube();
				}
			}

			Right = new ShipStructure();
			Right[0, 0, 0] = new BaseHullCube();
			Right[1, 0, 0] = new BaseHullCube();
			Right[2, 0, 0] = new BaseHullCube();

			foreach (int x in new[] { 3, 4, 5 })
			{
				foreach (int y in new[] { -1, 0, 1 })
				{
					Right[x, y, 0] = new BaseHullCube();
				}
			}


			WarpDriveJet jet = new WarpDriveJet(this);
			Left[-4, 0, 0] = jet;
			Updateables.Add(jet);

			jet = new WarpDriveJet(this);
			Right[4, 0, 0] = jet;
			Updateables.Add(jet);


			this[-4, 0, 0] = Left;
			this[4, 0, 0] = Right;

			Left.LocalMassCenterShift = Vector3.Zero; // !!!
			Right.LocalMassCenterShift = Vector3.Zero; // !!!

			Left.SetAngles(-ActivatingAngle, 0, 0);
			Right.SetAngles(ActivatingAngle, 0, 0);
		}

		#endregion Constructors

		#region IActivatable Implementation

		/// <summary>
		/// Получает значение, показывающее является ли этот объект активным в данный момент.
		/// </summary>
		public bool Active { get; private set; }

		private bool Deactivating;
		
		/// <summary>
		/// Активирует объект. Значение поля Active после активации будет равно true.
		/// Объект не будет активирован, если он заморожен (Frozen = true).
		/// </summary>
		public virtual void Activate()
		{
			if (!Frozen)
			{
				Deactivating = false;
				Active = true;
			}
		}

		/// <summary>
		/// Деактивирует объект. Значение поля Active после активации будет равно false.
		/// </summary>
		public virtual void Deactivate()
		{
			Active = false;
			Deactivating = true;
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

		public override IEnumerable<BaseStructureBlock> GetUpperInnerBlocks()
		{
			return new List<BaseStructureBlock> { this };
		}

		public override IEnumerable<BaseStructureBlock> GetAllInnerBlocks()
		{
			return new List<BaseStructureBlock> { this };
		}

		public override bool CanAffect
		{
			get { return WarpDriving; }
		}

		public override void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
			if (!Deactivating)
			{
				//float angleVelocityK = Math.Abs(TwaMath.Clamp(containerObject.LocalAngleVelocity.Length(), -1, 1));
				//float power = 50 + (float) Math.Pow(10, 7 * WarpDrivingLevel);
				float proj = TwaMath.GetProjectionToVector(containerObject.WorldLineVelocity, WorldDirection).Length();
				float power = 10000 + (float) Math.Pow(10, 7 * (1 - WarpDrivingLevel));

				//containerObject.WorldPosition += containerObject.WorldLineVelocity * power * proj;
				containerObject.AddForce(worldForce: WorldDirection * power);
			}
		}

		public override void UpdateState(float timeDelta)
		{
			base.UpdateState(timeDelta);

			foreach (IUpdateable updateable in Updateables)
			{
				updateable.UpdateState(timeDelta);
			}

			if (Active)
			{
				CurrentActivationTime = TwaMath.Clamp(CurrentActivationTime + timeDelta, 0, MaxActivationTime);
			}
			else
			{
				CurrentActivationTime = TwaMath.Clamp(CurrentActivationTime - timeDelta, 0, MaxActivationTime);
			}

			Left.SetAngles(-ActivatingAngle, 0, 0);
			Right.SetAngles(ActivatingAngle, 0, 0);
		}

		private class WarpDriveJet : SimpleEngine
		{
			private WarpDrive WarpDriveRef;

			public override bool CanAffect
			{
				get { return false; }
			}

			public WarpDriveJet(WarpDrive hyperDriveRef)
			{
				WarpDriveRef = hyperDriveRef;
			}

			public override void Activate()
			{
				base.Activate();
				DirectionsDots[0].IsVisible = true;
			}

			public override void Deactivate()
			{
				base.Deactivate();
				DirectionsDots[0].IsVisible = false;
			}

			public override void UpdateState(float timeDelta)
			{
				base.UpdateState(timeDelta); // ???

				if (Active ^ WarpDriveRef.WarpDriving)
				{
					if (WarpDriveRef.WarpDriving)
					{
						Activate();
					}
					else
					{
						Deactivate();
					}
				}
			}

			#region Direction Visualization

			protected override void InitializeDirectionDots()
			{
				int dotsCount = 9;
				DirectionsDots = new List<SimpleEngineDirectionDot>();

				float shift = 0;
				float radius;

				for (int i = 0; i < dotsCount; i++)
				{
					radius = 0.8f / (i + 1);

					if (i != 0)
					{
						shift += radius;
					}

					HyperDriveJetDirectionDot dot = new HyperDriveJetDirectionDot(this, radius, shift);

					dot.IsVisible = false;
					dot.MeshDataId = StaticGraphicsResources.EngineDirectionDotMeshDataId;
					dot.MaterialId = StaticGraphicsResources.WhiteMaterialId;

					DirectionsDots.Add(dot);
				}
			}

			protected class HyperDriveJetDirectionDot : SimpleEngineDirectionDot
			{
				private WarpDriveJet HyperDriveJetRef;

				public override float Shift
				{
					get { return 1.3f + base.Shift * 4 * HyperDriveJetRef.WarpDriveRef.WarpDrivingLevel; }
					set { base.Shift = value; }
				}

				public HyperDriveJetDirectionDot(SimpleEngine hyperDriveJetRef, float radius, float shift)
					: base(hyperDriveJetRef, radius, shift)
				{
					if (!(hyperDriveJetRef is WarpDriveJet))
					{
						throw new ArgumentNullException("hyperDriveJetRef");
					}

					HyperDriveJetRef = hyperDriveJetRef as WarpDriveJet;
				}
			}

			#endregion Direction Visualization
		}
	}
}
