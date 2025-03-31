sampler2D HDRTexture : register(s0);

float BloomIntensity : register(c1) = 0.8;
float BloomThreshold : register(c2) = 0.8;
float BloomBlurSize : register(c3) = 4.0;
float BloomExposure : register(c4) = 1.2;

float4 BloomEffectPS(float2 TexCoord : TEXCOORD0) : COLOR
{
    float4 BaseColor = tex2D(HDRTexture, TexCoord);

    float Luminance = dot(BaseColor.rgb, float3(0.2126, 0.7152, 0.0722));
    float Brightness = max(0, Luminance - BloomThreshold);
    float SoftBrightness = max(0, Brightness);
    SoftBrightness = SoftBrightness / (max(Brightness, 1e-5));
    float3 BrightColor = BaseColor.rgb * SoftBrightness;

    float4 BloomColor = float4(BrightColor, 1.0) * 0.132980;
    float2 PixelSizeX = float2(BloomBlurSize / 1920.0, 0);
    float2 PixelSizeY = float2(0, BloomBlurSize / 1080.0);

    static const float Weights[12] =
    {
        0.115876, 0.094397, 0.071948, 0.051350, 0.034317, 0.021449,
        0.012555, 0.006883, 0.003534, 0.001699, 0.000764, 0.000382
    };

    static const float Offsets[12] =
    {
        1.2, 2.5, 3.9, 5.4, 7.0, 8.7, 10.5, 12.4, 14.4, 16.5, 18.7, 21.0
    };

    [unroll]
    for (int i = 0; i < 12; i++)
    {
        float2 OffsetX = PixelSizeX * Offsets[i];
        float2 OffsetY = PixelSizeY * Offsets[i];

        float4 SampleXPos = tex2D(HDRTexture, TexCoord + OffsetX);
        float4 SampleXNeg = tex2D(HDRTexture, TexCoord - OffsetX);

        float4 SampleYPos = tex2D(HDRTexture, TexCoord + OffsetY);
        float4 SampleYNeg = tex2D(HDRTexture, TexCoord - OffsetY);

        float SampleLumXPos = dot(SampleXPos.rgb, float3(0.2126, 0.7152, 0.0722));
        float SampleLumXNeg = dot(SampleXNeg.rgb, float3(0.2126, 0.7152, 0.0722));
        float SampleLumYPos = dot(SampleYPos.rgb, float3(0.2126, 0.7152, 0.0722));
        float SampleLumYNeg = dot(SampleYNeg.rgb, float3(0.2126, 0.7152, 0.0722));

        float SampleBrightnessXPos = max(0, SampleLumXPos - BloomThreshold);
        float SampleBrightnessXNeg = max(0, SampleLumXNeg - BloomThreshold);
        float SampleBrightnessYPos = max(0, SampleLumYPos - BloomThreshold);
        float SampleBrightnessYNeg = max(0, SampleLumYNeg - BloomThreshold);

        float SampleSoftBrightnessXPos = max(0, SampleBrightnessXPos) / (max(SampleBrightnessXPos, 1e-5));
        float SampleSoftBrightnessXNeg = max(0, SampleBrightnessXNeg) / (max(SampleBrightnessXNeg, 1e-5));
        float SampleSoftBrightnessYPos = max(0, SampleBrightnessYPos) / (max(SampleBrightnessYPos, 1e-5));
        float SampleSoftBrightnessYNeg = max(0, SampleBrightnessYNeg) / (max(SampleBrightnessYNeg, 1e-5));

        BloomColor.rgb += SampleXPos.rgb * SampleSoftBrightnessXPos * Weights[i];
        BloomColor.rgb += SampleXNeg.rgb * SampleSoftBrightnessXNeg * Weights[i];
        BloomColor.rgb += SampleYPos.rgb * SampleSoftBrightnessYPos * Weights[i];
        BloomColor.rgb += SampleYNeg.rgb * SampleSoftBrightnessYNeg * Weights[i];
    }

    BloomColor.rgb *= BloomIntensity * BloomExposure;
    float3 FinalColor = lerp(BaseColor.rgb, BaseColor.rgb + BloomColor.rgb, BloomIntensity);

    return float4(FinalColor, BaseColor.a);
}

technique BloomEffect
{
    pass P0
    {
        PixelShader = compile ps_3_0 BloomEffectPS();
    }
}
