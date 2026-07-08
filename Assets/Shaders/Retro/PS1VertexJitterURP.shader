Shader "Retro/PS1 Vertex Jitter URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)

        _VertexSnap("Vertex Snap", Range(20, 512)) = 160
        _WobbleAmount("Wobble Amount", Range(0, 0.05)) = 0.003
        _WobbleSpeed("Wobble Speed", Range(0, 10)) = 1.2
        _FogAmount("Fog Amount", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _VertexSnap;
                float _WobbleAmount;
                float _WobbleSpeed;
                float _FogAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionOS = input.positionOS.xyz;

                float timeValue = _Time.y * _WobbleSpeed;

                float wobbleSeed =
                    dot(positionOS, float3(12.9898, 78.233, 37.719));

                float wobble =
                    sin(wobbleSeed + timeValue) * _WobbleAmount;

                positionOS += float3(
                    wobble,
                    wobble * 0.5,
                    -wobble * 0.35
                );

                float4 positionCS = TransformObjectToHClip(positionOS);

                float safeW = max(positionCS.w, 0.0001);
                float2 ndcPosition = positionCS.xy / safeW;

                ndcPosition = round(ndcPosition * _VertexSnap) / _VertexSnap;

                positionCS.xy = ndcPosition * positionCS.w;

                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 textureColor = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv
                );

                half4 finalColor = textureColor * _BaseColor;

                half3 foggedColor = MixFog(finalColor.rgb, input.fogCoord);
                finalColor.rgb = lerp(finalColor.rgb, foggedColor, _FogAmount);

                return finalColor;
            }

            ENDHLSL
        }
    }

    FallBack Off
}