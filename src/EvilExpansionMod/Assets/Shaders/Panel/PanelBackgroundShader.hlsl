sampler uImage0 : register(s0);

#define PIXEL_SIZE (2.0)

uniform float4 source;
uniform float4 colorLeft;

float4 main(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0 {
    float4 baseColor = tex2D(uImage0, coords) * color;

    if (baseColor.a <= 0.0) return baseColor;

    float fade = saturate(1.0 - coords.x);
    fade = pow(fade, 1.5);

    float3 finalRgb = lerp(baseColor.rgb, colorLeft.rgb, fade * colorLeft.a);

    return float4(finalRgb, baseColor.a);
}

technique Technique1 {
    pass PanelShader {
        PixelShader = compile ps_3_0 main();
    }
}