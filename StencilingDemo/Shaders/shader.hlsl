struct Material
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular; // Specular.w is the gloss factor
	float4 Reflect;
	bool Textured;
};

struct DirectionalLight
{
	float4 Ambient;
	float4 Diffuse;
	float4 Specular;
	float3 Direction; // Unused
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

cbuffer cbPerFrame : register(b0)
{
	DirectionalLight gDirLight;
	PointLight gPointLight;
	SpotLight gSpotLight;
	float3 gEyePosW;
};

cbuffer cbPerObject : register(b1)
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float4x4 gTextureTransform;
	Material gMaterial;
};

Texture2D gTexture : register(t0);
SamplerState gSamplerState : register(s0);

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
	float2 TexC : TEX_COORD;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	// Transform to world space.
	vout.PosW    = mul(float4(vin.Position, 1.0f), gWorld).xyz;
	vout.NormalW = mul(float4(vin.Normal, 0.0f), gWorldInvTranspose).xyz;

	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(vin.Position, 1.0f), gWorldViewProj);

	vout.TexC = mul(float4(vin.TexC, 0.0f, 1.0f), gTextureTransform).xy;

	// vin.Color is unused
	// vin.TangentU is unused

	return vout;
}

void ComputeDirectionalLight(
	Material mat, 
	DirectionalLight L, 
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

	// The light vector aims opposite the direction the light rays travel.
	float3 toLight = -L.Direction;

	// Add ambient term.
	ambient = mat.Ambient * L.Ambient;

	// Add diffuse and specular term, provided the surface is in 
	// the line of site of the light.
	
	float diffuseFactor = dot(toLight, normal);

	// Flatten to avoid dynamic branching.
	[flatten]
	if (diffuseFactor > 0.0f)
	{
		float3 v         = reflect(-toLight, normal);
		float specFactor = pow(max(dot(v, toEye), 0.0f), abs(mat.Specular.w));

		diffuse = diffuseFactor * mat.Diffuse * L.Diffuse;
		spec    = specFactor * (mat.Specular * L.Specular);
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

float4 PS(VertexOut pin) : SV_Target
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

	ComputeDirectionalLight(gMaterial, gDirLight, pin.NormalW, toEyeW, a, d, s);
	ambient += a;  
	diffuse += d;
	spec    += s;

	ComputePointLight(gMaterial, gPointLight, pin.PosW, pin.NormalW, toEyeW, a, d, s);
	ambient += a;  
	diffuse += d;
	spec    += s;
	
	ComputeSpotLight(gMaterial, gSpotLight, pin.PosW, pin.NormalW, toEyeW, a, d, s);
	ambient += a;  
	diffuse += d;
	spec    += s;
	
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
