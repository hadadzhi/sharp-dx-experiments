cbuffer ConstBuffer : register(b0)
{
	float4x4 TextureTransform;
}

Texture2D Image : register(t0);
SamplerState Sampler : register(s0);

void ScreenTriangleVS(
	in float4 position : POSITION,
	in float2 texcoord : TEXCOORD,
	out float4 svPosition : SV_POSITION,
	out float2 outTexcoord : TEXCOORD)
{
	svPosition = position;
	outTexcoord = mul(float4(texcoord, 0.0f, 1.0f), TextureTransform).xy;
}

void ScreenTrianglePS(
	in float4 svPosition : SV_POSITION,
	in float2 texcoord : TEXCOORD,
	out float4 outColor : SV_TARGET)
{
	outColor = Image.Sample(Sampler, texcoord);
}

void ScreenTrianglePSDepth(
	in float4 svPosition : SV_POSITION,
	in float2 texcoord : TEXCOORD,
	out float4 outColor : SV_TARGET)
{
	outColor = float4(Image.Sample(Sampler, texcoord).rrr, 1.0f);
}
