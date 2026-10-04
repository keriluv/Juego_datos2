Shader "Custom/BarycentricOutlineURP_Full"
{
    Properties
    {
        _FillColor ("Fill Color", Color) = (0.5, 0.8, 0.1, 1)
        [HDR] _OutlineColor ("Outline Color", Color) = (2, 2, 2, 1)
        
        _OutlineThickness ("Outline Thickness", Range(0.001, 0.2)) = 0.02
        _OutlineOffset ("Outline Offset", Range(0.0, 0.3)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry" 
        }

        // =====================================================
        // PASE 1: COLOR Y OUTLINE
        // =====================================================
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" } 

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend One Zero 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            struct GeomData
            {
                float4 positionCS : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FillColor;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _OutlineOffset;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            [maxvertexcount(3)]
            void geom(triangle Varyings IN[3], inout TriangleStream<GeomData> triStream)
            {
                GeomData OUT;
                
                OUT.positionCS = IN[0].positionCS; OUT.barycentric = float3(1, 0, 0); triStream.Append(OUT);
                OUT.positionCS = IN[1].positionCS; OUT.barycentric = float3(0, 1, 0); triStream.Append(OUT);
                OUT.positionCS = IN[2].positionCS; OUT.barycentric = float3(0, 0, 1); triStream.Append(OUT);
            }

            half4 frag(GeomData IN) : SV_Target
            {
                float minBary = min(min(IN.barycentric.x, IN.barycentric.y), IN.barycentric.z);
                
                float lineStart = smoothstep(_OutlineOffset - 0.005, _OutlineOffset + 0.005, minBary);
                float lineEnd = smoothstep(_OutlineOffset + _OutlineThickness - 0.005, _OutlineOffset + _OutlineThickness + 0.005, minBary);
                
                // saturate evita cualquier número negativo en el cálculo
                float isOutline = saturate(lineStart - lineEnd);
                
                float3 finalColor = lerp(_FillColor.rgb, _OutlineColor.rgb, isOutline);

                // Forzamos el Alpha a 1.0 para evitar perforaciones en la cámara
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // =====================================================
        // PASE 2: DEPTH ONLY (Para la niebla básica)
        // =====================================================
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }

        // =====================================================
        // PASE 3: DEPTH NORMALS (Requerido por Volumetric Fog y SSAO)
        // =====================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes 
            { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL;
            };
            struct Varyings 
            { 
                float4 positionCS : SV_POSITION; 
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target 
            { 
                // Empaqueta las normales en el formato que URP exige
                return half4(normalize(IN.normalWS), 0.0);
            }
            ENDHLSL
        }

        // =====================================================
        // PASE 4: SHADOW CASTER (Proyectar sombras en la niebla)
        // =====================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}