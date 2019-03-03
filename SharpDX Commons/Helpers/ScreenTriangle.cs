using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11Buffer = SharpDX.Direct3D11.Buffer;
using System.Runtime.InteropServices;
using System;
using SharpDX.Direct3D;

namespace SharpDXCommons.Helpers
{
	/// <summary>
	/// Use to render a fullscreen texture
	/// </summary>
	public class ScreenTriangle : IDisposable
	{
		private struct Vertex
		{
			public Vector4 Position;
			public Vector2 TexCoord;
		}
		
		private D3D11Buffer VertexBuffer;
		private D3D11Buffer ShaderConstantBuffer;
		private InputLayout InputLayout;

		private ShaderBytecode VSBytecode;
		private VertexShader VS;
		private PixelShader PS;
		private PixelShader DepthPS;

		public ScreenTriangle(D3D11Device device)
		{
			VSBytecode = ShaderBytecode.FromFile("ScreenTriangleVS.shd");
			VS = new VertexShader(device, VSBytecode);
			PS = new PixelShader(device, ShaderBytecode.FromFile("ScreenTrianglePS.shd"));
			DepthPS = new PixelShader(device, ShaderBytecode.FromFile("ScreenTrianglePSDepth.shd"));

			InputLayout = new InputLayout(
				device,
				VSBytecode,
				new InputElement[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0)
				}
			);

			Vertex[] vertices = new Vertex[]
			{
				new Vertex { Position = new Vector4(-1, 1, 1, 1), TexCoord = new Vector2(0, 0) },
				new Vertex { Position = new Vector4(3, 1, 1, 1), TexCoord = new Vector2(2, 0) },
				new Vertex { Position = new Vector4(-1, -3, 1, 1), TexCoord = new Vector2(0, 2) }
			};

			DataStream verticesDS = DataStream.Create(vertices, true, false);

			VertexBuffer = new D3D11Buffer(
				device,
				verticesDS,
				new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = (int) verticesDS.Length,
					StructureByteStride = 0,
					Usage = ResourceUsage.Default
				}
			);
			
			verticesDS.Dispose();

			ShaderConstantBuffer = new D3D11Buffer(
				device,
				Marshal.SizeOf(typeof(Matrix)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);
		}

		public void DrawImage(DeviceContext context, ShaderResourceView image, SamplerState sampler, Matrix textureTransform)
		{
			context.InputAssembler.InputLayout = InputLayout;

			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

			context.VertexShader.Set(VS);
			context.PixelShader.Set(PS);

			context.PixelShader.SetShaderResource(0, image);
			context.PixelShader.SetSampler(0, sampler);

			textureTransform.Transpose();

			context.UpdateSubresource(ref textureTransform, ShaderConstantBuffer);
			context.VertexShader.SetConstantBuffers(0, ShaderConstantBuffer);

			context.Draw(3, 0);
		}

		public void DrawDepth(DeviceContext context, ShaderResourceView depthImage, SamplerState sampler, Matrix textureTransform)
		{
			context.InputAssembler.InputLayout = InputLayout;

			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

			context.VertexShader.Set(VS);
			context.PixelShader.Set(DepthPS);

			context.PixelShader.SetShaderResource(0, depthImage);
			context.PixelShader.SetSampler(0, sampler);

			textureTransform.Transpose();

			context.UpdateSubresource(ref textureTransform, ShaderConstantBuffer);
			context.VertexShader.SetConstantBuffers(0, ShaderConstantBuffer);

			context.Draw(3, 0);
		}

		public void Draw(DeviceContext context)
		{
			context.InputAssembler.InputLayout = InputLayout;

			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));

			context.Draw(3, 0);
		}

		public void Dispose()
		{
			if (VertexBuffer != null) VertexBuffer.Dispose();
			if (ShaderConstantBuffer != null) ShaderConstantBuffer.Dispose();
			if (InputLayout != null) InputLayout.Dispose();
			if (VS != null) VS.Dispose();
			if (PS != null) PS.Dispose();
			if (DepthPS != null) DepthPS.Dispose();
		}
	}
}
