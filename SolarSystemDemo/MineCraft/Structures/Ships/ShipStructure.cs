using System.Collections.Generic;
using System.Linq;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.MineCraft.Cubes;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft.Structures.Ships
{
	/// <summary>
	/// Оптимизировання структура для кораблей.
	/// Не вызывает UpdateState для кубиков структуры.
	/// </summary>
	public class ShipStructure : CubicalStructure
	{
		#region Fields

		private List<BaseStructureBlock> HullBlocks;
		private bool NeedOptimizeStructure;
		private int OptimizedMeshDataId;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Создает экземпляр ShipStructure с размерами внутреннего трёхмерного массива 9x9x9.
		/// </summary>
		public ShipStructure()
			: this(9, 9, 9) { }

		/// <summary>
		/// Создает экземпляр ShipStructure с заданными размерами внутреннего трёхмерного массива.
		/// Для симметричности чётные значения размеров будут увеличиваться на 1.
		/// </summary>
		public ShipStructure(int sizeX, int sizeY, int sizeZ)
			: base(sizeX, sizeY, sizeZ)
		{
			HullBlocks = new List<BaseStructureBlock>();

			NeedOptimizeStructure = false;
			OptimizedMeshDataId = -1;
		}

		#endregion Constructors

		#region Methods for working with cells

		public override BaseStructureBlock this[int x, int y, int z]
		{
			get { return base[x, y, z]; }
			set { base[x, y, z] = value; NeedOptimizeStructure = true; }
		}

		#endregion Methods for working with cells

		#region Methods

		public void OptimizeStructure()
		{
			HullBlocks = Blocks.Where(b => b.BlockFunction == StructureBlockFunctions.Hull).ToList();
			Blocks = Blocks.Where(b => b.BlockFunction != StructureBlockFunctions.Hull).ToList();

			OptimizedMeshDataId = Scene.AddMeshData(MeshDataOptimizer.OptimizeCubeMeshData(HullBlocks, -LocalMassCenterShift));
			NeedOptimizeStructure = false;
		}

		public override void UpdateState(float timeDelta)
		{
			if (NeedOptimizeStructure)
			{
				OptimizeStructure();
			}

			base.UpdateState(timeDelta);
		}

		public override List<GraphicsData> GetGraphicsData()
		{
			List<GraphicsData> data = new List<GraphicsData>();

			if (OptimizedMeshDataId != -1)
			{
				foreach (BaseSceneObject sceneObject in GetUpperInnerBlocks())
				{
					data.AddRange(sceneObject.GetGraphicsData());
				}

				data.Add(
					new GraphicsData
					{
						IsVisible = true,

						RealPosition = this.WorldPosition,
						RotationQuaternion = this.WorldRotationQuaternion,
						ScalingVector = this.ScalingVector,

						MeshDataId = OptimizedMeshDataId,
						MaterialId = StaticGraphicsResources.CubeMaterialId,
						TextureId = -1
					}
				);
			}

			return data;
		}

		#endregion Methods
	}
}
