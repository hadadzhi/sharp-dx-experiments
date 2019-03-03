using System;
using SharpDXCommons;
using SharpDXCommons.Helpers;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Windows.Forms;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using System.Text;

namespace GaussianFilter
{
	class GaussianFilter : SharpDXApplication
	{
		private ShaderResourceView InputSRV;
		private ScreenTriangle Tri;
		
		private VertexShader FilterVS;
		private PixelShader FilterHorizontalPassPS;
		private PixelShader FilterVerticalPassPS;

		private Image FilterIntermediateTarget;

		private int FilterRadius = 10;

		private float[] Weights;
		private Vector2[] OffsetsH;
		private Vector2[] OffsetsV;

		private int TargetWidth;
		private int TargetHeight;

		private readonly float Sqrt2Pi = (float) Math.Sqrt(2 * Math.PI);

		private bool DoFilter = true;

		public GaussianFilter(GraphicsConfiguration configuration)
			: base(configuration, "Gaussian Filter")
		{
			TargetWidth = configuration.DisplayMode.Width;
			TargetHeight = configuration.DisplayMode.Height;

			ImageLoadInformation loadInfo = new ImageLoadInformation
			{
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Filter = FilterFlags.SRgbIn | FilterFlags.Point,
				Format = Format.R8G8B8A8_UNorm_SRgb,
				OptionFlags = ResourceOptionFlags.None,
				MipLevels = 1,
				FirstMipLevel = 0,
				Usage = ResourceUsage.Immutable
			};
			InputSRV = ShaderResourceView.FromFile(Device, "lara.png", loadInfo);

			FilterIntermediateTarget = new Image(Device, TargetWidth, TargetHeight, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown);

			Tri = new ScreenTriangle(Device);
			
			CalculateOffsets();
			CalculateWeights();
			CompileShaders();

			RenderWindow.KeyDown += OnKeyDown;
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Space:
				{
					DoFilter = !DoFilter;
					break;
				}
				case Keys.Add:
				{
					FilterRadius++;
					FilterRadius = MathUtil.Clamp(FilterRadius, 1, 100);
					Log.Info(String.Format("Radius: {0}", FilterRadius));
					
					CalculateOffsets();
					CalculateWeights();
					CompileShaders();

					break;
				}
				case Keys.Subtract:
				{
					FilterRadius--;
					FilterRadius = MathUtil.Clamp(FilterRadius, 1, 100);
					Log.Info(String.Format("Radius: {0}", FilterRadius));
					
					CalculateOffsets();
					CalculateWeights();
					CompileShaders();

					break;
				}
			}
		}
		
		protected override void RenderScene()
		{
			Context.ClearState();
			Context.ClearRenderTargetView(BackbufferRTV, Color4.Black);
			Context.Rasterizer.SetViewport(0, 0, TargetWidth, TargetHeight);

			if (DoFilter)
			{
				Context.VertexShader.Set(FilterVS);

				// Horizontal pass
				Context.OutputMerger.SetTargets(FilterIntermediateTarget.RTV);
				Context.PixelShader.Set(FilterHorizontalPassPS);
				Context.PixelShader.SetShaderResource(0, InputSRV);
				Tri.Draw(Context);

				// Vertical pass
				Context.OutputMerger.SetTargets(BackbufferRTV);
				Context.PixelShader.Set(FilterVerticalPassPS);
				Context.PixelShader.SetShaderResource(0, FilterIntermediateTarget.SRV);
				Tri.Draw(Context);
			}
			else
			{
				Context.OutputMerger.SetTargets(BackbufferRTV);
				Tri.DrawImage(Context, InputSRV, PipelineStates.Sampler.BorderPoint, Matrix.Identity);
			}

			Context.ClearState();
		}

