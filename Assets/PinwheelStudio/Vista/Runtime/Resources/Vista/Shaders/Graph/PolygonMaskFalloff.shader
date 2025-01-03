Shader "Hidden/Vista/Graph/PolygonMaskFalloff"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		ColorMask R
		Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../Includes/Math.hlsl"

			struct appdata
			{
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct v2f
			{
				float4 color: COLOR;
				float4 vertex: SV_POSITION;
				float4 localPos: TEXCOORD1;
			};

			sampler2D _FalloffMap;
			int _HasFalloffMap;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.localPos = v.vertex;
				return o;
			}

			float frag(v2f input): SV_Target
			{
				float f = input.color.r;
				if (_HasFalloffMap)
				{
					float falloff = tex2D(_FalloffMap, input.localPos).r;
					float f0 = f * falloff;
					float f1 = f + falloff;
					float f2 = lerp(f0, f1, f);
					f = saturate(f2);
				}
				return f;
			}
			ENDCG

		}
	}
}
