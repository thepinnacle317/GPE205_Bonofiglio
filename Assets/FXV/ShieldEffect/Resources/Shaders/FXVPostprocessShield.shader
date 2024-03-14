Shader "Hidden/FXV/FXVPostprocessShield" 
{
	Properties 
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 300
	
		Blend Off
		ZWrite Off

        // 0: Gaussian
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

            uniform float4 _MainTex_TexelSize;

            sampler2D _MainTex;

            uniform float GAUSSIAN_COEFF[16];
            uniform int GAUSSIAN_KERNEL_RADIUS;
            uniform int GAUSSIAN_TEXEL_SIZE;

            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 finalColor = fixed4(0.0, 0.0, 0.0, 0.0);

               float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS; idx <= GAUSSIAN_KERNEL_RADIUS; idx++)
                {
                    offsetUV = i.uv + float2(idx, 0.0) * texelSize;
                    finalColor += tex2D(_MainTex, offsetUV) * GAUSSIAN_COEFF[idx + GAUSSIAN_KERNEL_RADIUS]; 
                }

                for (idx = -GAUSSIAN_KERNEL_RADIUS; idx <= GAUSSIAN_KERNEL_RADIUS; idx++)
                {
                    offsetUV = i.uv + float2(0.0, idx) * texelSize;
                    finalColor += tex2D(_MainTex, offsetUV) * GAUSSIAN_COEFF[idx + GAUSSIAN_KERNEL_RADIUS];
                }

                return finalColor;
            }
            ENDCG
        }

        
        // 1: Box
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

            uniform float4 _MainTex_TexelSize;

            sampler2D _MainTex;

            half3 Sample (float2 uv) 
            {
			    return tex2D(_MainTex, uv).rgb;
		    }

            half3 SampleBox (float2 uv) 
            {
			    float4 o = _MainTex_TexelSize.xxxx * float2(-1, 1).xxyy;
			    half3 s =
				    Sample(uv + o.xy) + Sample(uv + o.zy) +
				    Sample(uv + o.xw) + Sample(uv + o.zw);
			    return s * 0.25f;
		    }

            fixed4 frag (v2f i) : SV_Target
            {
            	half4 finalColor = half4(SampleBox(i.uv), 1);

                return finalColor;
            }
            ENDCG
        }

        // 2: Gaussian Horizontal
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

            uniform float4 _MainTex_TexelSize;

            sampler2D _MainTex;

            uniform float GAUSSIAN_COEFF[16];
            uniform int GAUSSIAN_KERNEL_RADIUS;
            uniform int GAUSSIAN_TEXEL_SIZE;

            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 finalColor = fixed4(0.0, 0.0, 0.0, 0.0);

                float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS; idx <= GAUSSIAN_KERNEL_RADIUS; idx++)
                {
                    offsetUV = i.uv + float2(idx, 0.0) * texelSize;
                    finalColor += tex2D(_MainTex, offsetUV) * GAUSSIAN_COEFF[idx + GAUSSIAN_KERNEL_RADIUS]; 
                }

                return finalColor;
            }
            ENDCG
        }

        // 3: Gaussian Vertical
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

            uniform float4 _MainTex_TexelSize;

            sampler2D _MainTex;

            uniform float GAUSSIAN_COEFF[16];
            uniform int GAUSSIAN_KERNEL_RADIUS;
            uniform int GAUSSIAN_TEXEL_SIZE;

            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 finalColor = fixed4(0.0, 0.0, 0.0, 0.0);

                float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS; idx <= GAUSSIAN_KERNEL_RADIUS; idx++)
                {
                    offsetUV = i.uv + float2(0.0, idx) * texelSize;
                    finalColor += tex2D(_MainTex, offsetUV) * GAUSSIAN_COEFF[idx + GAUSSIAN_KERNEL_RADIUS];
                }

                return finalColor;
            }
            ENDCG
        }

	}
	FallBack "Diffuse"
}
