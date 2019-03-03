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
			public readonly RasterizerState FrontCullSlopeScaledBias;

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

				FrontCullSlopeScaledBias = new RasterizerState(
					device,
					new RasterizerStateDescription
					{
						CullMode = CullMode.Front,
						DepthBias = 0,
						DepthBiasClamp = 0.0f,
						SlopeScaledDepthBias = -1.0f,
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
			public readonly DepthStencilState ColorPass;
			public readonly DepthStencilState DisableDepthWrites;

			internal DepthStencilStates(Device device)
			{
				Default = null;

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
				Default = null;

				BlendStateDescription blendDesc = new BlendStateDescription
				{
					AlphaToCoverageEnable = false,
					IndependentBlendEnable = false
				};

				blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
				{
					IsBlendEnabled = true,
					SourceBlend = BlendOption.SourceAlpha,
					DestinationBlend = BlendOption.InverseSourceAlpha,
					BlendOperation = BlendOperation.Add,
					SourceAlphaBlend = BlendOption.One,
					DestinationAlphaBlend = BlendOption.Zero,
					AlphaBlendOperation = BlendOperation.Add,
					RenderTargetWriteMask = ColorWriteMaskFlags.All
				};

				AlphaBlend = new BlendState(device, blendDesc);

				blendDesc.RenderTarget[0].RenderTargetWriteMask = 0;

				DisableRTWrites = new BlendState(device, blendDesc);
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

			internal SamplerStates(Device device)
			{
				Default = null;

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

				ShadowMapPCF = new SamplerState(
					device,
					new SamplerStateDescription
					{
						Filter = Filter.ComparisonMinMagLinearMipPoint,
						AddressU = TextureAddressMode.Border,
						AddressV = TextureAddressMode.Border,
						AddressW = TextureAddressMode.Border,
						BorderColor = new Color4(0f),
						ComparisonFunction = Comparison.Less
					}
				);
			}
		}

		#endregion
	}
}
