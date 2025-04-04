float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

TextureCube EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
    Texture = <EnvironmentMap>;
};

struct VertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float3 WorldNormal : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 ViewDirection : TEXCOORD2;
};


// Function to convert 4x4 view matrix to 3x3 and back to 4x4
float4x4 RemoveTranslationFromMatrix(float4x4 viewMatrix)
{
    // Extract the upper-left 3x3 portion of the 4x4 matrix (rotation & scale)
    float3x3 rotationMatrix = (float3x3)viewMatrix;

    // Rebuild the 4x4 matrix
    float4x4 result;
    
    // Fill in the upper-left 3x3 with the rotation matrix
    result[0] = float4(rotationMatrix[0], 0.0f);
    result[1] = float4(rotationMatrix[1], 0.0f);
    result[2] = float4(rotationMatrix[2], 0.0f);
    
    // Set the homogeneous row and column
    result[3] = float4(0.0f, 0.0f, 0.0f, 1.0f);

    return result;
}


// Vertex Shader
VertexOutput VS(VertexInput input)
{
    VertexOutput output;
    float4x4 worldNoTranslation = RemoveTranslationFromMatrix(World);
    float4 worldPosition = mul(input.Position, worldNoTranslation);

    output.WorldPosition = worldPosition.xyz;
    output.Position = mul(worldPosition, mul(RemoveTranslationFromMatrix(View), Projection));
    output.WorldNormal = normalize(mul(input.Normal, (float3x3)worldNoTranslation));
    output.ViewDirection = normalize(-output.WorldPosition);

    return output;
}

float4 PS(VertexOutput input) : COLOR
{
    float3 reflection = texCUBElod(EnvironmentMapSampler, float4(-input.ViewDirection, 0)).rgb;
    
    // Return the color of the reflection
    return float4(reflection, 1.0);
}



technique EnvironmentMap
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
