#include "../Common.h"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uTime;
float uStepY;
float uScale = 1;

float4 uColor1;
float4 uColor2;
float uStepColor;

float4 PS(TrailPSInput output) : COLOR0
{
    float s1 = tex2D(uImage0, output.uv * uScale - float2(uTime, 0)).r;
    float s2 = tex2D(uImage1, output.uv * uScale - float2(uTime * 0.75, 0)).r;
    
    float s = (s1 * 0.5 + s2 * 0.5) * (1 - output.uv.x) * sin(output.uv.y * PI);

    float alpha = step(uStepY, s);
    float color = step(uStepY, s - uStepColor);

    return lerp(uColor1, uColor2, color) * alpha;
}

technique
{
    pass
    {
        PixelShader = compile ps_3_0 PS();
    }
}