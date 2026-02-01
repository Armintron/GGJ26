Shader "Custom/UnlitChowderFlipbook"
{
    Properties
    {
        _MainTex ("Sprite Sheet", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        [Header(Flipbook Settings)]
        _Columns ("Columns (Horizontal)", Float) = 4
        _Rows ("Rows (Vertical)", Float) = 4
        _Speed ("Animation Speed (FPS)", Float) = 12

        [Header(Screen Projection)]
        _PatternScale ("Pattern Scale", Float) = 1.0
        [Toggle] _FixAspectRatio ("Fix Aspect Ratio", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

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
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _Columns;
            float _Rows;
            float _Speed;
            float _PatternScale;
            float _FixAspectRatio;

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                if (_FixAspectRatio > 0.5)
                {

                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    screenUV.x *= aspect;
                }

                screenUV *= _PatternScale;

                float2 cellSize = float2(1.0 / _Columns, 1.0 / _Rows);

                float2 cellUV = frac(screenUV) * cellSize;

                float totalFrames = _Columns * _Rows;
                float currentFrame = floor(_Time.y * _Speed);
                float frameIndex = fmod(currentFrame, totalFrames);

                float currentColumn = fmod(frameIndex, _Columns);

                float currentRow = (_Rows - 1) - floor(frameIndex / _Columns);

                float2 offset = float2(currentColumn * cellSize.x, currentRow * cellSize.y);

                float2 finalUV = cellUV + offset;

                fixed4 col = tex2D(_MainTex, finalUV) * _Color;
                return col;
            }
            ENDCG
        }
    }
}