Shader "SupGames/Mobile/ColorGrading"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata 
	{
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
	};

	struct v2f 
	{
		fixed4 pos : POSITION;
		fixed4 uv  : TEXCOORD0;
#if defined(BLUR)
		fixed4  uv1 : TEXCOORD1;
		fixed4  uv2 : TEXCOORD2;
#endif
#if defined(SHARPEN)
		fixed4  uv3 : TEXCOORD3;
#endif
	};

	sampler2D _MainTex;
	sampler2D _MaskTex;
	fixed4 _Color;
	fixed _HueCos;
	fixed _HueSin;
	fixed3 _HueVector;
	fixed _Contrast;
	fixed _Brightness;
	fixed _Saturation;
	fixed _Exposure;
	fixed _Gamma;
	fixed _Blur;
	fixed _CentralFactor;
	fixed _SideFactor;
	fixed _Vignette;
	fixed4 _MainTex_TexelSize;

	v2f vert(appdata i) 
	{
		v2f o;
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv.xy =  i.uv;	
		o.uv.zw = i.uv - 0.5h;
#if defined(BLUR)
		o.uv1 = fixed4(i.uv - _MainTex_TexelSize.xy * fixed2(1.3846153846h, 1.3846153846h)*_Blur, i.uv + _MainTex_TexelSize.xy * fixed2(1.3846153846h, 1.3846153846h)*_Blur);
		o.uv2 = fixed4(i.uv - _MainTex_TexelSize.xy * fixed2(3.2307692308h, 3.2307692308h)*_Blur, i.uv + _MainTex_TexelSize.xy * fixed2(3.2307692308h, 3.2307692308h)*_Blur);
#endif
#if defined(SHARPEN)
		o.uv3 = fixed4(i.uv - _MainTex_TexelSize.xy, i.uv + _MainTex_TexelSize.xy);
#endif
		return o;
	} 

	fixed4 fragFilter(v2f i) : COLOR 
	{
		fixed4 c = tex2D(_MainTex, i.uv.xy);
#if defined(SHARPEN)
		c *= _CentralFactor;
		c -= tex2D(_MainTex, i.uv3.xy) * _SideFactor;
		c -= tex2D(_MainTex, i.uv3.xw) * _SideFactor;
		c -= tex2D(_MainTex, i.uv3.zy) * _SideFactor;
		c -= tex2D(_MainTex, i.uv3.zw) * _SideFactor;
#endif
#if defined(BLUR)
		fixed4 m = tex2D(_MaskTex, i.uv.xy);
		fixed4 tempc = c;
		c *= 0.227027027h;
		c += tex2D(_MainTex, i.uv1.xy)*0.3162162162h;
		c += tex2D(_MainTex, i.uv1.zw)*0.3162162162h;
		c += tex2D(_MainTex, i.uv2.xy)*0.0702702703h;
		c += tex2D(_MainTex, i.uv2.zw)*0.0702702703h;
		c = lerp(tempc, c, m.r);
#endif
		c.rgb = c * _HueCos + cross(_HueVector,c.rgb)*_HueSin + _HueVector * dot(_HueVector,c.rgb)*(1 - _HueCos);
		c.rgb = (c.rgb - 0.5h) * _Contrast + _Brightness;
		c.rgb = lerp(dot(c.rgb, fixed3(0.299h, 0.587h, 0.114h)), c.rgb, _Saturation);
		c.rgb *= (pow(2, _Exposure) - _Gamma)*_Color;
		c.rgb *= 1.0h - dot(i.uv.zw, i.uv.zw) * _Vignette;
		return c;
	}

	ENDCG 
		
	Subshader 
	{
		Pass 
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }      
	      CGPROGRAM
	      #pragma vertex vert
	      #pragma fragment fragFilter
	      #pragma fragmentoption ARB_precision_hint_fastest
		  #pragma shader_feature BLUR
		  #pragma shader_feature SHARPEN
	      ENDCG
	  	}
	}
	Fallback off
}