
namespace SolarSystemDemo.MineCraft.Cubes.Hulls
{
	public class BaseHullCube : Cube
	{
		public override StructureBlockFunctions BlockFunction
		{
			get { return StructureBlockFunctions.Hull; }
		}

		public override int Id
		{
			get { return HullRegistry.BaseHullCubeId; }
		}


		//public override int MaterialId
		//{
		//	get { return Graphics.StaticGraphicsResources.PlanetMaterialId; }
		//	set { }
		//}

		//public override int TextureId
		//{
		//	get { return Graphics.StaticGraphicsResources.CubeTestTextureId; }
		//	set { }
		//}
	}
}
