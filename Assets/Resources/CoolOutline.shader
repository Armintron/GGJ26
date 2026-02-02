Shader "Unlit/EdEddnEddyOutline"
{
    Properties
    {
        [Header(Colors)]
        _MainColor ("Inside Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)

        [Header(Outline Settings)]
        _OutlineWidth ("Outline Width", Range(0.0, 0.2)) = 0.02
        
        [Header(Shake Settings)]
        _ShakeSpeed ("Shake Speed (FPS)", Range(0, 24)) = 12
        _ShakeAmplitude ("Shake Amplitude", Range(0.0, 0.05)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        // PASS 1: The Shaky Outline
        Pass
        {
            Name "Outline"
            Cull Front // Render the backfaces (inverted hull)
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            float _OutlineWidth;
            float4 _OutlineColor;
            float _ShakeSpeed;
            float _ShakeAmplitude;

            // Simple pseudo-random noise function
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;

                // 1. Calculate the "Stepped Time" to mimic low framerate animation
                // We floor the time multiplied by speed to get discrete steps (like 12fps)
                float timeStep = floor(_Time.y * _ShakeSpeed);

                // 2. Generate noise based on vertex position + timeStep
                // We use vertex.xy + vertex.z to get a unique seed per vertex
                float noiseVal = random(v.vertex.xy + v.vertex.z + timeStep);
                
                // Map noise from 0..1 to -1..1
                float displacement = (noiseVal - 0.5) * 2.0;

                // 3. Apply the shake to the Normal direction
                // We add the displacement to the width so the line gets thicker/thinner/wobbly
                float currentWidth = _OutlineWidth + (displacement * _ShakeAmplitude);

                // 4. Extrude the vertex along the normal (Model Space)
                float3 position = v.vertex.xyz + v.normal * currentWidth;

                // 5. Convert to Clip Space
                o.pos = UnityObjectToClipPos(position);
                o.color = _OutlineColor;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

        // PASS 2: The Inside Color
        Pass
        {
            Name "Fill"
            Cull Back // Standard culling
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _MainColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _MainColor;
            }
            ENDCG
        }
    }
}