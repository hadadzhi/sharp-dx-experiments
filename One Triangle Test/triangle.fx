struct VertexIn
{
	float3 Position : POSITION;
	float4 Color : COLOR;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	float4 Color: COLOR;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.Position.xyz = vin.Position.xyz;
	vout.Position.w = 1.0f;
	vout.Color = vin.Color;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return pin.Color;
}
