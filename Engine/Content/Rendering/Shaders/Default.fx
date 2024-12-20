float4x4 World;
float4x4 View;
float4x4 Projection;
float Alpha = 1;
int Lighting = 1;
int VertexColors = 0;

Texture2D DiffuseTexture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <DiffuseTexture>;
};

Texture2D EmissionTexture;
sampler2D EmissionTextureSampler = sampler_state
{
    Texture = <EmissionTexture>;
};

float4 AmbientColor;
float4 DiffuseColor;
float4 EmissionColor;
float3 dirLightDirection[16];
float dirLightIntensity[16];
float3 dirLightColor[16];


float3 pointLightPositions[16];
float pointLightIntensities[16];
float3 pointLightColors[16];

struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD2;
    float4 WorldPosition : TEXCOORD1;
    float4 LocalPosition : TEXCOORD4;
    float4 ViewPosition : TEXCOORD3;
};

VertexOutput VS(VertexInput input)
{
    VertexOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.WorldPosition = worldPosition;

    output.Position = mul(worldPosition, mul(View, Projection));
    output.LocalPosition = output.Position;

    float3 worldNormal = mul(input.Normal, (float3x3) World);
    worldNormal = normalize(worldNormal);
    output.WorldNormal = worldNormal;

    float4 viewPosition = mul(worldPosition, View);
    output.ViewPosition = viewPosition;

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    return output;
}



float4 PS(VertexOutput input) : COLOR
{
    float4 textureColor = tex2D(DiffuseTextureSampler, input.TexCoord);
    float4 emissiontexColor = tex2D(EmissionTextureSampler, input.TexCoord);

    float3 normalizedNormal = normalize(input.WorldNormal);
    float3 LightColor = AmbientColor.rgb;

    if (Lighting == 1)
    {
        for (int i = 0; i < 16; i++)
        {
            // Directional lights
            float dirLightDot = max(dot(normalizedNormal, normalize(dirLightDirection[i])), 0.0);
            float3 dirLightContribution = dirLightColor[i] * dirLightIntensity[i] * dirLightDot;
            LightColor += dirLightContribution;

            // Point lights
            float3 lightDir = normalize(pointLightPositions[i] - input.WorldPosition.xyz);
            float distance = length(pointLightPositions[i] - input.WorldPosition.xyz);
            float attenuation = 1.0 / (1.0 + 0.1 * distance + 0.01 * distance * distance);
            attenuation = min(attenuation, 1.0);
            float pointLightDot = max(dot(normalizedNormal, lightDir), 0.0);
            float3 pointLightContribution = pointLightColors[i] * pointLightIntensities[i] * pointLightDot * attenuation;
            LightColor += pointLightContribution;
        }
    }
    else
    {
        LightColor = float3(1, 1, 1);
    }

    LightColor = clamp(LightColor, 0.0, 1.0);
    
    float3 VertColor = float3(1,1,1);
    if (VertexColors == 1)
    {
        VertColor = input.Color.rgb;
    }
    
    float3 finalColor = (textureColor.rgb * DiffuseColor.rgb * LightColor * VertColor) + (emissiontexColor.rgb * EmissionColor.rgb);

    float alpha = clamp(Alpha * textureColor.a * DiffuseColor.a, 0, 1);
    
    if (alpha < 0.1)
    {
        discard;
    }
    
    return float4(finalColor.rgb, alpha);
}


technique BasicShader
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}