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
using Buffer = SharpDX.Direct3D11.Buffer;

namespace HillsDemo
{
	struct Vertex
	{
		public Vector3 Position;
		public Color4 Color;
	}

	[StructLayout(LayoutKind.Explicit, Size = 64)]
	struct ShaderConstant
	{
		[FieldOffset(0)]
		public Matrix gWorldViewProj;
	}

	public class HillsDemo : SharpDXApplication
	{
		private Buffer constantBuffer;

		private Matrix view = Matrix.Identity;
		private Matrix proj = Matrix.Identity;

		private float cameraTheta = 1.5f * (float) Math.PI;
		private float cameraPhi = 0.35f * (float) Math.PI;
		private float cameraRadius = 1000.0f;


		private Vertex[] Vertices;
		private uint[] Indices;


		public HillsDemo(GraphicsConfiguration config) : base(config, "HillsDemo")
		{
			Init();
			TargetsResized += OnBuffersResized;
		}

		private void Init()
		{
			MeshData grid = GeometryGenerator.CreateGrid(1000.0f, 1000.0f, 1000, 1000);
			
			Vertices = new Vertex[grid.Vertices.Length];
			Indices = grid.Indices;

			for (int i = 0; i < grid.Vertices.Length; i++)
			{
				Vector3 p = grid.Vertices[i].Position;
				p.Y = GetHeight(p.X, p.Z);
				Vertices[i].Position = p;
			}

			float[] heights = new float[Vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
				heights[i] = Vertices[i].Position.Y;

			float min = heights.Min();
			float max = heights.Max();
			
			for (int i = 0; i < grid.Vertices.Length; i++)
			{
				float height = Vertices[i].Position.Y - min;
				float percent = height / (max - min);
				float wavelength = (750 - 380) * percent + 380;

				Vertices[i].Color = NiceFunctions.WavelengthToRGB(wavelength);
			}

			DataStream verticesDS = DataStream.Create<Vertex>(Vertices, true, false);
			DataStream indicesDS = DataStream.Create<uint>(Indices, true, false);

			Buffer vb = new Buffer(
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

			Buffer ib = new Buffer(
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

			Context.InputAssembler.SetIndexBuffer(ib, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vb, Marshal.SizeOf(typeof(Vertex)), 0));

			Context.VertexShader.Set(vs);
			Context.PixelShader.Set(ps);

			verticesDS.Dispose();
			indicesDS.Dispose();
			vsbytecode.Dispose();
			vs.Dispose();
			ps.Dispose();
			layout.Dispose();

			/*Context.Rasterizer.State = new RasterizerState(
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
			);*/

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
		}

		protected override void RenderScene()
		{
			base.RenderScene();
			UpdateWorldViewProj();
			Context.DrawIndexed(Indices.Length, 0, 0);
		}

		protected override void UpdateScene(double delta)
		{
			// Convert Spherical to Cartesian coordinates.
			float x = (float)(cameraRadius * Math.Sin(cameraPhi) * Math.Cos(cameraTheta));
			float z = (float)(cameraRadius * Math.Sin(cameraPhi) * Math.Sin(cameraTheta));
			float y = cameraRadius * (float)Math.Cos(cameraPhi);

			// Build the view matrix.
			Vector3 eye = new Vector3(x, y, z);
			Vector3 target = Vector3.Zero;
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

			view = Matrix.LookAtLH(eye, target, up);

			//cameraPhi += (float) (Math.PI / 4 * delta);
			cameraTheta += (float)(Math.PI / 4 * delta);
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
			return 0.1f * (z * (float)Math.Sin(0.025 * x) + x * (float)Math.Cos(0.025 * z));
		}
	}
}
