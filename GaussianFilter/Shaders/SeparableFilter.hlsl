//
// Configurable macros. Define actual values at compile time! This is just to avoid compiler errors.
//
#ifndef ARRAY_LENGTH
#define ARRAY_LENGTH 1
#endif

#ifndef WEIGHTS
#define WEIGHTS { 1.0, }
#endif

#ifndef OFFSETS_H
#define OFFSETS_H { { 0, 0 }, }
#endif

#ifndef OFFSETS_V
#define OFFSETS_V { { 0, 0 }, }
#endif

#ifndef TARGET_COMPONENTS
#define TARGET_COMPONENTS 4
#endif

#ifndef APPLY_TO_ALPHA
#define APPLY_TO_ALPHA 0
#endif

//
// Non-configurable macros.
//
#if TARGET_COMPONENTS == 1
#define IO_TYPE float
#elif TARGET_COMPONENTS == 2
#define IO_TYPE float2
#elif TARGET_COMPONENTS == 4 && APPLY_TO_ALPHA == 0
#define IO_TYPE float3
#elif TARGET_COMPONENTS == 4 && APPLY_TO_ALPHA == 1
#define IO_TYPE float4
#endif

static const float Weights[] = WEIGHTS;
static const float2 OffsetsH[] = OFFSETS_H;
static const float2 OffsetsV[] = OFFSETS_V;

Texture2D<IO_TYPE> InputTex : register(t0);
SamplerState PointSampler { Filter = MIN_MAG_MIP_POINT; AddressU = Clamp; AddressV = Clamp; };

void SeparableFilterVS(in float4 position : POSITION, out float4 svPosition : SV_POSITION, inout float2 texcoord : TEXCOORD0)
{
	svPosition = position;
}

void HorizontalPassPS(in float4 position : SV_POSITION, in float2 texcoord : TEXCOORD0, out IO_TYPE output : SV_TARGET0)
{
	IO_TYPE result = (IO_TYPE) (0);

	[unroll]
	for (int i = 0; i < ARRAY_LENGTH; i++)
	{
		result += Weights[i] * InputTex.Sample(PointSampler, texcoord + OffsetsH[i]);
	}

	output = result;
}

void VerticalPassPS(in float4 position : SV_POSITION, in float2 texcoord : TEXCOORD0, out IO_TYPE output : SV_TARGET0)
{
	IO_TYPE result = (IO_TYPE) (0);

	[unroll]
	for (int i = 0; i < ARRAY_LENGTH; i++)
	{
		result += Weights[i] * InputTex.Sample(PointSampler, texcoord + OffsetsV[i]);
	}

	output = result;
}
