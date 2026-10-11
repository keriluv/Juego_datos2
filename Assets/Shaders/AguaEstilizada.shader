Shader "Juego/AguaEstilizada"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _Color ("Color", Color) = (0.08, 0.42, 0.62, 1)
        _ColorProfundo ("Color profundo", Color) = (0.02, 0.16, 0.32, 1)
        _Scroll ("Velocidad UV", Vector) = (0.04, 0.02, 0, 0)
        _WaveHeight ("Altura de ola", Float) = 0.45
        _WaveScale ("Escala de ola", Float) = 0.12
        _WaveSpeed ("Velocidad de ola", Float) = 0.9
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
            Name "Agua"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ColorProfundo;
                float4 _Scroll;
                float _WaveHeight;
                float _WaveScale;
                float _WaveSpeed;
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
                float altura : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 pos = input.positionOS.xyz;
                float tiempo = _Time.y * _WaveSpeed;
                float ola = sin(pos.x * _WaveScale + tiempo) + cos(pos.z * _WaveScale * 0.85 + tiempo * 0.8);
                ola *= 0.5;
                pos.y += ola * _WaveHeight;
                output.positionCS = TransformObjectToHClip(pos);
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw + _Scroll.xy * _Time.y;
                output.altura = ola;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 agua = lerp(_ColorProfundo, _Color, saturate(input.altura + 0.5));
                return tex * agua;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
