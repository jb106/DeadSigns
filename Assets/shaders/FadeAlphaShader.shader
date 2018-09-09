﻿Shader "Hidden/FadeAlphaShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tex("Texture", 2D) = "white"{}
		_Normal("Normal", 2D) = "bump"{}
		_Amount("Amount", Range(0, 1)) = 0
		_Distorsion("Distorsion", Range(0, 1)) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _Tex;
			sampler2D _Normal;
			float _Distorsion;
			float _Amount;

			fixed4 frag (v2f i) : SV_Target
			{
				//Sample de la texture source

				fixed4 col = tex2D(_Tex, i.uv);

				col.a = saturate(col.a + (_Amount * 2 - 1));

				half2 normal = UnpackNormal(tex2D(_Normal, i.uv)).xy;
				fixed4 srcCol = tex2D(_MainTex, i.uv + normal * col.a * _Distorsion);


				fixed4 overlayCol = srcCol * col * 2;
				overlayCol = lerp(srcCol, overlayCol, 0.9);

				fixed4 output = lerp(srcCol, overlayCol, col.a);
				return output;
			}
			ENDCG
		}
	}
}
