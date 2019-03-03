#include "Common.hlsl"
#include "Lighting.hlsl"

Texture2D gTexture : register(t0);
SamplerState gSamplerState : register(s0);

Texture2DArray gShadowMap : register(t1);
SamplerComparisonState gShadowMapSamplerState : register(s1);

void VS(in VSInput input, out VSOutput output)
{
	output.PosW = mul(float4(input.Position, 1.0f), gWorld).xyz;
	output.NormalW = mul(float4(input.Normal, 0.0f), gWorldInvTranspose).xyz;

	output.PosH = mul(float4(input.Position, 1.0f), gWorldViewProj);

	output.TexC = mul(float4(input.TexC, 0.0f, 1.0f), gTextureTransform).xy;

	output.PosLightSpace = mul(float4(output.PosW, 1.0f), gLightViewProj);;

	output.PosV = mul(float4(output.PosW, 1.0f), gCameraView).xyz;

	// Positions for velocity calculation
	output.VelocityCurrPos = output.PosH.xyw;
	output.VelocityPrevPos = mul(float4(input.Position, 1.0f), gPreviousWorldViewProj).xyw;
	
	// Positions in projection space are in [-1, 1] range, while texture
	// coordinates are in [0, 1] range. So, we divide by 2 to get velocities in
	// the scale (and flip the y axis):
	output.VelocityCurrPos.xy *= float2(0.5, -0.5);
	output.VelocityPrevPos.xy *= float2(0.5, -0.5);

	// Apply jitter for temporal antialiasing
	output.PosH.xy += Jitter * output.PosH.w;
}

void PS(in VSOutput input, out float4 output : SV_Target0, out float depth : SV_Depth, out float2 velocity : SV_Target1)
{
	// Interpolating the normal could have unnormalized it
	input.NormalW = normalize(input.NormalW);

	float3 toEyeW = gEyePosW - input.PosW;
	float distToEye = length(toEyeW);

	toEyeW = normalize(toEyeW);

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 specular = float4(0.0f, 0.0f, 0.0f, 0.0f);

	output.a = 1.0;

	float percentLit;

	[flatten]
	if (gUseFullSceneSM)
	{
		percentLit = ComputePCFPercentLit(input.PosLightSpace, gShadowMap, 1, gShadowMapSamplerState);
	}
	else
	{
		percentLit = ComputePCFPercentLitCSM(input.PosV.z, input.PosLightSpace, gShadowMap, gShadowMapSamplerState);
	}

	ComputeDirectionalLight(gMaterial, gDirLight, input.NormalW, toEyeW, ambient, diffuse, specular, percentLit);

	if (gHighlightCascades && !gUseFullSceneSM)
	{
		for (uint i = 0; i < gNumSplits; i++)
		{
			[flatten]
			if (input.PosV.z < gSplitPosition[i])
			{
				diffuse *= 0.75;
				diffuse += lerp(float4(0.0f, 1.0f, 0.0f, 1.0f), float4(1.0f, 0.0f, 0.0f, 1.0f), i == 0 ? 0 : (float) i / (gNumSplits - 1)) * 0.25;

				specular *= 0.75;
				specular += lerp(float4(0.0f, 1.0f, 0.0f, 1.0f), float4(1.0f, 0.0f, 0.0f, 1.0f), i == 0 ? 0 : (float) i / (gNumSplits - 1)) * 0.25;

				break;
			}
		}
	}

	[flatten]
	if (gMaterial.Textured)
	{
		float4 texColor = gTexture.Sample(gSamplerState, input.TexC);
		ambient *= texColor;
		diffuse *= texColor;
		output.a *= texColor.a;
	}

	output = ambient + diffuse + specular;
	output.a *= gMaterial.Diffuse.a;

	ApplyFog(output, distToEye);

	// Logarithmic depth transform
	float zv = input.PosH.w;
	float near = CameraNear;
	float far = CameraFar;
	depth = log(zv / near) / log(far / near);

	// Velocity calculation
	// Convert to homogeneous points by dividing by w:
	input.VelocityCurrPos.xy /= input.VelocityCurrPos.z; // w is stored in z
	input.VelocityPrevPos.xy /= input.VelocityPrevPos.z; // w is stored in z

	// Calculate velocity in homogeneous projection space:
	velocity = input.VelocityCurrPos.xy - input.VelocityPrevPos.xy;
}

void OverlayVS(in float3 pos : POSITION, in float3 tex : TEXCOORD, out float4 posout : SV_Position, out float3 texout : TEXCOORD)
{
	posout = float4(pos, 1.0);
	texout = tex;
}

void OverlayPS(in float4 ignored : SV_Position, in float3 tex : TEXCOORD, out float4 output : SV_Target)
{
	output.rgba = gShadowMap.Sample(gSamplerState, tex.xyz).r;
}
