texture uImage0Texture;
sampler uImage0 = sampler_state
{
    texture = <uImage0Texture>;
    Filter = MIN_MAG_MIP_POINT;
    AddressU = wrap;
    AddressV = wrap;
};

float4x4 uTransformMatrix;
float2 uImage0Size;

struct VSInput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

struct PSInput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

PSInput VertexShaderFunction(VSInput input)
{
    PSInput output;
    output.color = input.color;
    output.position = mul(input.position, uTransformMatrix);
    output.uv = input.position.xy / uImage0Size;
    
    return output;
}

float4 PixelShaderFunction(PSInput input) : COLOR0
{
    return tex2D(uImage0, input.uv);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
