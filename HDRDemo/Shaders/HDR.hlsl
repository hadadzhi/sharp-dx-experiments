// Simple HDR tone mapping shader

cbuffer ExposureConstBuffer : register(b0)
{
	float Exposure;
}

Texture2D InputTex : register(t0);
SamplerState PointSampler { Filter = MIN_MAG_MIP_POINT; AddressU = Clamp; AddressV = Clamp; };

void ToneMappingVS(in float4 position : POSITION, in float2 texcoord : TEXCOORD, out float4 svPosition : SV_POSITION, out float2 outTexcoord : TEXCOORD)
{
	svPosition = position;
	outTexcoord = texcoord;
}

// Fit of ACES Filmic Tone Mapping Curve, by Krzysztof Narkowicz
// This version does not include gamma correction -- we do not need it as we are rendering into an sRGB target
float3 ACESFilm(float3 x)
{
	float a = 2.51f;
	float b = 0.03f;
	float c = 2.43f;
	float d = 0.59f;
	float e = 0.14f;
	return saturate( x * (a * x + b) / ( x * (c * x + d) + e ) );
}

float3 ReinhardLuminance(float3 x)
{
	// Luminance calculation weights
	float3 lWeights = float3(0.2126, 0.7152, 0.0722);

	// Extract the input luminance
	float lIn = dot(x, lWeights);
	
	// [TODO]
	// lScaled = keyValue * (lIn / lAverage)
	// Attention! The Reinhard paper most probably contains an error: (1/N) is in the wrong place, assuming that by log-average they meant geometric mean
	// The following is geometric mean written in terms of logarithms
	// lAverage = exp((1/N) * sumOverAllPixels(log(luminance_of_pixel))), N -- number of pixels
	// lAverage can be computed by iteratively downsampling the input texture by 2 until we get a 1-pixel image, which contains the average
	// keyValue is a free parameter that control the overall brightness of the result, [TODO] determine keyValue automatically
	float keyValue = 1.0;
	float lScaled = keyValue * lIn;

	// Minimum luminance that will be mapped to lOut == 1.
	// [TODO] Need a fast algorithm to compute the maximum luminance in input, or its approximation
	//float lMax = dot((float3)(0.6) * Exposure, lWeights);

	// Reinhard's tone mapping operator
	//float lOut = lScaled * (1 + (lScaled / (lMax * lMax))) / (lScaled + 1);
	float lOut = lScaled / (1 + lScaled);

	return x * (lOut / lIn);
}

float3 Reinhard(float3 x)
{
	return x / (x + 1);
}

float3 None(float3 x)
{
	return x;
}

void ToneMappingPS(in float4 svPosition : SV_POSITION, in float2 texcoord : TEXCOORD, out float4 output : SV_TARGET)
{
	float3 hdr = InputTex.Sample(PointSampler, texcoord).rgb;
	output = float4(Reinhard(hdr * Exposure), 1.0f);
}
