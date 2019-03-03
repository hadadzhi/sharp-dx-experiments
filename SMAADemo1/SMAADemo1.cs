using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDXCommons;
using SharpDXCommons.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SMAA = SharpSMAA.SMAA;

namespace SMAADemo1
{
	class SMAADemo1 : SharpDXApplication
	{
		private SMAA SMAA;

		private ScreenTriangle Triangle;

		private enum ShowModes { Color, Depth, Edges, Weights }

		private ShowModes ShowMode = ShowModes.Color;

		private bool SMAAEnabled = true;
		private bool SMAAPredication = false;
		private bool SMAAReprojection = false;

		private SMAA.Inputs SMAAInput = SMAA.Inputs.Color;
		private SMAA.Modes SMAAMode = SMAA.Modes.SMAA_1x;
		private SMAA.Presets SMAAPreset = SMAA.Presets.Ultra;

		private ShaderResourceView UnigineColor;
		private ShaderResourceView UnigineDepth;
		private Image UnigineColorGamma;
		private Image[] SMAAResult = new Image[2];

		private SharpSMAA.SMAARenderTarget SMAATarget;
		private bool Jitter;

		public SMAADemo1(GraphicsConfiguration configuration) : base(configuration, "SMAADemo1")
		{
			Triangle = new ScreenTriangle(Device);

			TargetsResized += UpdateSMAA;
			RenderWindow.KeyDown += OnKeyDown;
			RenderWindow.FormBorderStyle = FormBorderStyle.Fixed3D;

			ImageLoadInformation info = new ImageLoadInformation
			{
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Filter = FilterFlags.SRgbIn | FilterFlags.Point,
				FirstMipLevel = 0,
				Format = Format.R8G8B8A8_UNorm_SRgb,
				OptionFlags = ResourceOptionFlags.None
			};

			UnigineColor = ShaderResourceView.FromFile(Device, "Unigine01.png", info);
			UnigineDepth = ShaderResourceView.FromFile(Device, "Unigine01.dds");

			UnigineColorGamma = new Image(Device, 1280, 720, Format.R8G8B8A8_Typeless, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown);
			SMAATarget = new SharpSMAA.SMAARenderTarget(Device, 1280, 720);
			SMAAResult[0] = new Image(Device, 1280, 720, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown);
			SMAAResult[1] = new Image(Device, 1280, 720, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown);
		}

		private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch(e.KeyCode)
			{
				case Keys.NumPad1:
				{
					SMAAMode = SMAA.Modes.SMAA_1x;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.NumPad2:
				{
					SMAAMode = SMAA.Modes.SMAA_T2x;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.D1:
				{
					SMAAPreset = SMAA.Presets.Low;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.D2:
				{
					SMAAPreset = SMAA.Presets.Medium;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.D3:
				{
					SMAAPreset = SMAA.Presets.High;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.D4:
				{
					SMAAPreset = SMAA.Presets.Ultra;
					UpdateSMAA(1280, 720);
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
				case Keys.S:
				{
					SMAAInput = SMAA.Inputs.Depth;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.D:
				{
					SMAAInput = SMAA.Inputs.Luma;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.F:
				{
					SMAAInput = SMAA.Inputs.Color;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.R:
				{
					SMAAReprojection = !SMAAReprojection;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.Space:
				{
					SMAAPredication = !SMAAPredication;
					UpdateSMAA(1280, 720);
					break;
				}
				case Keys.E:
				{
					SMAAEnabled = !SMAAEnabled;
					break;
				}
				case Keys.J:
				{
					Jitter = !Jitter;
					break;
				}
			}
		}

		private void UpdateSMAA(int newWidth, int newHeight)
		{
			if (SMAA != null) SMAA.Dispose();
			SMAA = new SMAA(Device, newWidth, newHeight, SMAAMode, SMAAPreset, SMAAInput, SMAAPredication, SMAAReprojection);
		}

		protected override void RenderScene()
		{
			Context.ClearState();
			Context.ClearRenderTargetView(BackbufferRTV, Color4.Black);
			Context.ClearRenderTargetView(UnigineColorGamma.RTV, Color4.Black);
			Context.ClearRenderTargetView(SMAATarget.RTV, Color4.Black);
			Context.ClearRenderTargetView(SMAAResult[SMAA.CurrentFrameIndex].RTV, Color4.Black);
			
			if (SMAAEnabled)
			{
				Context.OutputMerger.SetRenderTargets(SMAATarget.RTV);
				Context.Rasterizer.SetViewport(0, 0, 1280, 720);

				Triangle.DrawImage(Context, UnigineColor, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
				
				SMAA.Run(Context, SMAATarget.SRV, SMAATarget.GammaSRV, UnigineDepth, null);
			}

			Context.OutputMerger.SetRenderTargets(BackbufferRTV);
			Context.Rasterizer.SetViewport(DefaultViewport);

			switch (ShowMode)
			{
				case ShowModes.Color:
				{
					if (SMAAEnabled)
					{
						Triangle.DrawImage(Context, SMAA.Result.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
					}
					else
					{
						Triangle.DrawImage(Context, UnigineColor, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
					}
					break;
				}
				case ShowModes.Depth:
				{
					Triangle.DrawDepth(Context, UnigineDepth, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
					break;
				}
				case ShowModes.Edges:
				{
					if (SMAAEnabled)
					{
						Triangle.DrawImage(Context, SMAA.Edges.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
					}
					break;
				}
				case ShowModes.Weights:
				{
					if (SMAAEnabled)
					{
						Triangle.DrawImage(Context, SMAA.BlendingWeights.SRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
					}
					break;
				}
			}
		}
	}
}
