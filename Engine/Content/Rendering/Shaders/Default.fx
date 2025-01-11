float4x4 World;
float4x4 View;
float4x4 Projection;
float3 ViewPosition;

float Alpha = 1;
float VertexColors = 0;
float Lighting = 0;

float4 BaseColor;
Texture2D BaseColorTexture;
sampler2D BaseColorSampler = sampler_state
{
    Texture = <BaseColorTexture>;
};

float4 EmissionColor;
Texture2D EmissionColorTexture;
sampler2D EmissionColorSampler = sampler_state
{
    Texture = <EmissionColorTexture>;
};

float MetallicIntensity = 0;
Texture2D MetallicTexture;
sampler2D MetallicSampler = sampler_state
{
    Texture = <MetallicTexture>;
};

float RoughnessIntensity = 0.5;
Texture2D RoughnessTexture;
sampler2D RoughnessSampler = sampler_state
{
    Texture = <RoughnessTexture>;
};

Texture2D NormalTexture;
sampler2D NormalSampler = sampler_state
{
    Texture = <NormalTexture>;
};

Texture2D AmbientOcclusionTexture;
sampler2D AOSampler = sampler_state
{
    Texture = <AmbientOcclusionTexture>;
};

TextureCube EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
    Texture = <EnvironmentMap>;
};

float3 CameraPosition;
float4 AmbientColor;

float3 DirLightDirection[16];
float3 DirLightColor[16];
float DirLightIntensity[16];

float3 PointLightPositions[16];
float3 PointLightColors[16];
float PointLightIntensities[16];

struct VertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
    float3 ViewDirection : TEXCOORD3;
    float4 Color : COLOR0;
    float3 Tangent : TEXCOORD4;
    float3 Bitangent : TEXCOORD5;
};
VertexOutput VS(VertexInput input)
{
    VertexOutput output;
    float4 worldPosition = mul(input.Position, World);
    output.WorldPosition = worldPosition.xyz;
    output.Position = mul(worldPosition, mul(View, Projection));
    float3 worldNormal = mul(input.Normal, (float3x3) World);
    output.WorldNormal = normalize(worldNormal);
    float3 viewDirection = normalize(CameraPosition - output.WorldPosition);
    output.ViewDirection = viewDirection;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    float3 p1 = float3(input.Position.x, input.Position.y, input.Position.z);
    float3 p2 = float3(input.Position.x + 1, input.Position.y, input.Position.z);
    float3 p3 = float3(input.Position.x, input.Position.y + 1, input.Position.z);

    float2 uv1 = input.TexCoord;
    float2 uv2 = input.TexCoord + float2(0.01, 0);
    float2 uv3 = input.TexCoord + float2(0, 0.01);

    float2 deltaUV1 = uv2 - uv1;
    float2 deltaUV2 = uv3 - uv1;

    float3 deltaPos1 = p2 - p1;
    float3 deltaPos2 = p3 - p1;

    float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
    float3 tangent = normalize(r * (deltaUV2.y * deltaPos1 - deltaUV1.y * deltaPos2));
    float3 bitangent = normalize(r * (-deltaUV2.x * deltaPos1 + deltaUV1.x * deltaPos2));

    output.Tangent = tangent;
    output.Bitangent = bitangent;

    return output;
}

float3 FresnelSchlick(float3 F0, float3 normal, float roughness)
{
    float3 halfVec = normalize(normal);
    float cosTheta = clamp(dot(normal, halfVec), 0.0, 1.0);
    float factor = pow(1.0 - cosTheta, 5.0);
    return F0 + (1.0 - F0) * factor;
}

