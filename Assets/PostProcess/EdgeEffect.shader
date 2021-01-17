Shader "Hidden/Edge Effect"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
			// Custom post processing effects are written in HLSL blocks,
			// with lots of macros to aid with platform differences.
			// https://github.com/Unity-Technologies/PostProcessing/wiki/Writing-Custom-Effects#shader
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

			#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

			TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
			TEXTURE2D_SAMPLER2D(_CameraNormalsTexture, sampler_CameraNormalsTexture);
			TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        
			float4 _MainTex_TexelSize;
			float _edge_size;
			float _depth_threshold;
			float _normal_threshold;
			float _depth_normal_threshold;
			float _depth_threshold_scale;
			float4x4 _clip_to_view;

			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoordStereo : TEXCOORD1;
				float3 view_space_dir : TEXCOORD2;
			#if STEREO_INSTANCING_ENABLED
				uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
			#endif
			};

			#if STEREO_INSTANCING_ENABLED
			float _DepthSlice;
			#endif

			Varyings Vert(AttributesDefault v)
			{
				Varyings o;
				o.vertex = float4(v.vertex.xy, 0.0, 1.0);
				o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
				o.view_space_dir = mul(_clip_to_view, o.vertex).xyz;

			#if UNITY_UV_STARTS_AT_TOP
				o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
			#endif

				o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

				return o;
			}

			float4 Frag(Varyings i) : SV_Target
			{

				float halfScaleFloor = floor(_edge_size * 0.5);
				float halfScaleCeil = ceil(_edge_size * 0.5);

				float2 bottomLeftUV = i.texcoord - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
				float2 topRightUV = i.texcoord + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;  
				float2 bottomRightUV = i.texcoord + float2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
				float2 topLeftUV = i.texcoord + float2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

				float depth0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV).r;
				float depth1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV).r;
				float depth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV).r;
				float depth3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV).r;

				float3 normal0 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomLeftUV).rgb;
				float3 normal1 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topRightUV).rgb;
				float3 normal2 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomRightUV).rgb;
				float3 normal3 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topLeftUV).rgb;

				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

				float diff_depth_0 = abs(depth1-depth0);
				float diff_depth_1 = abs(depth3-depth2);

				float3 diff_normal_0 = normal1-normal0;
				float3 diff_normal_1 = normal3-normal2;

				float edge_depth = sqrt(pow(diff_depth_0, 2) + pow(diff_depth_1, 2))*100;
				float3 view_normal = normal0*2-1;
				float ndotv = 1-dot(view_normal, -i.view_space_dir);

				float normalThreshold01 = saturate((ndotv - _depth_normal_threshold) / (1 - _depth_normal_threshold));
				float normalThreshold = normalThreshold01 * _depth_threshold_scale  + 1;

				_depth_threshold = _depth_threshold*depth0*normalThreshold;
				edge_depth = edge_depth > _depth_threshold ? 1 : 0;

				float edge_normal = sqrt(dot(diff_normal_0, diff_normal_0)+dot(diff_normal_1, diff_normal_1));
				edge_normal = edge_normal > _normal_threshold ? 1 : 0;

				float edge = 1-max(edge_normal, edge_depth);
				//return depth0*10;
				return color*edge;
			}
			ENDHLSL
		}
    }
}