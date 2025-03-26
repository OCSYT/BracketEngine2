// HDR Color Texture (the result from scene rendering)
sampler2D HDRTexture : register(s0);

// Bloom parameters
float BloomIntensity : register(c1) = 1.0; // Overall bloom strength
float BloomThreshold : register(c2) = 1.0; // Brightness threshold for bloom
float BloomBlurSize : register(c3) = 1.0; // Size of blur kernel
float BloomExposure : register(c4) = 1.0; // Exposure adjustment

// Single-pass ultra-high-quality bloom effect
float4 BloomEffectPS(float2 TexCoord : TEXCOORD0) : COLOR
{
    // Sample base color
    float4 baseColor = tex2D(HDRTexture, TexCoord);
    
    // Extract bright areas
    float luminance = dot(baseColor.rgb, float3(0.2126, 0.7152, 0.0722));
    float3 brightColor = baseColor.rgb * max(0, luminance - BloomThreshold) / max(luminance, 0.001);
    
    // Ultra-high-quality Gaussian blur with more samples
    float4 bloomColor = float4(brightColor, 1.0) * 0.159576; // Center weight
    float2 pixelSizeX = float2(BloomBlurSize / 1920.0, 0);
    float2 pixelSizeY = float2(0, BloomBlurSize / 1080.0);
    
    // 25-tap Gaussian kernel (12 either side + center)
    const int SAMPLE_COUNT = 12;
    float weights[SAMPLE_COUNT] =
    {
        0.132980, // Closest to center
        0.115876,
        0.094397,
        0.071948,
        0.051350,
        0.034317,
        0.021449,
        0.012555,
        0.006883,
        0.003534,
        0.001699,
        0.000764 // Furthest
    };
    
    float offsets[SAMPLE_COUNT] =
    {
        1.2, // Tighter spacing near center
        2.5,
        3.9,
        5.4,
        7.0,
        8.7,
        10.5,
        12.4,
        14.4,
        16.5,
        18.7,
        21.0 // Wider spacing further out
    };
    
    // Perform ultra-high-quality blur
    [unroll]
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        float2 offsetX = pixelSizeX * offsets[i];
        float2 offsetY = pixelSizeY * offsets[i];
        
        // Horizontal samples
        float4 sampleXPos = tex2D(HDRTexture, TexCoord + offsetX);
        float4 sampleXNeg = tex2D(HDRTexture, TexCoord - offsetX);
        
        // Vertical samples
        float4 sampleYPos = tex2D(HDRTexture, TexCoord + offsetY);
        float4 sampleYNeg = tex2D(HDRTexture, TexCoord - offsetY);
        
        // Extract bright components with proper weighting
        float lumXPos = dot(sampleXPos.rgb, float3(0.2126, 0.7152, 0.0722));
        float lumXNeg = dot(sampleXNeg.rgb, float3(0.2126, 0.7152, 0.0722));
        float lumYPos = dot(sampleYPos.rgb, float3(0.2126, 0.7152, 0.0722));
        float lumYNeg = dot(sampleYNeg.rgb, float3(0.2126, 0.7152, 0.0722));
        
        bloomColor.rgb += max(0, sampleXPos.rgb * (lumXPos - BloomThreshold) / max(lumXPos, 0.001)) * weights[i];
        bloomColor.rgb += max(0, sampleXNeg.rgb * (lumXNeg - BloomThreshold) / max(lumXNeg, 0.001)) * weights[i];
        bloomColor.rgb += max(0, sampleYPos.rgb * (lumYPos - BloomThreshold) / max(lumYPos, 0.001)) * weights[i];
        bloomColor.rgb += max(0, sampleYNeg.rgb * (lumYNeg - BloomThreshold) / max(lumYNeg, 0.001)) * weights[i];
    }
    
    // Apply exposure and intensity
    bloomColor.rgb *= BloomExposure * BloomIntensity;
    
    // Combine with base color
    float3 finalColor = baseColor.rgb + bloomColor.rgb;

    
    return float4(finalColor, baseColor.a);
}

// Single technique
technique BloomEffect
{
    pass P0
    {
        PixelShader = compile ps_3_0 BloomEffectPS();
    }
}