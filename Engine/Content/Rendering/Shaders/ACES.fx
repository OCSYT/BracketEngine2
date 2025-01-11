    sampler2D TextureSampler : register(s0);
    float Exposure : register(c0); // Exposure parameter
    float Gamma : register(c1); // Gamma correction parameter

    float3 ACESFilm(float3 x)
    {
        float a = 2.51f;
        float b = 0.03f;
        float c = 2.43f;
        float d = 0.59f;
        float e = 0.14f;
        return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
    }

    float3 GammaCorrect(float3 color, float gamma)
    {
        return pow(color, 1.0 / gamma);
    }


    float4 Main(float2 texCoord : TEXCOORD0) : COLOR
    {
        float4 OriginalColor = tex2D(TextureSampler, texCoord);
        float3 linearColor = OriginalColor.rgb;
        float3 tonemappedColor = ACESFilm(linearColor) * Exposure;
        tonemappedColor = GammaCorrect(tonemappedColor, Gamma);
        return float4(tonemappedColor, 1.0);
    }

    technique ToneMappingTech
    {
        pass Pass1
        {
            PixelShader = compile ps_3_0 Main();
        }
    }
