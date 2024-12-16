// Based on Unity built-in shader. Copyright (c) 2016 Unity Technologies. MIT license (see UnityShadersLicense.txt)

Shader "Insane Systems/Unit with House Color" 
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_HouseColor("House Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_Metallness("Metallic", 2D) = "black" {}
		[Toggle] _MetallnessSmoothness("Use metallic alpha as smoothness", Range(0,1)) = 0
	
		_Roughness("Roughness", 2D) = "white" {}
		[Toggle] _InvertRoughness("Invert rougnhess (if smoothness)", Range(0,1)) = 0
	
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Normal Strength", Range(0, 4)) = 1
		_HouseColorTex ("House Color Tex", 2D) = "black" {}
		_HouseColorStrength("House Color Strength", Range(0, 1)) = 1
		_OcclusionTex ("Occlusion Tex", 2D) = "white" {}
		_Occlusion ("Occlusion strength", Range(0,1)) = 1
		_EmissionColor("Emission Color", Color) = (0,0,0)
		_EmissionScale("Emission Strength", Range(0, 10)) = 1
		_EmissionTex("Emission", 2D) = "white" {}
				
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.7
	}
	SubShader {
		Tags { "RenderType" = "TransparentCutout" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alphatest:_Cutoff fullforwardshadows
		
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Metallness;
		sampler2D _Roughness;
		sampler2D _EmissionTex;
		sampler2D _HouseColorTex;
		sampler2D _OcclusionTex;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_Metallness;
			float2 uv_Roughness;
		};
		
		half _Occlusion;
		half _MetallnessSmoothness;
		half _InvertRoughness;
		half _BumpScale;
		half _EmissionScale;
		half _HouseColorStrength;
		fixed4 _Color;
		fixed4 _EmissionColor;
		fixed4 _HouseColor;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
		
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 emission = tex2D (_EmissionTex, IN.uv_MainTex) * _EmissionColor * _EmissionScale;
			fixed4 houseC = tex2D (_HouseColorTex, IN.uv_MainTex);
			fixed4 aoTex = tex2D(_OcclusionTex, IN.uv_MainTex);
			fixed4 metallness = tex2D(_Metallness, IN.uv_Metallness);
			fixed4 roughness = tex2D(_Roughness, IN.uv_Roughness);

			aoTex = lerp(fixed4(1, 1, 1, 1), aoTex, _Occlusion);

			o.Albedo = lerp(c.rgb * aoTex.rgb, houseC.rgb * _HouseColor, houseC.r * _HouseColor.a * _HouseColorStrength);
			o.Metallic = metallness.r;

			if (_MetallnessSmoothness > 0)
				o.Smoothness = metallness.a;
			else if (_InvertRoughness > 0)
				o.Smoothness = roughness.r;
			else
				o.Smoothness = 1 - roughness.r;
		
			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
			o.Emission = emission;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
