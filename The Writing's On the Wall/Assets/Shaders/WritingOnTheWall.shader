Shader "Hidden/WritingOnTheWall"
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

			float _FrustumWidth;
			float _FrustumHeight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.ray = float3(o.vertex.x * 0.5 * _FrustumWidth, o.vertex.y * 0.5 * _FrustumHeight, 1);
				o.ray = mul(unity_CameraToWorld, o.ray);
                return o;
            }

            sampler2D _MainTex;
			sampler2D _CameraDepthTexture;

			sampler2D _OverlayColorTex;
			sampler2D _OverlayObjectDepthTex;
			sampler2D _OverlayBackgroundDepthTex;
			float4x4 _OldWorldToView;
			float3 _OldCameraPosition;
			float3 _OldCameraForwardVector;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, i.uv);

				// Get the fragment's uv position on the stored textures
				float3 worldPosition = _WorldSpaceCameraPos + i.ray * LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv));
				float4 projectedPos = mul(_OldWorldToView, float4(worldPosition.xyz, 1));
				float2 uv = projectedPos.xy / projectedPos.w;
				uv = (uv + float2(1, 1)) * 0.5;

				if (uv.x < 0 || uv.y < 0 || uv.x > 1 || uv.y > 1) return col;

				float fragDepth = dot(worldPosition - _OldCameraPosition, _OldCameraForwardVector);

				// Sample the RenderTextures
				fixed4 col2 = tex2D(_OverlayColorTex, uv);
				float texDepthObject = LinearEyeDepth(tex2D(_OverlayObjectDepthTex, uv));
				float texDepthBackground = LinearEyeDepth(tex2D(_OverlayBackgroundDepthTex, uv));

				if (texDepthBackground > 900 && fragDepth > 900 || // Handle surfaces near the far plane
					fragDepth > texDepthObject && fragDepth < texDepthBackground + 0.1)
				{
					return lerp(col, col2, col2.a);
				}
				else
				{
					return col;
				}
            }
            ENDCG
        }
    }
}
