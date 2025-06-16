Shader "Hidden/Kino/Glitch/Digital"
{
    Properties
    {
        _MainTex    ("-", 2D) = "" {}
        _NoiseTex ("-", 2D) = "" {}
        _TrashTex ("-", 2D) = "" {}
        _Intensity ("Intensity", Range(0,1)) = 0
        _GlitchColor ("Glitch Color", Color) = (1,1,1,1) // Added: New color property
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _NoiseTex;
    sampler2D _TrashTex;
    float _Intensity;
    float4 _GlitchColor; // Added: Declaration of the new color variable

    float4 frag(v2f_img i) : SV_Target
    {
        float4 glitch = tex2D(_NoiseTex, i.uv);

        float thresh = 1.001 - _Intensity * 1.001;
        float w_d = step(thresh, pow(glitch.z, 2.5)); // displacement glitch
        float w_f = step(thresh, pow(glitch.w, 2.5)); // frame glitch
        float w_c = step(thresh, pow(glitch.z, 3.5)); // color glitch

        // Displacement.
        float2 uv = frac(i.uv + glitch.xy * w_d);
        float4 source = tex2D(_MainTex, uv);

        // Mix with trash frame, tinted by GlitchColor
        float3 trashColor = tex2D(_TrashTex, uv).rgb;
        // Blend the trash texture's color with _GlitchColor based on _GlitchColor's alpha
        trashColor = lerp(trashColor, _GlitchColor.rgb, _GlitchColor.a);
        float3 color = lerp(source, trashColor, w_f).rgb;

        // Shuffle color components.
        float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.5);
        color = lerp(color, neg, w_c);

        // Additive tint for the glitch, weighted by intensity
        // This overlays the _GlitchColor on top of the glitched areas for both color and frame glitches.
        color = lerp(color, color + _GlitchColor.rgb * _Intensity * 0.5, w_c);
        color = lerp(color, color + _GlitchColor.rgb * _Intensity * 0.5, w_f);

        return float4(color, source.a);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}