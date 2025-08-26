sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);

float4x4 uTransformMatrix;
float uTime;
float4 uColor;

struct VSInput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    output.color = input.color;
    output.position = mul(input.position, uTransformMatrix);
    output.uv = input.uv;
    
    return output;
}

const float PI = 3.14159265;

float4 PixelShaderFunction(VSOutput output) : COLOR0
{
    float x = (output.uv.y * 2 - 1) 
        * 1.5 + sin(output.uv.x * 3.5 + uTime) * 0.5 * output.uv.x;
    float z = -(output.uv.x - 1);

    float s = 1 - x * x - z * z;
    if (s < 0) return float4(0, 0, 0, 0); 

    float y = sqrt(s);
    
    float lonH = atan2(y, x);
    float latH = acos(z);
    float2 uvH = float2(lonH / (2 * PI), latH / PI);

    float s0 = tex2D(uImage0, 0.25 * (uvH + float2(uTime + (1 - output.uv.x), 0))).r;
    float s1 = tex2D(uImage1, output.uv + float2(-uTime, 0)).r;
    float s2 = tex2D(uImage2, output.uv * 0.5 + float2(0, uTime)).r;
    return lerp(output.color, uColor, step(s2, 0.1)) * step(s0, 0.6) * step(s2, sin(output.uv.y * PI) - output.uv.x);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}