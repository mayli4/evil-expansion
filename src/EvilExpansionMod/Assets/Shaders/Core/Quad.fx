#include "../Common.h"

sampler uTexture : register(s0);

matrix uMatrix;
float4 uColor;

struct VSInput
{
    float3 position : POSITION;
    float2 uv : TEXCOORD;
};

QuadPSInput VS(VSInput input)
{
    QuadPSInput output;
    output.position = mul(float4(input.position, 1), uMatrix);
    output.uv = input.uv;

    return output;
}

float4 PS(QuadPSInput input) : COLOR0
{
    return tex2D(uTexture, input.uv) * uColor;
}

technique
{
    pass
    {
        VertexShader = compile VS_VERSION VS();
        PixelShader = compile PS_VERSION PS();
    }
}