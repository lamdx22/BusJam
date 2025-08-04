Shader "Hiker/ToyColorBloomUnlit"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _BloomColor ("Bloom", Color) = (1,1,1,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows finalcolor:mycolor

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
          float2 uv_MainTex;
        };

        half _Glossiness;
        fixed4 _BloomColor;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            o.Albedo = _Color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0.0;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }

        void mycolor (Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            color.rgb = lerp(color.rgb, _BloomColor.rgb, _BloomColor.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
