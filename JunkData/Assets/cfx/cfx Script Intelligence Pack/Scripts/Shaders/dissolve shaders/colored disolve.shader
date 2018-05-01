//
// colored dissolve shader 
// (C) 2017 by cf/x AG and Christian Franz
//
// based loosely on "Burn Dissolve"
//
Shader "colored dissolve"
{
	Properties
	{
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		_Albedo("Albedo", 2D) = "white" {}
		_Normals("Normals", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_DissolveStretch("Dissolve Stretch", Range( 1 , 2)) = 1
		_DissolveAmount("Dissolve Amount", Range( 0 , 1)) = 0
		_ColorRamp("Color Ramp", 2D) = "white" {}
		_ColorTrigger("Color Trigger", Range( 0 , 0.01)) = 0.01
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
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
		uniform float _ColorTrigger;
		uniform float _DissolveAmount;
		uniform float _DissolveStretch;
		uniform sampler2D _TextureSample1;
		uniform float4 _TextureSample1_ST;
		uniform sampler2D _ColorRamp;
		uniform float _MaskClipValue = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = tex2D( _Normals, uv_Normals ).rgb;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float temp_output_6_0 = ( (0.51 + (_DissolveAmount) * (-0.6 - 0.51)) + ( _DissolveStretch * tex2D( _TextureSample1, uv_TextureSample1 ).r ) );
			float temp_output_19_0 = ( 1.0 - clamp( (-4.0 + (temp_output_6_0) * (8.0)) , 0.0 , 1.0 ) );
			float2 appendResult13 = float2( temp_output_19_0 , 0 );
			o.Emission = ( step( _ColorTrigger , _DissolveAmount ) * temp_output_19_0 * tex2D( _ColorRamp, appendResult13 ) ).xyz;
			o.Alpha = 1;
			clip( temp_output_6_0 - _MaskClipValue );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
