// 
// cfx simple dissolve, dissolve pattern based on perlin noise
//   (C) 2017 by Christian Franz and cf/x AG
//
Shader "simple dissolve"
{
	Properties
	{
//		[HideInInspector] __dirty( "", Int ) = 1
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "white" {}
		_DissolvePattern("Dissolve Pattern", 2D) = "white" {}
		_DissolveStretch("Dissolve Stretch", Range( 1 , 2)) = 0
		_DissolveAmount("Dissolve Amount", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _DissolveAmount;
		uniform float _DissolveStretch;
		uniform sampler2D _DissolvePattern;
		uniform float4 _DissolvePattern_ST;
		uniform float _MaskClipValue = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = tex2D( _Normals, uv_Normals ).rgb;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			o.Alpha = 1;
			float2 uv_DissolvePattern = i.uv_texcoord * _DissolvePattern_ST.xy + _DissolvePattern_ST.zw;
			clip( ( (0.51 + (_DissolveAmount - 0.0) * (-0.6 - 0.51) / (1.0 - 0.0)) + ( _DissolveStretch * tex2D( _DissolvePattern, uv_DissolvePattern ).r ) ) - _MaskClipValue );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
