sampler uImage0 : register(s0);

texture tex1;
sampler2D sampler1 = sampler_state {
    texture = <tex1>;
};

float stepY;
float time;
float4 color;

float2 rotate(float r, float2 uv)
{
    float rotCos = cos(r);
    float rotSin = sin(r);
    return float2(uv.x * rotCos - uv.y * rotSin, uv.x * rotSin + uv.y * rotCos);
}

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float dist = length(uv - 0.5);

    float distRot = dist * 3.0;
    float2 uv1 = rotate(-time + pow(distRot * 4.0, 4.0), uv - 0.5) + 0.5;

    float alpha = step(tex2D(uImage0, uv1).r, stepY);
    alpha = step(smoothstep(tex2D(sampler1, uv + time * 0.01).r, 0.0, 0.5 - dist), alpha);

    return color * alpha * step(dist, 0.5);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};