sampler uImage0 : register(s0);

texture noisetex;
sampler2D fireNoise = sampler_state {
    texture = <noisetex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture noisetex2;
sampler2D fireNoise2 = sampler_state {
    texture = <noisetex2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float level;
float3 liquidColor;
float rot;

float uNoiseStrength;
float uDarkenStrength;

float uNoise1ScrollSpeedX;
float uNoise2Scale;
float2 uNoise2ScrollVector;

float uTime;

float plot(float2 st, float pct){
  return smoothstep(pct, pct/0.95f, st.y);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 {
    float2 uv = coords;
    
    uv.y = 1.0 - uv.y;

    if (level <= 0.070) {
        return float4(0.0f, 0.0f, 0.0f, 0.0f); 
    }
    
    float4 material = tex2D(uImage0, coords);
    if (material.a < 0.005) {
        return float4(0.0, 0.0, 0.0, 0.0); 
    }
    
    float2 scrolledUv1 = uv;
    scrolledUv1.x += uTime * uNoise1ScrollSpeedX;
    
    float noiseValue1 = tex2D(fireNoise, scrolledUv1).r;
    float displacement = (noiseValue1 - 0.5) * uNoiseStrength;
    float distortedUvX = uv.x + displacement;
    
    float wave = (0.07 * (level * 30 + sin(distortedUvX + uTime * 2.0)) * 0.5) + -0.030;
    
    float3 finalRGBColor = liquidColor;

    float pct = plot(uv, wave);
    finalRGBColor = (1.0-pct) * finalRGBColor; 
    float2 baseScaledUv2 = uv * uNoise2Scale; 
    
    float2 scrolledUv2 = baseScaledUv2 + uTime * uNoise2ScrollVector;
    float noiseValue2 = tex2D(fireNoise2, scrolledUv2).r;
    
    float2 scrolledUv3 = baseScaledUv2 + uTime * (-uNoise2ScrollVector * 0.3);
    float noiseValue3 = tex2D(fireNoise2, scrolledUv3).r;
    
    float darken = 1.0 - (1.0 - noiseValue2 * noiseValue3) * uDarkenStrength;
    darken = saturate(darken);
    
    finalRGBColor *= darken;

    float finalAlpha = 1.0 - pct;
    
    return float4(finalRGBColor, finalAlpha);
}

technique Technique1 {
    pass VignettePass {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};