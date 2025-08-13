sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float uLength;
float4 uColor1;
float4 uColor2;
float uTime;

float2 rotate(float r, float2 uv)
{
    float rotCos = cos(r);
    float rotSin = sin(r);
    return float2(uv.x * rotCos - uv.y * rotSin, uv.x * rotSin + uv.y * rotCos);
}

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float distMask = uv.y - 0.5 + uv.x * sin(-uTime + uv.x * 4) * 0.7;
    
    float2 sampleUV = float2(uv.x * uLength / 1000.0, uv.y * 0.25);
    float s1 = tex2D(uImage0, sampleUV + float2(uTime, 0)).r;
    float s2 = tex2D(uImage1, sampleUV + float2(uTime * 0.333, 0)).r;

    float s3 = tex2D(uImage0, uv * float2(1, 0.1) - uTime * 0.2).r;
    float s4 = tex2D(uImage1, uv * float2(1, 0.1) - uTime * 0.1).r;
    
    return lerp(uColor1, uColor2, step(s3 * s4, 0.2)) * step(distMask - 0.2, s1 * s2);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};