#ifndef _COMMON_HLSL
#define _COMMON_HLSL

/// PSSM constants
static const int CSM_MAX_SPLITS = 9;

struct Material
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular; // Specular.w is the glossiness
	float4 Reflect;

	bool Textured;
};

struct DirectionalLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Direction;
};

struct PointLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Position;
	float Range;
	float3 Att;
};

struct SpotLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;

	float3 Position;
	float Range;

	float3 Direction;
	float Spot;

	float3 Att;
};

cbuffer cbConstants : register(b2)
{
	float4 gFogColor = float4(0.4f, 0.5f, 0.7f, 1.0f);
	float gFogStartDist = 100;
	float gFullFogDist = 500;

	float gShadowMapSize;
	bool gUseFullSceneSM;

	bool gHighlightCascades;
}

cbuffer cbPerFrame : register(b0)
{
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;

	float3 gEyePosW;
	uint gNumSplits;

	float4x4 gLightViewProj;
	float4x4 gCameraView;

	float CameraNear;
	float CameraFar;

	float2 Jitter;

	float4x4 gLightCrop[CSM_MAX_SPLITS];
	float gSplitPosition[CSM_MAX_SPLITS];
};

cbuffer cbPerObject : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float4x4 gPreviousWorldViewProj;
	float4x4 gTextureTransform;

	Material gMaterial;
};

struct VSInput
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
	float3 TangentU : TANGENT_U;
	float2 TexC : TEX_COORD;
	float4 Color : COLOR;

	uint InstanceID : SV_InstanceID;
};

struct VSOutput
{
	float4 PosH : SV_POSITION;
	float3 PosW : WORLD_POSITION;
	float3 PosV : VIEW_POSITION;
	float3 VelocityCurrPos : TEXCOORD2;
	float3 VelocityPrevPos : TEXCOORD3;
	float3 NormalW : NORMAL;
	float2 TexC : TEXCOORD0;
	float4 PosLightSpace : TEXCOORD1;
};

#endif
