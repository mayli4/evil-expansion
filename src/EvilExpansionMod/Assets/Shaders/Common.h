#define VS_VERSION vs_2_0
#define PS_VERSION ps_2_0

#define PI 3.14
#define WHITE float4(1, 1, 1, 1)

struct QuadPSInput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
};

struct TrailPSInput 
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};
