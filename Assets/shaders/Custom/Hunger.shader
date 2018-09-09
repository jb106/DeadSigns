Shader "Hidden/Custom/Hunger"
{
	HLSLINCLUDE

#include "PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_Diffuse, sampler_Diffuse);
		TEXTURE2D_SAMPLER2D(_Normal, sampler_Normal);

	float _Blend;
	float _Distortion;


	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

		float4 hungerDistortion = SAMPLE_TEXTURE2D(_Normal, sampler_Normal, i.texcoord);

		float4 hungerAlpha = SAMPLE_TEXTURE2D(_Diffuse, sampler_Diffuse, i.texcoord + hungerDistortion * _Distortion).a;
		float4 hungerCol = SAMPLE_TEXTURE2D(_Diffuse, sampler_Diffuse, i.texcoord);



		hungerAlpha = saturate(hungerAlpha + (_Blend * 2 - 1));

		float4 srcCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

		float4 overlayCol = srcCol * hungerCol * 2;
		overlayCol = lerp(srcCol, overlayCol, 0.95);


		//Ici c'est juste avant qu'on mix les couleurs, on peut donc effectuer une distortion
		

		float4 output = lerp(srcCol, overlayCol, hungerAlpha);

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