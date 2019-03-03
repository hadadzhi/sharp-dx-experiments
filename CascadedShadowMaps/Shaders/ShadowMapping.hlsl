#include "Common.hlsl"

struct ShadowMapVSOutput
{
	float4 PosLightSpace : SV_Position;
	uint InstanceID : INSTANCE;
};

struct ShadowMapGSOutput
{
	float4 PosLightCropped : SV_Position;
	uint RenderTargetArrayIndex : SV_RenderTargetArrayIndex;
};

void VSShadow(VSInput input, out ShadowMapVSOutput output)
{
	float4 posWorld = mul(float4(input.Position, 1.0f), gWorld);
	output.PosLightSpace = mul(posWorld, gLightViewProj);
	output.InstanceID = input.InstanceID;
}

[maxvertexcount(3 * CSM_MAX_SPLITS)]
void GSCloning(triangle ShadowMapVSOutput input[3], inout TriangleStream<ShadowMapGSOutput> stream)
{
	for (uint split = 0; split < gNumSplits; split++)
	{
		ShadowMapGSOutput output;		
		output.RenderTargetArrayIndex = split;

		[unroll]
		for (uint vertex = 0; vertex < 3; vertex++)
		{
			output.PosLightCropped = mul(input[vertex].PosLightSpace, gLightCrop[split]);
			stream.Append(output);
		}

		stream.RestartStrip();
	}
}

[maxvertexcount(3)]
void GSInstancing(triangle ShadowMapVSOutput input[3], inout TriangleStream<ShadowMapGSOutput> stream)
{
	ShadowMapGSOutput output;
	uint split = input[0].InstanceID;

	output.RenderTargetArrayIndex = split;

	[unroll]
	for (uint vertex = 0; vertex < 3; vertex++)
	{
		output.PosLightCropped = mul(input[vertex].PosLightSpace, gLightCrop[split]);
		stream.Append(output);
	}

	stream.RestartStrip();
}
