using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SolarSystemDemo.GeoMath;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.MineCraft.Cubes;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft.Structures
{
	/// <summary>
	/// !!! Почему-то неправильно считается CubicalStructure.WorldMatrix
	/// </summary>
	public class CubicalStructure : BaseStructureBlock
	{
		public override StructureBlockFunctions BlockFunction
		{
			get { return StructureBlockFunctions.Structure; }
		}


		// трехмерный массив кубов
		private Dictionary<int, Dictionary<int, Dictionary<int, BaseStructureBlock>>> Structure;

		protected List<BaseStructureBlock> Blocks;

		#region Constructors

		/// <summary>
		/// Создает экземпляр CubicalStructure с размерами внутреннего трёхмерного массива 9x9x9.
		/// </summary>
		public CubicalStructure() : this(9, 9, 9) { }

		/// <summary>
		/// Создает экземпляр CubicalStructure с заданными размерами внутреннего трёхмерного массива.
		/// Для симметричности чётные значения размеров будут увеличиваться на 1.
		/// </summary>
		public CubicalStructure(int sizeX, int sizeY, int sizeZ)
			: base()
		{
			int halfX = sizeX / 2;
			int halfY = sizeY / 2;
			int halfZ = sizeZ / 2;

			Structure = new Dictionary<int, Dictionary<int, Dictionary<int, BaseStructureBlock>>>();

			for (int i = -halfX; i <= halfX; i++)
			{
				Dictionary<int, Dictionary<int, BaseStructureBlock>> zyArray = new Dictionary<int, Dictionary<int, BaseStructureBlock>>();

				for (int j = -halfY; j <= halfY; j++)
				{
					Dictionary<int, BaseStructureBlock> zArray = new Dictionary<int, BaseStructureBlock>();

					for (int k = -halfZ; k <= halfZ; k++)
					{
						zArray.Add(k, null);
					}

					zyArray.Add(j, zArray);
				}

				Structure.Add(i, zyArray);
			}

			Blocks = new List<BaseStructureBlock>();

			_Mass = 0;
		}

		#endregion Constructors

		#region Methods for working with cells

		public virtual BaseStructureBlock this[int x, int y, int z]
		{
			get
			{
				if (IsCellInitialized(x, y, z)) { return Structure[x][y][z]; }
				else { return null; }
			}
			set
			{
				Vector3 relativePosition = new Vector3(x, y, z);

				bool wasCellInitialized = IsCellInitialized(x, y, z);

				if (wasCellInitialized)
				{
					BaseStructureBlock replacedBlock = Structure[x][y][z];

					if (replacedBlock != null)
					{
						// подумать, какую позицию и т.п. прописывать кубу после удаления из структуры
						replacedBlock.RemoveFromStructure();
						Blocks.RemoveAll(c => c.RelativePosition == relativePosition);
						RemoveMass(replacedBlock);
					}
				}

				BaseStructureBlock block = value;

				if (block != null)
				{
					if (!wasCellInitialized)
					{
						InitializeCell(x, y, z);
					}

					block.AddToStructure(this, x, y, z);

					Blocks.Add(block);
					AppendMass(block);
				}

				Structure[x][y][z] = block;
			}
		}

		private bool IsCellInitialized(int x, int y, int z)
		{
			return Structure.ContainsKey(x) && Structure[x].ContainsKey(y) && Structure[x][y].ContainsKey(z);
		}

		/// <summary>
		/// Добавляет новую ячейку по указанным координатам
		/// </summary>
		private void InitializeCell(int x, int y, int z)
		{
			if (!Structure.ContainsKey(x))
			{
				Structure.Add(x, new Dictionary<int, Dictionary<int, BaseStructureBlock>>());
			}

			if (!Structure[x].ContainsKey(y))
			{
				Structure[x].Add(y, new Dictionary<int, BaseStructureBlock>());
			}

			if (!Structure[x][y].ContainsKey(z))
			{
				Structure[x][y].Add(z, null);
			}
		}

		#endregion Methods for working with cells

		#region Dealing with Mass

		public override SpaceVector WorldZeroPosition
		{
			get
			{
				if (!IsInStructure)
				{
					return base.WorldZeroPosition;
				}
				else
				{
					return Overstructure.WorldZeroPosition + TwaMath.RotateVector(
						RelativePosition,
						Overstructure.WorldRotationQuaternion
					);
				}
			}
		}

		private float _Mass;
		public override float Mass
		{
			get { return _Mass; }
		}

		public override Vector3 LocalMassCenterShift { get; set; }

		public virtual void AppendMass(BaseStructureBlock block)
		{
			if (Mass == 0)
			{
				_Mass += block.Mass;
				LocalMassCenterShift = block.RelativePosition + block.LocalMassCenterShift;
			}
			else
			{
				Vector3 r = LocalMassCenterShift * Mass;
				_Mass += block.Mass;
				r += (block.RelativePosition + block.LocalMassCenterShift) * block.Mass;
				LocalMassCenterShift = r / Mass;
			}
		}

		public virtual void RemoveMass(BaseStructureBlock block)
		{
			// Проверять, что Mass > 0 ?

			Vector3 r = LocalMassCenterShift * Mass;
			_Mass -= block.Mass;
			r -= (block.RelativePosition + block.LocalMassCenterShift) * block.Mass;
			LocalMassCenterShift = r / Mass;
		}

		public override float CalculateMomentOfInertia(Vector3 axis)
		{
			float momentOfInertia = 0;

			foreach (BaseStructureBlock block in GetAllInnerBlocks())
			{
				Vector3 r = block.BaseRelPosition - LocalMassCenterShift;
				Vector3 rProj = TwaMath.GetProjectionToPlane(r, axis);

				momentOfInertia += block.Mass * rProj.LengthSquared();
			}

			return momentOfInertia;
		}

		#endregion Dealing with Mass

		public override IEnumerable<BaseStructureBlock> GetUpperInnerBlocks()
		{
			return Blocks;
		}

		public override IEnumerable<BaseStructureBlock> GetAllInnerBlocks()
		{
			List<BaseStructureBlock> entities = new List<BaseStructureBlock>();

			foreach (BaseStructureBlock block in Blocks)
			{
				entities.AddRange(block.GetAllInnerBlocks());
			}

			return entities;
		}

		public override IEnumerable<BaseInteractiveObject> GetUpperInnerEntities()
		{
			return Blocks;
		}

		public override IEnumerable<BaseInteractiveObject> GetAllInnerEntities()
		{
			List<BaseInteractiveObject> entities = new List<BaseInteractiveObject>();

			foreach (BaseInteractiveObject block in Blocks)
			{
				entities.AddRange(block.GetAllInnerEntities());
			}

			return entities;
		}

		public override bool CanAffect
		{
			get { return true; }
		}

		public override void Affect(BaseInteractiveObject containerObject, IEnumerable<BaseInteractiveObject> otherObjects)
		{
			foreach (BaseInteractiveObject entity in GetUpperInnerEntities().Where(e => e.CanAffect))
			{
				entity.Affect(containerObject, otherObjects);
			}
		}

		public override void UpdateState(float timeDelta)
		{
			base.UpdateState(timeDelta);

			foreach (BaseStructureBlock block in Blocks)
			{
				block.UpdateState(timeDelta);
			}
		}

		#region Graphics Overrides

		public override List<GraphicsData> GetGraphicsData()
		{
			List<GraphicsData> data = new List<GraphicsData>();

			foreach (BaseStructureBlock block in Blocks)
			{
				data.AddRange(block.GetGraphicsData());
			}

			return data;
		}

		#endregion Graphics Overrides
	}
}
