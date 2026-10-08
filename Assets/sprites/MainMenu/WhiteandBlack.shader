Shader "UI/WhiteandBlack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Offset ("UV Offset", Vector) = (0,0,0,0)
        _NoiseAmount ("Noise Amount", Range(0,1)) = 0.1
        _ScanlineSpeed ("Scanline Speed", Range(0,10)) = 3
        _Jitter ("Jitter", Range(0,0.1)) = 0.02
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float _ScanlineSpeed;
            float _NoiseAmount;
            float _Jitter;

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // IMPORTANT for UI
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color; // UI tint
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            { float2 uv = i.uv;

            uv.y += _Time.y * 0.05;

            float scan = sin((uv.y + _Time.y * _ScanlineSpeed) * 200.0) * 0.002;

            float jitter = (sin(_Time.y * 50.0 + uv.y * 100.0) * 0.5 + 0.5) * _Jitter;

            uv.x += scan + jitter;

            float noise = frac(sin(dot(uv * _Time.y, float2(12.9898,78.233))) * 43758.5453);

            fixed4 col = tex2D(_MainTex, uv) * i.color;

            float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
            float3 bw = float3(gray, gray, gray);

            bw += (noise - 0.5) * _NoiseAmount;

            bw *= 0.95 + sin(_Time.y * 10.0) * 0.05;

            return fixed4(bw, col.a);
            }
            
            ENDCG
        }
    }
}