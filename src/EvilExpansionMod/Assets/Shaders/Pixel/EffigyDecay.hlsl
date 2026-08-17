sampler uImage0 : register(s0);

texture noiseTexture;
sampler noise : register(s1);

float2 noiseStretch;
float2 noiseOffset;
float fadeProgress;

float2 frameUVStart;
float2 frameUVSize;
float2 framePixelSize;
float dissolvePixelSize;
float edgeSoftness;
float ditherStrength;
float4 sampleColor;

static const float bayer2x2[2][2] = {
    { 0.20f, 0.60f },
    { 0.80f, 0.40f }
};

float4 main(float4 col : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
    float4 base = tex2D(uImage0, coords);
    if (base.a < 0.005) return 0;

    float2 frameUV = (coords - frameUVStart) / frameUVSize;

    float2 framePixels = frameUV * framePixelSize;
    float2 pixelizedPixels = floor(framePixels / dissolvePixelSize) * dissolvePixelSize;
    float2 pixelizedFrameUV = pixelizedPixels / framePixelSize;

    float3 finalRgb = lerp(base.rgb, float3(0.0, 0.0, 0.0), saturate(fadeProgress / 0.3));

    float dissolveThreshold = saturate((fadeProgress - 0.2) / 0.6);

    float fadeCoords = (1.0 - smoothstep(0.3, 0.7, pixelizedFrameUV.y)) * (1.0 - dissolveThreshold);
    float2 noiseCoords = pixelizedFrameUV * noiseStretch + noiseOffset;
    
    float distanceThreshold = tex2D(noise, noiseCoords).r - (dissolveThreshold * 1.3f) + 0.3f + fadeCoords * 0.4f;
    float edgeAlpha = saturate(distanceThreshold / 0.3f);

    float2 ditherGrid = floor(pixelizedPixels / dissolvePixelSize);
    int2 bayerCoord = int2(fmod(abs(ditherGrid), 2.0));
    
    float bayerThreshold = bayer2x2[bayerCoord.x][bayerCoord.y];

    float ditherMask = step(bayerThreshold, edgeAlpha);
    float ditheredEdge = ditherMask * edgeAlpha;

    float ditherFact = saturate(dissolveThreshold * 4.0f); 
    float noiseEdge = lerp(edgeAlpha, ditheredEdge, ditherFact);
    float finalAlpha = base.a * noiseEdge * 1.0 - dissolveThreshold;

    return float4(finalRgb, finalAlpha) * sampleColor;
}

technique Technique1 {
    pass DecayPass {
        PixelShader = compile ps_3_0 main();
    }
}