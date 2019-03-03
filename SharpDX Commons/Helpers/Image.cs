using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11Device = SharpDX.Direct3D11.Device;

namespace SharpDXCommons.Helpers
{
	/// <summary>
	/// Wraps a 2D texture, different views to it, and convenience methods
	/// </summary>
	public class Image : IDisposable
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public Texture2D Texture { get; private set; }

		public ShaderResourceView SRV
		{
			get
			{
				if (ShaderResourceView != null)
				{
					return ShaderResourceView;
				}
				else
				{
					ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
					{
						Format = SRVFormat,
						Dimension = SampleDesc.Count == 1 ? 
							ShaderResourceViewDimension.Texture2D
							: ShaderResourceViewDimension.Texture2DMultisampled
					};

					srvDesc.Texture2D.MipLevels = 1;
					srvDesc.Texture2D.MostDetailedMip = 0;

					ShaderResourceView = new ShaderResourceView(Device, Texture, srvDesc);

					return ShaderResourceView;
				}
			}
		}

		public RenderTargetView RTV
		{
			get
			{
				if (RenderTargetView != null)
				{
					return RenderTargetView;
				}
				else
				{
					RenderTargetViewDescription rtvDesc = new RenderTargetViewDescription
					{
						Format = RTVFormat,
						Dimension = SampleDesc.Count == 1 ? 
							RenderTargetViewDimension.Texture2D
							: RenderTargetViewDimension.Texture2DMultisampled
					};

					rtvDesc.Texture2D.MipSlice = 0;

					RenderTargetView = new RenderTargetView(Device, Texture, rtvDesc);

					return RenderTargetView;
				}
			}
		}

		public DepthStencilView DSV
		{
			get
			{
				if (DepthStencilView != null)
				{
					return DepthStencilView;
				}
				else
				{
					DepthStencilViewDescription dsvDesc = new DepthStencilViewDescription
					{
						Format = DSVFormat,
						Dimension = SampleDesc.Count == 1 ?
							DepthStencilViewDimension.Texture2D
							: DepthStencilViewDimension.Texture2DMultisampled,
						Flags = DepthStencilViewFlags.None
					};

					dsvDesc.Texture2D.MipSlice = 0;

					DepthStencilView = new DepthStencilView(Device, Texture, dsvDesc);

					return DepthStencilView;
				}
			}
		}

		private D3D11Device Device;

		private ShaderResourceView ShaderResourceView = null;
		private RenderTargetView RenderTargetView = null;
		private DepthStencilView DepthStencilView = null;
		
		private Format TexFormat;
		private Format SRVFormat;
		private Format RTVFormat;
		private Format DSVFormat;

		private SampleDescription SampleDesc;

		public Image(D3D11Device device, int width, int height, Format texFormat, Format srvFormat, Format rtvFormat, Format dsvFormat)
			: this(device, width, height, texFormat, srvFormat, rtvFormat, dsvFormat, new SampleDescription { Count = 1, Quality = 0 }) {}

		public Image(D3D11Device device, int width, int height, Format texFormat, Format srvFormat, Format rtvFormat, Format dsvFormat, SampleDescription sampleDesc)
		{
			Device = device;
			TexFormat = texFormat;
			SRVFormat = srvFormat;
			RTVFormat = rtvFormat;
			DSVFormat = dsvFormat;
			SampleDesc = sampleDesc;

			Width = width;
			Height = height;

			Texture2DDescription description = new Texture2DDescription
			{
				ArraySize = 1,
				BindFlags = IsValidDepthStencilFormat(texFormat) ?
					BindFlags.ShaderResource | BindFlags.DepthStencil
					: BindFlags.ShaderResource | BindFlags.RenderTarget,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = texFormat,
				Width = width,
				Height = height,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = sampleDesc,
				Usage = ResourceUsage.Default
			};

			Texture = new Texture2D(device, description);
		}

		private bool IsValidDepthStencilFormat(Format texFormat)
		{
			switch(texFormat)
			{
				case Format.R16_Typeless:
				case Format.D16_UNorm:
				case Format.R32_Typeless:
				case Format.D32_Float:
				case Format.R24G8_Typeless:
				case Format.D24_UNorm_S8_UInt:
				case Format.R32G8X24_Typeless:
				case Format.D32_Float_S8X24_UInt:
					return true;
				default:
					return false;
			}
		}

		public void SetViewport(DeviceContext context, float minDepth = 0.0f, float maxDepth = 1.0f)
		{
			context.Rasterizer.SetViewport(0, 0, Width, Height, minDepth, maxDepth);
		}

		public void Resize(int newWidth, int newHeight)
		{
			// Save Texture2DDescription
			Texture2DDescription saveDesc = Texture.Description;

			// Reset everything
			Dispose();

			// New dimensions
			saveDesc.Width = newWidth;
			saveDesc.Height = newHeight;

			// Views created at first property access
			Texture = new Texture2D(Device, saveDesc);
		}

		public void Dispose()
		{
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}

			if (ShaderResourceView != null)
			{
				ShaderResourceView.Dispose();
				ShaderResourceView = null;
			}
			if (RenderTargetView != null)
			{
				RenderTargetView.Dispose();
				RenderTargetView = null;
			}

			if (DepthStencilView != null)
			{
				DepthStencilView.Dispose();
				DepthStencilView = null;
			}
		}
	}
}
