Shader "Hidden/FXV/FXVPostprocessBlitAdd" 
{
	Properties 
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorMultiplier ("ColorMultiplier", Range(0,5)) = 1.0
	}
	SubShader 
    {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 300

        Blend One One
		ZWrite Off

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            sampler2D _MainTex;

            float _ColorMultiplier;

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * pow(_ColorMultiplier, 1.0/2.2);
            }
            ENDCG
        }
	}
	FallBack "Diffuse"
}
