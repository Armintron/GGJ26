Shader "Custom/HazyVolumeCube"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.5, 0.6, 0.7, 1)
        _FogDensity ("Fog Density", Range(0, 5)) = 1.0
        _MaxOpacity ("Max Fog Opacity", Range(0, 1)) = 0.8
        _RefractionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02
        _HazeScale ("Haze Noise Scale", Float) = 10.0
        _HazeSpeed ("Haze Speed", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
        LOD 100

        ZWrite Off
        ZTest LEqual
        Cull Back

        GrabPass { "_GrabTexture" }

        Pass
        {
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
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float eyeDepth : TEXCOORD1;
            };

            sampler2D _GrabTexture;
            sampler2D _CameraDepthTexture;

            float4 _FogColor;
            float _FogDensity;
            float _MaxOpacity;
            float _RefractionStrength;
            float _HazeScale;
            float _HazeSpeed;

            // --- NOISE FUNCTIONS ---
            float hash(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x),
                            lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                
                // Calculate surface depth (Linear)
                float3 viewPos = UnityObjectToViewPos(v.vertex);
                o.eyeDepth = -viewPos.z;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. NORMALIZE COORDINATES
                // Convert from weird projection space to simple 0-1 UVs immediately
                float2 screenUV = i.grabPos.xy / i.grabPos.w;

                // 2. CALCULATE DISTORTION
                float2 noiseUV = screenUV * _HazeScale;
                noiseUV.y += _Time.y * _HazeSpeed;
                float haze = noise(noiseUV);

                // Calculate the offset we WANT to apply
                float2 offset = (haze - 0.5) * _RefractionStrength;
                float2 distortedUV = screenUV + offset;

                // 3. DEPTH CHECK (The Fix)
                // Check the depth at the spot we WANT to grab from
                float distortedDepthRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, distortedUV);
                float distortedDepth = LinearEyeDepth(distortedDepthRaw);

                // Check depth at the original spot (background wall)
                float originalDepthRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float originalDepth = LinearEyeDepth(originalDepthRaw);

                // LOGIC:
                // If the object at the distorted UV is CLOSER than the cube surface...
                // ...it means we hit a foreground object (Hand/Gun).
                // We also add a tiny buffer (0.1) to prevent flickering on the cube's own surface.
                if (distortedDepth < (i.eyeDepth - 0.1))
                {
                    // REJECT: Don't distort. Use original UV.
                    distortedUV = screenUV;
                }

                // 4. SAMPLE COLOR
                // Use tex2D (standard) because we already divided by W
                fixed3 sceneColor = tex2D(_GrabTexture, distortedUV).rgb;

                // 5. CALCULATE FOG
                // We use the 'originalDepth' for fog thickness calculation
                // so the fog doesn't jitter with the refraction
                float fogThickness = max(0.0, originalDepth - i.eyeDepth);
                float fogFactor = 1.0 - exp(-fogThickness * _FogDensity);
                fogFactor = min(fogFactor, _MaxOpacity);

                // 6. FINAL BLEND
                fixed3 finalRGB = lerp(sceneColor, _FogColor.rgb, fogFactor);

                return fixed4(finalRGB, 1.0);
            }
            ENDCG
        }
    }
}