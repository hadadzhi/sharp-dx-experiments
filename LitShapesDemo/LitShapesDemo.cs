using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons;
using SharpDXCommons.Cameras;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LitShapesDemo
{
	public class LitShapesDemo : SharpDXApplication
	{
		#region Fields

		private Matrix Proj;
		private Matrix ViewProj;

		private Buffer ConstantBufferPerObject;
		private Buffer ConstantBufferPerFrame;

		private OrbitalControls CameraControls;

		private int BoxVerticesOffset;
		private int LargeBoxVerticesOffset;
		private int GridVerticesOffset;
		private int SphereVerticesOffset;
		private int GeoSphereVerticesOffset;
		private int CylinderVerticesOffset;
		private int SkullVerticesOffset;

		private int BoxIndicesOffset;
		private int LargeBoxIndicesOffset;
		private int GridIndicesOffset;
		private int SphereIndicesOffset;
		private int GeoSphereIndicesOffset;
		private int CylinderIndicesOffset;
		private int SkullIndicesOffset;

		private int BoxIndexCount;
		private int LargeBoxIndexCount;
		private int GridIndexCount;
		private int SphereIndexCount;
		private int GeoSphereIndexCount;
		private int CylinderIndexCount;
		private int SkullIndexCount;

		private Material BoxMaterial;
		private Material LargeBoxMaterial;
		private Material GridMaterial;
		private Material SphereMaterial;
		private Material GeoSphereMaterial;
		private Material CylinderMaterial;
		private Material WavesMaterial;
		private Material SkullMaterial;

		private ShaderResourceView GridTexture;
		private ShaderResourceView WavesTexture;
		private ShaderResourceView CenterSphereTexture;
		private ShaderResourceView BoxTexture;
		private ShaderResourceView LargeBoxTexture;
		private ShaderResourceView CylinderTexture;
		private ShaderResourceView GeoSphereTexture;

		private DirectionalLight DirLight;
		private PointLight PointLight;
		private SpotLight SpotLight;

		private Matrix[] SphereWorld = new Matrix[10];
		private Matrix[] CylWorld = new Matrix[10];
		private Matrix BoxWorld;
		private Matrix LargeBoxWorld;
		private Matrix CenterSphereWorld;
		private Matrix SkullWorld;

		private Matrix WavesTexTransform;
		private Matrix GridTexTransform;
		private Matrix LargeBoxTexTransform;

		private Buffer VertexBuffer;
		private Buffer IndexBuffer;
		private VertexBufferBinding VertexBinding;

		private PixelShader PhongPS;

		private bool DepthPassEnabled = false;

		private Waves Waves;
		private Buffer WavesVB;
		private Buffer WavesIB;
		private VertexBufferBinding WavesVBBinding;

		private double Elapsed;
		private Random Random = new Random();

		#endregion

		public LitShapesDemo(GraphicsConfiguration configuration)
			: base(configuration, "Lighting Demo")
		{
			CameraControls = new OrbitalControls(RenderWindow, Vector3.Zero, 5, 500, 100);
			CameraControls.Install();

			CreateWaves();
			CreateGeometryBuffers();

			CreateLights();
			CreateMaterials();

			CreateWorldMatrices();

			InitResources();

			TargetsResized += OnBuffersResized;

			RenderWindow.KeyDown += (o, e) =>
			{
				if (e.KeyData == System.Windows.Forms.Keys.Z)
				{
					DepthPassEnabled = !DepthPassEnabled;
					Log.Info("DepthPassEnabled = " + DepthPassEnabled);
				}
			};

			Log.Info("DepthPassEnabled = " + DepthPassEnabled);
		}

		private void InitResources()
		{
			PhongPS = new PixelShader(Device, ShaderBytecode.FromFile("pixel.shd"));

			ShaderBytecode vsbytecode = ShaderBytecode.FromFile("vertex.shd");
			VertexShader vs = new VertexShader(Device, vsbytecode);

			InputLayout layout = new InputLayout(
				Device,
				vsbytecode,
				Vertex.GetInputElements()
			);

			Context.InputAssembler.InputLayout = layout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			Context.VertexShader.Set(vs);

			vsbytecode.Dispose();
			vs.Dispose();
			layout.Dispose();

			ConstantBufferPerFrame = new Buffer(
				Device,
				Marshal.SizeOf(typeof(CBPerFrame)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			ConstantBufferPerObject = new Buffer(
				Device,
				Marshal.SizeOf(typeof(CBPerObject)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.VertexShader.SetConstantBuffer(1, ConstantBufferPerObject);
			Context.PixelShader.SetConstantBuffers(0, 2, new Buffer[] { ConstantBufferPerFrame, ConstantBufferPerObject });

			GridTexture = ShaderResourceView.FromFile(Device, "Textures/sand.jpg");
			WavesTexture = ShaderResourceView.FromFile(Device, "Textures/water2.jpg");
			GeoSphereTexture = ShaderResourceView.FromFile(Device, "Textures/water1.dds");
			CenterSphereTexture = ShaderResourceView.FromFile(Device, "Textures/water2.dds");
			BoxTexture = ShaderResourceView.FromFile(Device, "Textures/rock.jpg");
			LargeBoxTexture = ShaderResourceView.FromFile(Device, "Textures/floor.dds");
			CylinderTexture = ShaderResourceView.FromFile(Device, "Textures/bricks.dds");

			// Tile textures
			WavesTexTransform = Matrix.Scaling(4);
			GridTexTransform = Matrix.Scaling(4);
			LargeBoxTexTransform = Matrix.Scaling(1.25f);
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
			GridMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.1f, 0.1f, 0.1f, 8.0f),
				Textured = true
			};

			BoxMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.3f, 0.3f, 0.3f, 1000.0f),
				Textured = true
			};

			LargeBoxMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.1f, 0.1f, 0.1f, 500f),
				Textured = true
			};

			SphereMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.8f, 0.8f, 0.8f, 100.0f),
				Textured = true
			};

			GeoSphereMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.8f, 0.8f, 0.8f, 100.0f),
				Textured = true
			};

			CylinderMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = Vector4.One,
				Specular = new Vector4(0.1f, 0.1f, 0.1f, 1f),
				Textured = true
			};

			WavesMaterial = new Material
			{
				Ambient = Vector4.One,
				Diffuse = new Vector4(0.8f, 0.8f, 0.9f, 1.0f),
				Specular = new Vector4(1.0f, 1.0f, 1.0f, 1000.0f),
				Textured = false
			};

			SkullMaterial = new Material
			{
				Ambient = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1.0f),
				Specular = new Vector4(0.8f, 0.8f, 0.8f, 1000f)
			};
		}

		private void CreateWaves()
		{
			Waves = new Waves(160, 160, 1f, 0.03f, 3.25f, 0.4f);

			// Create the index buffer.  The index buffer is fixed, so we only 
			// need to create and set once.
			uint[] indices = new uint[3 * Waves.TriangleCount];

			// Iterate over each quad.
			int m = Waves.RowCount;
			int n = Waves.ColumnCount;
			int k = 0;

			for (int i = 0; i < m - 1; i++)
			{
				for (int j = 0; j < n - 1; j++)
				{
					indices[k] = (uint) (i * n + j);
					indices[k + 1] = (uint) (i * n + j + 1);
					indices[k + 2] = (uint) ((i + 1) * n + j);

					indices[k + 3] = (uint) ((i + 1) * n + j);
					indices[k + 4] = (uint) (i * n + j + 1);
					indices[k + 5] = (uint) ((i + 1) * n + j + 1);

					k += 6; // next quad
				}
			}

			WavesIB = new Buffer(
				Device,
				3 * (int) Waves.TriangleCount * Marshal.SizeOf(typeof(uint)),
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.UpdateSubresource(indices, WavesIB);

			WavesVB = new Buffer(
				Device,
				(int) Waves.VertexCount * Marshal.SizeOf(typeof(Vertex)),
				ResourceUsage.Default,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			WavesVBBinding = new VertexBufferBinding(WavesVB, Marshal.SizeOf(typeof(Vertex)), 0);
		}

		private void CreateWorldMatrices()
		{
			BoxWorld = Matrix.Multiply(Matrix.Scaling(2.0f, 1.0f, 2.0f), Matrix.Translation(0.0f, 0.5f, 0.0f));
			LargeBoxWorld = Matrix.Translation(0.0f, -3f, 0.0f);
			CenterSphereWorld = Matrix.Multiply(Matrix.Scaling(2.0f, 2.0f, 2.0f), Matrix.Translation(0.0f, 2.0f, 0.0f));
			SkullWorld = Matrix.Multiply(Matrix.Scaling(5f), Matrix.Translation(0.0f, 20.0f, 0.0f));

			for (int i = 0; i < 5; i++)
			{
				CylWorld[2 * i] = Matrix.Translation(-5, 1.5f, -10 + i * 5);
				CylWorld[2 * i + 1] = Matrix.Translation(5, 1.5f, -10 + i * 5);

				Vector3 p1 = new Vector3(-5, 3.5f, -10 + i * 5);
				Vector3 p2 = new Vector3(5, 3.5f, -10 + i * 5);

				SphereWorld[2 * i] = Matrix.Translation(p1);
				SphereWorld[2 * i + 1] = Matrix.Translation(p2);
			}
		}

		private void CreateGeometryBuffers()
		{
			MeshData box = GeometryGenerator.CreateBox(1, 1, 1);
			MeshData largeBox = GeometryGenerator.CreateBox(15, 6, 26);
			MeshData grid = GeometryGenerator.CreateGrid(160, 160, 50, 50, true);
			MeshData sphere = GeometryGenerator.CreateSphere(0.5f, 100, 100);
			MeshData geoSphere = GeometryGenerator.CreateGeosphere(0.5f, 4);
			MeshData cylinder = GeometryGenerator.CreateCylinder(0.5f, 0.35f, 3, 100, 1);
			MeshData skull = GeometryGenerator.LoadModel("Models/skull.txt");

			BoxVerticesOffset = 0;
			LargeBoxVerticesOffset = BoxVerticesOffset + box.Vertices.Length;
			GridVerticesOffset = LargeBoxVerticesOffset + largeBox.Vertices.Length;
			SphereVerticesOffset = GridVerticesOffset + grid.Vertices.Length;
			GeoSphereVerticesOffset = SphereVerticesOffset + sphere.Vertices.Length;
			CylinderVerticesOffset = GeoSphereVerticesOffset + geoSphere.Vertices.Length;
			SkullVerticesOffset = CylinderVerticesOffset + cylinder.Vertices.Length;

			BoxIndexCount = box.Indices.Length;
			LargeBoxIndexCount = largeBox.Indices.Length;
			GridIndexCount = grid.Indices.Length;
			SphereIndexCount = sphere.Indices.Length;
			GeoSphereIndexCount = geoSphere.Indices.Length;
			CylinderIndexCount = cylinder.Indices.Length;
			SkullIndexCount = skull.Indices.Length;

			BoxIndicesOffset = 0;
			LargeBoxIndicesOffset = BoxIndicesOffset + BoxIndexCount;
			GridIndicesOffset = LargeBoxIndicesOffset + LargeBoxIndexCount;
			SphereIndicesOffset = GridIndicesOffset + GridIndexCount;
			GeoSphereIndicesOffset = SphereIndicesOffset + SphereIndexCount;
			CylinderIndicesOffset = GeoSphereIndicesOffset + GeoSphereIndexCount;
			SkullIndicesOffset = CylinderIndicesOffset + CylinderIndexCount;

			int totalVertices =
				box.Vertices.Length +
				largeBox.Vertices.Length +
				grid.Vertices.Length +
				sphere.Vertices.Length +
				geoSphere.Vertices.Length +
				cylinder.Vertices.Length +
				skull.Vertices.Length;

			int totalIndices =
				BoxIndexCount +
				LargeBoxIndexCount +
				GridIndexCount +
				SphereIndexCount +
				GeoSphereIndexCount +
				CylinderIndexCount +
				SkullIndexCount;

			VertexBuffer = new Buffer(
				Device,
				totalVertices * Marshal.SizeOf(typeof(Vertex)),
				ResourceUsage.Default,
				BindFlags.VertexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			List<Vertex> vertexList = new List<Vertex>();

			vertexList.AddRange(box.Vertices);
			vertexList.AddRange(largeBox.Vertices);
			vertexList.AddRange(grid.Vertices);
			vertexList.AddRange(sphere.Vertices);
			vertexList.AddRange(geoSphere.Vertices);
			vertexList.AddRange(cylinder.Vertices);
			vertexList.AddRange(skull.Vertices);

			Vertex[] vertexArray = vertexList.ToArray();

			Context.UpdateSubresource(vertexArray, VertexBuffer);

			IndexBuffer = new Buffer(
				Device,
				totalIndices * Marshal.SizeOf(typeof(uint)),
				ResourceUsage.Default,
				BindFlags.IndexBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			List<uint> indexList = new List<uint>();

			indexList.AddRange(box.Indices);
			indexList.AddRange(largeBox.Indices);
			indexList.AddRange(grid.Indices);
			indexList.AddRange(sphere.Indices);
			indexList.AddRange(geoSphere.Indices);
			indexList.AddRange(cylinder.Indices);
			indexList.AddRange(skull.Indices);

			uint[] indexArray = indexList.ToArray();

			Context.UpdateSubresource(indexArray, IndexBuffer);

			VertexBinding = new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0);
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			Proj = Matrix.PerspectiveFovLH(0.25f * (float) Math.PI, (float) newWidth / newHeight, 0.1f, 10000f);
		}

		protected override void UpdateScene(double delta)
		{
			ViewProj = Matrix.Multiply(CameraControls.GetViewMatrix(), Proj);

			DirLight.Direction = Vector3.Transform(DirLight.Direction, Matrix3x3.RotationY((float) (Math.PI * delta / 10)));

			PointLight.Position = Vector3.Transform(PointLight.Position, Matrix3x3.RotationY((float) (Math.PI * delta / 5)));
			PointLight.Position.Y = GeometryGenerator.GetHillHeight(PointLight.Position.X, PointLight.Position.Z) + 8.0f;

			Vector3 cameraPosition = CameraControls.GetCameraPosition();

			SpotLight.Position = cameraPosition;
			SpotLight.Direction = Vector3.Normalize(Vector3.Negate(cameraPosition));

			CBPerFrame cbPerFrame = new CBPerFrame
			{
				DirLight = DirLight,
				PointLight = PointLight,
				SpotLight = SpotLight,
				ViewerPosition = cameraPosition
			};

			Context.UpdateSubresource(ref cbPerFrame, ConstantBufferPerFrame);

			//
			// Every quarter second, generate a random wave.
			//
			Elapsed += delta;

			if (Elapsed >= 0.25)
			{
				int i = 5 + Random.Next(150);
				int j = 5 + Random.Next(150);

				float r = Random.NextFloat(-2, 2);

				Waves.Disturb(i, j, r);
				Elapsed = 0;
			}

			Waves.Update((float) delta);

			// Animate water texture
			WavesTexTransform = Matrix.Multiply(WavesTexTransform, Matrix.Translation(0.05f * (float) delta, 0.1f * (float) delta, 0));
		}

		protected override void RenderScene()
		{
			base.RenderScene();

			#region Depth

			if (DepthPassEnabled)
			{
				Context.PixelShader.Set(null);
				Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.Default;

				DrawShapes();
				DrawWaves();
			}

			#endregion

			#region Color

			Context.PixelShader.Set(PhongPS);

			Context.OutputMerger.DepthStencilState =
				DepthPassEnabled ?
					PipelineStates.DepthStencil.ColorPass :
					PipelineStates.DepthStencil.Default;

			DrawShapes();
			DrawWaves();

			#endregion
		}

		private void DrawShapes()
		{
			Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, VertexBinding);

			Context.Rasterizer.State = PipelineStates.Rasterizer.DisableBackfaceCulling;

			// Grid
			SetPerObjectState(Matrix.Translation(0, -5, 0), GridMaterial, GridTexture, GridTexTransform);
			Context.DrawIndexed(GridIndexCount, GridIndicesOffset, GridVerticesOffset);

			Context.Rasterizer.State = PipelineStates.Rasterizer.Default;

			// Box
			SetPerObjectState(BoxWorld, BoxMaterial, BoxTexture, Matrix.Identity);
			Context.DrawIndexed(BoxIndexCount, BoxIndicesOffset, BoxVerticesOffset);

			// LargeBox
			SetPerObjectState(LargeBoxWorld, LargeBoxMaterial, LargeBoxTexture, LargeBoxTexTransform);
			Context.DrawIndexed(LargeBoxIndexCount, LargeBoxIndicesOffset, LargeBoxVerticesOffset);

			// Center sphere
			SetPerObjectState(CenterSphereWorld, SphereMaterial, CenterSphereTexture, Matrix.Identity);
			Context.DrawIndexed(SphereIndexCount, SphereIndicesOffset, SphereVerticesOffset);

			// Skull
			SetPerObjectState(SkullWorld, SkullMaterial, null, Matrix.Identity);
			Context.DrawIndexed(SkullIndexCount, SkullIndicesOffset, SkullVerticesOffset);

			// Cylinders
			for (int i = 0; i < CylWorld.Length; i++)
			{
				SetPerObjectState(CylWorld[i], CylinderMaterial, CylinderTexture, Matrix.Identity);
				Context.DrawIndexed(CylinderIndexCount, CylinderIndicesOffset, CylinderVerticesOffset);
			}

			// Cylinder spheres
			Context.PixelShader.SetShaderResource(0, GeoSphereTexture);
			for (int i = 0; i < CylWorld.Length; i++)
			{
				SetPerObjectState(SphereWorld[i], GeoSphereMaterial, GeoSphereTexture, Matrix.Identity);
				Context.DrawIndexed(GeoSphereIndexCount, GeoSphereIndicesOffset, GeoSphereVerticesOffset);
			}
		}

		private void DrawWaves()
		{
			Context.UpdateSubresource(Waves.vertices, WavesVB);

			Context.InputAssembler.SetIndexBuffer(WavesIB, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, WavesVBBinding);

			Context.Rasterizer.State = PipelineStates.Rasterizer.DisableBackfaceCulling;
			Context.OutputMerger.BlendState = PipelineStates.Blend.AlphaBlend;
			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.DisableDepthWrites;

			SetPerObjectState(Matrix.Translation(0, -5, 0), WavesMaterial, WavesTexture, WavesTexTransform);
			Context.DrawIndexed(Waves.TriangleCount * 3, 0, 0);

			Context.Rasterizer.State = PipelineStates.Rasterizer.Default;
			Context.OutputMerger.BlendState = PipelineStates.Blend.Default;
			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.Default;
		}

		private void SetPerObjectState(Matrix world, Material material, ShaderResourceView texture, Matrix textureTransform)
		{
			CBPerObject cbPerObject = new CBPerObject
			{
				World = Matrix.Transpose(world),
				WorldInvTranspose = Matrix.Invert(world),
				WorldViewProj = Matrix.Transpose(Matrix.Multiply(world, ViewProj)),
				TextureTransform = Matrix.Transpose(textureTransform),
				Material = material
			};

			Context.UpdateSubresource(ref cbPerObject, ConstantBufferPerObject);
			Context.PixelShader.SetShaderResource(0, texture);
			Context.PixelShader.SetSampler(0, PipelineStates.Sampler.WrappedAnisotropic);
		}
	}
}
