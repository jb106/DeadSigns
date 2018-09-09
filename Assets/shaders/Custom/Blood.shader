Shader "Hidden/Custom/Blood"
{
	HLSLINCLUDE

#include "PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_Diffuse, sampler_Diffuse);
		TEXTURE2D_SAMPLER2D(_Normal, sampler_Normal);

	float _Blend;
	float _Distortion;

	inline float4 UnpackNormal(float4 packednormal)
	{
		float4 normal;
		normal.xy = packednormal.wy * 2 - 1;
		normal.z = sqrt(1 - normal.x*normal.x - normal.y * normal.y);
		normal.a = 0;
		return normal;
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

		float4 bloodCol = SAMPLE_TEXTURE2D(_Diffuse, sampler_Diffuse, i.texcoord);
		float4 bloodNormal = UnpackNormal(SAMPLE_TEXTURE2D(_Normal, sampler_Normal, i.texcoord));
		float4 bloodAlpha = SAMPLE_TEXTURE2D(_Diffuse, sampler_Diffuse, i.texcoord).a;

		bloodAlpha = saturate(bloodAlpha + (_Blend * 2 - 1));

		float4 srcCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + bloodNormal * bloodAlpha * _Distortion);

		float4 overlayCol = srcCol * bloodCol * 2;
		overlayCol = lerp(srcCol, overlayCol, 0.95);

		float4 output = lerp(srcCol, overlayCol, bloodAlpha);

		return output;
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