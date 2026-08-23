#include "../Common.h"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);

float uStepThreshold;
float uStepColor;

float uTime;
float uLength;
float uProgress;

float4 uColor1;
float4 uColor2;
float4 uColor3;

float4 PS(QuadPSInput input) : COLOR0 {
    float2 uv = input.uv;

    float s0 = tex2D(uImage0, float2(uv.x * uLength / 1000 - uTime, uv.y));
    float s1 = tex2D(uImage1, float2(uv.x * uLength / 1000 + uTime * 0.85, uv.y));

    float s = s0 * 0.5 + s1 * 0.5;

    float sinY = sin(uv.y * PI);
    float sinX = sin(uv.x * PI);

    float stepValue = 1 - sinY
        + uStepThreshold
        + sin(uv.x * 3 + uTime * 0.4) * 0.06
        + sin(uv.x * 7.3 + 0.3789457 + uTime * 0.65) * 0.08;

    return lerp(uColor3, lerp(uColor1, uColor2, step(s - uStepColor, stepValue)), step(s - uStepColor - 0.1, stepValue)) * step(stepValue, s);
}

technique {
    pass {
        PixelShader = compile ps_3_0 PS();
    }
};