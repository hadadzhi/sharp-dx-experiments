using System;
using System.Runtime.InteropServices;

using SharpDXCommons.Helpers;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Collections.Generic;
using SharpDX.Direct3D;

namespace SharpSMAA
{
	public class SMAA : IDisposable
	{
		public enum Modes { SMAA_1x, SMAA_T2x, SMAA_S2x, SMAA_4x }
		public enum Presets { Low, Medium, High, Ultra }
		public enum Inputs { Depth, Luma, Color }

		#region Fields
		// Parameters
		public Modes Mode { get; set; }
		public Presets Preset { get; set; }
		public Inputs Input { get; set; }

		public bool Predication { get; set; }
		public bool Reprojection { get; set; }

		public int CurrentFrameIndex { get { return FrameIndex; } }
		public int PreviousFrameIndex { get { return (FrameIndex + 1) % 2; } }

		public SMAARenderTarget Result { get { return ResultTarget; } }

		public Image Edges { get; private set; }
		public Image BlendingWeights { get; private set; }

		// Pre-computed area and search textures
		public ShaderResourceView AreaTexSRV { get; private set; }
		public ShaderResourceView SearchTexSRV { get; private set; }

		private Texture2D AreaTex;
		private Texture2D SearchTex;

		// Stencil for optimization
		public Image Stencil { get; private set; }

		public Device Device { get; private set; }

		public int TargetWidth { get; private set; }
		public int TargetHeight { get; private set; }

		private ConstantBufferStruct ConstBufferStruct;
		private Buffer ConstBuffer;
		private Buffer SubSampleIndicesBuffer;

		private ScreenTriangle Tri;

		private DepthStencilState DisableDepthStencil;
		private DepthStencilState DisableDepthUseStencil;
		private DepthStencilState DisableDepthReplaceStencil;
		private BlendState Blending;
		private BlendState NoBlending;

		private int FrameIndex = 0;

		private SMAARenderTarget[] TempTarget;
		private SMAARenderTarget ResultTarget;

		private int[] MSAAOrderMap;
		#endregion
		#region Shader Slots
		private const int ConstantBufferSlot = 0;
		private const int SubSampleIndicesSlot = 1;

		private const int ColorTexSlot = 0;
		private const int ColorTexGammaSlot = 1;
		private const int ColorTexPrevSlot = 2;
		private const int ColorTexMSSlot = 3;
		private const int DepthTexSlot = 4;
		private const int VelocityTexSlot = 5;

		private const int EdgesTexSlot = 6;
		private const int BlendTexSlot = 7;

		private const int AreaTexSlot = 8;
		private const int SearchTexSlot = 9;
		#endregion
		#region Shaders
		private VertexShader EdgeDetectionVS;
		private VertexShader BlendingWeightCalculationVS;
		private VertexShader NeighborhoodBlendingVS;
		private VertexShader ResolveVS;
		private VertexShader SeparateVS;

		private PixelShader LumaEdgeDetectionPS;
		private PixelShader ColorEdgeDetectionPS;
		private PixelShader DepthEdgeDetectionPS;
		private PixelShader BlendingWeightCalculationPS;
		private PixelShader NeighborhoodBlendingPS;
		private PixelShader ResolvePS;
		private PixelShader SeparatePS;
		#endregion

