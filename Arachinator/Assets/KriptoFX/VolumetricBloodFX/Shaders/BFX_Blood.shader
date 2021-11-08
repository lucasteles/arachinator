Shader "KriptoFX/BFX/BFX_Blood"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
 	_SpecColor("SpecularColor", Color) = (1,1,1,1)
        _boundingMax("Bounding Max", Float) = 1.0
        _boundingMin("Bounding Min", Float) = 1.0
        _numOfFrames("Number Of Frames", int) = 240
        _speed("Speed", Float) = 0.33
        _HeightOffset("_Height Offset", Vector) = (0, 0, 0)
        //[MaterialToggle] _pack_normal("Pack Normal", Float) = 0
        _posTex("Position Map (RGB)", 2D) = "white" {}
        _nTex("Normal Map (RGB)", 2D) = "grey" {}
        _SunPos("Sun Pos", Vector) = (1, 0.5, 1, 0)


    }
    SubShader
    {

        Tags{ "Queue" = "AlphaTest+1"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
           // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD2;
                float4 screenPos : TEXCOORD4;
                float3 viewDir : TEXCOORD5;
                float height : TEXCOORD6;
               // UNITY_FOG_COORDS(8)

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;
half4 _SpecColor;

            float4 _HeightOffset;
            float _HDRFix;
            float4 _SunPos;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _UseCustomTime)
                UNITY_DEFINE_INSTANCED_PROP(float, _TimeInFrames)
                UNITY_DEFINE_INSTANCED_PROP(float, _LightIntencity)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float timeInFrames;
                float currentSpeed = 1.0f / (_numOfFrames / _speed);
                timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _UseCustomTime) > 0.5 ? UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames) : 1;

                float4 texturePos = tex2Dlod(_posTex, float4(v.uv.x, (timeInFrames + v.uv.y), 0, 0));
                float3 textureN = tex2Dlod(_nTex, float4(v.uv.x, (timeInFrames + v.uv.y), 0, 0));


                #if !UNITY_COLORSPACE_GAMMA
                    texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
                    textureN = LinearToGammaSpace(textureN);
                #endif

                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;

                o.worldNormal = textureN.xzy * 2 - 1;
                o.worldNormal.x *= -1;
                o.viewDir = ObjSpaceViewDir(v.vertex);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeGrabScreenPos(o.pos);


                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                i.worldNormal = normalize(i.worldNormal);
                i.viewDir = normalize(i.viewDir);

                half fresnel = saturate(1 - dot(i.worldNormal, i.viewDir));
                half intencity = UNITY_ACCESS_INSTANCED_PROP(Props, _LightIntencity);
                half3 grabColor = intencity * 0.25;
                half light = max(0.001, dot(normalize(i.worldNormal), normalize(_SunPos.xyz)));
                light = pow(light, 50) * 10;
#if !UNITY_COLORSPACE_GAMMA
                _Color.rgb = _Color.rgb * .6;
                fresnel = fresnel * fresnel;
#endif
                grabColor *= _Color.rgb;
                grabColor = lerp(grabColor * 0.15, grabColor, fresnel);
                grabColor = min(grabColor, _Color.rgb * 0.55);

                half3 color = grabColor.xyz + saturate(light) * intencity * _SpecColor.xyz * _SpecColor.a;
                return half4(color, 1);

            }
            ENDCG
        }

        //you can optimize it by removing shadow rendering and depth writing
        //start remove line

        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;

            float4 _HeightOffset;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _TimeInFrames)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float timeInFrames;
                float currentSpeed = 1.0f / (_numOfFrames / _speed);
                timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames);
                float4 texturePos = tex2Dlod(_posTex, float4(v.uv.x, (timeInFrames + v.uv.y), 0, 0));

#if !UNITY_COLORSPACE_GAMMA
                texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
#endif

                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

        //end remove light
    }
}
