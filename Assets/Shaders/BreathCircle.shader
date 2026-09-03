Shader "Gongbi/BreathCircle"
{
    Properties
    {
        _Progress ("Breath Progress", Range(0, 1)) = 0.5
        _InhaleColor ("Inhale Color", Color) = (0.85, 0.92, 0.95, 0.6)
        _ExhaleColor ("Exhale Color", Color) = (0.15, 0.12, 0.10, 0.8)
        _MaxRadius ("Max Radius", Range(0.1, 2.0)) = 1.5
        _MinRadius ("Min Radius", Range(0.0, 0.5)) = 0.2
        _Feather ("Edge Feather", Range(0.01, 0.5)) = 0.15
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Back
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            float _Progress;
            fixed4 _InhaleColor;
            fixed4 _ExhaleColor;
            float _MaxRadius;
            float _MinRadius;
            float _Feather;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = i.uv - 0.5;
                float dist = length(center) * 2.0;

                float radius = lerp(_MinRadius, _MaxRadius, _Progress);

                float ring = smoothstep(radius - _Feather, radius, dist) *
                             (1.0 - smoothstep(radius, radius + _Feather, dist));

                fixed4 col = lerp(_ExhaleColor, _InhaleColor, _Progress);
                col.a *= ring;

                return col;
            }
            ENDCG
        }
    }
}
