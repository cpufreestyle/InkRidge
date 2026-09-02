Shader "Gongbi/Toon"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.6,0.6,0.6,1)
        _OutlineColor ("Outline Color", Color) = (0.08,0.06,0.04,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.05)) = 0.012
        _ToonSpecColor ("Specular Color", Color) = (0.9,0.8,0.6,1)
        _ToonSpecPower ("Specular Power", Range(1, 200)) = 64
        _ShadowBand ("Shadow Band Threshold", Range(0.0, 1.0)) = 0.35
        _MidBand ("Mid Band Threshold", Range(0.0, 1.0)) = 0.7
        _ShadowIntensity ("Shadow Intensity", Range(0.0, 1.0)) = 0.55
        _LightTint ("Light Tint Influence", Range(0.0, 1.0)) = 0.25
        _WindSway ("Wind Sway Amount", Range(0.0, 1.0)) = 0.0
        _InkWash ("Base Ink Wash", Range(0.0, 0.5)) = 0.15
        _ColorJitter ("Per-Instance Color Jitter", Range(0.0, 0.3)) = 0.06
        _PaperGrain ("Paper Grain", Range(0.0, 0.1)) = 0.02
        _RimColor ("Rim Light Color", Color) = (1.0, 0.96, 0.88, 1)
        _RimIntensity ("Rim Intensity", Range(0.0, 1.0)) = 0.18
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
        // Note: GPU instancing disabled due to per-material _WindSway and wind vertex displacement.
        // Static batching handles most draw call reduction for this project.

        // ── Pass 1: Outline (back-face extrusion) ──
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            fixed4 _OutlineColor;
            float _WindSway;

            // Global wind params (set by WindSystem.cs)
            float _WindSpeed;
            float _WindMagnitude;
            float _WindTurbulence;
            float _WindDirectionX;
            float _WindDirectionZ;
            float _GustDensity;
            float _WindIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Wind displacement — always computed, _WindSway scales it
                float heightFactor = saturate(worldPos.y * 0.12);
                float swayPhase = _Time.y * _WindSpeed + worldPos.y * _GustDensity * 0.3;
                float sway = sin(swayPhase) * _WindMagnitude * _WindIntensity * _WindSway * heightFactor * 3.0;
                float turb = sin(swayPhase * _WindTurbulence + worldPos.x * 0.5) * 0.3;
                sway += turb * _WindMagnitude * _WindIntensity * _WindSway * heightFactor * 3.0;

                worldPos.x += _WindDirectionX * sway;
                worldPos.z += _WindDirectionZ * sway;

                float3 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).xyz;
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                viewPos += viewNormal * _OutlineWidth;
                o.pos = mul(UNITY_MATRIX_P, float4(viewPos, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // ── Pass 2: Cel-shaded surface ──
        CGPROGRAM
        #pragma surface surf Cel vertex:vert fullfwdshadows
        #pragma target 3.0

        fixed4 _MainColor;
        fixed4 _ShadowColor;
        fixed4 _ToonSpecColor;
        float  _ToonSpecPower;
        float  _ShadowBand;
        float  _MidBand;
        float  _ShadowIntensity;
        float  _LightTint;
        float  _WindSway;
        float  _InkWash;
        float  _ColorJitter;
        float  _PaperGrain;
        fixed4 _RimColor;
        float  _RimIntensity;
        float  _RimPower;

        // Global wind params
        float _WindSpeed;
        float _WindMagnitude;
        float _WindTurbulence;
        float _WindDirectionX;
        float _WindDirectionZ;
        float _GustDensity;
        float _WindIntensity;

        half4 LightingCel(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half NdotL = dot(s.Normal, lightDir);

            half cel;
            if (NdotL < _ShadowBand)
                cel = 1.0 - _ShadowIntensity;
            else if (NdotL < _MidBand)
                cel = 1.0 - _ShadowIntensity * 0.5;
            else
                cel = 1.0;

            half3 halfVec = normalize(lightDir + viewDir);
            half NdotH = max(dot(s.Normal, halfVec), 0);
            half spec = step(0.5, pow(NdotH, _ToonSpecPower));

            // Soft rim backlight — paper-glow on silhouettes, reads as 背光宣纸.
            half rim = pow(1.0 - saturate(dot(s.Normal, viewDir)), _RimPower);

            half3 baseColor = lerp(_ShadowColor.rgb, _MainColor.rgb, cel);
            baseColor += spec * _ToonSpecColor.rgb * 0.35;
            baseColor += rim * _RimColor.rgb * _RimIntensity;

            baseColor *= lerp(fixed3(1,1,1), _LightColor0.rgb, _LightTint) * (atten * 2);

            return half4(baseColor, s.Alpha);
        }

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // Cheap stable hash — baked into the combined mesh so batching keeps it.
        float hashW(float3 p)
        {
            return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453);
        }

        void vert(inout appdata_full v)
        {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            // Wind displacement — always computed, _WindSway scales it (0 = no wind)
            float heightFactor = saturate(worldPos.y * 0.12);
            float swayPhase = _Time.y * _WindSpeed + worldPos.y * _GustDensity * 0.3;
            float sway = sin(swayPhase) * _WindMagnitude * _WindIntensity * _WindSway * heightFactor * 3.0;
            float turb = sin(swayPhase * _WindTurbulence + worldPos.x * 0.5) * 0.3;
            sway += turb * _WindMagnitude * _WindIntensity * _WindSway * heightFactor * 3.0;

            worldPos.x += _WindDirectionX * sway;
            worldPos.z += _WindDirectionZ * sway;

            v.vertex = mul(unity_WorldToObject, float4(worldPos, 1.0));
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 albedo = _MainColor.rgb;

            // Per-instance pigment variation — hash of world position breaks the
            // uniform clone look without breaking static batching (baked into mesh).
            float jitter = (hashW(floor(IN.worldPos * 2.0)) - 0.5) * _ColorJitter;
            albedo *= 1.0 + jitter;

            // Ink-wash pooling: pigment sinks toward the base (ink painting cue).
            float inkT = saturate((1.5 - IN.worldPos.y) / 1.5);
            albedo *= lerp(1.0, 0.72, inkT * _InkWash * 2.0);

            // Paper grain — high-frequency luminance noise, silk/宣纸 texture cue.
            albedo += (hashW(floor(IN.worldPos * 14.0)) - 0.5) * _PaperGrain;

            o.Albedo = albedo;
            o.Alpha = _MainColor.a;
        }
        ENDCG
    }

    Fallback "Diffuse"
}
