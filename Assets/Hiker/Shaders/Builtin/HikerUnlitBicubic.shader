Shader "Hiker/UnlitBicubic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTex_TexelSize ("Texel Size", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 100

        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 cubic(float x)
            {
                float a = -0.5;
                float4 coeffs;
                coeffs.x = ((a * (x + 1) - 5 * a) * (x + 1) + 8 * a) * (x + 1) - 4 * a;
                coeffs.y = ((a + 2) * x - (a + 3)) * x * x + 1;
                coeffs.z = ((a + 2) * (1 - x) - (a + 3)) * (1 - x) * (1 - x) + 1;
                coeffs.w = ((a * (2 - x) - 5 * a) * (2 - x) + 8 * a) * (2 - x) - 4 * a;
                return coeffs;
            }

            fixed4 textureBicubic(sampler2D tex, float2 uv, float2 texelSize)
            {
                float2 coord = uv / texelSize - 0.5;
                float2 baseCoord = floor(coord);
                float2 f = coord - baseCoord;

                float4 wx = cubic(f.x);
                float4 wy = cubic(f.y);

                float4 color = 0;
                for (int m = -1; m <= 2; ++m)
                {
                    for (int n = -1; n <= 2; ++n)
                    {
                        float2 offset = texelSize * float2(m, n);
                        float2 sampleUV = (baseCoord + float2(m, n) + 0.5) * texelSize;
                        float weight = wx[m + 1] * wy[n + 1];
                        color += tex2D(tex, sampleUV) * weight;
                    }
                }

                return color;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return textureBicubic(_MainTex, i.uv, _MainTex_TexelSize.xy);
            }

            ENDCG
        }
    }
}
