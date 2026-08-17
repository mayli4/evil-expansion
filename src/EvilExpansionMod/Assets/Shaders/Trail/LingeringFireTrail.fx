matrix mat;
float time;
float stepY;
float scale = 1;

float uvScaleX;
float uvScaleY;

float4 baseColor;

float flipped;

texture texture1;
sampler2D sampler1 = sampler_state
{
    texture = <texture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture texture2;
sampler2D sampler2 = sampler_state
{
    texture = <texture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct VSInput
{
    float4 position : POSITION;
    float2 coords : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 coords : TEXCOORD;
    float4 color : COLOR;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    output.color = input.color;
    output.position = mul(input.position, mat);
    output.coords = input.coords;
    
    return output;
}

const float PI = 3.14;

float4 PixelShaderFunction(VSOutput output) : COLOR0{
    float2 uv = output.coords;

    if (flipped == 1) {
        uv.y = 1.0 - uv.y;
    }

    float2 sampledUV = float2(uv.x * uvScaleX, uv.y * uvScaleY);

    float s1 = tex2D(sampler1, sampledUV - float2(0, time)).r; 
    float s2 = tex2D(sampler2, sampledUV + float2(0, time)).r;

    float s2Alpha = lerp(1, s2, uv.x); 
    
    float alphaCutoff = step(
        stepY, 
        s1 * (1.0 - uv.y) * sin(uv.x * PI)
    );
    
    float alpha = alphaCutoff * s2Alpha;
    
    float baseBlendFactor = 1.0 - uv.y;
    baseBlendFactor = pow(baseBlendFactor, 6.0);
    baseBlendFactor = saturate(baseBlendFactor * 3.0);

    float4 finalColor = lerp(output.color, baseColor, baseBlendFactor);

    return finalColor * alpha;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}