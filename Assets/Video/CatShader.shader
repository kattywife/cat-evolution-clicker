Shader "Custom/RemoveBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorToMask ("Color to Remove", Color) = (1,1,1,1) // По умолчанию белый
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _Softness ("Softness", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha 
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorToMask;
            float _Threshold;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Вычисляем разницу между цветом пикселя и цветом, который надо удалить
                float diff = distance(col.rgb, _ColorToMask.rgb);
                
                // Вычисляем прозрачность
                float alpha = smoothstep(_Threshold, _Threshold + _Softness, diff);
                
                col.a = alpha;
                return col;
            }
            ENDCG
        }
    }
}