		public SMAA(Device device, int targetWidth, int targetHeight, Modes mode, Presets preset, Inputs input, bool predication, bool reprojection)
		{
			// Init fields
			Mode = mode;
			Preset = preset;
			Input = input;
			Predication = (input != Inputs.Depth) ? predication : false;
			Reprojection = (mode == Modes.SMAA_T2x || mode == Modes.SMAA_4x) ? reprojection : false;
			Device = device;
			TargetWidth = targetWidth;
			TargetHeight = targetHeight;

			LoadPrecomputedTextures();

			if (mode == Modes.SMAA_S2x || mode == Modes.SMAA_4x)
			{
				DetectMSAAOrder();
			}

			// Init images
			Edges = new Image(Device, TargetWidth, TargetHeight, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.Unknown);
			BlendingWeights = new Image(Device, TargetWidth, TargetHeight, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.R8G8B8A8_UNorm, Format.Unknown);
			Stencil = new Image(Device, TargetWidth, TargetHeight, Format.R24G8_Typeless, Format.Unknown, Format.Unknown, Format.D24_UNorm_S8_UInt);

			#region Load shaders
			EdgeDetectionVS = new VertexShader(device, ShaderBytecode.FromFile("SharpSMAAEdgeDetectionVS.shd"));
			BlendingWeightCalculationVS = new VertexShader(device, ShaderBytecode.FromFile("SharpSMAABlendingWeightCalculationVS.shd"));
			NeighborhoodBlendingVS = new VertexShader(device, ShaderBytecode.FromFile("SharpSMAANeighborhoodBlendingVS.shd"));
			ResolveVS = new VertexShader(device, ShaderBytecode.FromFile("SharpSMAAResolveVS.shd"));
			SeparateVS = new VertexShader(device, ShaderBytecode.FromFile("SharpSMAASeparateVS.shd"));
			LumaEdgeDetectionPS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAALumaEdgeDetectionPS.shd"));
			ColorEdgeDetectionPS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAAColorEdgeDetectionPS.shd"));
			DepthEdgeDetectionPS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAADepthEdgeDetectionPS.shd"));
			BlendingWeightCalculationPS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAABlendingWeightCalculationPS.shd"));
			NeighborhoodBlendingPS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAANeighborhoodBlendingPS.shd"));
			ResolvePS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAAResolvePS.shd"));
			SeparatePS = new PixelShader(device, ShaderBytecode.FromFile("SharpSMAASeparatePS.shd"));
			#endregion

			#region Create buffers/pipeline states/samplers/...
			// Create constant buffers
			ConstBuffer = new Buffer(
				Device,
				Marshal.SizeOf(typeof(ConstantBufferStruct)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			SubSampleIndicesBuffer = new Buffer(
				Device,
				Marshal.SizeOf(typeof(Vector4)),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0
			);

			// Createe pipeline states
			DisableDepthStencil = new DepthStencilState(
				Device,
				new DepthStencilStateDescription
				{
					IsDepthEnabled = false,
					IsStencilEnabled = false
				}
			);

			DisableDepthUseStencil = new DepthStencilState(
				Device,
				new DepthStencilStateDescription
				{
					IsDepthEnabled = false,
					IsStencilEnabled = true,
					FrontFace = new DepthStencilOperationDescription
					{
						Comparison = Comparison.Equal,
						FailOperation = StencilOperation.Keep,
						PassOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Keep
					},
					BackFace = new DepthStencilOperationDescription
					{
						Comparison = Comparison.Equal,
						FailOperation = StencilOperation.Keep,
						PassOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Keep
					},
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF
				}
			);

			DisableDepthReplaceStencil = new DepthStencilState(
				Device,
				new DepthStencilStateDescription
				{
					IsDepthEnabled = false,
					IsStencilEnabled = true,
					FrontFace = new DepthStencilOperationDescription
					{
						Comparison = Comparison.Always,
						FailOperation = StencilOperation.Replace,
						PassOperation = StencilOperation.Replace,
						DepthFailOperation = StencilOperation.Replace
					},
					BackFace = new DepthStencilOperationDescription
					{
						Comparison = Comparison.Always,
						FailOperation = StencilOperation.Replace,
						PassOperation = StencilOperation.Replace,
						DepthFailOperation = StencilOperation.Replace
					},
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF
				}
			);

			BlendStateDescription blendingDesc = new BlendStateDescription
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false
			};
			blendingDesc.RenderTarget[0] = new RenderTargetBlendDescription
			{
				IsBlendEnabled = true,
				SourceBlend = BlendOption.BlendFactor,
				DestinationBlend = BlendOption.InverseBlendFactor,
				BlendOperation = BlendOperation.Add,
				AlphaBlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};
			Blending = new BlendState(Device, blendingDesc);

			blendingDesc.RenderTarget[0].IsBlendEnabled = false;
			NoBlending = new BlendState(Device, blendingDesc);
			#endregion

			#region Init the const buffer
			ConstBufferStruct = new ConstantBufferStruct();

			switch (Preset)
			{
				case Presets.Low:
				{
					ConstBufferStruct.Threshold = 0.2f;
					ConstBufferStruct.MaxSearchSteps = 4;
					ConstBufferStruct.MaxSearchStepsDiag = 2;
					ConstBufferStruct.CornerRounding = 0;
					break;
				}
				case Presets.Medium:
				{
					ConstBufferStruct.Threshold = 0.15f;
					ConstBufferStruct.MaxSearchSteps = 8;
					ConstBufferStruct.MaxSearchStepsDiag = 4;
					ConstBufferStruct.CornerRounding = 0;
					break;
				}
				case Presets.High:
				{
					ConstBufferStruct.Threshold = 0.1f;
					ConstBufferStruct.MaxSearchSteps = 16;
					ConstBufferStruct.MaxSearchStepsDiag = 8;
					ConstBufferStruct.CornerRounding = 25;
					break;
				}
				case Presets.Ultra:
				{
					ConstBufferStruct.Threshold = 0.05f;
					ConstBufferStruct.MaxSearchSteps = 32;
					ConstBufferStruct.MaxSearchStepsDiag = 16;
					ConstBufferStruct.CornerRounding = 25;
					break;
				}
			}

			ConstBufferStruct.Predication = Predication;
			ConstBufferStruct.Reprojection = Reprojection;

			ConstBufferStruct.RenderTargetMetrics = new Vector4(1.0f / TargetWidth, 1.0f / TargetHeight, TargetWidth, TargetHeight);

			// Load data to the constant buffer
			device.ImmediateContext.UpdateSubresource(ref ConstBufferStruct, ConstBuffer);
			#endregion

			// Create the screen triangle
			Tri = new ScreenTriangle(Device);

			// Create the final result target
			ResultTarget = new SMAARenderTarget(Device, TargetWidth, TargetHeight);
			Device.ImmediateContext.ClearRenderTargetView(ResultTarget.RTV, new Color4(0));

			// Create the intermediate results targets
			int tempTargets = 0;
			switch (Mode)
			{
				case Modes.SMAA_1x: tempTargets = 0; break;
				case Modes.SMAA_T2x: tempTargets = 2; break;
				case Modes.SMAA_S2x: tempTargets = 2; break;
				case Modes.SMAA_4x: tempTargets = 4; break;
			}
			TempTarget = new SMAARenderTarget[tempTargets];
			for (int i = 0; i < tempTargets; i++)
			{
				TempTarget[i] = new SMAARenderTarget(Device, targetWidth, TargetHeight);
				Device.ImmediateContext.ClearRenderTargetView(TempTarget[i].RTV, new Color4(0));
			}
		}

		private void LoadPrecomputedTextures()
		{
			// Create the area texture
			DataBuffer areaBuffer = DataBuffer.Create(AreaTexture.Bytes);
			DataRectangle areaData = new DataRectangle(areaBuffer.DataPointer, AreaTexture.Pitch);
			Texture2DDescription areaDesc = new Texture2DDescription
			{
				Width = AreaTexture.Width,
				Height = AreaTexture.Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.R8G8_UNorm,
				SampleDescription = new SampleDescription { Count = 1, Quality = 0},
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			AreaTex = new Texture2D(Device, areaDesc, areaData);
			AreaTexSRV = new ShaderResourceView(Device, AreaTex);
			areaBuffer.Dispose();

			// Create the search texture
			DataBuffer searchBuffer = DataBuffer.Create(SearchTexture.Bytes);
			DataRectangle searchData = new DataRectangle(searchBuffer.DataPointer, SearchTexture.Pitch);
			Texture2DDescription searchDesc = new Texture2DDescription
			{
				Width = SearchTexture.Width,
				Height = SearchTexture.Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.R8_UNorm,
				SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			SearchTex = new Texture2D(Device, searchDesc, searchData);
			SearchTexSRV = new ShaderResourceView(Device, SearchTex);
			searchBuffer.Dispose();
		}
		
		public Vector2 CurrentJitter
		{
			get
			{
				Vector2 jitter = Vector2.Zero;
				switch (Mode)
				{
					case Modes.SMAA_T2x:
					{
						jitter = new Vector2(0.25f, -0.25f);
						jitter *= 1 - 2 * CurrentFrameIndex; // Change sign each frame
						jitter *= new Vector2(2.0f / TargetWidth, 2.0f / TargetHeight); // Transform to screen space
						break;
					}
					case Modes.SMAA_4x:
					{
						jitter = new Vector2(0.125f, 0.125f);
						jitter *= 1 - 2 * CurrentFrameIndex; // Change sign each frame
						jitter *= new Vector2(2.0f / TargetWidth, 2.0f / TargetHeight); // Transform to screen space
						break;
					}
				}
				return jitter;
			}
		}

		/// <summary>
		/// The entry point for the SMAA run.
		/// This method clears the pipeline state before exiting.
		/// For color and luma edge detection, a gamma-corrected view of the color input is required.
		/// For depth edge detection, depth input is required. Alternatively, if predication is enabled, depth input will be used for predication.
		/// For T2x and 4x, a velocity buffer is required and the scene must be rendered with subpixel jitter. Use CurrentJitter.
		/// For S2x and 4x, the scene must be rendered into a 2x multisampled texture with StandardMultisampleQualityLevels.StandardMultisamplePattern.
		/// For correct SMAA results, the scene must be rendered into an sRGB render target, and sRGB and non-sRGB views should be provided as inputs for SMAA. Use SMAARenderTarget.
		/// </summary>
		/// <param name="context">The rendering context</param>
		/// <param name="colorSRV">Current frame, linear view.</param>
		/// <param name="colorGammaSRV">Current frame, gamma-corrected view.</param>
		/// <param name="prevColorSRV">Previous frame (with SMAA applied), linear view. Needed for T2x and 4x modes.</param>
		/// <param name="depthSRV">Current depth buffer or predication input. Depth needed for depth edge detection mode, alternatively used for predication when enabled.</param>
		/// <param name="velocitySRV">Current velocity buffer. Needed for T2x and 4x modes.</param>
		/// <param name="outputRTV">The antialised image will go here. Must be gamma-corrected (texture and view created with *_SRgb format).</param>
		public void Run(DeviceContext context, ShaderResourceView colorSRV, ShaderResourceView colorGammaSRV, ShaderResourceView depthSRV, ShaderResourceView velocitySRV)
		{
			// Reset the context before we begin
			context.ClearState();

			context.Rasterizer.SetViewport(0, 0, TargetWidth, TargetHeight);

			context.PixelShader.SetShaderResource(AreaTexSlot, AreaTexSRV);
			context.PixelShader.SetShaderResource(SearchTexSlot, SearchTexSRV);
			
			context.PixelShader.SetShaderResource(DepthTexSlot, depthSRV);
			context.PixelShader.SetShaderResource(VelocityTexSlot, velocitySRV);

			context.PixelShader.SetConstantBuffer(SubSampleIndicesSlot, SubSampleIndicesBuffer);
			context.PixelShader.SetConstantBuffer(ConstantBufferSlot, ConstBuffer);
			context.VertexShader.SetConstantBuffer(ConstantBufferSlot, ConstBuffer);

			context.OutputMerger.SetDepthStencilState(DisableDepthStencil, 0);

			// Run SMAA
			switch (Mode)
			{
				case Modes.SMAA_1x:
				{
					Run(context, colorSRV, colorGammaSRV, ResultTarget.RTV, 0);
					break;
				}
				case Modes.SMAA_T2x:
				{
					Run(context, colorSRV, colorGammaSRV, TempTarget[CurrentFrameIndex].RTV, 0);
					Reproject(context, TempTarget[CurrentFrameIndex].SRV, TempTarget[PreviousFrameIndex].SRV, ResultTarget.RTV);
					break;
				}
				case Modes.SMAA_S2x:
				{
					Separate(context, colorSRV, TempTarget[0].RTV, TempTarget[1].RTV);
					Run(context, TempTarget[0].SRV, TempTarget[0].GammaSRV, ResultTarget.RTV, 0);
					Run(context, TempTarget[1].SRV, TempTarget[1].GammaSRV, ResultTarget.RTV, 1);
					break;
				}
				case Modes.SMAA_4x:
				{
					Separate(context, colorSRV, TempTarget[2].RTV, TempTarget[3].RTV);
					Run(context, TempTarget[2].SRV, TempTarget[2].GammaSRV, TempTarget[CurrentFrameIndex].RTV, 0);
					Run(context, TempTarget[3].SRV, TempTarget[3].GammaSRV, TempTarget[CurrentFrameIndex].RTV, 1);
					Reproject(context, TempTarget[CurrentFrameIndex].SRV, TempTarget[PreviousFrameIndex].SRV, ResultTarget.RTV);
					break;
				}
			}

			// If running in temporal mode, advance the frame index
			if (Mode == Modes.SMAA_T2x || Mode == Modes.SMAA_4x)
			{
				FrameIndex = (FrameIndex + 1) % 2;
			}

			// Reset the context before exiting
			context.ClearState();
		}

		private void Run(DeviceContext context, ShaderResourceView colorSRV, ShaderResourceView colorGammaSRV, RenderTargetView outputRTV, int pass)
		{
			Vector4 subSampleIndices = CalculateSubSampleIndices(pass);
			context.UpdateSubresource(ref subSampleIndices, SubSampleIndicesBuffer);

			context.PixelShader.SetShaderResource(ColorTexSlot, colorSRV);
			context.PixelShader.SetShaderResource(ColorTexGammaSlot, colorGammaSRV);

			if (pass == 0)
			{
				context.ClearRenderTargetView(outputRTV, new Color4(0));
			}
			
			RunEdgeDetection(context);
			RunBlendingWeightCalculation(context);
			RunNeighborhoodBlending(context, outputRTV, pass);
		}

		private void RunEdgeDetection(DeviceContext context)
		{
			context.VertexShader.Set(EdgeDetectionVS);

			switch (Input)
			{
				case Inputs.Depth: context.PixelShader.Set(DepthEdgeDetectionPS); break;
				case Inputs.Luma: context.PixelShader.Set(LumaEdgeDetectionPS); break;
				case Inputs.Color: context.PixelShader.Set(ColorEdgeDetectionPS); break;
			}

			context.ClearDepthStencilView(Stencil.DSV, DepthStencilClearFlags.Stencil, 1.0f, 0);
			context.ClearRenderTargetView(Edges.RTV, new Color4(0));

			context.OutputMerger.SetDepthStencilState(DisableDepthReplaceStencil, 1);
			context.OutputMerger.SetBlendState(NoBlending);
			context.OutputMerger.SetRenderTargets(Stencil.DSV, Edges.RTV);

			Tri.Draw(context);

			context.OutputMerger.ResetTargets();
		}

		private void RunBlendingWeightCalculation(DeviceContext context)
		{
			context.VertexShader.Set(BlendingWeightCalculationVS);
			context.PixelShader.Set(BlendingWeightCalculationPS);

			context.PixelShader.SetShaderResource(EdgesTexSlot, Edges.SRV);

			context.ClearRenderTargetView(BlendingWeights.RTV, new Color4(0));

			context.OutputMerger.SetDepthStencilState(DisableDepthUseStencil, 1);
			context.OutputMerger.SetBlendState(NoBlending);
			context.OutputMerger.SetRenderTargets(Stencil.DSV, BlendingWeights.RTV);

			Tri.Draw(context);

			context.OutputMerger.ResetTargets();
		}

		private void RunNeighborhoodBlending(DeviceContext context, RenderTargetView outputRTV, int pass)
		{
			context.VertexShader.Set(NeighborhoodBlendingVS);
			context.PixelShader.Set(NeighborhoodBlendingPS);

			context.PixelShader.SetShaderResource(BlendTexSlot, BlendingWeights.SRV);

			context.OutputMerger.SetDepthStencilState(DisableDepthStencil);
			switch(pass)
			{
				case 0: context.OutputMerger.SetBlendState(NoBlending); break;
				case 1: context.OutputMerger.SetBlendState(Blending, new Color4(0.5f)); break;
			}
			context.OutputMerger.SetRenderTargets(outputRTV);

			Tri.Draw(context);

			context.OutputMerger.ResetTargets();
		}

		private void Reproject(DeviceContext context, ShaderResourceView currentSRV, ShaderResourceView previousSRV, RenderTargetView outputRTV)
		{
			context.PixelShader.SetShaderResource(ColorTexSlot, currentSRV);
			context.PixelShader.SetShaderResource(ColorTexPrevSlot, previousSRV);

			context.VertexShader.Set(ResolveVS);
			context.PixelShader.Set(ResolvePS);

			context.OutputMerger.SetDepthStencilState(DisableDepthStencil);
			context.OutputMerger.SetBlendState(NoBlending);

			context.ClearRenderTargetView(outputRTV, new Color4(0));

			context.OutputMerger.SetRenderTargets(outputRTV);

			Tri.Draw(context);

			context.OutputMerger.ResetTargets();
		}

		private Vector4 CalculateSubSampleIndices(int pass)
		{
			switch (Mode)
			{
				default: return Vector4.Zero; // SMAA_1x
				case Modes.SMAA_T2x:
				case Modes.SMAA_S2x:
				{
					// Sample positions (bottom-to-top y axis):
					//  _______
					// | S1    |  S0:  0.25    -0.25
					// |       |  S1: -0.25     0.25
					// |____S0_|
					switch(Mode == SMAA.Modes.SMAA_T2x ? CurrentFrameIndex : MSAAOrderMap[pass])
					{
						// (it's 1 for the horizontal slot of S0 because horizontal
						//  blending is reversed: positive numbers point to the right)
						case 0: return new Vector4(1.0f, 1.0f, 1.0f, 0.0f); // S0
						case 1: return new Vector4(2.0f, 2.0f, 2.0f, 0.0f); // S1
						default: throw new IndexOutOfRangeException("Wrong subsample index for SMAA T2x or S2x mode");
					}
				}
				case Modes.SMAA_4x:
				{
					// Sample positions (bottom-to-top y axis):
					//   ________
					//  |  S1    |  S0:  0.3750   -0.1250
					//  |      S0|  S1: -0.1250    0.3750
					//  |S3      |  S2:  0.1250   -0.3750
					//  |____S2__|  S3: -0.3750    0.1250
					switch (2 * CurrentFrameIndex + MSAAOrderMap[pass])
					{
						case 0: return new Vector4(5.0f, 3.0f, 1.0f, 3.0f); // S0
						case 1: return new Vector4(4.0f, 6.0f, 2.0f, 3.0f); // S1
						case 2: return new Vector4(3.0f, 5.0f, 1.0f, 4.0f); // S2
						case 3: return new Vector4(6.0f, 4.0f, 2.0f, 4.0f); // S3
						default: throw new IndexOutOfRangeException("Wrong subsample index for SMAA 4x mode");
					}
				}
			}
		}

		private void DetectMSAAOrder()
		{
			#region Load shaders
			ShaderBytecode renderVSBytecode = ShaderBytecode.FromFile("SharpSMAADetectMSAAOrderRenderVS.shd");
			VertexShader renderVS = new VertexShader(Device, renderVSBytecode);
			VertexShader loadVS = new VertexShader(Device, ShaderBytecode.FromFile("SharpSMAADetectMSAAOrderLoadVS.shd"));
			PixelShader renderPS = new PixelShader(Device, ShaderBytecode.FromFile("SharpSMAADetectMSAAOrderRenderPS.shd"));
			PixelShader loadPS = new PixelShader(Device, ShaderBytecode.FromFile("SharpSMAADetectMSAAOrderLoadPS.shd"));
			#endregion

			#region Create a vertex buffer/input layout
			// Create a vertex buffer for a fullscreen quad
			DataStream dataStream = new DataStream(96, true, true);
			dataStream.Write(new Vector4(-1.0f, -1.0f, 1.0f, 1.0f));
			dataStream.Write(new Vector2(0.0f, 1.0f));

			dataStream.Write(new Vector4(-1.0f, 1.0f, 1.0f, 1.0f));
			dataStream.Write(new Vector2(0.0f, 0.0f));

			dataStream.Write(new Vector4(1.0f, -1.0f, 1.0f, 1.0f));
			dataStream.Write(new Vector2(1.0f, 1.0f));

			dataStream.Write(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			dataStream.Write(new Vector2(1.0f, 0.0f));

			Buffer vertexBuffer = new Buffer(
				Device,
				dataStream,
				new BufferDescription
				{
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Immutable,
					SizeInBytes = (int) dataStream.Length,
					StructureByteStride = 0
				}
			);

			InputLayout inputLayout = new InputLayout(
				Device,
				renderVSBytecode,
				new InputElement[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0)
				}
			);
			#endregion

			// Create the targets
			Image renderTargetMS = new Image(Device, 1, 1, Format.R8_UNorm, Format.R8_UNorm, Format.R8_UNorm, Format.Unknown, new SampleDescription { Count = 1, Quality = 0 });
			Image renderTarget = new Image(Device, 1, 1, Format.R8_UNorm, Format.R8_UNorm, Format.R8_UNorm, Format.Unknown);
			Texture2DDescription stagingTextureDesc = renderTarget.Texture.Description;
			stagingTextureDesc.Usage = ResourceUsage.Staging;
			stagingTextureDesc.BindFlags = BindFlags.None;
			stagingTextureDesc.CpuAccessFlags = CpuAccessFlags.Read;
			Texture2D stagingTexture = new Texture2D(Device, stagingTextureDesc);
			
			// Set up the pipeline
			Device.ImmediateContext.ClearState();
			Device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
			Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 24, 0));
			Device.ImmediateContext.Rasterizer.SetViewport(0, 0, 1, 1);
			Device.ImmediateContext.ClearRenderTargetView(renderTargetMS.RTV, new Color4(0.0f));
			Device.ImmediateContext.ClearRenderTargetView(renderTarget.RTV, new Color4(0.0f));

			// Render a quad that fills the left half of a 1x1 buffer:
			Device.ImmediateContext.VertexShader.Set(renderVS);
			Device.ImmediateContext.PixelShader.Set(renderPS);
			Device.ImmediateContext.OutputMerger.SetRenderTargets(renderTargetMS.RTV);
			Device.ImmediateContext.Draw(4, 0);
			Device.ImmediateContext.OutputMerger.ResetTargets();

			// Load the sample 0 from previous 1x1 buffer:
			Device.ImmediateContext.VertexShader.Set(loadVS);
			Device.ImmediateContext.PixelShader.Set(loadPS);
			Device.ImmediateContext.PixelShader.SetShaderResource(ColorTexMSSlot, renderTargetMS.SRV);
			Device.ImmediateContext.OutputMerger.SetRenderTargets(renderTarget.RTV);
			Device.ImmediateContext.Draw(4, 0);
			Device.ImmediateContext.OutputMerger.ResetTargets();

			// Copy the sample #0 into CPU memory:
			Device.ImmediateContext.CopyResource(renderTarget.Texture, stagingTexture);
			DataStream stagingDataStream;
			Device.ImmediateContext.MapSubresource(stagingTexture, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stagingDataStream);
			byte value = stagingDataStream.Read<byte>();
			Device.ImmediateContext.UnmapSubresource(stagingTexture, 0);

			// Set the map indices:
			MSAAOrderMap = new int[2];
			MSAAOrderMap[0] = (value == 255) ? 1 : 0;
			MSAAOrderMap[1] = (value != 255) ? 1 : 0;

			// Reset the pipeline to default state
			Device.ImmediateContext.ClearState();

			#region Clean up
			renderVSBytecode.Dispose();
			renderVS.Dispose();
			renderPS.Dispose();
			loadVS.Dispose();
			loadPS.Dispose();
			dataStream.Dispose();
			vertexBuffer.Dispose();
			inputLayout.Dispose();
			renderTargetMS.Dispose();
			renderTarget.Dispose();
			stagingTexture.Dispose();
			stagingDataStream.Dispose();
			#endregion
		}

		private void Separate(DeviceContext context, ShaderResourceView colorMultisampledSRV, RenderTargetView output0RTV, RenderTargetView output1RTV)
		{
			context.VertexShader.Set(SeparateVS);
			context.PixelShader.Set(SeparatePS);

			context.PixelShader.SetShaderResource(ColorTexMSSlot, colorMultisampledSRV);

			context.ClearRenderTargetView(output0RTV, new Color4(0));
			context.ClearRenderTargetView(output1RTV, new Color4(0));

			context.OutputMerger.SetDepthStencilState(DisableDepthStencil, 1);
			context.OutputMerger.SetBlendState(NoBlending);
			context.OutputMerger.SetTargets(output0RTV, output1RTV);

			Tri.Draw(context);

			context.OutputMerger.ResetTargets();
		}

		public void Dispose()
		{
			if (Edges != null) { Edges.Dispose(); Edges = null; }
			if (BlendingWeights != null) { BlendingWeights.Dispose(); BlendingWeights = null; }
			if (AreaTexSRV != null) { AreaTexSRV.Dispose(); AreaTexSRV = null; }
			if (SearchTexSRV != null) { SearchTexSRV.Dispose(); SearchTexSRV = null; }
			if (AreaTex != null) { AreaTex.Dispose(); AreaTex = null; }
			if (SearchTex != null) { SearchTex.Dispose(); SearchTex = null; }
			if (Stencil != null) { Stencil.Dispose(); Stencil = null; }
			if (EdgeDetectionVS != null) { EdgeDetectionVS.Dispose(); EdgeDetectionVS = null; }
			if (BlendingWeightCalculationVS != null) { BlendingWeightCalculationVS.Dispose(); BlendingWeightCalculationVS = null; }
			if (NeighborhoodBlendingVS != null) { NeighborhoodBlendingVS.Dispose(); NeighborhoodBlendingVS = null; }
			if (ResolveVS != null) { ResolveVS.Dispose(); ResolveVS = null; }
			if (SeparateVS != null) { SeparateVS.Dispose(); SeparateVS = null; }
			if (LumaEdgeDetectionPS != null) { LumaEdgeDetectionPS.Dispose(); LumaEdgeDetectionPS = null; }
			if (ColorEdgeDetectionPS != null) { ColorEdgeDetectionPS.Dispose(); ColorEdgeDetectionPS = null; }
			if (DepthEdgeDetectionPS != null) { DepthEdgeDetectionPS.Dispose(); DepthEdgeDetectionPS = null; }
			if (BlendingWeightCalculationPS != null) { BlendingWeightCalculationPS.Dispose(); BlendingWeightCalculationPS = null; }
			if (NeighborhoodBlendingPS != null) { NeighborhoodBlendingPS.Dispose(); NeighborhoodBlendingPS = null; }
			if (ResolvePS != null) { ResolvePS.Dispose(); ResolvePS = null; }
			if (SeparatePS != null) { SeparatePS.Dispose(); SeparatePS = null; }
			if (ConstBuffer != null) { ConstBuffer.Dispose(); ConstBuffer = null; }
			if (SubSampleIndicesBuffer != null) { SubSampleIndicesBuffer.Dispose(); SubSampleIndicesBuffer = null; }
			if (ResultTarget != null) { ResultTarget.Dispose(); ResultTarget = null; }
			if (TempTarget != null)
			{
				foreach (SMAARenderTarget t in TempTarget)
				{
					t.Dispose();
				}
				TempTarget = null;
			}
		}
	}

