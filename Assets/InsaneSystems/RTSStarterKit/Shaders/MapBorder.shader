Shader "Insane Systems/Map Border"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Color02 ("Second Color", Color) = (0,0,0,1)
		_MapSize("Map Size", Range(16, 512)) = 256
		
		_Noise01Sampler("Noise texture", 2D) = "white" {}
		_NoiseSpeed("Noise Speed", Range(0, 2)) = 0.1
		
		_BorderSmooth("Border smooth", Range(0, 100)) = 3
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "Render"="Transparent" "IgnoreProjector"="True"}
		LOD 200
		ZTest Always // to overlay whole map
		Blend SrcAlpha OneMinusSrcAlpha 
		
		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			float _MapSize;
			sampler2D _Noise01Sampler;
			float4 _Noise01Sampler_ST;
			float _NoiseSpeed;
			float _BorderSmooth;
			fixed4 _Color;
			fixed4 _Color02;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
	        };
	  
	        struct v2f
			{
        		float4 vertex : SV_POSITION;
        		float2 texcoord : TEXCOORD0;
	        	float3 worldPos : TEXCOORD1;
	        	UNITY_FOG_COORDS(2)
        		UNITY_VERTEX_OUTPUT_STEREO
	        };
			
			
			v2f vert(appdata v)
			{
				v2f o;
				
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _Noise01Sampler);
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				const float mapSize = _MapSize;

				fixed4 c = _Color;

				float3 pos = i.worldPos;

				float distanceFromBorderX = 0;
				float distanceFromBorderZ = 0;

				if (pos.x <= 0)
					distanceFromBorderX = abs(pos.x);
				else if (pos.x >= mapSize)
					distanceFromBorderX = abs(mapSize - pos.x);

				if (pos.z <= 0)
					distanceFromBorderZ = abs(pos.z);
				else if (pos.z >= mapSize)
					distanceFromBorderZ = abs(mapSize - pos.z);

				float alpha = 0;

				if (distanceFromBorderX > 0 && alpha == 0)
					alpha = lerp(0, 1, distanceFromBorderX / _BorderSmooth);

				if (distanceFromBorderZ > 0)
				{
					float newAlpha = lerp(0, 1, distanceFromBorderZ / _BorderSmooth);
					
					if (newAlpha > alpha)
						alpha = newAlpha;
				}
				
				fixed4 noise01 = tex2D(_Noise01Sampler, float2(i.texcoord.x + _Time.x * _NoiseSpeed, i.texcoord.y));
				fixed4 noise02 = tex2D(_Noise01Sampler, float2(i.texcoord.x * 0.5, i.texcoord.y * 0.5 + _Time.x * _NoiseSpeed));
				float finalNoise = clamp(noise01.r * noise02.r * 2, 0, 1);

				alpha = clamp(alpha, 0, 1);
				
				c.a = alpha;
				c.rgb = lerp(_Color, _Color02, finalNoise);
				
				//UNITY_APPLY_FOG(i.fogCoord, c);
				
				return c;
	         }
			ENDCG
		}
	}
	FallBack "Diffuse"
}
