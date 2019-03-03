using System;
using SharpDXCommons;
using SharpDXCommons.Helpers;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Windows.Forms;

namespace HDRDemo
{
	class HDRDemo : SharpDXApplication
	{
		private ShaderResourceView HDRImage;
		private bool HDREnabled = true;
		private VertexShader ToneMappingVS;
		private PixelShader ToneMappingPS;
		private Buffer ExposureConstBuffer;
		private float Exposure = 1.0f;
		private ScreenTriangle Tri;

		public HDRDemo(GraphicsConfiguration configuration) 
			: base(configuration, "HDR Tone Mapping Operators")
		{
			ImageLoadInformation loadInfo = new ImageLoadInformation
			{
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Filter = FilterFlags.None,
				Format = Format.R16G16B16A16_Float,
				OptionFlags = ResourceOptionFlags.None,
				MipLevels = 1,
				FirstMipLevel = 0,
				Usage = ResourceUsage.Immutable
			};

			HDRImage = ShaderResourceView.FromFile(Device, "HDR.dds", loadInfo);
			Tri = new ScreenTriangle(Device);

			ToneMappingVS = new VertexShader(Device, ShaderBytecode.FromFile("ToneMappingVS.shd"));
			ToneMappingPS = new PixelShader(Device, ShaderBytecode.FromFile("ToneMappingPS.shd"));

			ExposureConstBuffer = new Buffer(
				Device,
				16,
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			RenderWindow.KeyDown += OnKeyDown;
			RenderWindow.MouseWheel += OnMouseWheel;
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			switch (e.Delta / Math.Abs(e.Delta))
			{
				case 1:
				{
					Exposure *= 2;
					Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
					Log.Info(String.Format("Exposure: {0}", Exposure));
					break;
				}
				case -1:
				{
					Exposure /= 2;
					Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
					Log.Info(String.Format("Exposure: {0}", Exposure));
					break;
				}
			}
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.E:
				{
					HDREnabled = !HDREnabled;
					Log.Info(String.Format("HDR: {0}", HDREnabled));
					break;
				}
				case Keys.PageUp:
				{
					Exposure *= 2;
					Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
					Log.Info(String.Format("Exposure: {0}", Exposure));
					break;
				}
				case Keys.PageDown:
				{
					Exposure /= 2;
					Exposure = MathUtil.Clamp(Exposure, 1.0f / 1024, 1024);
					Log.Info(String.Format("Exposure: {0}", Exposure));
					break;
				}
			}
		}

		protected override void RenderScene()
		{
			Context.ClearState();
			Context.ClearRenderTargetView(BackbufferRTV, Color4.Black);
			Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

			Context.OutputMerger.SetTargets(DepthStencilView, BackbufferRTV);
			Context.OutputMerger.DepthStencilState = PipelineStates.DepthStencil.DisableDepthStencil;

			if (HDREnabled)
			{
				Context.VertexShader.Set(ToneMappingVS);
				Context.PixelShader.Set(ToneMappingPS);

				Context.PixelShader.SetShaderResource(0, HDRImage);
				Context.PixelShader.SetConstantBuffer(0, ExposureConstBuffer);

				Context.UpdateSubresource(ref Exposure, ExposureConstBuffer);

				Context.Rasterizer.SetViewport(DefaultViewport);

				Tri.Draw(Context);
			}
			else
			{
				Context.Rasterizer.SetViewport(DefaultViewport);
				Tri.DrawImage(Context, HDRImage, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
			}
		}
	}
}
