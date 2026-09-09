#include "../Common.h"

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uTime;
float uStepY;
float uScale = 1;

float4 PixelShaderFunction(TrailPSInput output) : COLOR0
{
    float s1 = tex2D(uImage0, output.uv * uScale - float2(uTime, 0)).r;
    float s2 = tex2D(uImage0, output.uv * uScale + float2(uTime, 0)).r;

    float s2Alpha = lerp(1, s2, output.uv.x);
    float alpha = step(
        uStepY, 
        s1 * (1 - output.uv.x) * sin(output.uv.y * PI)
    ) * s2Alpha;
    return output.color * alpha;
}

technique
{
    pass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}