Shader "Hidden/ToxicGasSimple"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            // --- NOISE MATH ---
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
            float fbm(float2 p) {
                float v = 0.0;
                v += 0.5 * noise(p); p *= 2.02;
                v += 0.25 * noise(p); p *= 2.03;
                v += 0.125 * noise(p); 
                return v;
            }
            // ------------------

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float _Density;
            float _DepthFalloff;
            float4 _GasColor;
            float _Speed;
            float _Scale;
            float _Distortion;
            
            // New Vignette Variables
            float _VignetteStrength;
            float4 _VignetteColor;

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Get Depth
                float depthRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float depth = Linear01Depth(depthRaw);

                // 2. Generate Noise / Gas
                float2 scroll = float2(_Time.y * _Speed, _Time.y * _Speed * 0.4);
                float2 noiseUV = (i.uv * _Scale) + scroll;
                float gasValue = fbm(noiseUV + depth * 2.0);

                // 3. Apply Distortion
                float2 distortOffset = (gasValue - 0.5) * _Distortion;
                distortOffset *= saturate(depth * 5.0); // Dampen near camera

                // 4. Sample Scene
                fixed4 col = tex2D(_MainTex, i.uv + distortOffset);

                // 5. Apply Gas Fog
                float fogFactor = saturate(depth * _DepthFalloff * (gasValue + 0.5));
                fogFactor = saturate(fogFactor * _Density);
                
                // Blend Scene with Gas
                fixed4 result = lerp(col, _GasColor, fogFactor);

                // 6. Apply Vignette (Mask Edges)
                // Calculate distance from center of screen
                float2 center = i.uv - 0.5;
                float dist = length(center);
                
                // Smoothstep creates a soft circle. 0.3 is inner edge, 0.8 is outer corner.
                float vignetteMask = smoothstep(0.3, 0.8, dist);
                
                // Apply vignette strength
                vignetteMask *= _VignetteStrength;

                // Blend result with Vignette Color (usually black)
                result = lerp(result, _VignetteColor, vignetteMask);

                return result;
            }
            ENDCG
        }
    }
}