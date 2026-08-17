#include "../Common.h"

sampler2D uImage0 : register(s0);

matrix uMatrix;
int uSpriteRotation = 0;

const float2 RotationTable[8] = {
    float2(1, 0),
    float2(0, 1),

    float2(0, -1),
    float2(1, 0),

    float2(-1, 0),
    float2(0, -1),

    float2(0, 1),
    float2(-1, 0)
};

TrailPSInput VS(TrailVSInput input)
{
    TrailPSInput output;
    output.position = mul(float4(input.position, 1), uMatrix);
    output.color = input.color;
    output.uv = input.uv;

    return output;
}

float4 PS(TrailPSInput input) : COLOR0
{
    int index = uSpriteRotation % 4;
    float2 rotX = RotationTable[index * 2];
    float2 rotY = RotationTable[index * 2 + 1];

    float2 uv = float2(
        rotX.x * input.uv.x + rotX.y * input.uv.y, 
        rotY.x * input.uv.x + rotY.y * input.uv.y);
    
    return input.color * tex2D(uImage0, uv);
}

technique
{
    pass
    {
        VertexShader = compile VS_VERSION VS();
        PixelShader = compile PS_VERSION PS();
    }
}