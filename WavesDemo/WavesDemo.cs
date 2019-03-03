using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons;
using SharpDXCommons.Cameras;
using Buffer = SharpDX.Direct3D11.Buffer;
using Waves = WavesDemo.Waves;

namespace WavesDemo
{
	struct Vertex
	{
		public Vector3 Position;
		public Color4 Color;
	}

	struct ShaderConstant
	{
		public Matrix gWorldViewProj;
	}

	public class WavesDemo : SharpDXApplication
	{
		private Buffer constantBuffer;

		private Matrix view = Matrix.Identity;
		private Matrix proj = Matrix.Identity;
				
		private static int wavesWidth = 200;
		private Waves waves;

		private OrbitalControls cameraControls;

		private Buffer landVB;
		private Buffer landIB;
		private int landIndexCount;
		private Buffer wavesVB;
		private Buffer wavesIB;

		private double t = 0;

		private Random random = new Random();

		public WavesDemo(GraphicsConfiguration config) : base(config, "WavesDemo")
		{
			Init();
			TargetsResized += OnBuffersResized;
		}

		private void Init()
		{
			BuildLandGeometryBuffers();
			BuildWavesGeometryBuffers();

			PixelShader ps = new PixelShader(Device, ShaderBytecode.FromFile("pixel.shd"));
			ShaderBytecode vsbytecode = ShaderBytecode.FromFile("vertex.shd");
			VertexShader vs = new VertexShader(Device, vsbytecode);
			InputLayout layout = new InputLayout(
				Device,
				vsbytecode,
				new InputElement[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
					new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
				}
			);

			Context.InputAssembler.InputLayout = layout;
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			Context.VertexShader.Set(vs);
			Context.PixelShader.Set(ps);
			
			vsbytecode.Dispose();
			vs.Dispose();
			ps.Dispose();
			layout.Dispose();

			Context.Rasterizer.State = new RasterizerState(
				Device,
				new RasterizerStateDescription
				{
					CullMode = CullMode.Back,
					DepthBias = 0,
					DepthBiasClamp = 0.0f,
					FillMode = FillMode.Solid,
					IsAntialiasedLineEnabled = false,
					IsDepthClipEnabled = false,
					IsFrontCounterClockwise = false,
					IsMultisampleEnabled = false,
					IsScissorEnabled = false,
					SlopeScaledDepthBias = 0.0f
				}
			);

			constantBuffer = new Buffer(
				Device,
				Marshal.SizeOf(typeof(ShaderConstant)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			Context.VertexShader.SetConstantBuffer(0, constantBuffer);

			cameraControls = new OrbitalControls(RenderWindow, Vector3.Zero, 50, 500, 200);
			cameraControls.Install();
		}
		
		private void BuildLandGeometryBuffers()
		{
			MeshData grid = GeometryGenerator.CreateGrid(160.0f, 160.0f, 100, 100);

			landIndexCount = grid.Indices.Length;
			Vertex[] vertices = new Vertex[grid.Vertices.Length];
			
			for (int i = 0; i < grid.Vertices.Length; i++)
			{
				Vector3 p = grid.Vertices[i].Position;
				p.Y = GetHeight(p.X, p.Z);
				vertices[i].Position = p;

				// Color the vertex based on its height.
				if (p.Y < -10.0f)
				{
					// Sandy beach color.
					vertices[i].Color = Color.SandyBrown;
				}
				else if (p.Y < 5.0f)
				{
					// Light yellow-green.
					vertices[i].Color = Color.LightGreen;
				}
				else if (p.Y < 12.0f)
				{
					// Dark yellow-green.
					vertices[i].Color = Color.DarkGreen;
				}
				else if (p.Y < 20.0f)
				{
					// Dark brown.
					vertices[i].Color = Color.DarkOrange;
				}
				else
				{
					// White snow.
					vertices[i].Color = Color.White;
				}
			}

			DataStream verticesDS = DataStream.Create<Vertex>(vertices, true, false);
			DataStream indicesDS = DataStream.Create<uint>(grid.Indices, true, false);

			landVB = new Buffer(
				Device,
				verticesDS,
				new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = (int)verticesDS.Length,
					Usage = ResourceUsage.Immutable
				}
			);

			landIB = new Buffer(
				Device,
				indicesDS,
				new BufferDescription
				{
					BindFlags = BindFlags.IndexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = (int)indicesDS.Length,
					Usage = ResourceUsage.Immutable
				}
			);

			verticesDS.Dispose();
			indicesDS.Dispose();
		}

		private void BuildWavesGeometryBuffers()
		{
			waves = new Waves((uint) wavesWidth, (uint) wavesWidth, 0.8f, 0.03f, 6.5f, 2.0f);

			// Create the vertex buffer.  Note that we allocate space only, as
			// we will be updating the data every time step of the simulation.
			BufferDescription vbd = new BufferDescription
			{
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = Marshal.SizeOf(typeof(Vertex)) * (int)waves.VertexCount,
				Usage = ResourceUsage.Default
			};

			wavesVB = new Buffer(Device, vbd);
			Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(wavesVB, Marshal.SizeOf(typeof(Vertex)), 0));

			// Create the index buffer.  The index buffer is fixed, so we only 
			// need to create and set once.
			uint[] indices = new uint[3 * waves.TriangleCount];

			// Iterate over each quad.
			uint m = waves.RowCount;
			uint n = waves.ColumnCount;
			int k = 0;

			for (uint i = 0; i < m - 1; i++)
			{
				for (uint j = 0; j < n - 1; j++)
				{
					indices[k] = i * n + j;
					indices[k + 1] = i * n + j + 1;
					indices[k + 2] = (i + 1) * n + j;

					indices[k + 3] = (i + 1) * n + j;
					indices[k + 4] = i * n + j + 1;
					indices[k + 5] = (i + 1) * n + j + 1;

					k += 6; // next quad
				}
			}

			BufferDescription ibd = new BufferDescription
			{
				BindFlags = BindFlags.IndexBuffer,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = Marshal.SizeOf(typeof(uint)) * indices.Length,
				Usage = ResourceUsage.Immutable
			};

			DataStream indicesDS = DataStream.Create<uint>(indices, true, false);
			wavesIB = new Buffer(Device, indicesDS, ibd);

			Context.InputAssembler.SetIndexBuffer(wavesIB, Format.R32_UInt, 0);

			indicesDS.Dispose();
		}

