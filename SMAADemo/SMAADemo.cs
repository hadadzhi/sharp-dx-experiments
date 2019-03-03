using SharpDX;
using SharpDX.Direct3D11;

using SharpDXCommons;
using SharpDXCommons.Helpers;
using System.Drawing;
using System.Windows.Forms;
using System;

using Image = SharpDXCommons.Helpers.Image;
using SharpDX.DXGI;

using SharpSMAA;

namespace SMAADemo
{
	class SMAADemo : SharpDXApplication
	{
		private enum ShowModes { Color, Depth, Edges, Weights }

		private ScreenTriangle Tri;
		private ShaderResourceView SourceColorSRV;
		private ShaderResourceView SourceDepthSRV;
		private Matrix TexCoordTransform = Matrix.Identity;
		private int ImageWidth;
		private int ImageHeight;

		private bool SMAAEnabled = true;
		private SMAA SMAA;
		private SMAA.Inputs SMAAInput = SMAA.Inputs.Color;
		private SMAA.Modes SMAAMode = SMAA.Modes.SMAA_1x;
		private SMAA.Presets SMAAPreset = SMAA.Presets.Ultra;
		private SMAARenderTarget SMAATarget;
		private Image SMAAResult;
		private ShowModes ShowMode = ShowModes.Color;
		
		private SamplerState Sampler;
		
