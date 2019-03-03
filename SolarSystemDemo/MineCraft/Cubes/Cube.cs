using System.Collections.Generic;
using SolarSystemDemo.Graphics;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.MineCraft.Cubes
{
	public class Cube : BaseStructureBlock
	{
		public override float Mass
		{
			get { return 1; }
		}

		public Cube()
			: base()
		{
			MeshDataId = StaticGraphicsResources.CubeMeshDataId;
			MaterialId = StaticGraphicsResources.CubeMaterialId;
		}

		public override IEnumerable<BaseStructureBlock> GetUpperInnerBlocks()
		{
			return new List<BaseStructureBlock> { this };
		}

		public override IEnumerable<BaseStructureBlock> GetAllInnerBlocks()
		{
			return new List<BaseStructureBlock> { this };
		}

		public override IEnumerable<BaseInteractiveObject> GetUpperInnerEntities()
		{
			return new List<BaseInteractiveObject> { this };
		}

		public override IEnumerable<BaseInteractiveObject> GetAllInnerEntities()
		{
			return new List<BaseInteractiveObject> { this };
		}
	}
}
