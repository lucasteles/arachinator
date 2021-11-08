Shader "KriptoFX/BFX/BFX_Decal"
{
	Properties
	{
		[HDR] _TintColor("Tint Color", Color) = (1,1,1,1)
		_MainTex("NormalAlpha", 2D) = "white" {}
		_LookupFade("Lookup Fade Texture", 2D) = "white" {}
		_Cutout("Cutout", Range(0, 1)) = 1
		_CutoutTex("CutoutDepth(XZ)", 2D) = "white" {}
	[Space]
		_SunPos("Sun Pos", Vector) = (1, 0.5, 1, 0)
		//[Toggle(CLAMP_SIDE_SURFACE)] _ClampSideSurface("Clamp side surface", Int) = 0
	}


	SubShader
	{
		Tags{ "Queue" = "AlphaTest"}
		Blend DstColor SrcColor
		//	Blend SrcAlpha OneMinusSrcAlpha

		Cull Front
		ZTest Always
		ZWrite Off

		Pass
		{

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_fog
				#pragma multi_compile_instancing
				#pragma multi_compile _ USE_CUSTOM_DECAL_LAYERS
				#pragma multi_compile _ USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE
				#pragma shader_feature CLAMP_SIDE_SURFACE


				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _Flowmap;
				sampler2D _LookupFade;
				sampler2D _CutoutTex;

				float4 _MainTex_ST;
				float4 _MainTex_NextFrame;
				float4 _CutoutTex_ST;

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(half4, _TintColor)
					UNITY_DEFINE_INSTANCED_PROP(half, _Cutout)
					UNITY_DEFINE_INSTANCED_PROP(float, _LightIntencity)
				UNITY_INSTANCING_BUFFER_END(Props)


				half4 _CutoutColor;
				half4 _FresnelColor;
				half4 _DistortionSpeedScale;
				UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
				UNITY_DECLARE_DEPTH_TEXTURE(_LayerDecalDepthTexture);
				half InterpolationValue;
				half _AlphaPow;
				half _DistortSpeed;
				half _DistortScale;
				float4 _SunPos;
				half _DepthMul;
				half3 _DecalForwardDir;

				struct appdata_t {
					float4 vertex : POSITION;
					float4 normal : NORMAL;
					half4 color : COLOR;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half4 color : COLOR;

					float4 screenUV : TEXCOORD0;
					float4 ray : TEXCOORD1;
					float3 viewDir : TEXCOORD2;
					float4 screenPos : TEXCOORD3;
					float3 rayCameraOffset : TEXCOORD5;
					UNITY_FOG_COORDS(4)

					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};


				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;

					//o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
					o.screenUV = ComputeScreenPos(o.vertex);
					o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
					o.screenPos = ComputeGrabScreenPos(o.vertex);
					UNITY_TRANSFER_FOG(o,o.vertex);



					float3 viewRay = UnityObjectToViewPos(v.vertex).xyz;
					o.ray.w = viewRay.z;
					viewRay *= -1;
					o.ray.xyz = mul((float3x3)mul(unity_WorldToObject, UNITY_MATRIX_I_V), viewRay);
					o.rayCameraOffset = mul(mul(unity_WorldToObject, UNITY_MATRIX_I_V), float4(0, 0, 0, 1)).xyz;

					return o;
				}


				half4 frag(v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);



#if USE_CUSTOM_DECAL_LAYERS
					float depth = SAMPLE_DEPTH_TEXTURE(_LayerDecalDepthTexture, i.screenUV.xy / i.screenUV.w);
					float depthMask = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenUV.xy / i.screenUV.w);
					float fade = depth < depthMask - 0.0005 ? 0 : 1;

#if USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE

					fade = 1 - fade;
					depth = depthMask;
#endif
#else
					float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenUV.xy / i.screenUV.w);
#endif

					i.ray /= i.ray.w;
					float zEye = LinearEyeDepth(depth);
					float3 decalSpaceScenePos = i.rayCameraOffset + i.ray.xyz * zEye;



					float3 stepVal = saturate((0.5 - abs(decalSpaceScenePos.xyz)) * 10000);
					half lookupHeight = tex2D(_LookupFade, float2(decalSpaceScenePos.y + 0.5, 0));

					float projClipFade = stepVal.x * stepVal.y * stepVal.z * lookupHeight;
#if USE_CUSTOM_DECAL_LAYERS
					projClipFade *= fade;
#endif


#if CLAMP_SIDE_SURFACE
#ifdef UNITY_UV_STARTS_AT_TOP
					half3 n = normalize(cross(ddx(decalSpaceScenePos), ddy(decalSpaceScenePos) * _ProjectionParams.x));
#else
					half3 n = normalize(cross(ddx(decalSpaceScenePos), -ddy(decalSpaceScenePos) * _ProjectionParams.x));
#endif

					half angle = abs(dot(n, _DecalForwardDir));

					angle = angle > 0.1 ? 1 : 0;
					projClipFade *= angle;
					//return float4(saturate(angle), 0, 0, 1);

#endif


					float2 uv = decalSpaceScenePos.xz + 0.5;
					float2 uvMain = uv * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 uvCutout = (decalSpaceScenePos.xz + 0.5) * _CutoutTex_ST.xy + _CutoutTex_ST.zw;

					half4 normAlpha = tex2D(_MainTex, uvMain);
					half4 res = 0;
					res.a = saturate(normAlpha.w * 2);
					if (res.a < 0.1 || projClipFade < 0.1) discard;

					normAlpha.xy = normAlpha.xy * 2 - 1;
					float3 normal = normalize(float3(normAlpha.x, 1, normAlpha.y));

					half3 mask = tex2D(_CutoutTex, uvCutout).xyz;
					//mask = LinearToGammaSpace(mask);
					half cutout = 0.5 + UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout) * i.color.a * 0.5;

					half alphaMask = saturate((mask.r - (cutout * 2 - 1)) * 20) * res.a;
					half colorMask = saturate((mask.r - (cutout * 2 - 1)) * 5) * res.a;
					res.a = alphaMask;
					res.a = saturate(res.a * projClipFade);


					float intencity = UNITY_ACCESS_INSTANCED_PROP(Props, _LightIntencity);
					float light = max(0.001, dot(normal, normalize(_SunPos.xyz)));
					light = pow(light, 150) * 3 * intencity;
					light *= (1 - mask.z * colorMask);

					float4 tintColor = UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor);
					tintColor *= tintColor;
					res.rgb = lerp(tintColor.rgb * 2, tintColor.rgb * 0.25, mask.z * colorMask) + light;


					half fresnel = (1 - dot(normal, normalize(i.viewDir)));
					fresnel = pow(fresnel + 0.1, 5);

					//UNITY_APPLY_FOG_COLOR(i.fogCoord, res, half4(1, 1, 1, 1));
					return lerp(0.5, res, res.a);

					return res;
				}

			ENDCG
	}

	}

}
