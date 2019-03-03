#include "Structs.hlsl"

cbuffer cbPerFrame : register(b0)
{
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePosW;
	float4x4 gDirLightViewProj;
};

cbuffer cbPerObject : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float4x4 gTextureTransform;
	Material gMaterial;
};

cbuffer cbConstants : register(b2)
{
	float gShadowMapSize;
}

Texture2D gTexture : register(t0);
SamplerState gSamplerState : register(s0);

Texture2D gShadowMap : register(t1);
SamplerComparisonState gShadowMapSamplerState : register(s1);

struct VertexIn
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
	float3 TangentU : TANGENT_U;
	float2 TexC : TEX_COORD;
	float4 Color : COLOR;
};

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float3 PosW : POSITION;
	float3 NormalW : NORMAL;
	float2 TexC : TEXCOORD0;
	float4 ShadowMapC : TEXCOORD1;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	// Transform to world space.
	vout.PosW    = mul(float4(vin.Position, 1.0f), gWorld).xyz;
	vout.NormalW = mul(float4(vin.Normal, 0.0f), gWorldInvTranspose).xyz;

	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(vin.Position, 1.0f), gWorldViewProj);

	// Transform texture coordinates
	vout.TexC = mul(float4(vin.TexC, 0.0f, 1.0f), gTextureTransform).xy;

	// Transform to shadow map space
	vout.ShadowMapC = mul(float4(vout.PosW, 1.0f), gDirLightViewProj);

	// Transform NDC to texture coordinates
	vout.ShadowMapC.x *= 0.5;
	vout.ShadowMapC.x += 0.5;
	vout.ShadowMapC.y *= -0.5;
	vout.ShadowMapC.y += 0.5;
	
	// vin.Color is unused
	// vin.TangentU is unused

	return vout;
}

float ComputeDirLightShadowFactor(float4 shadowMapC)
{
	// Manual perspective divide is required if the shadow map is rendered using perspective projection
	// Unnecessary for orthographic projection (w == 1)
	shadowMapC.xyz /= shadowMapC.w;

	static const float texelSize = 1.0f / gShadowMapSize;
	float shadowFactor = 0.0f;
	float deltaU, deltaV;

	// 4x4 manual PCF
	[unroll]
	for (deltaV = -1.5; deltaV <= 1.5; deltaV += 1.0)
	{
		[unroll]
		for (deltaU = -1.5; deltaU <= 1.5; deltaU += 1.0)
		{
			// Each sample does 4 tap PCF
			shadowFactor += gShadowMap.SampleCmpLevelZero(
				gShadowMapSamplerState,
				shadowMapC.xy + float2(deltaU, deltaV) * texelSize,
				shadowMapC.z
			);
		}
	}

	return shadowFactor / 16.0f;
}

void ComputeDirectionalLight(
	Material mat,
	DirectionalLight L,
	float3 normal,
	float3 toEye,
	float4 shadowMapC,
	out float4 ambient,
	out float4 diffuse,
	out float4 spec)
{
	// Initialize outputs.
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

	// The light vector aims opposite the direction the light rays travel.
	float3 toLight = -L.Direction;

	// Add ambient term.
	ambient = mat.Ambient * L.Ambient;

	// Add diffuse and specular term, provided the surface is in 
	// the line of site of the light.
	float diffuseFactor = dot(toLight, normal);
	float shadowFactor = ComputeDirLightShadowFactor(shadowMapC);

	// Flatten to avoid dynamic branching.
	[flatten]
	if (diffuseFactor * shadowFactor > 0.0f)
	{
		float3 v         = reflect(-toLight, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), abs(mat.Specular.w));

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse * shadowFactor;
		spec = specFactor * (mat.Specular * L.Specular) * shadowFactor;
	}
}

