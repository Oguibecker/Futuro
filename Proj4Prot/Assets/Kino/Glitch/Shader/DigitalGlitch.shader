Shader "Hidden/Kino/Glitch/Digital"
{
    Properties
    {
        _MainTex        ("-", 2D) = "" {}
        _NoiseTex ("-", 2D) = "" {}
        _TrashTex ("-", 2D) = "" {}
        _Intensity ("Intensity", Range(0,1)) = 0
        _GlitchColor ("Glitch Color", Color) = (1,1,1,1) // Default can still be white for now
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _NoiseTex;
    sampler2D _TrashTex;
    float _Intensity;
    float4 _GlitchColor;

    float4 frag(v2f_img i) : SV_Target
    {
        float4 glitch = tex2D(_NoiseTex, i.uv);

        // Calculate thresholds for different glitch types
        float thresh = 1.001 - _Intensity * 1.001; // Low intensity -> high thresh (less glitch)
                                                 // High intensity -> low thresh (more glitch)

        float w_d = step(thresh, pow(glitch.z, 2.5)); // displacement glitch weight
        float w_f = step(thresh, pow(glitch.w, 2.5)); // frame glitch weight
        float w_c = step(thresh, pow(glitch.z, 3.5)); // color glitch weight (uses z, so same noise channel as displacement but different power)

        // 1. Displacement: Apply UV shift
        // 'uv' is the sample coordinate for both MainTex and TrashTex
        float2 uv = frac(i.uv + glitch.xy * w_d); // glitch.xy scales with w_d (displacement)

        // Sample the main texture with the (potentially) displaced UVs
        float4 source = tex2D(_MainTex, uv);

        // 2. Trash Frame Glitch: Mix with previous frame, heavily influenced by _GlitchColor
        float3 trashColor = tex2D(_TrashTex, uv).rgb;
        
        // Option A: Blend trashColor strongly towards _GlitchColor based on w_f and _Intensity
        // This will make the trash frame areas more directly take on _GlitchColor.
        // We use a stronger blend factor here to ensure the _GlitchColor dominates.
        float trashBlendFactor = w_f * _Intensity * 2.0; // Multiplying by 2.0 or more for stronger effect
        trashBlendFactor = saturate(trashBlendFactor); // Ensure it stays between 0 and 1
        trashColor = lerp(trashColor, _GlitchColor.rgb, trashBlendFactor);

        // 3. Combine displaced source and glitched trash frame
        // The w_f mask controls the blend between source and the glitched trashColor
        float3 finalColor = lerp(source.rgb, trashColor, w_f).rgb;

        // 4. Color Shuffle Glitch: Apply color component reshuffling
        // The 'neg' part creates the color inverse/shuffle effect
        float3 neg = saturate(finalColor.grb + (1 - dot(finalColor, 1)) * 0.5); // (1 - dot(color, 1)) is luminance
        // Blend finalColor towards the shuffled/inverted color based on w_c
        finalColor = lerp(finalColor, neg, w_c);
        
        // 5. Final Glitch Color Overlay (most direct way to inject _GlitchColor into the 'noise')
        // This is where we ensure _GlitchColor tints the active glitch effects strongly.
        // We'll blend the `finalColor` towards the `_GlitchColor` directly,
        // weighted by the active glitch masks and intensity.

        // Combine the masks to create a single 'glitch activity' factor
        float overallGlitchActivity = max(w_c, w_f); // If either color or frame glitch is active

        // Option A: Direct blend towards _GlitchColor
        // This will make the glitched areas *become* _GlitchColor as intensity increases.
        finalColor = lerp(finalColor, _GlitchColor.rgb, overallGlitchActivity * _Intensity * 2.0); // Stronger influence

        // Option B (Alternative for a more "additive glow" if A is too flat):
        // finalColor += _GlitchColor.rgb * overallGlitchActivity * _Intensity * 1.5; // Additive, might still saturate
        // finalColor = saturate(finalColor); // Clamp to prevent values exceeding 1.0


        return float4(finalColor, source.a); // Use source.a for alpha, assuming no transparency from glitch
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