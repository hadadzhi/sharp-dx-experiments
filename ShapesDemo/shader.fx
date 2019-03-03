cbuffer cbPerObject
{
	float4x4 gWorldViewProj;
};

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
	float4 Color : COLOR;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(vin.Position, 1.0f), gWorldViewProj);
	
	// Just pass vertex color into the pixel shader.
	vout.Color = vin.Color;
	
	return vout;
}

float4 PS(VertexOut pin) : SV_Target
{
	return pin.Color;
}