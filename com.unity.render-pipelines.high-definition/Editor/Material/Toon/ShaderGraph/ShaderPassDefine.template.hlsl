// Setup a define to say we are an toon shader
#define SHADER_TOON
$EnableShadowMatte: #define _ENABLE_SHADOW_MATTE

// Following Macro are only used by Toon material
#if defined(_ENABLE_SHADOW_MATTE) && SHADERPASS == SHADERPASS_FORWARD_TOON
#define LIGHTLOOP_DISABLE_TILE_AND_CLUSTER
#define HAS_LIGHTLOOP
#endif
