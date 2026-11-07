Shader "Juego/AguaGerstner"
{
    Properties
    {
        _Profundo ("Profundo", Color) = (0.059, 0.227, 0.400, 0.94)
        _Superficie ("Superficie", Color) = (0.161, 0.714, 0.839, 0.88)
        _Espuma ("Espuma", Color) = (0.92, 0.96, 1.0, 1)
        _Contacto ("Contacto", Range(0.2, 8)) = 2.4
        _Cresta ("Cresta", Range(0, 2)) = 1.1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Agua"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Profundo;
                float4 _Superficie;
                float4 _Espuma;
                float _Contacto;
                float _Cresta;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float cresta : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float AlturaOla(float2 xz, float2 direccion, float amplitud, float frecuencia, float velocidad)
            {
                float2 d = normalize(direccion);
                float fase = frecuencia * dot(d, xz) + _Time.y * velocidad * frecuencia;
                return amplitud * sin(fase);
            }

            float3 Desplazar(float3 posicion)
            {
                float2 xz = posicion.xz;
                float3 suma = posicion;
                // Ola 1
                {
                    float2 d = normalize(float2(1.0, 0.2));
                    float fase = 0.16 * dot(d, xz) + _Time.y * 1.05 * 0.16;
                    suma.xz += 0.40 * 0.18 * d * cos(fase);
                    suma.y += 0.18 * sin(fase);
                }
                // Ola 2
                {
                    float2 d = normalize(float2(-0.35, 1.0));
                    float fase = 0.28 * dot(d, xz) + _Time.y * 1.35 * 0.28;
                    suma.xz += 0.30 * 0.12 * d * cos(fase);
                    suma.y += 0.12 * sin(fase);
                }
                // Ola 3
                {
                    float2 d = normalize(float2(0.15, -1.0));
                    float fase = 0.45 * dot(d, xz) + _Time.y * 1.70 * 0.45;
                    suma.xz += 0.22 * 0.07 * d * cos(fase);
                    suma.y += 0.07 * sin(fase);
                }
                return suma;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 mundo = TransformObjectToWorld(input.positionOS.xyz);
                float yInicial = mundo.y;
                mundo = Desplazar(mundo);
                output.cresta = mundo.y - yInicial;
                output.positionWS = mundo;
                output.positionCS = TransformWorldToHClip(mundo);
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float cresta = saturate(input.cresta * _Cresta + 0.45);
                float3 color = lerp(_Profundo.rgb, _Superficie.rgb, cresta);
                float espumaCresta = smoothstep(0.08, 0.22, input.cresta);

                float2 uv = input.screenPos.xy / input.screenPos.w;
                float escena = SampleSceneDepth(uv);
                float ojoEscena = LinearEyeDepth(escena, _ZBufferParams);
                float ojoAgua = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float contacto = 1.0 - saturate((ojoEscena - ojoAgua) * _Contacto);
                float espuma = saturate(espumaCresta + contacto * contacto);
                color = lerp(color, _Espuma.rgb, espuma * 0.85);
                float alpha = lerp(_Profundo.a, 1.0, espuma);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
