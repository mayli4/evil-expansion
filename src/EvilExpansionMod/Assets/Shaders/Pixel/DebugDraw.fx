#include "../Common.h" 

sampler uImage0 : register(s0);

float4 PS(QuadPSInput input) : COLOR0 {
    float4 s = tex2D(uImage0, input.uv);
    return lerp(s, float4(0.5, 0, 0, 0.5), step(s, 0.01));
}

technique {
    pass {
        PixelShader = compile ps_3_0 PS();
    }
};