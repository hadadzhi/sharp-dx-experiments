// SMAA parameters
cbuffer ConstBuffer : register(b0)
{
	float4 RenderTargetMetrics;
	float4 SubSampleIndices;

	float Threshold;
	int MaxSearchSteps;
	int MaxSearchStepsDiag;
	float CornerRounding;

	bool Predication;
	bool Reprojection;
};

// Use a real macro here for maximum performance!
#define SMAA_RT_METRICS RenderTargetMetrics
#define SMAA_THRESHOLD Threshold
#define SMAA_MAX_SEARCH_STEPS MaxSearchSteps
#define SMAA_MAX_SEARCH_STEPS_DIAG MaxSearchStepsDiag
#define SMAA_CORNER_ROUNDING CornerRounding
//#define SMAA_PREDICATION Predication
//#define SMAA_REPROJECTION Reprojection

// Set the HLSL version:
#define SMAA_HLSL_4_1

// Now include the SMAA header
#include "SMAA.hlsl"

/**
* Input textures
*/
Texture2D ColorTex					: register(t0); // Main input
Texture2D ColorTexPrev				: register(t1); // Previous frame, used in T2x and 4x modes
Texture2DMS<float4, 2> ColorTexMS	: register(t2); // Input for separation, used in S2x and 4x modes
Texture2D DepthTex					: register(t3); // Input depth or input for Predication
Texture2D VelocityTex				: register(t4); // Velocity, used in T2x and 4x modes

/**
* Temporal textures
*/
Texture2D EdgesTex					: register(t5);
Texture2D BlendTex					: register(t6);

/**
* Pre-computed area and search textures
*/
Texture2D AreaTex					: register(t7);
Texture2D SearchTex					: register(t8);

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
	//return SMAALumaEdgeDetectionPS(texcoord, offset, ColorTex, DepthTex);
	return SMAALumaEdgeDetectionPS(texcoord, offset, ColorTex);
}

float2 SharpDX_SMAAColorEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	//return SMAAColorEdgeDetectionPS(texcoord, offset, ColorTex, DepthTex);
	return SMAAColorEdgeDetectionPS(texcoord, offset, ColorTex);
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
	//return SMAANeighborhoodBlendingPS(texcoord, offset, ColorTex, BlendTex, VelocityTex);
	return SMAANeighborhoodBlendingPS(texcoord, offset, ColorTex, BlendTex);
}

float4 SharpDX_SMAAResolvePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0) : SV_TARGET
{
	//return SMAAResolvePS(texcoord, ColorTex, ColorTexPrev, VelocityTex);
	return SMAAResolvePS(texcoord, ColorTex, ColorTexPrev);
}

void SharpDX_SMAASeparatePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	out float4 target0 : SV_TARGET0,
	out float4 target1 : SV_TARGET1)
{
	SMAASeparatePS(position, texcoord, target0, target1, ColorTexMS);
}
