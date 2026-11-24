Shader "INTO LIGHT/Effects/Pause Background Blur"
{
    Properties
    {
        _Radius ("Blur Radius", Range(0.0, 16.0)) = 2.0
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

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float sigma = max(_Radius, 0.1);
                float invTwoSigmaSquared = 0.5 / (sigma * sigma);
                float2 texel = _BlitTexture_TexelSize.xy;

                float4 colorSum = 0.0;
                float weightSum = 0.0;

                int radius = min((int)ceil(3.0 * sigma), 16);

                for (int y = -16; y <= 16; y++)
                {
                    if (abs(y) > radius)
                        continue;

                    for (int x = -16; x <= 16; x++)
                    {
                        if (abs(x) > radius)
                            continue;

                        float2 d = float2((float)x, (float)y);
                        float weight = exp(-dot(d, d) * invTwoSigmaSquared);

                        float2 uv = input.texcoord + d * texel;

                        float4 pixel = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

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