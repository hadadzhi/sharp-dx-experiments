using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDXCommons
{
	public class ShadowMap : IDisposable
	{
		private Texture2D ShadowMapTexture;
		
		public ShaderResourceView ShaderResourceView { get; private set; }
		public DepthStencilView DepthStencilView { get; private set; }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public ShadowMap(Device device, float width, float height, int arraySize)
		{
			Width = (int) width;
			Height = (int) height;

			ShadowMapTexture = new Texture2D(
				device,
				new Texture2DDescription
				{
					ArraySize = arraySize,
					BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					Format = Format.R16_Typeless,
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
					Format = Format.R16_UNorm,
					Dimension = ShaderResourceViewDimension.Texture2DArray,
					Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
					{
						ArraySize = arraySize,
						FirstArraySlice = 0,
						MipLevels = 1,
						MostDetailedMip = 0
					}
				}
			);

			DepthStencilView = new DepthStencilView(
				device,
				ShadowMapTexture,
				new DepthStencilViewDescription
				{
					Dimension = DepthStencilViewDimension.Texture2DArray,
					Flags = DepthStencilViewFlags.None,
					Format = Format.D16_UNorm,
					Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource
					{
						ArraySize = arraySize,
						FirstArraySlice = 0,
						MipSlice = 0
					}
				}
			);
		}

		public void Dispose()
		{
			DepthStencilView.Dispose();
			ShaderResourceView.Dispose();
			ShadowMapTexture.Dispose();
		}
	}
}