float3 CalculatePBRLighting(VertexOutput input, float3 normal, float3 albedo, float metallic, float roughness, float ao)
{
    albedo = albedo * ao;
    roughness = max(0.1, roughness);

    float3 F0 = lerp(0, albedo, metallic);

    float3 finalColor = AmbientColor.rgb * albedo;

    float3 V = normalize(-input.ViewDirection);
    float3 R = reflect(-V, normal);
    float3 reflection = texCUBE(EnvironmentMapSampler, R).rgb;

    float3 kSpecular = F0 * reflection;
    float3 kDiffuse = (1.0 - F0) * albedo;

    for (int i = 0; i < 16; i++)
    {
        float3 lightDir = normalize(DirLightDirection[i]);
        float3 halfVec = normalize(lightDir + normal);

        float NDF = pow(max(dot(normal, halfVec), 0.0), roughness * 128.0);
        float NdotL = max(dot(normal, lightDir), 0.0);

        float3 F = FresnelSchlick(F0, normal, roughness);

        float3 kSpecularLight = (NDF * F) / (max(dot(normal, lightDir), 0.0) + 0.001);
        float3 kDiffuseLight = albedo * (1.0 - F);

        finalColor += (kDiffuseLight + kSpecularLight) * DirLightColor[i] * DirLightIntensity[i] * NdotL
                    + (kSpecular * DirLightColor[i] * DirLightIntensity[i] * NdotL);

        float3 lightPos = PointLightPositions[i];
        float3 lightDirPoint = normalize(lightPos - input.WorldPosition);
        float distance = length(lightPos - input.WorldPosition);
        float attenuation = 1.0 / (1.0 + 0.1 * distance + 0.01 * distance * distance);

        float3 halfVecPoint = normalize(lightDirPoint + normal);
        float NDFPoint = pow(max(dot(normal, halfVecPoint), 0.0), roughness * 128.0);
        float NdotLPoint = max(dot(normal, lightDirPoint), 0.0);

        float3 FPoint = FresnelSchlick(F0, normal, roughness);

        float3 kSpecularPoint = (NDFPoint * FPoint) / (max(dot(normal, lightDirPoint), 0.0) + 0.001);
        float3 kDiffusePoint = albedo * (1.0 - FPoint);

        finalColor += (kDiffusePoint + kSpecularPoint) * PointLightColors[i] * PointLightIntensities[i] * attenuation * NdotLPoint
                    + (kSpecular * PointLightColors[i] * PointLightIntensities[i] * attenuation * NdotLPoint);
    }

    return finalColor;
}

float4 PS(VertexOutput input) : COLOR
{
    float4 albedo = tex2D(BaseColorSampler, input.TexCoord) * BaseColor;
    if (VertexColors == 1)
    {
        albedo.rgb *= input.Color.rgb;
    }

    float3 emission = tex2D(EmissionColorSampler, input.TexCoord).rgb * EmissionColor.rgb;
    float metallic = tex2D(MetallicSampler, input.TexCoord).r * MetallicIntensity;
    float roughness = tex2D(RoughnessSampler, input.TexCoord).r * RoughnessIntensity;
    float ao = tex2D(AOSampler, input.TexCoord).r;

    float3 tangentNormal = tex2D(NormalSampler, input.TexCoord).rgb * 2.0 - 1.0;

    float3 tangent = normalize(input.Tangent);
    float3 bitangent = normalize(cross(input.WorldNormal, tangent));
    float3 surfaceNormal = normalize(input.WorldNormal);

    float3x3 TBN = float3x3(tangent, bitangent, surfaceNormal);

    float3 worldNormal = normalize(mul(tangentNormal, TBN));
    float3 finalColor = float3(1, 1, 1);
    if (Lighting == 1)
    {
        finalColor = CalculatePBRLighting(input, worldNormal, albedo.rgb, metallic, roughness, ao) + emission;
    }
    else
    {
        finalColor = (albedo.rgb * ao) + emission;
    }

    if (Alpha * BaseColor.a * albedo.a < 0.1)
    {
        discard;
    }
    
    return float4(finalColor, Alpha * BaseColor.a * albedo.a);
}

technique PBRShader
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
