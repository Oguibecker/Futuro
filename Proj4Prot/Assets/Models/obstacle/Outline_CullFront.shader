Shader "Custom/Outline_CullFront_Emissive" // Changed shader name slightly for clarity
{
    Properties
    {
        _OutlineColor ("Outline Color (RGB) / Intensity (A)", Color) = (1,1,1,1) // Now controls color and intensity
        _OutlineThickness ("Outline Thickness", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" } // Render after other opaque objects
        LOD 100

        Pass
        {
            Cull Front
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha // Keeping blend for flexibility, though often not needed for opaque outlines

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR; // This will carry the emission color to the fragment shader
            };

            // _OutlineColor is now a fixed4, its 'a' (alpha) component will be used for intensity
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                v.vertex.xyz += v.normal * _OutlineThickness;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Multiply the RGB color by the alpha component for intensity
                // This creates a HDR color value that Bloom can react to
                o.color.rgb = _OutlineColor.rgb * _OutlineColor.a; // RGB * Intensity
                o.color.a = 1.0; // Keep alpha at 1.0 for the outline itself if not truly transparent
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Simply return the calculated color.
                // This color will be read by the Bloom effect.
                return i.color;
            }
            ENDCG
        }
    }
}