using SharpDX;
using SharpDX.Direct3D11;
using SharpDXCommons;
using SharpDXCommons.Cameras;
using System;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpriteDemo
{
	struct TreePointSprite
	{
		public Vector3 Pos;
		public Vector2 Size;
	};

	public class SpriteDemo : SharpDXApplication
	{
		#region Fields

		private Matrix Proj;
		private Matrix ViewProj;

		private Buffer ConstantBufferPerObject;
		private Buffer ConstantBufferPerFrame;

		private OrbitalControls CameraControls;

		private Matrix BoxWorld;
		private int BoxVerticesOffset;
		private int BoxIndicesOffset;
		private int BoxIndexCount;
		private Material BoxMaterial;
		private ShaderResourceView BoxTexture;
		private Buffer BoxVB;
		private Buffer BoxIB;
		private VertexBufferBinding BoxVertexBinding;

		private const int TreeCount = 4;
		private Material TreeMaterial;
		private Buffer TreeVB;
		private Buffer TreeIB;
		private VertexBufferBinding TreeVertexBinding;

		private DirectionalLight DirLight;
		private PointLight PointLight;
		private SpotLight SpotLight;

		private double Elapsed;
		private Random Random = new Random();

		#endregion Fields

		public SpriteDemo(GraphicsConfiguration configuration)
			: base(configuration, "Sprite Demo")
		{
			CameraControls = new OrbitalControls(RenderWindow, Vector3.Zero, 5, 500, 100);
			CameraControls.Install();

			CreateBoxGeometryBuffers();
			CreateTreesGeometryBuffers();

			CreateLights();
			CreateMaterials();

			CreateWorldMatrices();

			TargetsResized += OnBuffersResized;
		}

		private void CreateBoxGeometryBuffers()
		{
			MeshData box = GeometryGenerator.CreateBox(1.0f, 1.0f, 1.0f);

			BoxVB = new Buffer(
				Device,
				box.Vertices.Length * Marshal.SizeOf(typeof(Vertex)),
				ResourceUsage.Default,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.UpdateSubresource(box.Vertices, BoxVB);

			BoxIB = new Buffer(
				Device,
				box.Indices.Length * Marshal.SizeOf(typeof(uint)),
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.UpdateSubresource(box.Indices, BoxIB);

			BoxVertexBinding = new VertexBufferBinding(BoxVB, Marshal.SizeOf(typeof(Vertex)), 0);
		}

		private void CreateTreesGeometryBuffers()
		{
			TreePointSprite[] v = new TreePointSprite[TreeCount];

			for (uint i = 0; i < TreeCount; i++)
			{
				float x = Random.Next(-35, 35);
				float y = Random.Next(-35, 35);
				float z = 0;

				v[i].Pos = new Vector3(x, y, z);
				v[i].Size = new Vector2(24.0f, 24.0f);
			}

			TreeVB = new Buffer(
				Device,
				TreeCount * Marshal.SizeOf(typeof(TreePointSprite)),
				ResourceUsage.Immutable,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);
		}

		private void CreateLights()
		{
			DirLight = new DirectionalLight
			{
				Ambient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f),
				Diffuse = new Vector4(0.3f, 0.3f, 0.3f, 1.0f),
				Specular = new Vector4(0.3f, 0.3f, 0.3f, 1.0f),
				Direction = Vector3.Negate(Vector3.Normalize(CameraControls.GetCameraPosition()))
			};

			SpotLight = new SpotLight
			{
				Ambient = new Vector4(0.0f, 0.0f, 0.1f, 1.0f),
				Diffuse = new Vector4(0.0f, 0.0f, 0.5f, 1.0f),
				Specular = new Vector4(0.0f, 0.0f, 0.5f, 1.0f),
				Att = new Vector3(1.0f, 0.0f, 0.0f),
				Spot = 125,
				Range = 10000.0f
			};

			PointLight = new PointLight
			{
				Ambient = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				Specular = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				Att = new Vector3(0.0f, 0.0f, 0.01f),
				Range = 100,
				Position = new Vector3(0, 30, 30)
			};
		}

		private void CreateMaterials()
		{
			BoxMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.3f, 0.3f, 0.3f, 1000.0f),
				Textured = true
			};

			TreeMaterial = new Material
			{
				Ambient = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(0.2f, 0.2f, 0.2f, 16.0f),
				Textured = true
			};
		}

		private void CreateWorldMatrices()
		{
			BoxWorld = Matrix.Multiply(Matrix.Scaling(15.0f, 15.0f, 15.0f), Matrix.Translation(0.0f, 0.0f, 0.0f));
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			Proj = Matrix.PerspectiveFovLH(0.25f * (float) Math.PI, (float) newWidth / newHeight, 0.1f, 10000f);
		}
	}
}
