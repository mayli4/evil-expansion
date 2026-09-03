#include "../Common.h"
#include "../syntax.h"

sampler2D uImage0 : register(s0);

float2 uTexelSize;
float uThreshold;
float uIntensity;

static const float Offsets[5] = { -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923 };
static const float Weights[5] = { 0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027 };

float4 main(QuadPSInput input) : COLOR0 {
    float4 original = tex2D(uImage0, input.uv);

    float3 bloom = float3(0.0, 0.0, 0.0);
    float alpha = 0.0;
    float totalWeight = 0.0;

    [unroll]
    for (int x = 0; x < 5; x++){
        float2 xOffset = float2(Offsets[x] * uTexelSize.x, 0.0);
        float xWeight = Weights[x];

        [unroll]
        for (int y = 0; y < 5; y++){
            float2 offset = xOffset + float2(0.0, Offsets[y] * uTexelSize.y);
            float weight = xWeight * Weights[y];

            float4 s = tex2D(uImage0, input.uv + offset);
            float luma = dot(s.rgb, float3(0.2126, 0.7152, 0.0722));
            float contribution = max(luma - uThreshold, 0.0);

            bloom += s.rgb * contribution * weight;
            alpha += s.a * weight;
            totalWeight += weight;
        }
    }

    float invWeight = 1.0 / max(totalWeight, 0.00001);
    return float4(original.rgb + (bloom * invWeight) * uIntensity, alpha * invWeight);
}

BEGIN_TECHNIQUE(Technique1)
    BEGIN_PASS(BloomPass)
        PIXEL_SHADER(compile ps_3_0 main())
    END_PASS
END_TECHNIQUE