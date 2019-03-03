using SharpDX;
using SolarSystemDemo.GeoMath;

namespace SolarSystemDemo.Graphics
{
	public struct GraphicsData
	{
		public bool IsVisible;

		public SpaceVector RealPosition;
		public Vector3 GraphicsPosition;
		public Quaternion RotationQuaternion;
		public Vector3 ScalingVector;

		public int MeshDataId;
		public int MaterialId;
		public int TextureId;
	}
}
