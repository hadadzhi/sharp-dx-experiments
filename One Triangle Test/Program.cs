using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using SharpDXCommons;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace One_Triangle_Test
{
	class OneTriangleTest : SharpDXApplication
	{
		public OneTriangleTest(GraphicsConfiguration configuration)
			: base(configuration, "One Triangle")
		{
			Vertex[] triangleVertices = 
			{
				new Vertex { Position = new Vector3(0.0f, 0.5f, 0.5f), Color = new Color4(1.0f, 0, 0, 1.0f) },
				new Vertex { Position = new Vector3(0.5f, -0.5f, 0.5f), Color = new Color4(0, 1.0f, 0, 1.0f) },
				new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.5f), Color = new Color4(0, 0, 1.0f, 1.0f) }
			};

			//uint[] triangleIndices = { 0, 1, 2 };

			//DataStream triangleVerticesDS = DataStream.Create<Vertex>(triangleVertices, true, false);
			//DataStream triangleIndicesDS = DataStream.Create<uint>(triangleIndices, true, false);

			//Buffer triangleVB = new Buffer(
			//	Device, 
			//	triangleVerticesDS, 
			//	new BufferDescription
			//	{
			//		BindFlags = BindFlags.VertexBuffer,
			//		CpuAccessFlags = CpuAccessFlags.None,
			//		OptionFlags = ResourceOptionFlags.None,
			//		SizeInBytes = (int) triangleVerticesDS.Length,
			//		Usage = ResourceUsage.Immutable
			//	}
			//);

			//Buffer triangleIB = new Buffer(
			//	Device, 
			//	triangleIndicesDS, 
			//	new BufferDescription
			//	{
			//		BindFlags = BindFlags.IndexBuffer,
			//		CpuAccessFlags = CpuAccessFlags.None,
			//		OptionFlags = ResourceOptionFlags.None,
			//		SizeInBytes = (int) triangleIndicesDS.Length,
			//		Usage = ResourceUsage.Immutable
			//	}
			//);

			//triangleIndicesDS.Dispose();
			//triangleVerticesDS.Dispose();
			
			Vertex[] vertices = new Vertex[]
			{
				new Vertex { Position = new Vector3(0.0f, 0.5f, 0.5f), Color = new Color4(1.0f, 0, 0, 1.0f) },
				new Vertex { Position = new Vector3(0.5f, -0.5f, 0.5f), Color = new Color4(1.0f, 0, 0, 1.0f) },
				new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.5f), Color = new Color4(1.0f, 0, 0, 1.0f) }
			};

			DataStream verticesds = DataStream.Create(vertices, true, false);

			Buffer triangleVB = new Buffer(
				Device,
				verticesds,
				new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = (int) verticesds.Length,
					StructureByteStride = 0,
					Usage = ResourceUsage.Default
				}
			);

			verticesds.Dispose();

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
			Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

			//Context.InputAssembler.SetIndexBuffer(triangleIB, Format.R32_UInt, 0);
			Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVB, Marshal.SizeOf(typeof(Vertex)), 0));

			Context.VertexShader.Set(vs);
			Context.PixelShader.Set(ps);

			vsbytecode.Dispose();
			vs.Dispose();
			ps.Dispose();
			layout.Dispose();
		}

		protected override void RenderScene()
		{
			base.RenderScene();
			//Context.DrawIndexed(3, 0, 0);
			Context.Draw(3, 0);
		}
	}

	struct Vertex
	{
		public Vector3 Position;
		public Color4 Color;
	}

	class Program
	{
		public static void Main()
		{
			Application.EnableVisualStyles();

			GraphicsSettingsDialog settings = new GraphicsSettingsDialog();

			if (settings.ShowDialog() == DialogResult.OK)
			{
				OneTriangleTest test = new OneTriangleTest(settings.Configuration);
				test.Run();
			}
		}
	}
}
