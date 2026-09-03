Shader "Gongbi/BreathGlow"
{
    Properties
    {
        _Progress ("Breath Progress", Range(0, 1)) = 0.5
        _CoreColor ("Core Color", Color) = (1, 0.95, 0.85, 0.8)
        _EdgeColor ("Edge Color", Color) = (0.3, 0.5, 0.7, 0.3)
        _MaxRadius ("Max Radius", Range(0.5, 3.0)) = 2.0
        _Softness ("Softness", Range(0.1, 1.0)) = 0.4
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            fixed4 _CoreColor;
            fixed4 _EdgeColor;
            float _MaxRadius;
            float _Softness;
            float _PulseSpeed;

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
                // Scale the quad based on breath progress
                float scale = lerp(0.3, _MaxRadius, _Progress);
                v.vertex.xyz *= scale;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = i.uv - 0.5;
                float dist = length(center) * 2.0;

                // Radial gradient: bright core fading to edge
                float core = 1.0 - smoothstep(0.0, 0.3, dist);
                float glow = 1.0 - smoothstep(0.2, 1.0, dist);

                // Pulse: subtle breathing animation on top of progress
                float pulse = sin(_Time.y * _PulseSpeed) * 0.05 + 0.95;

                // Combine: core color at center, edge color at edges
                fixed4 col = lerp(_EdgeColor, _CoreColor, core + glow * 0.5);
                col.a *= (core * 0.8 + glow * 0.3) * pulse * _Progress;
                col.a = saturate(col.a);

                return col;
            }
            ENDCG
        }
    }
}
