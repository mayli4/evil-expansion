sampler uImage0 : register(s0);

texture tex1;
sampler2D sampler1 = sampler_state {
    texture = <tex1>;
};

float size = 1;
float time;
float4 color1;
float4 color2;

float2 rotate(float r, float2 uv)
{
    float rotCos = cos(r);
    float rotSin = sin(r);
    return float2(uv.x * rotCos - uv.y * rotSin, uv.x * rotSin + uv.y * rotCos);
}

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float uvMult = 2 / size;
    float dist = length(uv * 2 / size - uvMult / 2);
    float distMask = abs(dist - 0.5) + 0.1;
    
    float s1 = tex2D(uImage0, rotate(time, uv - 0.5)).r;
    float s2 = tex2D(sampler1, rotate(-time, uv - 0.5)).r;
    
    return lerp(color1, color2, step(s1, 0.25)) * step(distMask * 2.3 - 0.2, s1 * s2);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};