Shader "Juego/CieloDorado"
{
    Properties
    {
        _Horizonte ("Horizonte", Color) = (1.0, 0.55, 0.22, 1)
        _Medio ("Medio", Color) = (0.98, 0.62, 0.38, 1)
        _Cenit ("Cenit", Color) = (0.28, 0.22, 0.48, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Background"
            "Queue" = "Background"
        }
        Cull Front
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Horizonte;
                float4 _Medio;
                float4 _Cenit;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float altura = saturate(input.uv.y);
                float3 color = lerp(_Horizonte.rgb, _Medio.rgb, smoothstep(0.0, 0.35, altura));
                color = lerp(color, _Cenit.rgb, smoothstep(0.42, 1.0, altura));
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
