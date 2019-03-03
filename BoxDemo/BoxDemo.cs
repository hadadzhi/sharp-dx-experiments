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

namespace BoxDemoComeBack
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

	public class BoxDemo : SharpDXApplication
	{
		private Buffer constantBuffer;

		private Matrix view = Matrix.Identity;
		private Matrix proj = Matrix.Identity;

		private float cameraTheta = 1.5f * (float) Math.PI;
		private float cameraPhi = 0.35f * (float) Math.PI;
		private float cameraRadius = 5.0f;

		public BoxDemo(GraphicsConfiguration config) : base(config, "BoxDemoComeBack") 
		{ 
			Init();
			TargetsResized += OnBuffersResized;
		}

		private void Init()
		{
			Vertex[] vertices = new Vertex[]
			{
				new Vertex() { Position = new Vector3(-1.0f, -1.0f, -1.0f), Color = Color.White },
				new Vertex() { Position = new Vector3(-1.0f, +1.0f, -1.0f), Color = Color.Black },
				new Vertex() { Position = new Vector3(+1.0f, +1.0f, -1.0f), Color = Color.Red },
				new Vertex() { Position = new Vector3(+1.0f, -1.0f, -1.0f), Color = Color.Green },
				new Vertex() { Position = new Vector3(-1.0f, -1.0f, +1.0f), Color = Color.Blue },
				new Vertex() { Position = new Vector3(-1.0f, +1.0f, +1.0f), Color = Color.Yellow },
				new Vertex() { Position = new Vector3(+1.0f, +1.0f, +1.0f), Color = Color.Cyan },
				new Vertex() { Position = new Vector3(+1.0f, -1.0f, +1.0f), Color = Color.Magenta }
			};

			uint[] indices = new uint[] {
				// front face
				0, 1, 2,
				0, 2, 3,

				// back face
				4, 6, 5,
				4, 7, 6,

				// left face
				4, 5, 1,
				4, 1, 0,

				// right face
				3, 2, 6,
				3, 6, 7,

				// top face
				1, 5, 6,
				1, 6, 2,

				// bottom face
				4, 0, 3, 
				4, 3, 7
			};

			DataStream verticesDS = DataStream.Create<Vertex>(vertices, true, false);
			DataStream indicesDS = DataStream.Create<uint>(indices, true, false);

			Buffer vb = new Buffer(
				Device,
				verticesDS,
				new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = (int) verticesDS.Length,
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
					SizeInBytes = (int) indicesDS.Length,
					Usage = ResourceUsage.Immutable
				}
			);

			indicesDS.Dispose();
			verticesDS.Dispose();

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

			vsbytecode.Dispose();
			vs.Dispose();
			ps.Dispose();
			layout.Dispose();

			//Context.Rasterizer.State = new RasterizerState(
			//    Device, 
			//    new RasterizerStateDescription
			//    {
			//        CullMode = CullMode.None,
			//        DepthBias = 0,
			//        DepthBiasClamp = 0.0f,
			//        FillMode = FillMode.Solid,
			//        IsAntialiasedLineEnabled = false,
			//        IsDepthClipEnabled = false,
			//        IsFrontCounterClockwise = false,
			//        IsMultisampleEnabled = false,
			//        IsScissorEnabled = false,
			//        SlopeScaledDepthBias = 0.0f
			//    }
			//);

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
			Context.DrawIndexed(36, 0, 0);
		}

		protected override void UpdateScene(double delta)
		{
			// Convert Spherical to Cartesian coordinates.
			float x = (float) (cameraRadius * Math.Sin(cameraPhi) * Math.Cos(cameraTheta));
			float z = (float) (cameraRadius * Math.Sin(cameraPhi) * Math.Sin(cameraTheta));
			float y = cameraRadius * (float) Math.Cos(cameraPhi);

			// Build the view matrix.
			Vector3 eye = new Vector3(x, y, z);
			Vector3 target = Vector3.Zero;
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

			view = Matrix.LookAtLH(eye, target, up);

			//cameraPhi += (float) (Math.PI / 4 * delta);
			cameraTheta += (float) (Math.PI / 4 * delta);
		}

		private void OnBuffersResized(int newWidth, int newHeight)
		{
			proj = Matrix.PerspectiveFovLH(0.25f * (float)Math.PI, (float) newWidth / newHeight, 1.0f, 1000f);
		}

		private void UpdateWorldViewProj()
		{
			ShaderConstant shConst = new ShaderConstant
			{
				gWorldViewProj = Matrix.Transpose(Matrix.Multiply(view, proj))
			};

			Context.UpdateSubresource(ref shConst, constantBuffer);
		}
	}
}
