Shader "Gongbi/InkFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.85, 0.82, 0.78, 1.0)
        _FogDensity ("Fog Density", Range(0, 0.1)) = 0.015
        _FogStartY ("Fog Start Y (world)", Float) = 0.0
        _FogEndY ("Fog End Y (world)", Float) = 50.0
        _HeightFactor ("Height Fog Factor", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            fixed4 _FogColor;
            float _FogDensity;
            float _FogStartY;
            float _FogEndY;
            float _HeightFactor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.worldPos - _WorldSpaceCameraPos);
                float distFog = 1.0 - exp(-dist * _FogDensity);

                float heightT = saturate((i.worldPos.y - _FogStartY) / max(_FogEndY - _FogStartY, 0.001));
                float heightFog = (1.0 - heightT) * _HeightFactor;

                float totalFog = saturate(distFog + heightFog * distFog);
                return fixed4(_FogColor.rgb, 1.0 - totalFog);
            }
            ENDCG
        }
    }
}
