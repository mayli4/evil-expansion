#include "../tmlbuild.h"

texture sampleTexture;
sampler samplerTex : register(s0);

texture noiseMap;
sampler noiseMapSampler : register(s1);

float2 textureResolution;
float4 displaceFactors;
float time GLOBAL_TIME;
float2 worldPos;
float noiseStrength;

float4 baseColor;
float4 outlineColor;

static const float bayer4x4[4][4] = {
    {  0.0f / 16.0f,  8.0f / 16.0f,  2.0f / 16.0f, 10.0f / 16.0f },
    { 12.0f / 16.0f,  4.0f / 16.0f, 14.0f / 16.0f,  6.0f / 16.0f },
    {  3.0f / 16.0f, 11.0f / 16.0f,  1.0f / 16.0f,  9.0f / 16.0f },
    { 15.0f / 16.0f,  7.0f / 16.0f, 13.0f / 16.0f,  5.0f / 16.0f }
};

float dither(float2 coords, float opacity, float steps){
    float2 framePixels = coords * textureResolution;
    float2 ditherGrid = floor(framePixels + worldPos);
    int2 bayerCoord = int2(fmod(abs(ditherGrid), 4.0));
    float threshold = bayer4x4[bayerCoord.x][bayerCoord.y];
    
    opacity += threshold / steps;
    return floor(opacity * steps) / steps;
}

float4 main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0 {
    coords = floor(coords * textureResolution) / textureResolution;
    float2 distortedcoords = coords + (tex2D(noiseMapSampler, worldPos / textureResolution + coords * 1.3 + float2(0, -time * 0.2)).xy - float2(0.5, 0.5)) * noiseStrength;
    
    float4 base = tex2D(samplerTex, distortedcoords).rgba;
    base.x = pow(base.x, 2);
    
    base.x = base.x * 0.1 + dither(coords, base.x, 4) * 0.8;

    float outlineMask = (1 - base.g) * base.x;
    
    float4 retColor = baseColor;
    retColor.xyzw *= base.x;
    
    retColor += outlineMask * outlineColor;
    return retColor * base.a * sampleColor;
}

technique Technique1 {
    pass SmokePass {
        PixelShader = compile ps_3_0 main();
    }
}