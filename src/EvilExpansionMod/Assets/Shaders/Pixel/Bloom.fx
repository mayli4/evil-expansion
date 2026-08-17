#include "../Common.h"

sampler uImage0 : register(s0);

float2 uSize;
float uThreshold;
float uIntensity;
int uSampleRadius = 4;

#define MAX_RADIUS 8

float4 PS(QuadPSInput input) : COLOR0
{
    float3 original = tex2D(uImage0, input.uv).rgb;

    float3 bloom = float3(0, 0, 0);
    float alpha = 0;
    float totalWeight = 0;

    int maxRadius = min(uSampleRadius, MAX_RADIUS);

    [unroll(MAX_RADIUS)]
    for (int x = -maxRadius; x <= maxRadius; x++)
    {
        [unroll(MAX_RADIUS)]
        for (int y = -maxRadius; y <= maxRadius; y++)
        {
            float2 offset = float2(x, y) / uSize;
            float4 s = tex2D(uImage0, input.uv + offset);

            float luma = dot(s.rgb, float3(0.2126, 0.7152, 0.0722));
            float contribution = max(luma - uThreshold, 0);

            // Gaussian
            float dist = length(float2(x, y));
            float weight = exp(-dist * dist / (2 * uSampleRadius));

            bloom += s.rgb * contribution * weight;
            alpha += s.a * weight;
            totalWeight += weight;
        }
    }

    return float4(
        original.rgb + bloom / max(totalWeight, 1e-5) * uIntensity, 
        alpha / max(totalWeight, 1e-5));
}

technique {
    pass {
        PixelShader = compile ps_3_0 PS();
    }
};