		protected override void RenderScene()
		{
			base.RenderScene();

			UpdateWorldViewProj();
			
			Context.DrawIndexed(3 * (int) waves.TriangleCount, 0, 0);
		}

		protected override void UpdateScene(double delta)
		{
			view = cameraControls.GetViewMatrix();

			#region Обновляем волны

			//
			// Every quarter second, generate a random wave.
			//
			t += delta;

			if (t >= 0.25)
			{
				uint i = 5 + (uint)random.Next(wavesWidth - 10);
				uint j = 5 + (uint)random.Next(wavesWidth - 10);

				float r = random.NextFloat(0.5f, 1.0f);

				waves.Disturb(i, j, r);
				t = 0;
			}

			waves.Update(delta);

			Vertex[] vertices = new Vertex[waves.VertexCount];
			for (int i = 0; i < waves.VertexCount; i++)
			{
				Vertex vertex = new Vertex
				{
					Position = waves[i],
				};

				float wavelength = (580 - 460) * vertex.Position.Y + 460;

				vertex.Color = NiceFunctions.WavelengthToRGB(wavelength);

				vertices[i] = vertex;
			}

			Context.UpdateSubresource(vertices, wavesVB);

			#endregion
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			proj = Matrix.PerspectiveFovLH(0.25f * (float)Math.PI, (float)newWidth / newHeight, 1.0f, 5000f);
		}

		private void UpdateWorldViewProj()
		{
			ShaderConstant shConst = new ShaderConstant
			{
				gWorldViewProj = Matrix.Transpose(Matrix.Multiply(view, proj))
			};

			Context.UpdateSubresource(ref shConst, constantBuffer);
		}

		private float GetHeight(float x, float z)
		{
			return 0.3f * (z * (float)Math.Sin(0.1f * x) + x * (float)Math.Cos(0.1f * z));
		}
	}
}
