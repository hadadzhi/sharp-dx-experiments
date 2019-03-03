// SMAA shader parameters
cbuffer SMAA_Shader_CBuffer : register(b0)
{
	float4 RenderTargetMetrics;
	float4 SubSampleIndices;

	float Threshold;
	float MaxSearchSteps;
	float MaxSearchStepsDiag;
	float CornerRounding;

	bool Predication;
	bool Reprojection;
};

// Use a real macro here for maximum performance!
#define SMAA_RT_METRICS RenderTargetMetrics
#define SMAA_PREDICATION Predication
#define SMAA_REPROJECTION Reprojection

// Set the HLSL version:
#define SMAA_HLSL_4_1

// Set preset defines:
//#define SMAA_PRESET_HIGH
#define SMAA_THRESHOLD Threshold
#define SMAA_MAX_SEARCH_STEPS MaxSearchSteps
#define SMAA_MAX_SEARCH_STEPS_DIAG MaxSearchStepsDiag
#define SMAA_CORNER_ROUNDING CornerRounding

// These were defined in SMAA.hlsl using Effects framework
// We do not use the effects framework, 
// so we will set them from the application code
SamplerState LinearSampler			: register(s0); // { Filter = MIN_MAG_LINEAR_MIP_POINT; AddressU = Clamp; AddressV = Clamp; };
SamplerState PointSampler			: register(s1); // { Filter = MIN_MAG_MIP_POINT; AddressU = Clamp; AddressV = Clamp; };

// Now include the SMAA header
#include "SMAA_modified.hlsl"

/**
* Input textures
*/
Texture2D ColorTex_sRGB				: register(t0);
Texture2D ColorTex					: register(t1);
Texture2D ColorTexPrev				: register(t2);
Texture2DMS<float4, 2> ColorTexMS	: register(t3);
Texture2D DepthTex					: register(t4);
Texture2D VelocityTex				: register(t5);

/**
* Temporal textures
*/
Texture2D EdgesTex					: register(t6);
Texture2D BlendTex					: register(t7);

/**
* Pre-computed area and search textures
*/
Texture2D AreaTex					: register(t8);
Texture2D SearchTex					: register(t9);

/**
* Function wrappers
*/
void SharpDX_SMAAEdgeDetectionVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float4 offset[3] : TEXCOORD1)
{
	svPosition = position;
	SMAAEdgeDetectionVS(texcoord, offset);
}

void SharpDX_SMAABlendingWeightCalculationVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float2 pixcoord : TEXCOORD1,
	out float4 offset[3] : TEXCOORD2)
{
	svPosition = position;
	SMAABlendingWeightCalculationVS(texcoord, pixcoord, offset);
}

void SharpDX_SMAANeighborhoodBlendingVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float4 offset : TEXCOORD1)
{
	svPosition = position;
	SMAANeighborhoodBlendingVS(texcoord, offset);
}

void SharpDX_SMAAResolveVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0)
{
	svPosition = position;
}

void SharpDX_SMAASeparateVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0)
{
	svPosition = position;
}

float2 SharpDX_SMAALumaEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAALumaEdgeDetectionPS(texcoord, offset, ColorTex, DepthTex);
}

float2 SharpDX_SMAAColorEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAAColorEdgeDetectionPS(texcoord, offset, ColorTex, DepthTex);
}

float2 SharpDX_SMAADepthEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAADepthEdgeDetectionPS(texcoord, offset, DepthTex);
}

float4 SharpDX_SMAABlendingWeightCalculationPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float2 pixcoord : TEXCOORD1,
	float4 offset[3] : TEXCOORD2) : SV_TARGET
{
	return SMAABlendingWeightCalculationPS(texcoord, pixcoord, offset, EdgesTex, AreaTex, SearchTex, SubSampleIndices);
}

float4 SharpDX_SMAANeighborhoodBlendingPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset : TEXCOORD1) : SV_TARGET
{
	return SMAANeighborhoodBlendingPS(texcoord, offset, ColorTex_sRGB, BlendTex, VelocityTex);
}

float4 SharpDX_SMAAResolvePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0) : SV_TARGET
{
	return SMAAResolvePS(texcoord, ColorTex_sRGB, ColorTexPrev, VelocityTex);
}

void SharpDX_SMAASeparatePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	out float4 target0 : SV_TARGET0,
	out float4 target1 : SV_TARGET1)
{
	SMAASeparatePS(position, texcoord, target0, target1, ColorTexMS);
}
