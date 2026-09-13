sampler uImage0 : register(s0);

float time;
float2 size;

float4 flameColor;
float4 coreColor;
float4 outerCoreColor;

float noiseScale;
float flameSize;

float flameStretchY;

float4 PS(float2 uv : TEXCOORD0) : COLOR0 {
    uv = float2(uv.y, uv.x);
	// uv.y = 1.0 - uv.y;

    float2 centeredUv = uv - 0.5;
    centeredUv.x *= size.x / size.y;
    
    float verticalStretchFactor = 1.5;
    centeredUv.y /= verticalStretchFactor;
    float2 flameLocalBaseAnchor = float2(0, -0.4); 
    
    
    float2 localUv = centeredUv - float2(0, 0.2);
    float2 scaledUv = (localUv - flameLocalBaseAnchor) / max(flameSize, 0.001f) + flameLocalBaseAnchor;
    
    float2 noiseUv = uv * noiseScale; 
    noiseUv.y -= time * 0.5;
    float noiseAmount = tex2D(uImage0, noiseUv).r;
    
    float uvintensity = pow(saturate(1.0 - scaledUv.y * 8.0), 2.2);
    noiseAmount *= uvintensity; 
    float yGradient = saturate(centeredUv.y + 0.25) * 0.5;
  
    float2 fireNoise = float2(noiseAmount * 0.1, noiseAmount * 2.5 * yGradient) + float2(0, 0.2);
    
    float ball = length(scaledUv + fireNoise) - 0.1f;
	ball = step(0.8, 1 - ball);
	
    float innerCoreMask = length(scaledUv + fireNoise + float2(0, 0.14)) - 0.1f;
    innerCoreMask = step(0.95, 1.0 - innerCoreMask);
    
	float outerCoreMask = length(scaledUv + fireNoise + float2(0, 0.05)) - 0.1f;
    outerCoreMask = step(0.95, 1.04 - outerCoreMask);
    
    float4 finalColor = float4(0, 0, 0, 0);
    
    finalColor = lerp(finalColor, flameColor, ball);
    finalColor = lerp(finalColor, outerCoreColor, outerCoreMask);
	finalColor = lerp(finalColor, coreColor, innerCoreMask);
    
    return finalColor;
}

technique {
    pass {
        PixelShader = compile ps_3_0 PS();
    }
};