		private void CompileShaders()
		{
			if (FilterVS != null) FilterVS.Dispose();
			if (FilterHorizontalPassPS != null) FilterHorizontalPassPS.Dispose();
			if (FilterVerticalPassPS != null) FilterVerticalPassPS.Dispose();

			// Filter kernel size in one direction in pixels
			int kernelSize = FilterRadius * 2 + 1;

			ShaderFlags flags = ShaderFlags.OptimizationLevel3 | ShaderFlags.AvoidFlowControl;
			ShaderMacro[] defines = new ShaderMacro[]
			{
				new ShaderMacro { Name = "ARRAY_LENGTH", Definition =  kernelSize.ToString() },
				new ShaderMacro { Name = "WEIGHTS", Definition =  GenerateWeightsMacro(Weights) },
				new ShaderMacro { Name = "OFFSETS_H", Definition =  GenerateOffsetsMacro(OffsetsH) },
				new ShaderMacro { Name = "OFFSETS_V", Definition =  GenerateOffsetsMacro(OffsetsV) }
			};
			FilterVS = new VertexShader(Device, ShaderBytecode.CompileFromFile("Shaders/SeparableFilter.hlsl", "SeparableFilterVS", "vs_5_0", flags, EffectFlags.None, defines));
			FilterHorizontalPassPS = new PixelShader(Device, ShaderBytecode.CompileFromFile("Shaders/SeparableFilter.hlsl", "HorizontalPassPS", "ps_5_0", flags, EffectFlags.None, defines));
			FilterVerticalPassPS = new PixelShader(Device, ShaderBytecode.CompileFromFile("Shaders/SeparableFilter.hlsl", "VerticalPassPS", "ps_5_0", flags, EffectFlags.None, defines));
		}

		private string GenerateOffsetsMacro(Vector2[] offsets)
		{
			StringBuilder b = new StringBuilder();

			// Open array initializer
			b.Append("{");

			for (int i = 0; i < offsets.Length; i++)
			{
				// Open vector initializer
				b.Append("{");

				b.Append(offsets[i].X);
				b.Append(",");
				b.Append(offsets[i].Y);

				// Close vector initializer
				b.Append("},");
			}

			// Close array initializer
			b.Append("}");

			return b.ToString();
		}
		
		private string GenerateWeightsMacro(float[] weights)
		{
			StringBuilder b = new StringBuilder();

			// Open array initializer
			b.Append("{");

			for (int i = 0; i < weights.Length; i++)
			{
				b.Append(weights[i]);
				b.Append(",");
			}

			// Close array initializer
			b.Append("}");

			return b.ToString();
		}

		private void CalculateWeights()
		{
			// Filter kernel size in one direction in pixels
			int kernelSize = FilterRadius * 2 + 1;
			
			float sigma = FilterRadius / 2.0f;

			float gaussIntegral = IntegrateGaussian(-FilterRadius - 0.5f, FilterRadius + 0.5f, sigma);
			float normalizationCoef = 1 / gaussIntegral;

			Weights = new float[kernelSize];

			for (int i = 0; i < kernelSize; i++)
			{
				Weights[i] = normalizationCoef * IntegrateGaussian(i - FilterRadius - 0.5f, i - FilterRadius + 0.5f, sigma);
			}
		}

		private float IntegrateGaussian(float left, float right, float sigma)
		{
			float dx = (right - left) / 1000.0f;
			float result = 0;

			// Integrate using trapezoidal technique
			for (float x = left; x < right; x += dx)
			{
				float vLeft = Gaussian(x, sigma);
				float vRight = Gaussian(x + dx, sigma);
				result += dx * (vRight + vLeft) / 2;
			}

			return result;
		}

		private void CalculateOffsets()
		{
			// Filter kernel size in one direction in pixels
			int kernelSize = FilterRadius * 2 + 1;

			// Pixel sizes in texture coordinates
			float PixelWidth = 1.0f / TargetWidth;
			float PixelHeight = 1.0f / TargetHeight;

			OffsetsH = new Vector2[kernelSize];
			OffsetsV = new Vector2[kernelSize];

			for (int i = 0; i < kernelSize; i++)
			{
				OffsetsH[i] = new Vector2((i - FilterRadius) * PixelWidth, 0.0f);
				OffsetsV[i] = new Vector2(0.0f, (i - FilterRadius) * PixelHeight);
			}
		}

		private float Gaussian(float x, float sigma)
		{
			return (1 / (Sqrt2Pi * sigma)) * (float) Math.Exp(-x * x / (2 * sigma * sigma));
		}
	}
}
