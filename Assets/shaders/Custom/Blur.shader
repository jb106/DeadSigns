Shader "Hidden/Custom/Blur"
{
	HLSLINCLUDE

#include "PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_BlurTex, sampler_BlurTex);

	float _Blend;


	float4 Frag(VaryingsDefault i) : SV_Target
	{
		half4 texcol = half4(0, 0, 0, 0);
		float remaining = 1.0f;
		float coef = 1;
		float fI = 0.1;
		for (int j = 0; j < 3; j++) {
			fI++;
			coef *= 0.32;
			texcol += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.texcoord.x, i.texcoord.y - fI * _Blend)) * coef;
			texcol += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.texcoord.x - fI * _Blend, i.texcoord.y)) * coef;
			texcol += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.texcoord.x + fI * _Blend, i.texcoord.y)) * coef;
			texcol += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.texcoord.x, i.texcoord.y + fI * _Blend)) * coef;

			remaining -= 4 * coef;
		}
		texcol += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord * remaining);

		return texcol;
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

#pragma vertex VertDefault
#pragma fragment Frag

			ENDHLSL
		}
	}
}