		public SMAADemo(GraphicsConfiguration conf) : base(conf, "SMAA Demo")
		{
			Tri = new ScreenTriangle(Device);
			Sampler = new SamplerState(
				Device,
				new SamplerStateDescription
				{
					AddressU = TextureAddressMode.Border,
					AddressV = TextureAddressMode.Border,
					AddressW = TextureAddressMode.Border,
					BorderColor = Color4.Black,
					Filter = Filter.MinLinearMagMipPoint
				}
			);

			TargetsResized += FitImage;
			RenderWindow.KeyDown += OnKeyDown;
			
			LoadImage();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.D1:
				{
					SMAAPreset = SMAA.Presets.Low;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.D2:
				{
					SMAAPreset = SMAA.Presets.Medium;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.D3:
				{
					SMAAPreset = SMAA.Presets.High;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.D4:
				{
					SMAAPreset = SMAA.Presets.Ultra;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.S:
				{
					SMAAInput = SMAA.Inputs.Depth;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.D:
				{
					SMAAInput = SMAA.Inputs.Luma;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.F:
				{
					SMAAInput = SMAA.Inputs.Color;
					UpdateSMAA(ImageWidth, ImageHeight);
					break;
				}
				case Keys.E:
				{
					SMAAEnabled = !SMAAEnabled;
					break;
				}
				case Keys.O:
				{
					LoadImage();
					break;
				}
				case Keys.Z:
				{
					ShowMode = ShowModes.Color;
					break;
				}
				case Keys.X:
				{
					ShowMode = ShowModes.Depth;
					break;
				}
				case Keys.C:
				{
					ShowMode = ShowModes.Edges;
					break;
				}
				case Keys.V:
				{
					ShowMode = ShowModes.Weights;
					break;
				}
				case Keys.R:
				{
					ResizeToImageSize();
					break;
				}
			}
		}

		private void UpdateSMAA(int newWidth, int newHeight)
		{
			if (SMAA != null) SMAA.Dispose();
			if (SMAATarget != null) SMAATarget.Dispose();
			if (SMAAResult != null) SMAAResult.Dispose();
			SMAA = new SMAA(Device, newWidth, newHeight, SMAAMode, SMAAPreset, SMAAInput, false, false);
			SMAATarget = new SMAARenderTarget(Device, newWidth, newHeight);
			SMAAResult = new Image(Device, newWidth, newHeight, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown);
		}

		private void LoadImage()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			
			if (dialog.ShowDialog() != DialogResult.None && dialog.FileName != null && dialog.FileName.Length > 0)
			{
				if (SourceColorSRV != null) { SourceColorSRV.Dispose(); SourceDepthSRV = null; }
				if (SourceDepthSRV != null) { SourceDepthSRV.Dispose(); SourceDepthSRV = null; }
				ImageLoadInformation loadInfo = new ImageLoadInformation
				{
					BindFlags = BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					Filter = FilterFlags.SRgbIn | FilterFlags.Point,
					FirstMipLevel = 0,
					Format = Format.R8G8B8A8_UNorm_SRgb,
					OptionFlags = ResourceOptionFlags.None
				};
				SourceColorSRV = ShaderResourceView.FromFile(Device, dialog.FileName, loadInfo);

				try // Try to load depth
				{
					string depthFileName = dialog.FileName.Substring(0, dialog.FileName.Length - 3) + "dds";
					SourceDepthSRV = ShaderResourceView.FromFile(Device, depthFileName);
				}
				catch (Exception)
				{
					// No depth available
				}
				
				ImageWidth = ((Texture2D) SourceColorSRV.Resource.NativePointer).Description.Width;
				ImageHeight = ((Texture2D) SourceColorSRV.Resource.NativePointer).Description.Height;

				UpdateSMAA(ImageWidth, ImageHeight);
				ResizeToImageSize();
			}

			dialog.Dispose();
		}

		private void ResizeToImageSize()
		{
			if (!IsFullscreen && !IsFullscreenWindow)
			{
				RenderWindow.ResizeClientArea(ImageWidth, ImageHeight);
			}
		}

		private void FitImage(int newWidth, int newHeight)
		{
			float imageRatio = (float) ImageWidth / ImageHeight;
			float screenRatio = (float) newWidth / newHeight;

			if (screenRatio > imageRatio)
			{
				float scrWidthInTexCoord = screenRatio / imageRatio;
				TexCoordTransform =
					Matrix.Scaling(scrWidthInTexCoord, 1, 1)
					* Matrix.Translation(-(scrWidthInTexCoord - 1) / 2, 0, 0);
			}
			else if (screenRatio < imageRatio)
			{
				float scrHeightInTexCoord = imageRatio / screenRatio;
				TexCoordTransform = 
					Matrix.Scaling(1, scrHeightInTexCoord, 1)
					* Matrix.Translation(0, -(scrHeightInTexCoord - 1) / 2, 0);
			}
			else
			{
				TexCoordTransform = Matrix.Identity;
			}
		}

		protected override void RenderScene()
		{
			if (SourceColorSRV == null)
			{
				return;
			}

			Context.ClearState();
			Context.ClearRenderTargetView(BackbufferRTV, Color4.Black);
			Context.ClearRenderTargetView(SMAATarget.RTV, Color4.Black);
			Context.ClearRenderTargetView(SMAAResult.RTV, Color4.Black);

			if (SMAAEnabled)
			{
				// Render image to a temporary target to apply SMAA
				Context.OutputMerger.SetRenderTargets(SMAATarget.RTV);
				Context.Rasterizer.SetViewport(0, 0, ImageWidth, ImageHeight);

				Tri.DrawImage(Context, SourceColorSRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);

				Context.OutputMerger.ResetTargets();

				// Apply SMAA
				SMAA.Run(Context, SMAATarget.SRV, SMAATarget.GammaSRV, SourceDepthSRV, null);
			}

			Context.OutputMerger.ResetTargets();

			Context.OutputMerger.SetRenderTargets(BackbufferRTV);
			Context.Rasterizer.SetViewport(DefaultViewport);

			switch (ShowMode)
			{
				case ShowModes.Color:
				{
					if (SMAAEnabled)
					{
						Tri.DrawImage(Context, SMAA.Result.SRV, Sampler, TexCoordTransform);
					}
					else
					{
						Tri.DrawImage(Context, SourceColorSRV, Sampler, TexCoordTransform);
					}
					break;
				}
				case ShowModes.Depth:
				{
					Tri.DrawDepth(Context, SourceDepthSRV, Sampler, TexCoordTransform);
					break;
				}
				case ShowModes.Edges:
				{
					if (SMAAEnabled)
					{
						Tri.DrawImage(Context, SMAA.Edges.SRV, Sampler, TexCoordTransform);
					}
					break;
				}
				case ShowModes.Weights:
				{
					if (SMAAEnabled)
					{
						Tri.DrawImage(Context, SMAA.BlendingWeights.SRV, Sampler, TexCoordTransform);
					}
					break;
				}
			}
		}
	}
}
