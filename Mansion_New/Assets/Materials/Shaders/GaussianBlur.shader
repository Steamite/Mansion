Shader "Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Blur Offset", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Offset;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(_Offset / _ScreenParams.x, _Offset / _ScreenParams.y);
                
                float4 color = tex2D(_MainTex, i.uv) * 0.227;
                color += tex2D(_MainTex, i.uv + float2(offset.x, 0)) * 0.316;
                color += tex2D(_MainTex, i.uv - float2(offset.x, 0)) * 0.316;
                color += tex2D(_MainTex, i.uv + float2(0, offset.y)) * 0.316;
                color += tex2D(_MainTex, i.uv - float2(0, offset.y)) * 0.316;
                
                return color;
            }
            ENDCG
        }
    }
}