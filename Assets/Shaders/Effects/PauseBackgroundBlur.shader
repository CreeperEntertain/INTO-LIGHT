Shader "INTO LIGHT/Effects/Pause Background Blur"
{
    Properties
    {
        _Radius ("Blur Radius", Range(0.0, 16.0)) = 2.0
        _MinBrightness ("Minimum Brightness", Range(0.0, 1.0)) = 0
        _MaxBrightness ("Maximum Brightness", Range(0.0, 1.0)) = 1
        _Blend ("Blend", Range(0.0, 1.0)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend Off

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureXR.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Radius;
            float _MinBrightness;
            float _MaxBrightness;
            float _Blend;

            float3 RemapBrightness(float3 color, float minBrightness, float maxBrightness)
            {
                return lerp(minBrightness.xxx, maxBrightness.xxx, color);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float blend = saturate(_Blend);

                float effectiveRadius = lerp(0.0, _Radius, blend);
                float effectiveMinBrightness = lerp(0.0, _MinBrightness, blend);
                float effectiveMaxBrightness = lerp(1.0, _MaxBrightness, blend);

                float4 sourcePixel = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                sourcePixel.rgb = RemapBrightness(sourcePixel.rgb, effectiveMinBrightness, effectiveMaxBrightness);

                if (effectiveRadius <= 1.0e-6f)
                    return sourcePixel;

                float sigma = max(effectiveRadius, 0.1);
                float invTwoSigmaSquared = 0.5 / (sigma * sigma);
                float2 texel = _BlitTexture_TexelSize.xy;

                float4 colorSum = 0.0;
                float weightSum = 0.0;

                int radius = min((int)ceil(3.0 * sigma), 16);

                for (int y = -radius; y <= radius; y += 2)
                {
                    int y1 = min(y + 1, radius);

                    float wy0 = exp(-(float)(y * y) * invTwoSigmaSquared);
                    float wy1 = (y1 == y) ? 0.0 :
                        exp(-(float)(y1 * y1) * invTwoSigmaSquared);

                    float weightY = wy0 + wy1;
                    float offsetY = (y * wy0 + y1 * wy1) / weightY;

                    for (int x = -radius; x <= radius; x += 2)
                    {
                        int x1 = min(x + 1, radius);

                        float wx0 = exp(-(float)(x * x) * invTwoSigmaSquared);
                        float wx1 = (x1 == x) ? 0.0 :
                            exp(-(float)(x1 * x1) * invTwoSigmaSquared);

                        float weightX = wx0 + wx1;
                        float offsetX = (x * wx0 + x1 * wx1) / weightX;

                        float weight = weightX * weightY;
                        float2 uv = input.texcoord +
                            float2(offsetX, offsetY) * texel;

                        float4 pixel = SAMPLE_TEXTURE2D_X(
                            _BlitTexture, sampler_LinearClamp, uv);

                        pixel.rgb = RemapBrightness(
                            pixel.rgb,
                            effectiveMinBrightness,
                            effectiveMaxBrightness);

                        colorSum += pixel * weight;
                        weightSum += weight;
                    }
                }

                return colorSum / max(weightSum, 1e-6);
            }

            ENDHLSL
        }
    }

    Fallback Off
}