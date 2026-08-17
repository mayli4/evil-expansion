sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float size = 1;
float progress;
float time;
float4 startColor;
float4 endColor;

float2 rotate(float r, float2 uv)
{
    float rotCos = cos(r);
    float rotSin = sin(r);
    return float2(uv.x * rotCos - uv.y * rotSin, uv.x * rotSin + uv.y * rotCos);
}

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float uvMult = 2 / size;
    float dist = length(uv * 2 / size - uvMult / 2);
    float distMask = abs(dist - progress);
    
    float s1 = tex2D(uImage0, rotate(time, uv - 0.5)).r;
    float s2 = tex2D(uImage1, rotate(-time, uv - 0.5)).r;
    
    return lerp(startColor, endColor, progress) * step(distMask, s1 * s2 * (1 - progress)) * step(dist, 1.0);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_3_0 frag();
    }
};