void ComputePointLight(
	Material mat, 
	PointLight L, 
	float3 pos, 
	float3 normal, 
	float3 toEye,	   
	out float4 ambient, 
	out float4 diffuse, 
	out float4 spec)
{
	// Initialize outputs.
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

	// The vector from the surface to the light.
	float3 toLight = L.Position - pos;
		
	// The distance from surface to light.
	float d = length(toLight);
	
	// Range test.
	[flatten]
	if( d > L.Range )
		return;
		
	// Normalize the light vector.
	toLight /= d;
	
	// Ambient term.
	ambient = mat.Ambient * L.Ambient;	

	// Add diffuse and specular term, provided the surface is in
	// the line of site of the light.
	float diffuseFactor = dot(toLight, normal);

	// Flatten to avoid dynamic branching.
	[flatten]
	if( diffuseFactor > 0.0f )
	{
		float3 v         = reflect(-toLight, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
					
		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec    = specFactor * mat.Specular * L.Specular;
	}

	// Attenuate
	float att = 1.0f / dot(L.Att, float3(1.0f, d, d*d));

	diffuse *= att;
	spec    *= att;
}

void ComputeSpotLight(
	Material mat, 
	SpotLight light, 
	float3 pos, 
	float3 normal, 
	float3 toEye,
	out float4 ambient, 
	out float4 diffuse, 
	out float4 spec)
{
	ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float3 toLight = light.Position - pos;
		
	// The distance from surface to light.
	float distToLight = length(toLight);
	
	[flatten]
	if (distToLight > light.Range)
	{
		return;
	}
		
	toLight /= distToLight; // Normalize
	
	ambient = mat.Ambient * light.Ambient;

	float diffuseFactor = dot(toLight, normal);

	[flatten]
	if(diffuseFactor > 0.0f)
	{
		float3 v = reflect(-toLight, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), mat.Specular.w);
					
		diffuse = diffuseFactor * mat.Diffuse * light.Diffuse;
		spec = specFactor * mat.Specular * light.Specular;
	}
	
	// Scale by spotlight factor and attenuate.
	float spot = pow(max(dot(-toLight, light.Direction), 0.0f), light.Spot);
	float att = spot / dot(light.Att, float3(1.0f, distToLight, distToLight * distToLight));

	ambient *= spot;
	diffuse *= att;
	spec    *= att;
}

float4 ComputeFoggedColor(float4 litColor, float distToEye)
{
	const float4 fogColor = float4(0.4f, 0.5f, 0.7f, 1.0f);
	const float fogStartDist = 5;
	const float fullFogDist = 50;
	const float fogging = saturate((distToEye - fogStartDist) / (fullFogDist - fogStartDist));

	return lerp(litColor, fogColor, fogging);
}

float4 PS(VertexOut pin) : SV_TARGET
{
	// Interpolating the normal could have unnormalized it
	pin.NormalW = normalize(pin.NormalW);

	float3 toEyeW = gEyePosW - pin.PosW;
	float distToEye = length(toEyeW);
	toEyeW /= distToEye; // Normalize

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 a, d, s;
	ComputeDirectionalLight(gMaterial, gDirLight, pin.NormalW, toEyeW, pin.ShadowMapC, a, d, s);
	ambient += a;
	diffuse += d;
	spec += s;

	ComputePointLight(gMaterial, gPointLight, pin.PosW, pin.NormalW, toEyeW, a, d, s);
	ambient += a;  
	diffuse += d;
	spec += s;
	
	ComputeSpotLight(gMaterial, gSpotLight, pin.PosW, pin.NormalW, toEyeW, a, d, s);
	ambient += a;  
	diffuse += d;
	spec += s;
	
	float4 litColor;

	[flatten]
	if (gMaterial.Textured)
	{
		float4 texColor = gTexture.Sample(gSamplerState, pin.TexC);
		litColor = texColor * (ambient + diffuse) + spec;
		litColor = ComputeFoggedColor(litColor, distToEye);
		litColor.a = gMaterial.Diffuse.a * texColor.a;
	}
	else
	{
		litColor = ambient + diffuse + spec;
		litColor = ComputeFoggedColor(litColor, distToEye);
		litColor.a = gMaterial.Diffuse.a;
	}

	return litColor;
}
