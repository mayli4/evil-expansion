#include "../Common.h"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uTime;
float uSize;
float4 uColor;

float4 PS(float2 uv : TEXCOORD0) : COLOR0 {
    float2 offsetUv = uv - 0.5;

    float len = length(offsetUv);
    float direction = offsetUv / len;

    float s0 = tex2D(uImage0, uv + direction * uTime * 0.01);

    return uColor * smoothstep(
        0,
        0.8, 
        sin(64000 * len / uSize - uTime)) * 2 * (0.5 - len) * clamp(0, 1, len * 20 - 0.025) * s0;
}

technique {
    pass {
        PixelShader = compile PS_VERSION PS();
    }
};