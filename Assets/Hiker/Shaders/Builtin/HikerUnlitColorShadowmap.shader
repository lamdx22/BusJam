Shader "Hiker/UnlitColorShadowmap"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_ColorCenter("Color Center", Color) = (1,1,1,1)
		_LightRange("Range", Range(0, 1)) = 0.75
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 150

	CGPROGRAM
	#pragma surface surf Unlit noforwardadd noambient nofog nometa nolppv nolightmap exclude_path:deferred
	#pragma vertex:vert
    #include "UnityCG.cginc"

	half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
		half4 c;
		c.rgb = s.Albedo*atten;
		c.a = s.Alpha;
		return c;
	}

	half4 _Color;
	half4 _ColorCenter;
	float _LightRange;
	// sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex : TEXCOORD0;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		// fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		float t = distance(IN.uv_MainTex, half2(0.5, 0.5)) * 2.0 / _LightRange;
		o.Albedo = lerp(_ColorCenter.rgb, _Color.rgb, clamp(t, 0, 1)) ;
		o.Alpha = _Color.a;
	}
	ENDCG
	}

	Fallback "Mobile/VertexLit"
}
