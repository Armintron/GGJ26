Shader "Hidden/PastelPainterlyStandard"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BrushTex ("Brush Normal Map", 2D) = "bump" {}
        _PaperTex ("Paper Texture", 2D) = "white" {}
        
        // --- ANIMATION OFFSETS ---
        _NoiseOffset1 ("Noise Offset 1", Vector) = (0,0,0,0)
        _NoiseOffset2 ("Noise Offset 2", Vector) = (0,0,0,0)
        _NoiseOffset3 ("Noise Offset 3", Vector) = (0,0,0,0)
        _VignetteNoiseOffset ("Vignette Offset", Vector) = (0,0,0,0)

        // --- GLOBAL SETTINGS ---
        _StrokeLength ("Stroke Length", Range(1, 10)) = 4.0
        _VerticalBias ("Vertical Bias", Range(0, 1)) = 0.0
        _PaperStrength ("Paper Strength", Range(0, 1)) = 0.5
        _DebugView ("Debug View", Float) = 0.0 
        
        // --- VIGNETTE 1 (STREAKY) ---
        _VignetteTex ("Vignette Noise Texture", 2D) = "white" {}
        _VignetteColor ("Vignette Color", Color) = (1,1,1,1)
        
        _VignetteSize ("Vignette Size", Range(0, 1.0)) = 0.3
        _VignetteSharpness ("Vignette Sharpness", Range(0.0, 1.0)) = 0.95 
        _BlurStrength ("Blur Strength", Range(0, 0.05)) = 0.01 

        // --- VIGNETTE 2 (SOLID SAFETY) ---
        _Vignette2Size ("Safety Vignette Size", Range(0, 1.0)) = 0.45
        _Vignette2Smoothness ("Safety Vignette Smoothness", Range(0.0, 1.0)) = 0.2

        // --- LAYERS ---
        _L1_Scale ("Layer 1 Scale", Float) = 4.0
        _L1_Str ("Layer 1 Strength", Range(0, 0.5)) = 0.05
        _L1_Rot ("Layer 1 Rotation", Range(-3.14, 3.14)) = 0.0

        _L2_Scale ("Layer 2 Scale", Float) = 6.0
        _L2_Str ("Layer 2 Strength", Range(0, 0.5)) = 0.03
        _L2_Rot ("Layer 2 Rotation", Range(-3.14, 3.14)) = 0.78

        _L3_Scale ("Layer 3 Scale", Float) = 12.0
        _L3_Str ("Layer 3 Strength", Range(0, 0.5)) = 0.02
        _L3_Rot ("Layer 3 Rotation", Range(-3.14, 3.14)) = -0.3
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

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _BrushTex;
            sampler2D _PaperTex;
            sampler2D _CameraDepthNormalsTexture;
            sampler2D _VignetteTex;

            float2 _NoiseOffset1, _NoiseOffset2, _NoiseOffset3, _VignetteNoiseOffset;
            float _StrokeLength, _VerticalBias, _PaperStrength, _DebugView;
            
            float4 _VignetteColor;
            float _VignetteSize, _VignetteSharpness, _BlurStrength;
            
            // New Vignette 2 Variables
            float _Vignette2Size, _Vignette2Smoothness;

            float _L1_Scale, _L1_Str, _L1_Rot;
            float _L2_Scale, _L2_Str, _L2_Rot;
            float _L3_Scale, _L3_Str, _L3_Rot;

            float2 GetBrushUV(float2 uv, float angle, float scale, float stretch, float2 offset) {
                float2 aspectUV = uv; 
                aspectUV.x *= _ScreenParams.x / _ScreenParams.y;
                aspectUV += offset;
                float2 baseUV = aspectUV * scale;
                baseUV.y /= stretch; 
                float c = cos(angle);
                float s = sin(angle);
                return float2(baseUV.x * c - baseUV.y * s, baseUV.x * s + baseUV.y * c);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. DEPTH & ANGLE
                float4 rawDepthNormal = tex2D(_CameraDepthNormalsTexture, i.uv);
                float3 normal;
                float depthUndistortedLinear; 
                DecodeDepthNormal(rawDepthNormal, depthUndistortedLinear, normal);
                float geoAngle = atan2(normal.y, normal.x);
                float baseAngle = lerp(geoAngle, 1.57, _VerticalBias); 

                // 2. BRUSH DISTORTION
                float2 uv1 = GetBrushUV(i.uv, baseAngle + _L1_Rot, _L1_Scale, _StrokeLength, _NoiseOffset1);
                float2 d1 = (tex2D(_BrushTex, uv1).xy * 2.0 - 1.0) * _L1_Str;

                float2 uv2 = GetBrushUV(i.uv, baseAngle + _L2_Rot, _L2_Scale, _StrokeLength * 0.8, _NoiseOffset2);
                float2 d2 = (tex2D(_BrushTex, uv2).xy * 2.0 - 1.0) * _L2_Str;

                float2 uv3 = GetBrushUV(i.uv, baseAngle + _L3_Rot, _L3_Scale, _StrokeLength * 0.5, _NoiseOffset3);
                float2 d3 = (tex2D(_BrushTex, uv3).xy * 2.0 - 1.0) * _L3_Str;

                float2 totalDistortion = d1 + d2 + d3;
                float2 distortedUV = i.uv + totalDistortion;

                // 3. DEPTH CHECK
                float4 rawDepthNormalDistorted = tex2D(_CameraDepthNormalsTexture, distortedUV);
                float3 normalDistorted; 
                float depthDistortedLinear;
                DecodeDepthNormal(rawDepthNormalDistorted, depthDistortedLinear, normalDistorted);
                float distDistorted = depthDistortedLinear;

                if (_DebugView > 0.5) return float4(depthUndistortedLinear * 50, depthDistortedLinear * 50, 0, 1);

                // 4. COLOR SELECTION
                fixed4 rawColor = tex2D(_MainTex, i.uv);
                fixed4 smearedColor = tex2D(_MainTex, distortedUV);
                fixed4 selectedColor = (depthUndistortedLinear > depthDistortedLinear - 0.01) ? smearedColor : rawColor;

                // --- 5. VIGNETTE PRE-CALCULATIONS ---
                float2 distFromCenter = abs(i.uv - 0.5);
                float boxDist = max(distFromCenter.x, distFromCenter.y); 
                
                // --- 6. BACKGROUND BLUR (Behind vignettes) ---
                float blurMask = smoothstep(_VignetteSize - 0.1, _VignetteSize + 0.3, boxDist);
                if (blurMask > 0.0 && _BlurStrength > 0.0)
                {
                    float blurAmt = _BlurStrength * blurMask;
                    fixed4 blurCol = 0;
                    blurCol += tex2D(_MainTex, i.uv + float2(-blurAmt, -blurAmt));
                    blurCol += tex2D(_MainTex, i.uv + float2(0, -blurAmt));
                    blurCol += tex2D(_MainTex, i.uv + float2(blurAmt, -blurAmt));
                    blurCol += tex2D(_MainTex, i.uv + float2(-blurAmt, 0));
                    blurCol += tex2D(_MainTex, i.uv); 
                    blurCol += tex2D(_MainTex, i.uv + float2(blurAmt, 0));
                    blurCol += tex2D(_MainTex, i.uv + float2(-blurAmt, blurAmt));
                    blurCol += tex2D(_MainTex, i.uv + float2(0, blurAmt));
                    blurCol += tex2D(_MainTex, i.uv + float2(blurAmt, blurAmt));
                    blurCol /= 9.0;
                    selectedColor = lerp(selectedColor, blurCol, blurMask);
                }

                // --- 7. VIGNETTE 1: SHARP STREAKS (The "Artistic" Layer) ---
                fixed4 vignetteNoise = tex2D(_VignetteTex, i.uv + _VignetteNoiseOffset);
                float noiseInfluence = vignetteNoise.r * 0.5; 
                float combinedValue = boxDist + noiseInfluence;
                float edgeThreshold = _VignetteSize; 
                float smoothness = (1.0 - _VignetteSharpness) * 0.2 + 0.001; 
                float whiteMask = smoothstep(edgeThreshold, edgeThreshold + smoothness, combinedValue);
                selectedColor = lerp(selectedColor, _VignetteColor, whiteMask);

                // --- 8. VIGNETTE 2: SOLID SAFETY (The "Coverage" Layer) ---
                // This ensures the corners are absolutely white regardless of noise holes
                // It uses a simple smoothstep based on box distance
                float safetyMask = smoothstep(_Vignette2Size, _Vignette2Size + _Vignette2Smoothness, boxDist);
                selectedColor = lerp(selectedColor, _VignetteColor, safetyMask);

                // 9. PAPER & FINISH
                fixed4 paper = tex2D(_PaperTex, i.uv * _L1_Scale); 
                float buildUp = length(totalDistortion) * 5.0; 
                selectedColor.rgb -= buildUp * 0.1; 

                float4 finalColor = lerp(selectedColor, selectedColor * paper, _PaperStrength);
                finalColor.a = distDistorted; 
                
                return finalColor;
            }
            ENDCG
        }
    }
}