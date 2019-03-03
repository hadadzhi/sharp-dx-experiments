#ifndef _BASIC_LIGHTING_FUNCTIONS_HLSL
#define _BASIC_LIGHTING_FUNCTIONS_HLSL

#include "Common.hlsl"

/// PCF constants
static const float gPCFKernelLeft = -1.5;
static const float gPCFKernelRight = 1.5;
static const float gPCFKernelHStep = 1.0;

static const float gPCFKernelBottom = -1.5;
static const float gPCFKernelTop = 1.5;
static const float gPCFKernelVStep = 1.0;

float ComputePCFPercentLit(float4 posLightSpace, Texture2DArray shadowMap, int arrayIndex, SamplerComparisonState shadowMapSampler)
{
	// Do perspective divide
	posLightSpace.xyz /= posLightSpace.w;

	// Transform to texture coordinates
	posLightSpace.x *= 0.5f;
	posLightSpace.x += 0.5f;
	posLightSpace.y *= -0.5f;
	posLightSpace.y += 0.5f;

	float texelSize = 1.0f / gShadowMapSize;

	// Avoid strange light stripe artifact
	[flatten]
	if (max(posLightSpace.x, posLightSpace.y) > 1.0f - texelSize || min(posLightSpace.x, posLightSpace.y) < texelSize) 
	{ 
		return 0.0f;
	}

	float percentLit = 0.0f;
	float2 offset;
	float3 sampleCoord;
	int taps = 0;

	[unroll]
	for (float deltaU = gPCFKernelLeft; deltaU <= gPCFKernelRight; deltaU += gPCFKernelHStep)
	{
		[unroll]
		for (float deltaV = gPCFKernelBottom; deltaV <= gPCFKernelTop; deltaV += gPCFKernelVStep)
		{
			offset = float2(deltaU, deltaV) * texelSize;
			sampleCoord = float3(posLightSpace.xy + offset, arrayIndex);

			percentLit += shadowMap.SampleCmpLevelZero(shadowMapSampler, sampleCoord, posLightSpace.z);

			++taps;
		}
	}

	return percentLit / taps;
}

float ComputePCFPercentLitCSM(float depthViewSpace, float4 posLightSpace, Texture2DArray shadowMap, SamplerComparisonState shadowMapSampler)
{
	for (uint i = 0; i < gNumSplits; i++)
	{
		[flatten]
		if (depthViewSpace < gSplitPosition[i])
		{
			return ComputePCFPercentLit(mul(posLightSpace, gLightCrop[i]), shadowMap, i, shadowMapSampler);
		}
	}

	return 0.0f;
}

void ComputeDirectionalLight(Material mat, DirectionalLight light, float3 normal, float3 toEye, inout float4 ambient, inout float4 diffuse, inout float4 spec, float percentLit)
{
	float3 toLight = normalize(-light.Direction);
	float4 lightFlux = max(0, dot(toLight, normal)) * percentLit * light.Diffuse; // light.Diffuse is light intensity per channel
	
	float specPower = mat.Specular.w;
	
	float3 halfway = normalize(toLight + toEye);
	float specAmount = pow(max(dot(halfway, normal), 0), specPower);
	
	float diffuseAmount = 1 - specAmount;
	
	const float pi = 3.1415927;

	ambient += mat.Ambient * light.Ambient;
	diffuse += mat.Diffuse * lightFlux * diffuseAmount / pi; // Divide by Pi to conserve energy
	spec += mat.Specular * lightFlux * specAmount * (specPower + 8) / (8 * pi);
}

void ApplyFog(inout float4 litColor, float distToEye)
{
	litColor = lerp(
		litColor,
		gFogColor,
		saturate((distToEye - gFogStartDist) / (gFullFogDist - gFogStartDist))
	);
}

#endif
