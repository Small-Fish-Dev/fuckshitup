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

    CreateTexture2D( g_ColorTexture ) < Attribute( "ColorTexture" ); SrgbRead( true ); >;
    CreateTexture2D( g_HoverTexture ) < Attribute( "HoverTexture" ); SrgbRead( true ); >;

    float4 MainPs( PS_INPUT i ) : SV_Target0
    { 
        // Stripe pattern
        const int STOPS = 40;
        const float3 COLOR_1 = float3(0.025, 0.025, 0.025);
        const float3 COLOR_2 = float3(0.015, 0.015, 0.015);

        float2 uv = i.vPositionSs.xy / g_vViewportSize.xy;
        float4 colorTex = g_ColorTexture.Sample( g_sPointWrap, uv.xy ).rgba;
        
        float coord = uv.x + uv.y + g_flTime * 0.01f;
        float delta = floor((coord * STOPS) % 2);

        float hoverAlpha = g_HoverTexture.Sample( g_sPointWrap, uv.xy ).a;
        float3 color = lerp(
            lerp(float3(0, 0.5, 0), float3(0, 0.2, 0), delta),
            lerp(COLOR_1, COLOR_2, delta),
            1 - hoverAlpha);

        // Outline
        const float2 STEP_SIZE = 0.003f; 
        const float3 OUTLINE_COLOR = float3(0, 0, 0);
        float alpha = -8.0 * colorTex.a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( STEP_SIZE.x, 0 ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( -STEP_SIZE.x, 0 ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( 0, STEP_SIZE.y ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( 0, -STEP_SIZE.y ) ).a;

        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( STEP_SIZE.x, STEP_SIZE.y ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( -STEP_SIZE.x, STEP_SIZE.x ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( STEP_SIZE.x, -STEP_SIZE.x ) ).a;
        alpha += g_ColorTexture.Sample( g_sPointWrap, uv.xy + float2( -STEP_SIZE.x, -STEP_SIZE.y ) ).a;

        // Final result
        color = lerp(color, OUTLINE_COLOR, ceil(alpha - colorTex.a + 0.5));
        return float4(color.rgb, colorTex.a + max(alpha, 0));
    }
}