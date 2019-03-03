using System;
using System.Collections.Generic;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.MineCraft.Cubes;
using SolarSystemDemo.MineCraft.Structures;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft
{
	public class BaseStructureBlock : BaseInteractiveObject
	{
		public virtual StructureBlockFunctions BlockFunction
		{
			get { throw new NotImplementedException(); }
		}

		public virtual int Id
		{
			get { return -1; }
		}

		#region Constructors

		public BaseStructureBlock()
			: base()
		{
			Overstructure = null;
		}

		#endregion Constructors

		#region Overridden Properties

		public override SpaceVector WorldPosition
		{
			get
			{
				if (IsInStructure)
				{
					return Overstructure.WorldZeroPosition + TwaMath.RotateVector(
						RelativePosition,
						Overstructure.WorldRotationQuaternion
					);
				}
				else
				{
					return base.WorldPosition;
				}
			}
			set
			{
				if (!IsInStructure)
				{
					base.WorldPosition = value;
				}
			}
		}


		public override Quaternion WorldRotationQuaternion
		{
			get
			{
				return IsInStructure ?
					Overstructure.WorldRotationQuaternion * LocalRotationQuaternion :
					base.WorldRotationQuaternion;
			}
		}

		public override Vector3 WorldLineVelocity
		{
			get { return IsInStructure ? BaseOverstructure.WorldLineVelocity : base.WorldLineVelocity; }
			set { if (!IsInStructure) { base.WorldLineVelocity = value; } }
		}

		#endregion Overridden Properties

		#region Overstructure

		protected BaseStructureBlock Overstructure;

		public bool IsInStructure
		{
			get { return Overstructure != null; }
		}

		public int StructureDepth
		{
			get
			{
				if (!IsInStructure) { return 0; }
				else { return 1 + Overstructure.StructureDepth; }
			}
		}

		public BaseStructureBlock BaseOverstructure
		{
			get
			{
				if (!IsInStructure) { return this; }
				else { return Overstructure.BaseOverstructure; }
			}
		}

		public Vector3 BaseRelPosition
		{
			get
			{
				if (!IsInStructure) { return Vector3.Zero; }
				else
				{
					return Overstructure.BaseRelPosition + TwaMath.RotateVector(
						RelativePosition,
						Overstructure.BaseRelRotationQuaternion
					); // ???
				}
			}
		}

		public Quaternion BaseRelRotationQuaternion // не работает
		{
			get
			{
				if (!IsInStructure) { return Quaternion.Identity; }
				else { return Overstructure.BaseRelRotationQuaternion * LocalRotationQuaternion; }
			}
		}

		public Vector3 BaseRelDirection
		{
			get { return TwaMath.CalculateDirection(BaseRelRotationQuaternion); }
		}

		public int SPosX { get; protected set; }
		public int SPosY { get; protected set; }
		public int SPosZ { get; protected set; }

		public Vector3 RelativePosition { get; private set; }

		#endregion Overstructure

		#region Dealing with Mass

		public virtual SpaceVector WorldZeroPosition
		{
			get { return WorldPosition - WorldMassCenterShift; }
		}

		public virtual Vector3 LocalMassCenterShift
		{
			get { return Vector3.Zero; }
			set { }
		}

		public virtual Vector3 WorldMassCenterShift
		{
			get { return TwaMath.RotateVector(LocalMassCenterShift, WorldRotationQuaternion); }
		}


		public virtual Vector3 BaseLocalMassCenterShift
		{
			get
			{
				if (!IsInStructure) { return LocalMassCenterShift; }
				else { return Overstructure.BaseLocalMassCenterShift; }
			}
		}

		public virtual Vector3 BaseWorldMassCenterShift
		{
			get
			{
				if (!IsInStructure) { return WorldMassCenterShift; }
				else { return Overstructure.BaseWorldMassCenterShift; }
			}
		}

		#endregion Dealing with Mass

		public virtual IEnumerable<BaseStructureBlock> GetUpperInnerBlocks()
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<BaseStructureBlock> GetAllInnerBlocks()
		{
			throw new NotImplementedException();
		}


		public virtual void AddToStructure(CubicalStructure structureRef, int posX, int posY, int posZ)
		{
			Overstructure = structureRef;

			SPosX = posX;
			SPosY = posY;
			SPosZ = posZ;

			RelativePosition = new Vector3(posX, posY, posZ);

			//UpdateState(0);
		}

		public virtual void RemoveFromStructure()
		{
			Overstructure = null;

			SPosX = 0;
			SPosY = 0;
			SPosZ = 0;
		}
	}
}
