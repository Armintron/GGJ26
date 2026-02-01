Shader "Hidden/ToxicGasPPS" 
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag

            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

            // --- FIXED: Use PPSv2 Texture Declarations ---
            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
            TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

            float _Density;
            float _DepthFalloff;
            float4 _GasColor;
            float _Speed;
            float _Scale;
            float _Distortion;
            
            float _VignetteStrength;
            float4 _VignetteColor;
            float _UnlitThreshold;
            float _UnlitStrength;

            // --- NOISE ---
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

            float4 Frag(VaryingsDefault i) : SV_Target
            {
                // FIXED: Use 3 arguments (Texture, Sampler, UV)
                float depthRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord);
                float depth = Linear01Depth(depthRaw);

                // Noise
                float2 scroll = float2(_Time.y * _Speed, _Time.y * _Speed * 0.4);
                float2 noiseUV = (i.texcoord * _Scale) + scroll;
                float gasValue = fbm(noiseUV + depth * 2.0);

                // Distortion
                float2 distortOffset = (gasValue - 0.5) * _Distortion;
                distortOffset *= saturate(depth * 5.0); 

                // FIXED: Use SAMPLE_TEXTURE2D for main scene color
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + distortOffset);

                // Unlit Protection
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                float protectUnlit = smoothstep(_UnlitThreshold, 1.0, luminance);

                // Gas Fog
                float fogFactor = saturate(depth * _DepthFalloff * (gasValue + 0.5));
                fogFactor = saturate(fogFactor * _Density);
                fogFactor = lerp(fogFactor, 0.0, protectUnlit * _UnlitStrength);

                float4 result = lerp(col, _GasColor, fogFactor);

                // Vignette
                float2 center = i.texcoord - 0.5;
                float dist = length(center);
                float vignetteMask = smoothstep(0.3, 0.8, dist);
                vignetteMask *= _VignetteStrength;

                result = lerp(result, _VignetteColor, vignetteMask);

                return result;
            }
            ENDHLSL
        }
    }
}