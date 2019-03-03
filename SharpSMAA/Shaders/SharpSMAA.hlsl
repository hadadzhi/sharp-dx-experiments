// SMAA parameters
cbuffer ConstBufferParameters : register(b0)
{
	float4 RenderTargetMetrics;

	float Threshold;
	int MaxSearchSteps;
	int MaxSearchStepsDiag;
	float CornerRounding;

	bool Predication;
	bool Reprojection;
};

cbuffer ConstBufferPerRun : register(b1)
{
	float4 SubSampleIndices;
}

// Use a real macro here for maximum performance!
#define SMAA_RT_METRICS RenderTargetMetrics
#define SMAA_THRESHOLD Threshold
#define SMAA_MAX_SEARCH_STEPS MaxSearchSteps
#define SMAA_MAX_SEARCH_STEPS_DIAG MaxSearchStepsDiag
#define SMAA_CORNER_ROUNDING CornerRounding
#define SMAA_PREDICATION Predication
#define SMAA_REPROJECTION Reprojection

// Set the HLSL version:
#define SMAA_HLSL_4

// Now include the SMAA header
#include "SMAA.hlsl"

/**
* Input textures
*/
Texture2D ColorTex					: register(t0);
Texture2D ColorTexGamma				: register(t1);
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
void SharpSMAAEdgeDetectionVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float4 offset[3] : TEXCOORD1)
{
	svPosition = position;
	SMAAEdgeDetectionVS(texcoord, offset);
}

void SharpSMAABlendingWeightCalculationVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float2 pixcoord : TEXCOORD1,
	out float4 offset[3] : TEXCOORD2)
{
	svPosition = position;
	SMAABlendingWeightCalculationVS(texcoord, pixcoord, offset);
}

void SharpSMAANeighborhoodBlendingVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0,
	out float4 offset : TEXCOORD1)
{
	svPosition = position;
	SMAANeighborhoodBlendingVS(texcoord, offset);
}

void SharpSMAAResolveVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0)
{
	svPosition = position;
}

void SharpSMAASeparateVS(
	float4 position : POSITION,
	out float4 svPosition : SV_POSITION,
	inout float2 texcoord : TEXCOORD0)
{
	svPosition = position;
}

float2 SharpSMAALumaEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAALumaEdgeDetectionPS(texcoord, offset, ColorTexGamma, DepthTex);
}

float2 SharpSMAAColorEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAAColorEdgeDetectionPS(texcoord, offset, ColorTexGamma, DepthTex);
}

float2 SharpSMAADepthEdgeDetectionPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset[3] : TEXCOORD1) : SV_TARGET
{
	return SMAADepthEdgeDetectionPS(texcoord, offset, DepthTex);
}

float4 SharpSMAABlendingWeightCalculationPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float2 pixcoord : TEXCOORD1,
	float4 offset[3] : TEXCOORD2) : SV_TARGET
{
	return SMAABlendingWeightCalculationPS(texcoord, pixcoord, offset, EdgesTex, AreaTex, SearchTex, SubSampleIndices);
}

float4 SharpSMAANeighborhoodBlendingPS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	float4 offset : TEXCOORD1) : SV_TARGET
{
	return SMAANeighborhoodBlendingPS(texcoord, offset, ColorTex, BlendTex, VelocityTex);
}

float4 SharpSMAAResolvePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0) : SV_TARGET
{
	return SMAAResolvePS(texcoord, ColorTex, ColorTexPrev, VelocityTex);
}

void SharpSMAASeparatePS(
	float4 position : SV_POSITION,
	float2 texcoord : TEXCOORD0,
	out float4 target0 : SV_TARGET0,
	out float4 target1 : SV_TARGET1)
{
	SMAASeparatePS(position, texcoord, target0, target1, ColorTexMS);
}

float4 SharpSMAADetectMSAAOrderRenderVS(float4 pos : POSITION, inout float2 coord : TEXCOORD0) : SV_POSITION{ pos.x = -0.5 + 0.5 * pos.x; return pos; }
float4 SharpSMAADetectMSAAOrderRenderPS(float4 pos : SV_POSITION, float2 coord : TEXCOORD0) : SV_TARGET{ return 1.0; }
float4 SharpSMAADetectMSAAOrderLoadVS(float4 pos : POSITION, inout float2 coord : TEXCOORD0) : SV_POSITION { return pos; }
float4 SharpSMAADetectMSAAOrderLoadPS(float4 pos : SV_POSITION, float2 coord : TEXCOORD0) : SV_TARGET   { int2 ipos = int2(pos.xy); return ColorTexMS.Load(ipos, 0); }