	#region Utils
	/// <summary>
	/// Non multisampled, gamma-corrected render target. Handles creation of different views needed for SMAA.
	/// Render the scene to the provided RTV, then use the provided SRVs (gamma and non-gamma) in the SMAA run.
	/// </summary>
	public class SMAARenderTarget : IDisposable
	{
		private Image Color;

		public RenderTargetView RTV { get { return Color.RTV; } }
		public ShaderResourceView SRV { get { return Color.SRV; } }
		public ShaderResourceView GammaSRV { get; private set; }

		public int Width { get { return Color.Width; } }
		public int Height { get { return Color.Height; } }

		public SMAARenderTarget(Device device, int width, int height, bool multisample = false)
		{
			SampleDescription sampleDesc = new SampleDescription
			{
				Count = multisample ? 2 : 1,
				Quality = multisample ? (int) StandardMultisampleQualityLevels.StandardMultisamplePattern : 0,
			};
			Color = new Image(device, width, height, Format.R8G8B8A8_Typeless, Format.R8G8B8A8_UNorm_SRgb, Format.R8G8B8A8_UNorm_SRgb, Format.Unknown, sampleDesc);
			GammaSRV = new ShaderResourceView(
				device,
				Color.Texture,
				new ShaderResourceViewDescription
				{
					Dimension = multisample ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D,
					Format = Format.R8G8B8A8_UNorm,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 }
				}
			);
		}

		public void Dispose()
		{
			if (GammaSRV != null) { GammaSRV.Dispose(); GammaSRV = null; }
			if (Color != null) { Color.Dispose(); Color = null; }
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 48)]
	internal struct ConstantBufferStruct
	{
		[FieldOffset(0)]
		public Vector4 RenderTargetMetrics;

		[FieldOffset(16)]
		public float Threshold;

		[FieldOffset(20)]
		public int MaxSearchSteps;

		[FieldOffset(24)]
		public int MaxSearchStepsDiag;

		[FieldOffset(28)]
		public float CornerRounding;

		[FieldOffset(32)]
		public bool Predication;

		[FieldOffset(36)]
		public bool Reprojection;
	};
	#endregion
}
