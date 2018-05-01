Shader "cfx/twoColorsGradual"
{

//
// Copyright (C) 2017 by cf/x and Christian Franz
//
// cf/x Image Lab shader for Unity
//
// Image Shader (camera f/x) that uses the pixel's brightness
// as an index into a dissolve between two colors. Can be used 
// for numerous effects, among them 'Night Vision Goggles' and 
// color bleach
//

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Left ("Left", Color) = (0.0, 0.0, 0.0, 1.0)
		_Right ("Right", Color) = (1.0, 1.0, 1.0, 1.0)
		_Blendback ("Blendback", Float) = 0.5
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
			fixed4 _Left;
			fixed4 _Right;
			fixed _Blendback;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 orig = col;
				fixed brt = (col.r + col.g, + col.b) / 3.0;

				col = lerp(_Left, _Right, brt); // make the color one between the two
				col = lerp(col, orig, _Blendback); // blend back with main image 
				return col;
			}
			ENDCG
		}
	}
}
