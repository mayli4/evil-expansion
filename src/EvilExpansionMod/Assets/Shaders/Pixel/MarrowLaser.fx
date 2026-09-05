#include "../Common.h" 

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uStepThreshold;
float uStepColor1;
float uStepColor2;

float uTime;
float uLength;
float uProgress;

float4 uColor1;
float4 uColor2;
float4 uColor3;

float4 PS(QuadPSInput input) : COLOR0 {
    float sMove = tex2D(uImage1, input.uv + float2(uTime * 0.05, 0)).r;
    float2 uv = input.uv + float2(sMove * 0.1, 0);

    float s = tex2D(uImage0, float2(uv.x * uLength / 100 - uTime, uv.y)).r;

    float sinY = sin(uv.y * PI);
    float alpha = step(uStepThreshold, s);

    return lerp(
        uColor1, 
        lerp(uColor2, uColor3, step(uStepThreshold, s - uStepColor1)), 
        smoothstep(uStepThreshold, uStepThreshold + 0.25, s - uStepColor2)) * alpha;
}

technique {
    pass {
        PixelShader = compile ps_3_0 PS();
    }
};