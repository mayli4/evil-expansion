#include "../Common.h"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);

float uTime;
float uProgress;
float uDirection;

float4 uColorA;
float4 uColorB;

float4 FS(TrailPSInput input) : COLOR0 {
    float2 uv = float2(input.uv.x, uDirection < 0 ? input.uv.y : 1 - input.uv.y);

    float s1 = tex2D(uImage1, float2(uv.x + uTime, uTime - uv.x + uv.y)).r;
    uv.x += s1 * 0.01 * uv.y;

    float s0 = tex2D(uImage0, float2(4 * uv.x - uTime * 0.023, uTime * 0.98 - uv.y)).r;
    float s2 = tex2D(uImage2, float2(4 * uv.x + uTime * 0.056, uTime * 0.7 - uv.y)).r;

    float s = s0 * 0.5 + s2 * 0.5;

    float xStep = 0.35 * uv.y + 0.4 * (1 - sin(PI * uv.x)) + 0.75 * uProgress * uProgress;
    return lerp(uColorA, lerp(uColorA, uColorB, step(s - 0.125, xStep)), sin(uv.y * PI)) * step(xStep, s);
}

technique Technique1 {
    pass FragPass {
        PixelShader = compile ps_3_0 FS();
    }
};