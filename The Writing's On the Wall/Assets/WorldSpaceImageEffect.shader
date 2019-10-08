Shader "Image Effects/WorldSpaceImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			uniform float _FrustumWidth;
			uniform float _FrustumHeight;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 ray : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.ray = float3(o.vertex.x * 0.5 * _FrustumWidth, o.vertex.y * 0.5 * _FrustumHeight, 1);
				o.ray = mul(unity_CameraToWorld, o.ray);
				return o;
			}


			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 worldPosition = _WorldSpaceCameraPos + i.ray * LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv));

				// Darken alternate cells
				float3 cellCentre = round(worldPosition);
				if ((cellCentre.x + cellCentre.y + cellCentre.z) % 2 == 0)
				{
					col *= 0.8;
				}

				return col;
			}
			ENDCG
        }
    }
}
