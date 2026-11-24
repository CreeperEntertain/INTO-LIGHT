Shader "INTO LIGHT/Effects/Pause Background Blur"
{
    Properties
    {
        _Radius ("Blur Radius", Range(0.1, 16.0)) = 2.0
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

            float GaussianWeight(int offset)
            {
                if (offset == 0)
                    return 0.402619946;
                if (offset == 1 || offset == -1)
                    return 0.244201342;
                return 0.054488685;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 texelStep = _BlitTexture_TexelSize.xy * max(_Radius, 0.1);
                float4 result = 0.0;

                for (int y = -2; y <= 2; y++)
                {
                    float weightY = GaussianWeight(y);

                    for (int x = -2; x <= 2; x++)
                    {
                        float weight = GaussianWeight(x) * weightY;
                        float2 offset = float2((float)x, (float)y) * texelStep;

                        result += SAMPLE_TEXTURE2D_X(
                            _BlitTexture,
                            sampler_LinearClamp,
                            input.texcoord + offset
                        ) * weight;
                    }
                }

                return result;
            }

            ENDHLSL
        }
    }

    Fallback Off
}