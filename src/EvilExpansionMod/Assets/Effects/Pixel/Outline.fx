sampler uImage0 : register(s0);

float2 size;
float4 color;

inline bool opaque(float2 coords)
{
  return tex2D(uImage0, coords).a > 0.01;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 {

    float2 fragCoord = coords * size;

    const float2 right = float2(1, 0);
    const float2 down = float2(0, 1);
    
    if (!opaque(coords) && (
        opaque((fragCoord + right) / size.xy) ||
        opaque((fragCoord + down) / size.xy) ||
        opaque((fragCoord - right) / size.xy) ||
        opaque((fragCoord - down) / size.xy)
    )) return color;
    return tex2D(uImage0, coords);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};