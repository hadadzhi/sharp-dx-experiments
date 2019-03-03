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
	return saturate(x * (a * x + b) / (x * (c * x + d) + e));
}

float3 ReinhardLuminance(float3 x)
{
	// Luminance is calculated as a weighted sum of RGB values: human eye has different sensitivity for different wavelengths
	float3 lVec = float3(0.2126, 0.7152, 0.0722);

	// Calculate the input luminance
	float lIn = dot(x, lVec);

	// [TODO]
	// lScaled = keyValue * (lIn / lAverage)
	// lAverage = (1/N) * exp(sumOverAllPixels(log(lIn(pixel)))), N -- number of pixels
	// lAverage can be computed by iteratively downsampling the input texture by 2 until we get a 1-pixel image, which contains the average
	// keyValue is a free parameter that control the overall brightness of the result, [TODO] determine keyValue automatically
	float keyValue = 0.18;
	float lScaled = keyValue * lIn;

	// Minimum luminance that will be mapped to lOut == 1.
	// [TODO] Need a fast algorithm to compute the maximum luminance in input, or its approximation
	float lMax = dot((float3)(0.6) * Exposure, lVec);

	// Reinhard's tone mapping operator
	//float lOut = lScaled * (1 + (lScaled / (lMax * lMax))) / (lScaled + 1);
	float lOut = lScaled / (lScaled + 1);

	// See notes (in the notepad (on the desk))
	return (x / lIn) * lOut;
}

float3 Reinhard(float3 x)
{
	return x / (x + 1);
}

float3 Exponential(float3 x)
{
	return 1.0f - exp(-x);
}

float3 None(float3 x)
{
	return x;
}

void ToneMappingPS(in float4 svPosition : SV_POSITION, in float2 texcoord : TEXCOORD, out float4 output : SV_TARGET)
{
	float3 hdr = InputTex.Sample(PointSampler, texcoord).rgb;
	output = float4(ReinhardLuminance(hdr * Exposure), 1.0f);
}
