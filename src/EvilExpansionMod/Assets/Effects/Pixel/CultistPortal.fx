sampler uImage0 : register(s0);

texture tex1;
sampler2D sampler1 = sampler_state {
    texture = <tex1>;
};

float time;
float4 color;

float2 rotate(float r, float2 uv)
{
    float rotCos = cos(r);
    float rotSin = sin(r);
    return float2(uv.x * rotCos - uv.y * rotSin, uv.x * rotSin + uv.y * rotCos);}

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float rotCos = cos(time);
    float rotSin = sin(time);

    float2 uv1 = rotate(time, uv - 0.5) + 0.5;
    float2 uv2 = rotate(-time, uv - 0.5) + 0.5;

    float s1 = tex2D(uImage0, uv1).r;
    float s2 = tex2D(sampler1, uv2).r;

    float dist = length(uv - 0.5);
    return color * s1 * s2 * step(dist, 0.5);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};