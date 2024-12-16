Shader "Insane Systems/Fog Of War"
{
	Properties
	{
		_Color ("Color", Color) = (0,0,0,1)
		_MapSize("Map Size", Range(16, 512)) = 256
		_MinFogStrength("Min Fog Strength", Range(0, 1)) = 0.2
		_FogStrength("Max Fog Strength", Range(0, 1)) = 0.8
		_Noise01Sampler("Noise texture", 2D) = "white" {}
		_NoiseSpeed("Noise Speed", Range(0, 2)) = 0.1
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
			
			#include "UnityCG.cginc"
			
			sampler2D _Noise01Sampler;
			float4 _Noise01Sampler_ST;
			float _NoiseSpeed;
			float _MapSize;
			float _MinFogStrength;
			float _FogStrength;
			fixed4 _Color;
			
			uniform float _Enabled;
	        uniform sampler2D _FOWVisionRadiusesTexture;
	        uniform sampler2D _FOWPositionsTexture;
	        uniform float _ActualUnitsCount;
	        uniform float _MaxUnits;

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
			
			void Remap_Float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
			{
			    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = _Color;
			
	            float alpha = _FogStrength;
				float3 pos = i.worldPos;
				pos.y = 0;
				
				fixed4 noise01 = tex2D(_Noise01Sampler, float2(i.texcoord.x + _Time.x * _NoiseSpeed, i.texcoord.y));
				fixed4 noise02 = tex2D(_Noise01Sampler, float2(i.texcoord.x * 0.5, i.texcoord.y * 0.5 + _Time.x * _NoiseSpeed));
				float alphaNoise = noise01.r * noise02.r * 2;
				
				if (_Enabled)
				{			
				    float2 textureResolution = float2(_MaxUnits, 1);
				    
	                for (int i = 0; i < _ActualUnitsCount; i++)
	                {
	                    float2 unitPixelCenterPos = float2(i, 0) + 0.5; // 0.5 to sample center of pixel due we work in texel space
	                    float3 unitPosition = tex2D(_FOWPositionsTexture, unitPixelCenterPos / textureResolution).rgb * 1024; //_Positions[i].xyz;
	                    unitPosition.y = 0; // RTS Kit - todo offset by unit height.
	                    
	                    float visionRadius = tex2D(_FOWVisionRadiusesTexture, unitPixelCenterPos / textureResolution).r * 512;
	                    
	                    float distanceToUnit = distance(unitPosition, pos);
	                    
	                    if (distanceToUnit < visionRadius)
	                    {
	                        float size = visionRadius - distanceToUnit;
	                        
	                        if (size < 1)
	                            alpha = lerp(alpha, 0, size); // Insane Systems: previous alpha used because this sector can be already visible by other unit.
	                        else 
	                            alpha = 0;
	                    }
	                }
	    
	                alpha = clamp(alpha, 0, 1);
				}
				else
				{
				    alpha = 0;
				}
				Remap_Float(alphaNoise * 2, float2(0, 1), float2(_MinFogStrength, _FogStrength), alphaNoise);
			
				c.a = clamp(alpha * alphaNoise, 0, _FogStrength);
				
				UNITY_APPLY_FOG(i.fogCoord, c);
				
				return c;
			}		
			ENDCG
		}
	}
	FallBack "Diffuse"
}
