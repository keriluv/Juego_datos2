Shader "Juego/ToonNaval"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _Calido ("Luz cálida", Color) = (1, 0.62, 0.27, 1)
        _Frio ("Sombra", Color) = (0.22, 0.30, 0.52, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Toon"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _Calido;
                float4 _Frio;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 normal = normalize(input.normalWS);
                float3 sol = normalize(float3(0.42, 0.62, 0.28));
                float luz = smoothstep(-0.05, 0.48, dot(normal, sol));
                float3 rampa = lerp(_Frio.rgb, _Calido.rgb, luz);
                return half4(_BaseColor.rgb * rampa, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
