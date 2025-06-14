Shader "Custom/BoxBlur_Horizontal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float _BlurSize;
            CBUFFER_END

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(vertex.xyz);
                o.uv = uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 texelSize = float2(_BlurSize / _ScreenParams.x, 0);
                
                // Sample 5 pixels horizontally
                float4 color = tex2D(_MainTex, i.uv) * 0.4;
                color += tex2D(_MainTex, i.uv - texelSize) * 0.2;
                color += tex2D(_MainTex, i.uv + texelSize) * 0.2;
                color += tex2D(_MainTex, i.uv - texelSize * 2) * 0.1;
                color += tex2D(_MainTex, i.uv + texelSize * 2) * 0.1;

                return color;
            }
            ENDHLSL
        }
    }
}