using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;

namespace SharpDXCommons
{
	public class PipelineStates
	{
		public readonly RasterizerStates Rasterizer;
		public readonly DepthStencilStates DepthStencil;
		public readonly BlendStates Blend;
		public readonly SamplerStates Sampler;

		internal PipelineStates(Device device)
		{
			Rasterizer = new RasterizerStates(device);
			DepthStencil = new DepthStencilStates(device);
			Blend = new BlendStates(device);
			Sampler = new SamplerStates(device);
		}

		#region Rasterizer
		
		public class RasterizerStates
		{
			public readonly RasterizerState Default;
			public readonly RasterizerState DisableBackfaceCulling;
			public readonly RasterizerState InverseWindingRule;
			public readonly RasterizerState FrontFaceCulling;
			public readonly RasterizerState DepthBias;

			internal RasterizerStates(Device device)
			{
				Default = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.Back,
						DepthBias = 0,
						DepthBiasClamp = 0.0f,
						SlopeScaledDepthBias = 0.0f,
						FillMode = FillMode.Solid,
						IsAntialiasedLineEnabled = false,
						IsDepthClipEnabled = true,
						IsFrontCounterClockwise = false,
						IsMultisampleEnabled = false,
						IsScissorEnabled = false
					}
				);

				DisableBackfaceCulling = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.None,
						DepthBias = 0,
						DepthBiasClamp = 0.0f,
						FillMode = FillMode.Solid,
						IsAntialiasedLineEnabled = false,
						IsDepthClipEnabled = true,
						IsFrontCounterClockwise = false,
						IsMultisampleEnabled = false,
						IsScissorEnabled = false,
						SlopeScaledDepthBias = 0.0f
					}
				);

				InverseWindingRule = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.Back,
						DepthBias = 0,
						DepthBiasClamp = 0.0f,
						FillMode = FillMode.Solid,
						IsAntialiasedLineEnabled = false,
						IsDepthClipEnabled = true,
						IsFrontCounterClockwise = true,
						IsMultisampleEnabled = false,
						IsScissorEnabled = false,
						SlopeScaledDepthBias = 0.0f
					}
				);

				FrontFaceCulling = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.Front,
						DepthBias = 0,
						DepthBiasClamp = 0.0f,
						SlopeScaledDepthBias = 0.0f,
						FillMode = FillMode.Solid,
						IsAntialiasedLineEnabled = false,
						IsDepthClipEnabled = true,
						IsFrontCounterClockwise = false,
						IsMultisampleEnabled = false,
						IsScissorEnabled = false
					}
				);

				DepthBias = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.Back,
						DepthBias = 25,
						DepthBiasClamp = 0.0f,
						SlopeScaledDepthBias = 1.0f,
						FillMode = FillMode.Solid,
						IsAntialiasedLineEnabled = false,
						IsDepthClipEnabled = true,
						IsFrontCounterClockwise = false,
						IsMultisampleEnabled = false,
						IsScissorEnabled = false
					}
				);
			}
		}

		#endregion

		#region DepthStencil

		public class DepthStencilStates
		{
			public readonly DepthStencilState Default;
			public readonly DepthStencilState InverseDepth;
			public readonly DepthStencilState ColorPass;
			public readonly DepthStencilState DisableDepthWrites;
			public readonly DepthStencilState DisableDepthStencil;

			internal DepthStencilStates(Device device)
			{
				Default = new DepthStencilState(
					device,
					new DepthStencilStateDescription
					{
						IsDepthEnabled = true,
						DepthWriteMask = DepthWriteMask.All,
						DepthComparison = Comparison.Less,
						IsStencilEnabled = false
					}
				);

				InverseDepth = new DepthStencilState(
					device,
					new DepthStencilStateDescription
					{
						IsDepthEnabled = true,
						DepthWriteMask = DepthWriteMask.All,
						DepthComparison = Comparison.Greater,
						IsStencilEnabled = false
					}
				);

				ColorPass = new DepthStencilState(
					device,
					new DepthStencilStateDescription
					{
						IsDepthEnabled = true,
						DepthWriteMask = DepthWriteMask.Zero,
						DepthComparison = Comparison.LessEqual,
						IsStencilEnabled = false
					}
				);

				DisableDepthWrites = new DepthStencilState(
					device,
					new DepthStencilStateDescription
					{
						IsDepthEnabled = true,
						DepthWriteMask = DepthWriteMask.Zero,
						DepthComparison = Comparison.Less,
						IsStencilEnabled = false
					}
				);

				DisableDepthStencil = new DepthStencilState(
					device,
					new DepthStencilStateDescription
					{
						IsDepthEnabled = false,
						IsStencilEnabled = false,
						DepthWriteMask = DepthWriteMask.Zero
					}
				);
			}
		}

		#endregion

		#region Blending

		public class BlendStates
		{
			public readonly BlendState Default;
			public readonly BlendState AlphaBlend;
			public readonly BlendState DisableRTWrites;

			internal BlendStates(Device device)
			{
				BlendStateDescription defaultBlendDesc = new BlendStateDescription
				{
					AlphaToCoverageEnable = false,
					IndependentBlendEnable = false
				};

				defaultBlendDesc.RenderTarget[0] = new RenderTargetBlendDescription
				{
					IsBlendEnabled = false,
					SourceBlend = BlendOption.One,
					DestinationBlend = BlendOption.Zero,
					BlendOperation = BlendOperation.Add,
					SourceAlphaBlend = BlendOption.One,
					DestinationAlphaBlend = BlendOption.Zero,
					AlphaBlendOperation = BlendOperation.Add,
					RenderTargetWriteMask = ColorWriteMaskFlags.All
				};

				// Default
				Default = new BlendState(device, defaultBlendDesc);

				// Alpha blend
				BlendStateDescription alphaBlendDesc = defaultBlendDesc;

				alphaBlendDesc.RenderTarget[0].IsBlendEnabled = true;
				alphaBlendDesc.RenderTarget[0].SourceBlend = BlendOption.Zero;
				alphaBlendDesc.RenderTarget[0].DestinationBlend = BlendOption.SourceColor;
				alphaBlendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;

				AlphaBlend = new BlendState(device, defaultBlendDesc);

				// Disable render target writes
				BlendStateDescription disableRTBlendDesc = defaultBlendDesc;

				disableRTBlendDesc.RenderTarget[0].RenderTargetWriteMask = 0;

				DisableRTWrites = new BlendState(device, defaultBlendDesc);
			}
		}

		#endregion

		#region Samplers

		public class SamplerStates
		{
			public readonly SamplerState Default;
			public readonly SamplerState WrappedAnisotropic;
			public readonly SamplerState WrappedLinear;
			public readonly SamplerState ShadowMapPCF;
			public readonly SamplerState BorderPoint;
			public readonly SamplerState BorderLinear;

			internal SamplerStates(Device device)
			{
				Default = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.MinMagMipLinear,
						AddressU = TextureAddressMode.Clamp,
						AddressV = TextureAddressMode.Clamp,
						AddressW = TextureAddressMode.Clamp,
						MinimumLod = -float.MaxValue,
						MaximumLod = float.MaxValue,
						MipLodBias = 0.0f,
						MaximumAnisotropy = 1,
						BorderColor = Color4.White,
						ComparisonFunction = Comparison.Never
					}
				);

				WrappedAnisotropic = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.Anisotropic,
						MaximumAnisotropy = 16,
						AddressU = TextureAddressMode.Wrap,
						AddressV = TextureAddressMode.Wrap,
						AddressW = TextureAddressMode.Wrap,
						MaximumLod = float.MaxValue,
						MinimumLod = 0.0f,
						MipLodBias = 0.0f
					}
				);

				WrappedLinear = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.MinMagMipLinear,
						AddressU = TextureAddressMode.Wrap,
						AddressV = TextureAddressMode.Wrap,
						AddressW = TextureAddressMode.Wrap,
						MaximumLod = float.MaxValue,
						MinimumLod = 0.0f,
						MipLodBias = 0.0f
					}
				);

				BorderLinear = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.MinMagMipLinear,
						AddressU = TextureAddressMode.Border,
						AddressV = TextureAddressMode.Border,
						AddressW = TextureAddressMode.Border,
						MaximumLod = float.MaxValue,
						MinimumLod = 0.0f,
						MipLodBias = 0.0f,
						BorderColor = Color4.Black
					}
				);

				BorderPoint = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.MinMagMipPoint,
						AddressU = TextureAddressMode.Border,
						AddressV = TextureAddressMode.Border,
						AddressW = TextureAddressMode.Border,
						MaximumLod = float.MaxValue,
						MinimumLod = 0.0f,
						MipLodBias = 0.0f,
						BorderColor = Color4.Black
					}
				);

				ShadowMapPCF = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.ComparisonMinMagLinearMipPoint,
						AddressU = TextureAddressMode.Border,
						AddressV = TextureAddressMode.Border,
						AddressW = TextureAddressMode.Border,
						BorderColor = new Color4(0.0f),
						ComparisonFunction = Comparison.Less
					}
				);
			}
		}

		#endregion
	}
}
