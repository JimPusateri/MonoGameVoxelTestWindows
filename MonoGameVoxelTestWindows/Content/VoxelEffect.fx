#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

// Matrices
float4x4 World;
float4x4 View;
float4x4 Projection;

// Curvature parameters
float CurvatureStrength = 0.05;  // How much to curve the world
float3 CameraPosition;            // Camera position for distance calculation

// Texture
texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    float4 worldPosition = mul(input.Position, World);
    
    // Apply world curvature - bend down based on distance from camera
    float3 offset = worldPosition.xyz - CameraPosition;
    float distXZ = sqrt(offset.x * offset.x + offset.z * offset.z);
    
    // Quadratic falloff for smooth curvature
    float curvature = distXZ * distXZ * CurvatureStrength;
    worldPosition.y -= curvature;
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tex2D(textureSampler, input.TexCoord);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
