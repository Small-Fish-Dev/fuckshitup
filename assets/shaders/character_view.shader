HEADER
{
	Description = "Character View";
}

MODES
{
	Default();
    VrForward();
}

COMMON
{
	#include "postprocess/shared.hlsl"
}

struct VS_INPUT
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
};

struct PS_INPUT
{
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs : SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs : SV_Position;
	#endif
};

VS
{
    PS_INPUT MainVs( VS_INPUT i )
    {
        PS_INPUT o;

        o.vPositionPs = float4( i.vPositionOs.xyz, 1.0f );

        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"

    CreateTexture2D( g_ColorTexture ) < Attribute( "ColorTexture" ); SrgbRead( true ); Filter( POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;

    /*
    void mainImage( out vec4 fragColor, in vec2 fragCoord )
    {
        const float STOPS = 10.0;
        const vec3 COLOR_1 = vec3(1, 0, 0);
        const vec3 COLOR_2 = vec3(0, 0, 0);
        
        // Normalized pixel coordinates (from 0 to 1)
        vec2 uv = fragCoord/iResolution.xy;
        // Output to screen
        float coord = uv.x + uv.y + iTime * -0.1f;
        float delta = floor(mod(coord * STOPS, 2.0f));
        vec3 col = mix(COLOR_1, COLOR_2, delta);
        fragColor = vec4(col,1.0);
    }
    */
    
    float4 MainPs( PS_INPUT i ) : SV_Target0
    { 
        const int STOPS = 10;
        const float3 COLOR_1 = float3(0, 0, 0);
        const float3 COLOR_2 = float3(1, 1, 1);

        float2 uv = i.vPositionSs.xy / g_vViewportSize.xy;
        float4 color = Tex2D( g_ColorTexture, uv.xy ).rgba;
        
        float coord = uv.x + uv.y + g_flTime * 0.1f;
        float delta = floor((coord * STOPS) % 2);
        return float4(lerp(COLOR_1, COLOR_2, delta), 1);
    }
}