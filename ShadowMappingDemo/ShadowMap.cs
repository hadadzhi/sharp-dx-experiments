using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;

namespace ShadowMappingDemo
{
	class ShadowMap : IDisposable
	{
		private Texture2D ShadowMapTexture;
		
		public ShaderResourceView ShaderResourceView { get; private set; }
		public DepthStencilView DepthStencilView { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public ShadowMap(Device device, float width, float height)
		{
			Width = (int) width;
			Height = (int) height;

			ShadowMapTexture = new Texture2D(
				device,
				new Texture2DDescription
				{
					ArraySize = 1,
					BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					Format = Format.R32_Typeless,
					Width = Width,
					Height = Height,
					MipLevels = 1,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Default,
					SampleDescription = new SampleDescription { Count = 1, Quality = 0 }
				}
			);

			ShaderResourceView = new ShaderResourceView(
				device,
				ShadowMapTexture,
				new ShaderResourceViewDescription
				{
					Format = Format.R32_Float,
					Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled
				}
			);

			DepthStencilView = new DepthStencilView(
				device,
				ShadowMapTexture,
				new DepthStencilViewDescription
				{
					Dimension = DepthStencilViewDimension.Texture2DMultisampled,
					Flags = DepthStencilViewFlags.None,
					Format = Format.D32_Float
				}
			);
		}

		public void Dispose()
		{
			ShaderResourceView.Dispose();
			DepthStencilView.Dispose();
			ShadowMapTexture.Dispose();
		}
	}
}
