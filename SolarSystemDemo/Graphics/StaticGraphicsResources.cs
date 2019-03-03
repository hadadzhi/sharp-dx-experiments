using SharpDX;
using SharpDX.Direct3D11;
using SharpDXCommons;
using SolarSystemDemo.Objects.Demo;

namespace SolarSystemDemo.Graphics
{
	public class StaticGraphicsResources
	{
		#region Common

		public static int WhiteMaterialId;
		public static int RedMaterialId;
		public static int GreenMaterialId;
		public static int BlueMaterialId;
		public static int OrangeMaterialId;

		#endregion Common

		#region Gameplay

		public static int CubeMeshDataId;

		public static int CubeMaterialId;
		public static int JetEngineMaterialId;
		public static int InertiaEngineMaterialId;

		public static int EngineDirectionDotMeshDataId;

		public static int HyperSpaceWindowSphereMeshDataId;
		public static int HyperSpaceWindowMaterialId;

		public static int CubeTestMeshDataId;
		public static int CubeTestTextureId;

		public static int StabilizerMaterialId;

		#endregion Gameplay

		#region SolarSystem

		private const uint PlanetMeshDataSliceCount = 50;
		private const uint PlanetMeshDataStackCount = 50;

		public static int SkySphereMeshDataId;
		public static int PlanetIdentitySphereMeshDataId;

		public static int SkySphereMaterialId;
		public static int SunMaterialId;
		public static int PlanetMaterialId;

		public static int SkySphereTextureId;
		public static int SunTextureId;
		public static int EarthTextureId;
		public static int MercuryTextureId;
		public static int VenusTextureId;
		public static int MoonTextureId;
		public static int MarsTextureId;

		public static int OrbitalDotMeshDataId;

		#endregion SolarSystem

		public static void InitializeStaticGraphicsData(Device device)
		{
			#region Common

			WhiteMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(1, 1, 1, 0),
					Diffuse = new Vector4(0, 0, 0, 0),
					Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
					Textured = false
				}
			);

			RedMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(1, 0, 0, 0),
					Diffuse = new Vector4(1, 0, 0, 0),
					Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
					Textured = false
				}
			);

			GreenMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0, 1, 0, 0),
					Diffuse = new Vector4(0, 1, 0, 0),
					Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
					Textured = false
				}
			);

			BlueMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0, 0, 1, 0),
					Diffuse = new Vector4(0, 0, 1, 0),
					Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
					Textured = false
				}
			);

			OrangeMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(1, 0.6f, 0, 0),
					Diffuse = new Vector4(0, 0, 0, 0),
					Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
					Textured = false
				}
			);

			#endregion Common

			#region Gameplay

			CubeMeshDataId = Scene.AddMeshData(GeometryGenerator.CreateBox(1, 1, 1));

			CubeMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0.2f, 0, 0, 0),
					Diffuse = new Vector4(0.7f, 0, 0, 0),
					Specular = new Vector4(0.3f, 0.3f, 0.3f, 0.3f),
					Textured = false
				}
			);

			JetEngineMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0, 0.2f, 0.2f, 0),
					Diffuse = new Vector4(0, 0.7f, 0.7f, 0),
					Specular = new Vector4(0.3f, 0.3f, 0.3f, 0.3f),
					Textured = false
				}
			);

			InertiaEngineMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0.3f, 0.3f, 0, 0),
					Diffuse = new Vector4(0.7f, 0.7f, 0, 0),
					Specular = new Vector4(0.3f, 0.3f, 0.3f, 0.3f),
					Textured = false
				}
			);

			EngineDirectionDotMeshDataId = Scene.AddMeshData(GeometryGenerator.CreateSphere(1, 30, 30));

			HyperSpaceWindowSphereMeshDataId = Scene.AddMeshData(GeometryGenerator.CreateSphere(10, 30, 30));
			HyperSpaceWindowMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0.9f, 0.9f, 1.0f, 0),
					Diffuse = new Vector4(0.0f, 0.0f, 0.0f, 0),
					Specular = new Vector4(0.3f, 0.3f, 0.3f, 0.3f),
					Textured = false
				}
			);

			CubeTestMeshDataId = Scene.AddMeshData(GeometryGenerator.CreateBox(4, 1, 8));
			CubeTestTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/cube_test.jpg"));

			StabilizerMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0, 0, 0.2f, 0),
					Diffuse = new Vector4(0, 0, 0.7f, 0),
					Specular = new Vector4(0.3f, 0.3f, 0.3f, 0.3f),
					Textured = false
				}
			);

			#endregion Gameplay

			#region SolarSystem

			#region Planet Meshes

			SkySphereMeshDataId = Scene.AddMeshData(
				GeometryGenerator.CreateSphere(
					1,
					PlanetMeshDataSliceCount / 2,
					PlanetMeshDataStackCount / 2
				)
			);

			PlanetIdentitySphereMeshDataId = Scene.AddMeshData(
				GeometryGenerator.CreateSphere(
					1,
					PlanetMeshDataSliceCount,
					PlanetMeshDataStackCount
				)
			);

			#endregion Planet Meshes

			#region Planet Materials

			SkySphereMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = Vector4.One,
					Diffuse = Vector4.Zero,
					Specular = Vector4.Zero,
					Textured = true
				}
			);

			SunMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = Vector4.One,
					Diffuse = Vector4.One,
					Specular = Vector4.Zero,
					Textured = true
				}
			);

			PlanetMaterialId = Scene.AddMaterial(
				new Material
				{
					Ambient = new Vector4(0.5f, 0.5f, 0.5f, 0f),
					Diffuse = Vector4.One,
					Specular = new Vector4(0.2f, 0.2f, 0.2f, 8.0f),
					Textured = true
				}
			);

			#endregion Planet Materials

			#region Planet Textures

			SkySphereTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/stars.jpg"));

			//SunTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/sun.gif"));
			SunTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/sunmap.jpg"));

			EarthTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/earthwithcloudsmab.jpg"));
			//EarthTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/earthmap1k.jpg"));
			//EarthTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/earthmap2k.jpg"));
			//EarthTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/earthmap10k.jpg"));
			//EarthTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/earthlights1k.jpg"));

			MercuryTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/mercurymap.jpg"));
			VenusTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/venusmap.jpg"));
			MoonTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/moonmap.jpg"));
			MarsTextureId = Scene.AddTexture(ShaderResourceView.FromFile(device, "Graphics/Textures/marsku.gif"));

			#endregion Planet Textures

			#region Dots

			OrbitalDotMeshDataId = Scene.AddMeshData(GeometryGenerator.CreateSphere(1, 30, 30));

			#endregion Dots

			#endregion SolarSystem
		}
	}
}
