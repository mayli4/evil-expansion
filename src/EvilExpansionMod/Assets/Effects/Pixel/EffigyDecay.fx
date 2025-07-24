sampler uImage0 : register(s0);

texture noisetex;
sampler2D fireNoise = sampler_state {
    texture = <noisetex>;
};

float prog;

float3 edgeColor;
float3 ashColor;

float noiseTexelAspect;
float frameTexelAspect;

float2 texSize;

float4 sampleColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 {
    float4 material = tex2D(uImage0, coords);

    if (material.a < 0.005) {
        return float4(0.0, 0.0, 0.0, 0.0);
    }
    
    float2 noiseSampleCoords = coords;

    float effectiveAspect = frameTexelAspect / noiseTexelAspect;

    if (effectiveAspect < 1.0) {
        noiseSampleCoords.x /= effectiveAspect;
        noiseSampleCoords.x -= (1.0 / effectiveAspect - 1.0) * 0.5;
    } else if (effectiveAspect > 1.0) {

        noiseSampleCoords.y *= effectiveAspect;
        noiseSampleCoords.y -= (effectiveAspect - 1.0) * 0.5;
    }
    
	float2 pixelSize = 1.5 / texSize;
    noiseSampleCoords = round(noiseSampleCoords / pixelSize) * pixelSize;
    
    float noiseValue = tex2D(fireNoise, noiseSampleCoords).r;
    
    float erosionAmount = smoothstep(prog - 0.2, prog, noiseValue);
    float borderStrength = smoothstep(0.0, 0.5, erosionAmount) - smoothstep(0.3, 1.0, erosionAmount);
    
    float3 final_rgb = material.rgb * (1.0 - erosionAmount);
    
    float3 fire = lerp(edgeColor, ashColor, smoothstep(0.7, 1., borderStrength)) * 3.0;

    final_rgb += borderStrength * fire;
    
    float final_alpha = material.a * (1.0 - erosionAmount);
    final_alpha = max(final_alpha, borderStrength);
    
    return float4(final_rgb, final_alpha) * sampleColor;
}

technique Technique1 {
    pass VignettePass {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};