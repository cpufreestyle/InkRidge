// Gongbi/InkSkybox — ink-wash gradient sky dome.
// Three-band gradient (zenith / horizon / below-horizon) + procedural drifting
// ink-wash cloud bands + paper grain. Single cheap fragment pass, no textures.
Shader "Gongbi/InkSkybox"
{
    Properties
    {
        _ZenithColor ("Zenith Color", Color) = (0.55, 0.62, 0.72, 1)
        _HorizonColor ("Horizon Color", Color) = (0.92, 0.90, 0.85, 1)
        _BottomColor ("Below Horizon Color", Color) = (0.55, 0.52, 0.48, 1)
        _CloudColor ("Cloud Color", Color) = (0.80, 0.79, 0.76, 1)
        _CloudCoverage ("Cloud Coverage", Range(0, 1)) = 0.45
        _CloudSpeed ("Cloud Drift Speed", Range(0, 0.1)) = 0.012
        _GrainAmount ("Paper Grain", Range(0, 0.1)) = 0.025
        _GradientPow ("Horizon Falloff", Range(0.2, 3)) = 0.7
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ZenithColor;
            fixed4 _HorizonColor;
            fixed4 _BottomColor;
            fixed4 _CloudColor;
            float _CloudCoverage;
            float _CloudSpeed;
            float _GrainAmount;
            float _GradientPow;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                // Native skybox cube: vertex position IS the view direction.
                o.dir = v.vertex.xyz;
                return o;
            }

            // 2D value noise, 3 octaves — cheap enough for a background pass.
            float2 hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash2(i).x;
                float b = hash2(i + float2(1, 0)).x;
                float c = hash2(i + float2(0, 1)).x;
                float d = hash2(i + float2(1, 1)).x;
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float clouds(float2 uv)
            {
                float t = _Time.y * _CloudSpeed;
                float n = vnoise(uv * 3.0 + float2(t, 0)) * 0.55
                        + vnoise(uv * 6.5 + float2(t * 1.7, 1.3)) * 0.30
                        + vnoise(uv * 13.0 + float2(t * 2.3, 2.6)) * 0.15;
                return n;
            }

            float grain(float2 p)
            {
                return (hash2(p).x - 0.5) * _GrainAmount;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                // Vertical gradient: below horizon -> bottom, above -> horizon to zenith.
                float up = dir.y;
                fixed3 col;
                if (up < 0)
                {
                    col = lerp(_HorizonColor.rgb, _BottomColor.rgb, saturate(-up * 2.2));
                }
                else
                {
                    col = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(saturate(up), _GradientPow));

                    // Ink-wash cloud bands on the upper dome.
                    // Dome projection: wider near horizon so bands read like distant mist.
                    float2 cuv = dir.xz / max(dir.y, 0.06);
                    float n = clouds(cuv * 0.5 + float2(0.0, 0.35));
                    float band = smoothstep(_CloudCoverage, _CloudCoverage + 0.22, n);
                    // Fade clouds toward zenith (mist hugs the horizon).
                    band *= saturate(up * 1.6);
                    col = lerp(col, _CloudColor.rgb, band * 0.65);
                }

                // Paper grain, stable per direction.
                col += grain(dir.xz * 220.0 + dir.y * 60.0);

                return fixed4(col, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}
